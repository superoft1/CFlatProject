using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Linq ;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.Test.Routing
{
  [DataContract]
  public class FieldDefinition
  {
    [DataMember] private List<RackDefinition>  Racks { get; set; }
    [DataMember] private List<EquipDefinition> Equips { get; set; }
    [DataMember] private List<RouteDefinition> Routes { get; set; }
    
    public IEnumerable<Route> GetRoutes()
    {
      foreach (var route in Routes)
      {
        yield return route.Route;
      }
    }
    
    public void Save(string testConfigFilePath)
    {
      var serializer = new DataContractJsonSerializer(typeof(FieldDefinition));
      using (var stream = new FileStream(testConfigFilePath, FileMode.Create, FileAccess.Write))
      {
        serializer.WriteObject(stream, this);
      }
    }

    public static FieldDefinition Load(string testConfigFilePath)
    {
      FieldDefinition rtnObj = null;
      var serializer = new DataContractJsonSerializer(typeof(FieldDefinition));
      using (var stream = new FileStream(testConfigFilePath, FileMode.Open, FileAccess.Read))
      {
        rtnObj = (FieldDefinition)serializer.ReadObject(stream);
      }
      return rtnObj;
    }
    
    public void Setup()
    {
      var document = CAD.Core.DocumentCollection.Instance.Current ?? CAD.Core.DocumentCollection.Instance.CreateNew();

      if ( Racks != null ) foreach ( var rack in Racks ) { rack.Setup( document ) ; }
      if ( Equips != null ) foreach (var equip in Equips) { equip.Setup(document); }
      if ( Routes != null ) foreach (var route in Routes) { route.Setup(document, Equips); }
    }
  }
}