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
  internal class TTypeSupportBodyCreator : SupportBodyCreator<TTypeSupportBodyImpl>
  {
    public TTypeSupportBodyCreator( Model.Support support ) : base( support )
    { }

    protected override void SetupGeometry( GameObject gameObject, TTypeSupportBodyImpl bodyImpl, Model.Support entity )
    {
      var leafEdge = entity.SupportTargets.FirstOrDefault();
      var pipe = leafEdge?.PipingPiece as Pipe;
      if ( null == pipe ) {
        // 実質非表示
        bodyImpl.SetupBadShape();
        return;
      }

      var cod = (entity.Parent as BlockPattern)?.GlobalCod;
      var edgeCod = leafEdge.GlobalCod;
      if ( null != cod ) {
        edgeCod = cod.LocalizeCodSys( edgeCod );
      }
      var dirX = edgeCod.DirectionX;
      if ( 0.5 <= Math.Abs( dirX.z ) ) {
        bodyImpl.SetupBadShape();
        return;
      }

      var bodyCodSys = new LocalCodSys3d( Vector3d.zero, dirX, Vector3d.zero, new Vector3d( 0, 0, 1 ) );

      double OVER_RATIO = 1.5;
      double minPos = -pipe.Diameter / 2 * (1 + OVER_RATIO), maxPos = pipe.Diameter / 2 * (1 + OVER_RATIO);
      foreach ( var target in entity.SupportTargets.Skip( 1 ) ) {
        // 複数エッジをサポートする場合、最大値と最小値を求める
        var targetOrigin = target.GlobalCod.Origin;
        if ( null != cod ) {
          targetOrigin = cod.LocalizePoint( targetOrigin );
        }
        var diam = target.PipingPiece.GetConnectPoint( 0 ).Diameter.OutsideMeter / 2 * (1 + OVER_RATIO);
        var posY = Vector3d.Dot( bodyCodSys.DirectionY, (targetOrigin - edgeCod.Origin) );
        if ( posY - diam < minPos ) minPos = posY - diam;
        else if ( maxPos < posY + diam ) maxPos = posY + diam;
      }

      //暫定
      bodyImpl.SetupShape(
        R: pipe.Diameter / 2,
        A: GetSupportDiameter( pipe.Diameter ),
        H: Math.Min( GetMaxH( pipe.Diameter ), edgeCod.Origin.z - pipe.Diameter / 2 ),
        L: Math.Min( GetMaxL( pipe.Diameter ), maxPos - minPos ),
        Slide: (minPos + maxPos) / 2
      );

      // 方向
      bodyImpl.transform.localRotation = bodyCodSys.Rotation;
    }

    private static double GetMaxL( double diameter )
    {
      return Model.Support.GetTTypeSupportMaxShoulderLength( diameter );
    }

    private static double GetMaxH( double diameter )
    {
      if ( diameter.ToInches() <= 6.0 - Tolerance.DistanceTolerance ) {
        return (2000.0).Millimeters();
      }
      else {
        return (2500.0).Millimeters();
      }
    }

    private static readonly double[] LINE_DIAMETERS = new double[] { 2, 3, 4, 6, 8, 10, 12, 14, 16, 18, 20, 24 };
    private static readonly double[] PIPE_DIAMETERS = new double[] { 2, 3, 3, 4, 6, 8, 8, 10, 10, 12, 14, 16 };

    private static double GetSupportDiameter( double diameter )
    {
      int index = Array.BinarySearch( LINE_DIAMETERS, diameter.ToInches() + Tolerance.DistanceTolerance );
      if ( index < 0 ) index = (~index) - 1;
      if ( index < 0 ) return diameter.Inches();  // 1インチより小さい場合は同一半径とする
      if ( PIPE_DIAMETERS.Length <= index ) return PIPE_DIAMETERS[PIPE_DIAMETERS.Length - 1].Inches();  // 大きすぎる場合は全て24インチ
      return PIPE_DIAMETERS[index].Inches();  // 表どおり
    }
  }
}
