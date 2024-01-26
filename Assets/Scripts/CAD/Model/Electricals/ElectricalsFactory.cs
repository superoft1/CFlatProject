using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CableRouting ;

namespace Chiyoda.CAD.Model.Electricals
{
  public class ElectricalsFactory
  {
    public static ElectricalDevices Create( Document doc, ElectricalDeviceType type )
    {
      var registration = GetRegistrationFunc( type ) ;
      var entity = registration( doc ) as ElectricalDevices ;
      doc.Electrical.Add( entity );
      return entity ;
    }
    private static Func<Document, Entity> GetRegistrationFunc( ElectricalDeviceType type  )
    {
      switch ( type ) {
        case ElectricalDeviceType.LocalPanel: return doc => doc.CreateEntity<LocalPanel>() ;
        case ElectricalDeviceType.Substation: return doc => doc.CreateEntity<SubStation>() ;
        default: throw new NotImplementedException();
      }
    }
    public static Cable CreateCable( Document doc, ICable prototype = null )
    {
      var entity = doc.CreateEntity<Cable>() as Cable; 
      doc.Electrical.Add( entity );
      if (prototype != null) {
        throw  new NotImplementedException();
      }
      return entity ;
    }
    public static CablePath CreateCablePath( Document doc, ICablePath prototype = null )
    {
      var entity = doc.CreateEntity<CablePath>() as CablePath; 
      doc.Electrical.Add( entity );
      if (prototype != null) {
        var min = prototype.Rect.min ;
        var max = prototype.Rect.max ;
        entity.Init( new UnityEngine.Vector3d( min.x, min.y, min.z), new UnityEngine.Vector3d( max.x, max.y, min.z) );
      }
      return entity ;
    }
  }
}