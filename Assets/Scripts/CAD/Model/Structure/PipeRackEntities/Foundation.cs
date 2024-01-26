using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal struct BearingCapacity
  {
    public static BearingCapacity KiloNewton( double value )
    {
      return new BearingCapacity( value ); 
    }
    
    private readonly double _capacity ; // kN/m^2

    public double BearingArea( double load ) => load / ( _capacity * 1000 ) ;

    public double Unit_kN => _capacity ;
    
    private BearingCapacity( double value )
    {
      _capacity = value ;
    }
  }

  [Entity( EntityType.Type.Foundation )]
  internal class Foundation : EmbodyStructure, IFoundation
  {
    private readonly Memento<bool> _isShallow ;
    private readonly Memento<BearingCapacity> _bearingCapacity ;
    private readonly Memento<double> _load ; // kN 
    private RackFrames _frames ;
    
    public Foundation( Document document ) : base( document )
    {
      _isShallow = CreateMementoAndSetupValueEvents( true ) ;
      _bearingCapacity = CreateMementoAndSetupValueEvents( BearingCapacity.KiloNewton( 200.0 ) ) ;
      _load = CreateMemento( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      if ( ! ( another is Foundation f ) ) {
        return ;
      }
      
      _isShallow.CopyFrom( f.IsShallow );
      _bearingCapacity.CopyFrom( BearingCapacity.KiloNewton( f._bearingCapacity.Value.Unit_kN ) ) ;
      _load.CopyFrom( f.Load );
    }

    public void Link( RackFrames frames )
    {
      _frames = frames ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.BaseData, "Shallow Type", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable )]
    public bool IsShallow
    {
      get => _isShallow.Value ;
      set => _isShallow.Value = value ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.BaseData, "Bearing Capacity", ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable )]
    public double Capacity
    {
      get => _bearingCapacity.Value.Unit_kN ;
      set => _bearingCapacity.Value = BearingCapacity.KiloNewton( value ) ;
    }

    public double Load
    {
      get => _load.Value ;
      set => _load.Value = value ;
    }

    public override IEnumerable<IStructurePart> StructureElements
    {
      get
      {
        var count = _frames[ 0 ].ColumnPositions.Count() * _frames.FrameCount ;
        var loadPerColumn = Load / count ;
        var w = Math.Sqrt( _bearingCapacity.Value.BearingArea( loadPerColumn ) ) ;
        var offset = 0.0 ;
        foreach ( var i in Enumerable.Range( 0, _frames.FrameCount ) ) {
          var f = _frames[ i ] ;
          offset += f.PositionOffset ;
          var cw = f[ 0 ].ColumnMaterial.MainSize ;
          var c = new ShallowFoundations( w, cw, f.ColumnPositions ) ;
          c.LocalCod = new LocalCodSys3d( new Vector3d( 0.0, offset, 0.0 ));
          yield return c ;
        }
      }    
    }
  }
}