using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class TrunnionSupportBodyImpl : MonoBehaviour
  {
    [SerializeField]
    Transform pole = null;

    public bool IsBadShape { get; private set; }

    /// <summary>
    /// 形状を構築する
    /// </summary>
    /// <param name="A">トラニオン外直径</param>
    /// <param name="Zover">基準点から上側に伸びる量</param>
    /// <param name="Zunder">基準点から下側に伸びる量</param>
    public void SetupShape(
        double A,
        double Zover,
        double Zunder
      )
    {
      double z = (Zover - Zunder) / 2;
      double length = (Zover + Zunder);

      pole.localPosition = new Vector3( 0, 0, (float)z );
      pole.localScale = new Vector3( (float)A, (float)(length / 2), (float)A );

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
