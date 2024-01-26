using System.Collections.Generic ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface ISpecificationService
  {
    IStructuralMaterial Column( IStructuralMaterial beam ) ;
    IStructuralMaterial Beam( double spanWidth ) ;

    IList<string> GetList() ;

    IStructuralMaterial Get( string name ) ;

    //IStructureElement GetElement( IStructuralMaterial material, double length ) ;
  }
}