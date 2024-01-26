using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Manager
{
  static class PipeManager
  {
    public static double GetFlexRatio( this LeafEdge leafEdge )
    {
      if ( null == leafEdge ) return 0;

      var pipe = leafEdge.PipingPiece as Pipe;
      if ( null == pipe ) return 0;

      return pipe.FlexRatio;
    }

    public static double GetMinimumLength( this LeafEdge leafEdge )
    {
      if ( null == leafEdge ) return 0;

      var pipe = leafEdge.PipingPiece as Pipe;
      if ( null == pipe ) return 0;
      
      var maxOletWidth1 = leafEdge.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term1 ) ;
      var maxOletWidth2 = leafEdge.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term2 ) ;

      return pipe.MinimumLengthWithoutOletRadius + maxOletWidth1 + maxOletWidth2 ;
    }

    public static void SetMinimumLength( this LeafEdge leafEdge, double value )
    {
      if ( null == leafEdge ) return;

      var pipe = leafEdge.PipingPiece as Pipe;
      if ( null == pipe ) return;

      var maxOletWidth1 = leafEdge.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term1 ) ;
      var maxOletWidth2 = leafEdge.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term2 ) ;

      pipe.MinimumLengthWithoutOletRadius = value - maxOletWidth1 - maxOletWidth2 ;
    }

    public static double GetMaxOletWidth( this LeafEdge leafEdge, int vertexIndex )
    {
      var v = leafEdge.GetVertex( vertexIndex )?.Partner ;
      if ( null == v ) return 0 ;

      var le = v.LeafEdge ;
      switch ( le?.PipingPiece ) {
        case WeldOlet wo :
        {
          var d = wo.Diameter / 2 ;
          switch ( v.ConnectPointIndex ) {
            case (int) WeldOlet.ConnectPointType.MainTerm1 :
              return Math.Max( d, GetMaxOletWidth( le, (int) WeldOlet.ConnectPointType.MainTerm2 ) ) ;
            case (int) WeldOlet.ConnectPointType.MainTerm2 :
              return Math.Max( d, GetMaxOletWidth( le, (int) WeldOlet.ConnectPointType.MainTerm1 ) ) ;
            default :
              return d ;
          }
        }

        case StubInReinforcingWeld sw :
        {
          var d = sw.Diameter / 2 ;
          switch ( v.ConnectPointIndex ) {
            case (int) StubInReinforcingWeld.ConnectPointType.MainTerm1 :
              return Math.Max( d, GetMaxOletWidth( le, (int) StubInReinforcingWeld.ConnectPointType.MainTerm2 ) ) ;
            case (int) StubInReinforcingWeld.ConnectPointType.MainTerm2 :
              return Math.Max( d, GetMaxOletWidth( le, (int) StubInReinforcingWeld.ConnectPointType.MainTerm1 ) ) ;
            default :
              return d ;
          }
        }
        
        default: return 0 ;
      }
    }

    public static double GetMinimumLength( this Pipe pipe )
    {
      if ( null == pipe ) return 0 ;

      if ( null != pipe.LeafEdge ) {
        return GetMinimumLength( pipe.LeafEdge ) ;
      }
      else {
        return pipe.MinimumLengthWithoutOletRadius ;
      }
    }

    public static void SetMinimumLength( this Pipe pipe, double value )
    {
      if ( null == pipe ) return ;

      if ( null != pipe.LeafEdge ) {
        SetMinimumLength( pipe.LeafEdge, value ) ;
      }
      else {
        pipe.MinimumLengthWithoutOletRadius = value ;
      }
    }

    /// <summary>
    /// 最小溶接間距離
    /// </summary>
    /// <returns></returns>
    public static double WeldMinDistance(this Pipe pipe)
    {
      if ( null == pipe ) return 0;

      return pipe.DiameterObj.WeldMinDistance() ;
    }
  }
}
