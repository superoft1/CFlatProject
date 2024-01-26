using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VtpAutoRouting.BridgeEntities
{
  public class RoutingResultToProDraw
  {
    public int ProDrawID { get; private set; }
    public string LineName { get; private set; }
    public Vector3[] ThroughPointsPosition { get; private set; }
    
    public RoutingResultToProDraw(int proDrawID, string lineName, IEnumerable<Vector3> points)
    {
      ProDrawID = proDrawID;
      LineName = lineName;
      // ひとまず、1列で返す
      ThroughPointsPosition = points.ToArray();
    }
  }
}