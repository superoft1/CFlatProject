using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.UI
{
  public static class RayCastUtil
  {
    /// <summary>
    /// レイとの当たり判定を行うBoundsの面
    /// </summary>
    public enum Surface
    {
      Top,
      Bottom,
      Front,
      Back,
      Right,
      Left,
    }

    /// <summary>
    /// レイと領域との衝突情報
    /// </summary>
    public class RayCastInfo
    {
      // 衝突した面
      public Surface hitSurface;

      // 衝突までの距離
      public float hitLength;

      // 衝突位置
      public Vector3 hitPosition;
    }

    /// <summary>
    /// マウス座標からレイを飛ばしてBoundsとの衝突情報を返す
    /// </summary>
    public static RayCastInfo RayCastFromMousePosition(Camera camera, Bounds bounds)
    {
      var boundsMin = bounds.min;
      var boundsMax = bounds.max;

      CreateRayFromMousePosition(camera, out Vector3 rayPos, out Vector3 rayDir);

      return RayCastBounds(rayPos, rayDir, bounds);
    }

    /// <summary>
    /// マウス座標から飛ばすレイの座標と向きを求める（正射影カメラ前提）
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="rayPos"></param>
    /// <param name="rayDir"></param>
    public static void CreateRayFromMousePosition(Camera camera, out Vector3 rayPos, out Vector3 rayDir)
    {
      // ※Unityの関数を使うなら以下でOK
      // var ray = camera.ScreenPointToRay(Input.mousePosition);
      // rayPos = ray.origin;
      // rayDir = ray.direction;

      // 画面中央からマウス座標までの値
      var dx = Input.mousePosition.x - Screen.width / 2f;
      var dy = Input.mousePosition.y - Screen.height / 2f;
        
      // 画面中央からマウス座標までのワールド座標系での距離
      var len = Mathf.Sqrt(dx * dx + dy * dy);
      len *= camera.orthographicSize * 2 / Screen.height;

      // 画面中央からマウス座標までのラジアン角
      var rad = Mathf.PI / 2f - Mathf.Atan2(dy, dx);

      // カメラの上ベクトルをカメラの向きを軸として回転させる
      var cameraTransform = camera.transform;
      var up = cameraTransform.up;
      var forward = cameraTransform.forward;
      var vec = Mathf.Sin(rad) * Vector3.Cross(up, forward)
                + Mathf.Cos(rad) * (up - Vector3.Dot(up, forward) * forward)
                + Vector3.Dot(up, forward) * forward;

      // レイを飛ばす位置を算出
      vec.Normalize();
      rayPos = cameraTransform.position + vec * len;
        
      // レイの向きはカメラの向きと一致する
      rayDir = forward;
    }

    /// <summary>
    /// レイとBoundsの衝突判定を行う
    /// </summary>
    /// <param name="rayPos"></param>
    /// <param name="rayDir"></param>
    /// <param name="bounds"></param>
    /// <param name="checkSurface"></param>
    /// <returns>衝突情報を返す。衝突してなければnullを返す</returns>
    private static RayCastInfo RayCastBounds(Vector3 rayPos, Vector3 rayDir, Bounds bounds, int checkSurface = 0x7fffffff)
    {
      var boundsMin = bounds.min;
      var boundsMax = bounds.max;
      RayCastInfo nearHitInfo = null;

      foreach (Surface surface in System.Enum.GetValues(typeof(Surface)))
      {
        if ((checkSurface & (1 << (int) surface)) == 0) continue;

        // rayがboundsの指定面に到達するまでの距離を求める
        // ex) 底面であれば(rayPos.z + rayDir.z * len = boundsMin.z)のlenを求める
        float len = -1f;
        switch (surface)
        {
          case Surface.Top:
            len = (boundsMax.z - rayPos.z) / rayDir.z;
            break;
          case Surface.Bottom:
            len = (boundsMin.z - rayPos.z) / rayDir.z;
            break;
          case Surface.Front:
            len = (boundsMax.y - rayPos.y) / rayDir.y;
            break;
          case Surface.Back:
            len = (boundsMin.y - rayPos.y) / rayDir.y;
            break;
          case Surface.Right:
            len = (boundsMax.x - rayPos.x) / rayDir.x;
            break;
          case Surface.Left:
            len = (boundsMin.x - rayPos.x) / rayDir.x;
            break;
        }

        // lenが負ならレイが進む方向に面はない。
        // またはすでに衝突済みの面があってそちらの方が距離が近ければこの面はもう調べない
        if (len < 0f || (nearHitInfo != null && nearHitInfo.hitLength < len)) continue;
        
        // posからdir方向にlenの長さだけ進んだ点
        var hitPos = rayPos + rayDir * len;
        
        // hitPosがboundsの中にあるかチェック
        switch (surface)
        {
          case Surface.Top:
          case Surface.Bottom:
            if (boundsMin.x > hitPos.x || hitPos.x > boundsMax.x ||
                boundsMin.y > hitPos.y || hitPos.y > boundsMax.y)
              continue;
            break;
          case Surface.Front:
          case Surface.Back:
            if (boundsMin.x > hitPos.x || hitPos.x > boundsMax.x ||
                boundsMin.z > hitPos.z || hitPos.z > boundsMax.z)
              continue;
            break;
          case Surface.Right:
          case Surface.Left:
            if (boundsMin.y > hitPos.y || hitPos.y > boundsMax.y ||
                boundsMin.z > hitPos.z || hitPos.z > boundsMax.z)
              continue;
            break;
        }

        nearHitInfo = new RayCastInfo()
        {
          hitSurface = surface,
          hitLength = len,
          hitPosition = hitPos
        };
      }

      return nearHitInfo;
    }
  }
}
