using System.Collections.Generic ;
using System.Linq ;
using UnityEngine;

namespace Chiyoda.CAD.Model.Structure
{
  public class HSteel : IStructurePart
  {
    public HSteel( double h, double b, double length )
    {
      H = h ;
      B = b ;
      Length = length ;
    }

    public double H { get ; }
    public double B { get ; }

    public double Length { get ; }

    public LocalCodSys3d LocalCod { get ; set ; }
  }

  public class ShallowFoundations : IStructurePart
  {
    private List<Vector3> _positions ;
    
    public ShallowFoundations( double width, double columnWidth, IEnumerable<Vector3> positions )
    {
      Width = width ;
      ColumnWidth = columnWidth ;
      _positions = positions.ToList() ;
    }
    
    public double Width { get ; }
    
    public double ColumnWidth { get ; }

    public IEnumerable<Vector3d> LocalPositions => _positions.Select( p => LocalCod.GlobalizePoint( p ) ) ;

    public LocalCodSys3d LocalCod { get ; set ; }
  }

  public class DeepFoundations : IStructurePart
  {
    public DeepFoundations( double width, Vector3 position )
    {
      Width = width ;
      
    }
    
    public double Width { get ; }
    public LocalCodSys3d LocalCod { get ; set ; }
  }
}