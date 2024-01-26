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
  internal class PipeShoeSupportBodyCreator : SupportBodyCreator<PipeShoeSupportBodyImpl>
  {
    public PipeShoeSupportBodyCreator( Model.Support support ) : base( support )
    { }

    protected override bool RecreateOnUpdate => false;

    protected override void SetupGeometry( GameObject gameObject, PipeShoeSupportBodyImpl bodyImpl, Model.Support entity )
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
      var dirX = edgeCod.GlobalizeVector( new Vector3d( 1, 0, 0 ) );
      if ( 0.5 <= Math.Abs( dirX.z ) ) {
        bodyImpl.SetupBadShape();
        return;
      }

      // 暫定値
      bodyImpl.SetupShape(
        R: pipe.Diameter / 2,
        T1: (5.0).Millimeters(),
        T2: (5.0).Millimeters(),
        A: (pipe.Diameter <= (2.0).Inches()) ? (100.0).Millimeters() : (150.0).Millimeters(),
        B: pipe.Diameter * 1.1,
        C: pipe.Diameter / 2,
        L: (pipe.Diameter <= (24.0).Inches()) ? (300.0).Millimeters() : (450.0).Millimeters(),
        Tpad: 0
       );

      // 方向
      bodyImpl.transform.localRotation = new LocalCodSys3d( Vector3d.zero, dirX, Vector3d.zero, new Vector3d( 0, 0, 1 ) ).Rotation;
    }
  }
}
