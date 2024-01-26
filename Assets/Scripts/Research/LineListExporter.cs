using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LineListExporter {

  public static void ExportResult(List<LineFinder.LineSet> lineSets)
  {
    var path = Application.dataPath + "/Outputs/LineList.csv";
    StreamWriter sw;
    FileInfo fi;
    fi = new FileInfo(path);
    sw = fi.CreateText();

    string s = "";
    foreach (var line in lineSets)
    {
      s += line.lineId;
      s += ",";
      s += (int)(line.from.x * 1000);
      s += ",";
      s += (int)(line.from.y * 1000);
      s += ",";
      s += (int)(line.from.z * 1000);
      s += ",";
      s += (int)(line.to.x * 1000);
      s += ",";
      s += (int)(line.to.y * 1000);
      s += ",";
      s += (int)(line.to.z * 1000);
      s += ",";
      s += System.Environment.NewLine;
    }
    sw.Write(s);
    sw.Flush();
    sw.Close();
  }
}
