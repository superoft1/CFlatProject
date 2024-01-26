using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
  /// <summary>
  /// 2DBox構造体
  /// </summary>
  public class Box2D
  {
    /// <summary>
    /// 頂点の最大値を取得します。
    /// </summary>
    public Vector2 Max { get; private set; }

    /// <summary>
    /// 頂点の最小値を取得します。
    /// </summary>
    public Vector2 Min { get; private set; }

    /// <summary>
    /// 中心値を取得します
    /// </summary>
    public Vector2 Center
    {
      get
      {
        return 0.5f * (Min + Max);
      }
    }

    /// <summary>
    /// 大きさを取得します
    /// </summary>
    public Vector2 Size
    {
      get
      {
        return (Max - Min);
      }
    }

    public Box2D(Vector2 center, Vector2 size)
    {
      Max = new Vector2(center.x + 0.5f * size.x, center.y + 0.5f * size.y);
      Min = new Vector2(center.x - 0.5f * size.x, center.y - 0.5f * size.y);
    }

    public void Add(Box2D box)
    {
      Max = new Vector2(Mathf.Max(Max.x, box.Max.x), Mathf.Max(Max.y, box.Max.y));
      Min = new Vector2(Mathf.Min(Min.x, box.Min.x), Mathf.Min(Min.y, box.Min.y));
    }

    public bool IsIntersect(Box2D box)
    {
      if (Max.x < box.Min.x)
      {
        return false;
      }
      if (box.Max.x < Min.x)
      {
        return false;
      }
      if (Max.y < box.Min.y)
      {
        return false;
      }
      if (box.Max.y < Min.y)
      {
        return false;
      }

      return true;
    }

    public bool IsInclude(Vector2 point)
    {
      if (Max.x < point.x)
      {
        return false;
      }
      if (point.x < Min.x)
      {
        return false;
      }
      if (Max.y < point.y)
      {
        return false;
      }
      if (point.y < Min.y)
      {
        return false;
      }

      return true;
    }
  }
}
