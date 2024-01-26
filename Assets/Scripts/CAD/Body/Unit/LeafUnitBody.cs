using System.Linq;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class LeafUnitBody : Body
  {
    private readonly Color _boundColor = new Color(248f / 255f, 152f / 255f, 0f / 255f);
    private BoxCollider _boxCollider;

    private void Start()
    {
      _boxCollider = GetComponentInChildren<BoxCollider>();
    }

    private void OnRenderObject()
    {
      if (!IsDrawBound) return;
      if (_boxCollider == null) return;

      var bounds = _boxCollider.bounds;
      var org = bounds.min;
      var size = bounds.size;

      Vector3[] lower =
      {
        org,
        org += new Vector3(size.x, 0.0f, 0.0f),
        org += new Vector3(0.0f, size.y, 0.0f),
        org += new Vector3(-size.x, 0.0f, 0.0f)
      };
      var upper = lower.Select(pos => pos + new Vector3(0.0f, 0.0f, size.z)).ToArray();

      GL.Begin(GL.LINE_STRIP);
      GL.Color(_boundColor);
      for (var i = 0; i < 5; ++i)
      {
        GL.Vertex(lower[i % 4]);
      }

      GL.End();

      GL.Begin(GL.LINE_STRIP);
      GL.Color(_boundColor);
      for (var i = 0; i < 5; ++i)
      {
        GL.Vertex(upper[i % 4]);
      }

      GL.End();

      GL.Begin(GL.LINES);
      GL.Color(_boundColor);
      for (var i = 0; i < 4; ++i)
      {
        GL.Vertex(lower[i]);
        GL.Vertex(upper[i]);
      }

      GL.End();
    }
  }
}