using System.Collections.Generic ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal static class MaterialSelectionViewModelHelper
  {
    private static readonly List<string> _shapeTypes = new List<string>()
    {
      SteelShapeType.H.ToKey(),
      "RC",
      SteelShapeType.L.ToKey(),
      SteelShapeType.C.ToKey(),
      SteelShapeType.T.ToKey(),
      SteelShapeType.Pipe.ToKey(),
      SteelShapeType.RB.ToKey(),
      SteelShapeType.FB.ToKey(),
    };  
    
    public static IList<string> ShapeTypes => _shapeTypes ;

    public static int TypeToListIndex( IStructuralMaterial material )
    {
      if ( !material.IsSteel ) {
        return 1 ;
      }
      switch ( material.ShapeType ) {
        case SteelShapeType.H: return 0 ;
        case SteelShapeType.L: return 2 ;
        case SteelShapeType.C: return 3 ;
        case SteelShapeType.T: return 4 ;
        case SteelShapeType.Pipe: return 5 ;
        case SteelShapeType.RB: return 6 ;
        case SteelShapeType.FB: return 7 ;
        default:
          return 0 ;
      }
    }

    public static IList<string> MaterialList( int shapeIndex ) => 
      IsSteel( shapeIndex )
        ? MaterialDataService.Steel( ToShapeType( shapeIndex ) ).GetList()
        : MaterialDataService.Rc.GetList() ;
    
    public static IList<string> MaterialList( IStructuralMaterial material ) =>
      material.IsSteel
        ? MaterialDataService.Steel( material.ShapeType ).GetList()
        : MaterialDataService.Rc.GetList() ;

    public static bool IsSteel( int index ) => index != 1 ;

    public static SteelShapeType ToShapeType( int index )
    {
      switch ( index ) {
        case 0 : return SteelShapeType.H ;
        case 1 : return SteelShapeType.Unknown ;
        case 2 : return SteelShapeType.L ;
        case 3 : return SteelShapeType.C ;
        case 4 : return SteelShapeType.T ;
        case 5 : return SteelShapeType.Pipe ;
        case 6 : return SteelShapeType.RB ;
        case 7: return SteelShapeType.FB ;
        default:
          return SteelShapeType.H ;
      }
    }
  }
}