using System;
using UnityEngine;

public static class GameObjectUtil
{
  /// <summary>
  /// 底面中心を原点　錐の軸をY軸とする円錐台メッシュを作る
  /// </summary>
  /// <returns>The taper body.</returns>
  /// <param name="bottomRadius">底面半径</param>
  /// <param name="topRadius">上面半径</param>
  /// <param name="length">長さ(高さ)</param>
  /// <param name="isClosedBottom">底面を閉じる円盤も作るかどうか</param>
  /// <param name="isClosedTop">上面を閉じる円盤も作るかどうか</param>
  public static GameObject CreateTaperBody(float bottomRadius, float topRadius, float length, bool isClosedBottom = false, bool isClosedTop = false)
  {
    const int vertNum = 20;

    var taper = new GameObject("Taper");
    var mesh = new Mesh();

    int numOfVertices = 2 * vertNum; // 側面用　底の円周上の点(vertNum) + 上の円周上の点(vertNum)
    if (isClosedBottom) numOfVertices += vertNum + 1; // 底面のディスク 円周上の点(vertNum)＋中心　法線が異なるので側面用の頂点を共有できない
    if (isClosedTop) numOfVertices += vertNum + 1; // 上面のディスク 円周上の点(vertNum)＋中心　法線が異なるので側面用の頂点を共有できない

    var vertices = new Vector3[numOfVertices];
    var normals = new Vector3[numOfVertices];
    var uvs = new Vector2[numOfVertices];

    int numOfTrisIndices = 6 * vertNum;
    if (isClosedBottom) numOfTrisIndices += 3 * vertNum;
    if (isClosedTop) numOfTrisIndices += 3 * vertNum;
    var tris = new int[numOfTrisIndices];

    var slopeAngle = Mathf.Atan((bottomRadius - topRadius) / length);
    var cosSlope = Mathf.Cos(slopeAngle);
    var sinSlope = Mathf.Sin(slopeAngle);

    var dTheta = 2.0f * Mathf.PI / vertNum;

    // 側面
    for (int i = 0; i < vertNum; ++i)
    {
      var cosTheta = Mathf.Cos(dTheta * i);
      var sinTheta = Mathf.Sin(dTheta * i);

      vertices[i] = new Vector3(bottomRadius * cosTheta, 0.0f, bottomRadius * sinTheta);
      vertices[i + vertNum] = new Vector3(topRadius * cosTheta, length, topRadius * sinTheta);
      normals[i] = new Vector3(cosSlope * cosTheta, sinSlope, cosSlope * sinTheta);
      normals[i + vertNum] = normals[i];
      uvs[i] = new Vector2(i / vertNum, 0.0f);
      uvs[i + vertNum] = new Vector2(i / vertNum, 1.0f);

      var nextIndex = (i == vertNum - 1) ? 0 : i + 1;
      tris[6 * i] = i;
      tris[6 * i + 1] = i + vertNum;
      tris[6 * i + 2] = nextIndex;
      tris[6 * i + 3] = nextIndex + vertNum;
      tris[6 * i + 4] = nextIndex;
      tris[6 * i + 5] = i + vertNum;
    }

    // 底面
    if (isClosedBottom)
    {
      int indexOffset = 2 * vertNum;
      int trisOffset = 6 * vertNum;
      // 底面中心
      int indexOfCenter = indexOffset + vertNum;
      vertices[indexOfCenter] = Vector3.zero;
      normals[indexOfCenter] = Vector3.down;
      uvs[indexOfCenter] = Vector2.zero;
      for (int i = 0; i < vertNum; ++i)
      {
        var cosTheta = Mathf.Cos(dTheta * i);
        var sinTheta = Mathf.Sin(dTheta * i);

        vertices[indexOffset + i] = new Vector3(bottomRadius * cosTheta, 0.0f, bottomRadius * sinTheta);
        normals[indexOffset + i] = Vector3.down;
        uvs[indexOffset + i] = new Vector2(i / vertNum, 0.0f);

        var nextIndex = (i == vertNum - 1) ? 0 : i + 1;
        tris[trisOffset + 3 * i] = indexOfCenter;
        tris[trisOffset + 3 * i + 1] = indexOffset + i;
        tris[trisOffset + 3 * i + 2] = indexOffset + nextIndex;
      }
    }

    if (isClosedTop)
    {
      int indexOffset = (isClosedBottom) ? 3 * vertNum + 1 : 2 * vertNum;
      int trisOffset = (isClosedBottom) ? 9 * vertNum : 6 * vertNum;
      // 底面中心
      int indexOfCenter = indexOffset + vertNum;
      vertices[indexOfCenter] = new Vector3(0.0f, 0.0f, length);
      normals[indexOfCenter] = Vector3.up;
      uvs[indexOfCenter] = Vector2.one;
      for (int i = 0; i < vertNum; ++i)
      {
        var cosTheta = Mathf.Cos(dTheta * i);
        var sinTheta = Mathf.Sin(dTheta * i);

        vertices[indexOffset + i] = new Vector3(topRadius * cosTheta, length, topRadius * sinTheta);
        normals[indexOffset + i] = Vector3.down;
        uvs[indexOffset + i] = new Vector2(i / vertNum, 1.0f);

        var nextIndex = (i == vertNum - 1) ? 0 : i + 1;
        tris[trisOffset + 3 * i] = indexOfCenter;
        tris[trisOffset + 3 * i + 1] = indexOffset + nextIndex;
        tris[trisOffset + 3 * i + 2] = indexOffset + i;
      }
    }

    mesh.vertices = vertices;
    mesh.normals = normals;
    mesh.uv = uvs;
    mesh.triangles = tris;

    var filter = taper.AddComponent<MeshFilter>();
    filter.mesh = mesh;

    var collider = taper.AddComponent<MeshCollider>();
    collider.sharedMesh = filter.sharedMesh;

    taper.AddComponent<MeshRenderer>();

    return taper;
  }
}
