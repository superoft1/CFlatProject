using System ;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model.Structure.CommonEntities
{
  [System.Serializable]
  public class PlacementEntity : Entity, IRelocatable
  {
    private readonly Memento<LocalCodSys3d> _localCod ;
    
    protected PlacementEntity( Document document ) : base( document )
    {
      _localCod = CreateMemento( LocalCodSys3d.Identity ) ;
      _localCod.AfterNewlyValueChanged += ( s, e ) => NewlyLocalCodChanged?.Invoke( this, EventArgs.Empty ) ;
      _localCod.AfterHistoricallyValueChanged += ( s, e ) => HistoricallyLocalCodChanged?.Invoke( this, EventArgs.Empty ) ; 
    }

    public event EventHandler NewlyLocalCodChanged;
    public event EventHandler HistoricallyLocalCodChanged;
    
    public event EventHandler LocalCodChanged
    {
      add
      {
        NewlyLocalCodChanged += value ;
        HistoricallyLocalCodChanged += value ;
      }
      remove
      {
        NewlyLocalCodChanged -= value ;
        HistoricallyLocalCodChanged -= value ;
      }
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      if ( another is PlacementEntity st ) {
        _localCod.Value = st._localCod.Value ;
      }
    }

    /// <summary>
    /// グローバル座標系上で表現されるローカル座標系
    /// </summary>
    /// <value>The structure cod.</value>
    public virtual LocalCodSys3d LocalCod
    {
      get => _localCod.Value ;
      set
      {
        _localCod.Value = value ;
        
      }
    }

    public LocalCodSys3d ParentCod
    {
      get
      {
        for ( var parent = Parent ; null != parent && ! ( parent is Document ) ; parent = parent.Parent ) {
          if ( parent is IPlacement p ) {
            return p.GlobalCod ;
          }
        }

        return LocalCodSys3d.Identity ;
      }
    }

    public LocalCodSys3d GlobalCod => ParentCod.GlobalizeCodSys( LocalCod ) ;
    
  }
}
