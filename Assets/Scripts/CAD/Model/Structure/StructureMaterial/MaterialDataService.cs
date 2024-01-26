namespace Chiyoda.CAD.Model.Structure
{
  internal static class MaterialDataService
  {
    private static readonly SteelSpecService SteelSpec = new SteelSpecService( SteelSpecificationType.JIS, new LightCaseRule() ) ;
    private static readonly RcSpecService RcSpec = new RcSpecService( new RcRule() ) ;

    public static ISpecificationService Steel( SteelShapeType type ) => SteelSpec.Get( type ) ;

    public static ISpecificationService Rc => RcSpec ; 

    public static void SetSteelSpecification( SteelSpecificationType type )
    {
      SteelSpec.ChangeSpec( type ) ;
    }

    public static IStructurePart GetElement( IStructuralMaterial material, double length )
    {
      return material.IsSteel ? SteelSpec.GetElement( material, length ) : RcSpec.GetElement( material, length ) ;
    }

    public static double SteelWeightPerLength( IStructuralMaterial material )
    {
      return material.IsSteel ? SteelSpec.SteelWeightPerLength( material ) : 0.0 ;
    }

    public static double SectionArea( IStructuralMaterial material )
    {
      return material.IsSteel ? 0.0 : RcSpec.SectionArea( material ) ;
    }
  }
}