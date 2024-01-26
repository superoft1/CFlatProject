using Chiyoda.CAD.Core ;
using Chiyoda.CableRouting ;
using Chiyoda.CableRouting.Math ;
using Vector3d = UnityEngine.Vector3d ;

namespace Chiyoda.CAD.Model.Electricals
{
  [Entity( EntityType.Type.CablePath )]
  public class CablePath : Electricals, ICablePath
  {
    private readonly Memento<Vector3d> _min ;
    private readonly Memento<Vector3d> _max ;
    private readonly Memento<int> _srcID ;

    public CablePath( Document document ) : base( document )
    {
      _min = new Memento<Vector3d>( this);
      _max = new Memento<Vector3d>( this);
      _srcID = new Memento<int>( this);
    }

    public void Init( Vector3d min, Vector3d max, Entity createdBy = null ) 
    {
      this._min.Value = min ;
      this._max.Value = max ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.Position, "Min", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.ReadOnly, IsEditableForBlockPatternChildren = false )]
    public Vector3d Min => _min.Value ;

    [Chiyoda.UI.Property( UI.PropertyCategory.Position, "Max", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.ReadOnly, IsEditableForBlockPatternChildren = false )]
    public Vector3d Max => _max.Value ;

    public Vector3d Origin => (Max+ Min)/2.0;
    public Vector3d Size => (Max - Min);

    #region ICable
    public CableRouting.Math.Rectangle Rect
    {
      get
      {
        return new Rectangle(new CableRouting.Math.Vector3d(Min.x,Min.y,Min.z), new CableRouting.Math.Vector3d(Max.x,Max.y,Max.z));
      }
    }
    #endregion
  }
}