using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using System;

namespace Chiyoda.CAD.Body
{

  public class TTypeSupportBodyImpl : MonoBehaviour
  {
    [SerializeField]
    Transform stantion = null;

    [SerializeField]
    Transform shoulder = null;

    public bool IsBadShape { get; private set; }

    /// <summary>
    /// 形状を構築する
    /// </summary>
    /// <param name="R">パイプ外半径</param>
    /// <param name="A">サポート外直径</param>
    /// <param name="H">サポート高さ</param>
    /// <param name="L">サポート幅</param>
    /// <param name="Slide">サポートY座標のずれ幅</param>
    public void SetupShape(
        double R,
        double A,
        double H,
        double L,
        double Slide = 0
      )
    {
      double z = (H + A / 2) / -2 - R;
      double length = H - A / 2;

      stantion.localPosition = new Vector3( 0, (float)Slide, (float)z );
      stantion.localScale = new Vector3( (float)A, (float)(length / 2), (float)A );

      shoulder.localPosition = new Vector3( 0, (float)Slide, -(float)(R + A / 2) );
      shoulder.localScale = new Vector3( (float)A, (float)(L / 2), (float)A );

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
