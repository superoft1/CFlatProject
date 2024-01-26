using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Routing ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Model
{
  public static class RoutingFactory
  {
    public static IEndPoint RegisterEndPoint( this Document doc, string lineId, Vector3d pos, Vector3d dir, double diameter )
    {
      var newPoint = doc.CreateEntity<EndPoint>() ; 
      newPoint.LineId = lineId ;
      newPoint.Origin = pos ;
      newPoint.NPS = diameter ;
      newPoint.Direction = dir ;
      return newPoint ;
    }

    public static IEndPoint RegisterEndPoint( this Document doc, string lineId, HalfVertex vertex )
    {
      var p = (EndPoint) doc.RegisterEndPoint( vertex ) ;
      p.LineId = lineId ;
      return p ;
    }
    
    public static IEndPoint RegisterEndPoint( this Document doc, HalfVertex vertex )
    {
      if ( vertex?.LeafEdge == null ) {
        return null ;
      }

      var newPoint = doc.CreateEntity<EndPoint>() ;
      newPoint.LinkPoint = vertex ;
      return newPoint ;
    }

    public static IBranch RegisterBranch( this Document doc, IEndPoint p, bool isStart )
    {
      var b = doc.CreateEntity<Branch>() ;
      b.Initialize( p, isStart );
      return b ;
    }


  }
  
}