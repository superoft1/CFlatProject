using System ;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.Test.Routing
{
  [DataContract]
  public class RouteDefinition
  {
    [DataMember] public string Name { get; set; }
    [DataMember] public List<string> FromList { get; set; }
    [DataMember] public List<string> ToList { get; set; }
    [DataMember] public bool IsEndPointDirectionFix { get; set; }
    [DataMember] public bool IsRoutingPipeRacks { get; set; }
    [DataMember] public string LineType { get; set; }
    [DataMember] public string Color { get; set; }
    [DataMember] public string FluidPhase { get; set; }

    [IgnoreDataMember] public Route Route { get; private set; }
    public void Setup(Document document, List<EquipDefinition> equips)
    {
      Route = document.CreateEntity<Route>();
      document.AddEdge(Route);
      Route.Name = Name;
      Route.LineId = Name;
      Route.LineType = LineType;
      Route.Color = Color;
      Route.FluidPhase = FluidPhase;
      Route.IsEndPointDirectionFix = IsEndPointDirectionFix;
      Route.IsRoutingPipeRacks = IsRoutingPipeRacks;

      IEndPoint Convert( (string equipName, bool isFrom) arg)
      {
        if ( equips.All( equip => equip.Name != arg.equipName ) ) {
          Debug.LogError( $"Cannot found {arg.equipName}" ) ;
          return null ;
        }
        var vertex = FindVertex( equips, arg.equipName ) ;
        if(arg.isFrom) vertex.Flow = HalfVertex.FlowType.FromThisToAnother ;
        else vertex.Flow = HalfVertex.FlowType.FromAnotherToThis ;
        return document.RegisterEndPoint( vertex ) ;
      }

      var fromPoints = FromList.Select(val => (equipName : val, isFrom : true) ).Select( Convert ).Where( p => p != null ).ToArray() ;
      var toPoints = ToList.Select(val => (equipName : val, isFrom : false) ).Select( Convert ).Where( p => p != null ).ToArray() ;
      if ( ! fromPoints.Any() || ! toPoints.Any() ) {
        Route = null ;
        return ;
      }
      
      Route.SetMainRoute( fromPoints.First(), toPoints.First() );
      fromPoints.Skip( 1 ).Select( p => document.RegisterBranch( p, true ) )
        .ForEach( b => Route.AddBranch( b ) );
      toPoints.Skip( 1 ).Select( p => document.RegisterBranch( p, false ) )
        .ForEach( b => Route.AddBranch( b ) );
      
    }
    
    private static HalfVertex FindVertex(List<EquipDefinition> equips, string equipName)
    {
      var equipment = equips.First(equip => equip.Name == equipName).Equipment;
      var nozzle = equipment.Nozzles.First();
      var vertex = equipment.LeafEdge.GetVertex( nozzle.NozzleNumber ) ;
      return vertex;
    }

    public static List<RouteDefinition> CreateDummy()
    {
      return new List<RouteDefinition>()
      {
        new RouteDefinition()
        {
          Name = "Route1",
          FromList =  new List<string>(){"EquipFrom1"},
          ToList = new List<string>(){"EquipTo1" ,"EquipTo2"},
          IsEndPointDirectionFix = true,
          IsRoutingPipeRacks = true,
          LineType = "P",
          Color = "Green",
          FluidPhase = "L"
        }
      };
    }
  }
}