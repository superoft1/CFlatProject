using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineNumberImporter {

  public static List<string> ImportData(string path)
  {
    var lines = new List<string>();
    using (var sr = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
    {
      while (!sr.EndOfStream)
      {
        var line = sr.ReadLine();
        var cell = line.Split(' ');

        var first = cell[0];
        lines.Add(first);
      }
    }
    return lines;
  }
}
