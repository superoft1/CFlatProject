namespace Chiyoda.CAD.Model.Structure
{
  internal static class StructureEnumExtensions
  {
    public static string ToSpecKey( this SteelSpecificationType type )
    {
      switch ( type ) {
        case SteelSpecificationType.Unknown: return "ASTM" ;
        case SteelSpecificationType.ASTM: return "ASTM" ;
        case SteelSpecificationType.JIS: return "JIS" ;
        case SteelSpecificationType.BS : return "BS" ;
        case SteelSpecificationType.EURO: return "EURO" ;
        default:
          return "ASTM" ;
      }
    }

    public static SteelShapeType GetSteelShapeType( this DB.StructureElementTable.Record rec )
    {
      switch ( rec.Type ) {
        case "H" : return SteelShapeType.H ;
        case "C" : return SteelShapeType.C ;
        case "L" : return SteelShapeType.L ;
        case "PIPE" : return SteelShapeType.Pipe ;
        case "RB" : return SteelShapeType.RB ;
        case "TEE" : return SteelShapeType.T ;
        case "FB" : return SteelShapeType.FB ;
        
        case "RC" : return SteelShapeType.Unknown ;
        default:
          return SteelShapeType.Unknown ;
      }  
    }

    public static string ToKey( this SteelShapeType t )
    {
      switch ( t ) {
        case SteelShapeType.H :
          return "H" ;
        case SteelShapeType.C :
          return "C" ;
        case SteelShapeType.L : 
          return "L" ;
        case SteelShapeType.Pipe : 
          return "PIPE"  ;
        case SteelShapeType.RB :
          return "RB" ; 
        case SteelShapeType.T :
          return "TEE" ; 
        case SteelShapeType.FB :
          return "FB" ;
        case SteelShapeType.Unknown:
          return "RC" ;
        default:
          return "" ;
      }  
    }
  }
}