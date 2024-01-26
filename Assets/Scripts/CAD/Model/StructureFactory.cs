using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure ;
using Chiyoda.CAD.Model.Structure.Entities;

namespace Chiyoda.CAD.Model
{
  public static class StructureFactory
  {
    public static IPipeRack CreatePipeRack( Document doc, PipeRackFrameType type, string name = "Pipe Rack" )
    {
      var registration = GetRegistrationFunc( type ) ;
      var rack = registration( doc ) ;
      rack.Name = name ;
      doc.Structures.Add( rack );
      return rack as IPipeRack;
    }
    
    public static IEquipmentStructure CreateEquipmentStructure( Document doc, string name = "Equipment Structure" )
    {
      var str = doc.CreateEntity<EquipmentStructure>() ;
      str.Name = name ;
      doc.Structures.Add( str );
      return str ;
    }

    private static Func<Document, Entity> GetRegistrationFunc( PipeRackFrameType type )
    {
      switch ( type ) {
        case PipeRackFrameType.Single:
          return doc => doc.CreateEntity<PipeRackSingle>();
        case PipeRackFrameType.Double:
          throw  new NotImplementedException();
        case PipeRackFrameType.OnePlusOne:
          return doc => doc.CreateEntity<PipeRack1Plus1>() ;
        default:
          return doc => doc.CreateEntity<PipeRackSingle>() ;
      }
    }
  }
}
