using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Body.Support;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  internal class TrunnionSupportBodyCreator : SupportBodyCreator<TrunnionSupportBodyImpl>
  {
    private struct PipeShapeData
    {
      public double Diameter { get; set; }
      public double ZOver { get; set; }
      public double ZUnder { get; set; }
    }

    public TrunnionSupportBodyCreator( Model.Support support ) : base( support )
    { }

    protected override void SetupGeometry( GameObject gameObject, TrunnionSupportBodyImpl bodyImpl, Model.Support entity )
    {
      var leafEdge = entity.SupportTargets.FirstOrDefault();
      var data = GetPipeShape( leafEdge, (entity.Parent as BlockPattern)?.GlobalCod );
      if ( null == data ) {
        // 実質非表示
        bodyImpl.SetupBadShape();
        return;
      }

      // 暫定値
      bodyImpl.SetupShape(
        A: GetTrunnionDiameter( data.Value.Diameter ),
        Zover: data.Value.ZOver,
        Zunder: data.Value.ZUnder
      );
    }

    private static PipeShapeData? GetPipeShape( LeafEdge leafEdge, LocalCodSys3d groupCodSys )
    {
      switch ( leafEdge?.PipingPiece ) {
        case Pipe pipe: return GetPipeShape( pipe, GetEdgeCodSys( leafEdge, groupCodSys ) );
        case PipingElbow90 elbow: return GetPipeShape( elbow, GetEdgeCodSys( leafEdge, groupCodSys ) );
        default: return null;
      }
    }

    private static PipeShapeData? GetPipeShape( Pipe pipe, LocalCodSys3d placementCodSys )
    {
      var dirX = placementCodSys.GlobalizeVector( new Vector3d( 1, 0, 0 ) );
      if ( 0.5 <= Math.Abs( dirX.z ) ) {
        return null;
      }

      return new PipeShapeData
      {
        Diameter = pipe.Diameter,
        ZOver = 0,
        ZUnder = placementCodSys.Origin.z,
      };
    }

    private static PipeShapeData? GetPipeShape( PipingElbow90 elbow, LocalCodSys3d placementCodSys )
    {
      foreach ( var cp in elbow.ConnectPoints ) {
        // 垂直上方に伸びる方向があるか？
        var dirZ = placementCodSys.GlobalizeVector( cp.Point );
        if ( 0.99 < dirZ.normalized.z ) {
          return new PipeShapeData
          {
            Diameter = cp.Diameter.OutsideMeter,
            ZOver = dirZ.z,
            ZUnder = placementCodSys.Origin.z,
          };
        }
      }

      return null;
    }

    private static LocalCodSys3d GetEdgeCodSys( LeafEdge leafEdge, LocalCodSys3d placementCodSys )
    {
      if ( null == placementCodSys ) {
        return leafEdge.GlobalCod;
      }
      else {
        return placementCodSys.LocalizeCodSys( leafEdge.GlobalCod );
      }
    }

    private static readonly double[] LINE_DIAMETERS = new[] { 1, 1.5,   2, 3, 4, 6, 8, 10, 12, 14, 16, 18, 20, 24, 26, 28, 30, 32, 34 };
    private static readonly double[] PIPE_DIAMETERS = new[] { 1, 1.5, 1.5, 2, 3, 4, 6,  6,  8, 10, 10, 12, 14, 16, 18, 18, 20, 20, 24 };

    private static double GetTrunnionDiameter( double diameter )
    {
      int index = Array.BinarySearch( LINE_DIAMETERS, diameter.ToInches() + Tolerance.DistanceTolerance );
      if ( index < 0 ) index = (~index) - 1;
      if ( index < 0 ) return diameter.Inches();  // 1インチより小さい場合は同一半径とする
      if ( PIPE_DIAMETERS.Length <= index ) return PIPE_DIAMETERS[PIPE_DIAMETERS.Length - 1].Inches();  // 大きすぎる場合は全て24インチ
      return PIPE_DIAMETERS[index].Inches();  // 表どおり
    }
  }
}
