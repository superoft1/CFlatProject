namespace Chiyoda.CAD.Model.Structure
{
  internal interface IStructuralMaterial
  {
    string Name { get ; }
    
    bool IsSteel { get ; }
    
    SteelShapeType ShapeType { get ; }

    double MainSize { get ; }
    
    double SubSize { get ; }

    MaterialRotation Rotation { get ; }

    IStructuralMaterial CreateCopy() ;
  }
}