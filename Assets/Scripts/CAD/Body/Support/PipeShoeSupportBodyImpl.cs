using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using System;

namespace Chiyoda.CAD.Body
{
  public class PipeShoeSupportBodyImpl : MonoBehaviour
  {
    [SerializeField]
    Transform plate = null;

    [SerializeField]
    Transform wallRight = null;

    [SerializeField]
    Transform wallLeft = null;

    public bool IsBadShape { get; private set; }

    /// <summary>
    /// 形状を構築する
    /// </summary>
    /// <param name="R">パイプ外半径</param>
    /// <param name="T1">板厚 (底)</param>
    /// <param name="T2">板厚 (縦)</param>
    /// <param name="A">高さ</param>
    /// <param name="B">底の幅</param>
    /// <param name="C">縦壁内の幅</param>
    /// <param name="L">長さ</param>
    /// <param name="Tpad">パッドの厚さ</param>
    public void SetupShape(
        double R,
        double T1,
        double T2,
        double A,
        double B,
        double C,
        double L,
        double Tpad
      )
    {
      var r2 = R + Tpad;
      var D2 = (r2 * r2 - C * C / 4);
      if ( D2 < 0 ) {
        SetupBadShape();
        return;
      }

      var D = Math.Sqrt( D2 );

      plate.localPosition = new Vector3( 0, 0, -(float)(D + A + T1 / 2) );
      plate.localScale = new Vector3( (float)L, ( float)B, (float)T1 );

      wallRight.localPosition = new Vector3( 0, (float)(C / 2 + T2 / 2), -(float)(D + A / 2) );
      wallRight.localScale = new Vector3( (float)L, (float)T2, (float)A );

      wallLeft.localPosition = new Vector3( 0, -(float)(C / 2 + T2 / 2), -(float)(D + A / 2));
      wallLeft.localScale = new Vector3( (float)L, (float)T2, (float)A );

      IsBadShape = false;
      transform.localScale = Vector3.one;
    }

    public void SetupBadShape()
    {
      IsBadShape = true;
      transform.localScale = Vector3.zero;
    }
  }
}
