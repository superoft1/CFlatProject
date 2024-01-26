using UnityEngine.Experimental.PlayerLoop ;

namespace Chiyoda.CAD.Model.Structure
{
  internal class SteelMaterial : IStructuralMaterial
  {
    public SteelMaterial( DB.StructureElementTable.Record rec, MaterialRotation r )
    {
      Name = rec.Standard ;
      ShapeType = rec.GetSteelShapeType() ;
      MainSize = 0.001 * ( rec.H_D ?? 300 );
      SubSize = 0.001 * ( rec.B ?? 300 ) ;
      Rotation = r ;
    }

    private SteelMaterial( SteelMaterial src )
    {
      Name = src.Name ;
      ShapeType = src.ShapeType ;
      MainSize = src.MainSize ;
      SubSize = src.SubSize ; 
      Rotation = src.Rotation ;
    }

    public IStructuralMaterial CreateCopy()
    {
      return new SteelMaterial( this ) ;
    }
    
    public string Name { get ; }
    public bool IsSteel => true ; 
    public SteelShapeType ShapeType { get ; }
    public double MainSize { get ; }
    
    public double SubSize { get ; }
    
    public MaterialRotation Rotation { get ; }
  }
}