using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CableRouting ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Electricals
{
  [Entity( EntityType.Type.Cable )]
  public class Cable : Electricals, ICable
  {
    private readonly Memento<double> _diameter ; // 単位はm
    private readonly MementoList<Vector3d> _points ; // 単位はm
    
    private readonly Memento<Vector3d> _from ; // 単位はm
    private readonly Memento<Vector3d> _to ;   // 単位はm
    
    
    public Cable( Document document ) : base( document )
    {
      _diameter = new Memento<double>(this);
      _diameter.Value = 0.1 ;
      _points = CreateMementoListAndSetupValueEvents<Vector3d>() ;
      
      _from = new Memento<Vector3d>( this);
      _to = new Memento<Vector3d>( this);
    }
 
    [Chiyoda.UI.Property( UI.PropertyCategory.Dimension, "Diameter", ValueType = UI.ValueType.Length,
      Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public double Diameter
    {
      get => _diameter.Value ;
      set => _diameter.Value = value;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.Position, "StartPoint", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.ReadOnly, IsEditableForBlockPatternChildren = false )]
    public Vector3d StartPoint
    {
      get
      {
        if (!_points.Any()) return Vector3d.zero ;
        return _points.First() ;
      }
    }
    
    public IList<Vector3d> Points => _points ;

    public IEnumerable<(Vector3d, Vector3d)> CableStartEnd
    {
      get
      {
        for(int i=0; i<Points.Count() - 1 ; i++)
        {
          yield return ( Points[i]-StartPoint, Points[i+1]-StartPoint ) ;
        }
      }
    }
    public override Bounds? GetGlobalBounds()
    {
      if (!this.Points.Any()) return null ;

      var bounds = new Bounds() ;
      var maxX = this.Points.Select( ele => ele.x ).Max() ;
      var maxY = this.Points.Select( ele => ele.y ).Max() ;
      var maxZ = this.Points.Select( ele => ele.z ).Max() ;
      var minX = this.Points.Select( ele => ele.x ).Min() ;
      var minY = this.Points.Select( ele => ele.y ).Min() ;
      var minZ = this.Points.Select( ele => ele.z ).Min() ;
      
      bounds.max = new Vector3((float)maxX, (float)maxY, (float)maxZ);
      bounds.min = new Vector3((float)minX, (float)minY, (float)minZ);
      return bounds ;
    }
    public void SetFromTo( ElectricalDevices fromDevice, ElectricalDevices toDevice)
    {
      _from.Value = fromDevice.Position ;
      _to.Value = toDevice.Position ;
    }
    #region  ICable
    public CableRouting.Math.Vector3d From =>
      new CableRouting.Math.Vector3d( _from.Value.x, _from.Value.y, _from.Value.z ) ;
    public CableRouting.Math.Vector3d To =>
      new CableRouting.Math.Vector3d( _to.Value.x, _to.Value.y, _to.Value.z ) ;
    public void Add( CableRouting.Math.Vector3d point )
    {
      _points.Add( new Vector3d( point.x, point.y, point.z ) ) ;
    }
    public void ClearPoints()
    {
      _points.Clear();
    }
    #endregion
  }
}