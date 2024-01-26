using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Chiyoda.CAD.Core;

public class WrlDeserealizer : MonoBehaviour
{
  public void ImportData(Document doc, string path)
  {
    using (var sr = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
    {
      while (!sr.EndOfStream)
      {
        var line = sr.ReadLine();
        var cell = line.Split(' ');

        var first = cell[0];
        if (first != "BEAM")
        {
          continue;
        }

        Get1Beam(cell);
      }
    }
  }

  void Get1Beam(string[] cell)
  {
    var start = GetVector3(cell, 2);
    var dir0 = GetVector3(cell, 6);
    var angle0 = GetAngle(cell, 9);
    var dir1 = GetVector3(cell, 11);
    var angle1 = GetAngle(cell, 14);
    var dir2 = GetVector3(cell, 16);
    var angle2 = GetAngle(cell, 19);
    var size = GetVector3(cell, 21);

    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.transform.localScale = size;
    cube.transform.localScale = new Vector3(0.2f, size.y, 0.2f);// 太さが入ってないので、こっちを使った方がそれっぽくなる

    cube.transform.rotation = GetQuaternion(dir0, angle0, dir1, angle1, dir2, angle2);
    var offset = GetQuaternion(dir0, angle0, dir1, angle1, dir2, angle2) * (size.magnitude * Vector3.up);
    cube.transform.position = start + 0.5f * offset;
  }

  Vector3 GetVector3(string[] cell, int id)
  {
    return new Vector3(-float.Parse(cell[id]), float.Parse(cell[id + 1]), float.Parse(cell[id + 2]));
  }

  float GetAngle(string[] cell, int id)
  {
    return 180f * float.Parse(cell[id]) / Mathf.PI;
  }

  Vector3 GetPositiveVectoe(Vector3 vec)
  {
    return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
  }

  Quaternion GetQuaternion(Vector3 dir0, float angle0, Vector3 dir1, float angle1, Vector3 dir2, float angle2)
  {
    return Quaternion.Euler(dir0 * angle0) * Quaternion.Euler(dir1 * angle1) * Quaternion.Euler(dir2 * angle2);
  }
}