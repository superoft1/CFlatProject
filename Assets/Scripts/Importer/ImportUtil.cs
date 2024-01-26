using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

static class ImportUtil
{
  public static List<(string type, List<string> values)> ParseCsv( string csvPath )
  {
    var typeValueList = new List<(string type, List<string> values)>();
    using ( var sr = new StreamReader( csvPath, System.Text.Encoding.UTF8 ) ) {
      while ( !sr.EndOfStream ) {
        var line = sr.ReadLine();
        if ( string.IsNullOrEmpty( line ) ) {
          continue;
        }

        var cell = line.TrimEnd(',').Split( ',' ).Select( t => t.Trim() ).ToList();
        if ( cell.Count < 1 ) {
          continue;
        }

        if (cell[0].StartsWith("#"))
        {
          // #はコメント行扱いとする
          continue;
        }

        typeValueList.Add((cell[0], cell.GetRange(1, cell.Count - 1)));
      }
    }

    return typeValueList;
  }

  public static (IEnumerable<string> idfPath, List<LocalCodSys3d> codSys) ParseIdfDir(string parentPath, List<string> idfLine)
  {
    var codSysList = new List<LocalCodSys3d>{new LocalCodSys3d()};
    var idfDir = Path.Combine(parentPath, idfLine[0]);
    var idfs = Directory.EnumerateFiles(idfDir, "*.id*", SearchOption.AllDirectories);
    var idfCod = new List<string>();
    idfCod.AddRange(idfLine.GetRange(1, idfLine.Count - 1) );
    // 手動で一発で位置合わせするのが大変なため
    for (var i = 0; i < idfCod.Count; i+=4) {
      var x = i ;
      var y = i + 1;
      var z = i + 2;
      
      var trans = z < idfCod.Count
                    ? new Vector3d( -double.Parse( idfCod[x] ) / 1000d, double.Parse( idfCod[y] ) / 1000d, double.Parse( idfCod[z] ) / 1000d )
                    : Vector3d.zero;
      
      var r = i + 3;
      var rot = r < idfCod.Count
                  ? ParseRot(idfCod[r])
                  : Quaternion.identity;
      
      codSysList.Add(new LocalCodSys3d(trans, rot, false));
    }

    return (idfs, codSysList);
  }

  public static Quaternion ParseRot(string str)
  {
    var val = str.Replace( "R(", "" ).Replace( ")", "" ).Split(' ').Where(s=>!string.IsNullOrEmpty( s )).Select(float.Parse).ToList();
    return Quaternion.Euler(val[0], val[1], val[2]);
  }
}

