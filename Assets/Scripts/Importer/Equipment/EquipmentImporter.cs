using System;
using System.Linq;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

public abstract class EquipmentImporter
{
  public abstract List<(Equipment equipment, LocalCodSys3d codSys3)> ImportData( Document doc, string path,
    LocalCodSys3d parentCod, bool createNozzle );

  public LocalCodSys3d GetLocalCodSys( string path )
  {
    using ( var sr = new System.IO.StreamReader( path, System.Text.Encoding.UTF8 ) ) {
      while ( !sr.EndOfStream ) {
        var line = sr.ReadLine();
        if (SholdIgnore(line)) continue;
        var cells = SplitLine(line);

        var origin = Origin( cells );
        var rotation = AngleAxis( cells );
        return new LocalCodSys3d( origin, rotation, false ); ;
      }
    }
    return LocalCodSys3d.Identity;
  }

  protected virtual Vector3d Origin( string[] cells )
  {
    return new Vector3d( -double.Parse( cells[3] ), double.Parse( cells[4] ), double.Parse( cells[5] ) ) / 1000d;
  }

  protected virtual Quaternion AngleAxis( string[] cells )
  {
    float.TryParse( cells[2], out var rotAngle );
    return Quaternion.AngleAxis( rotAngle, Vector3.forward );
  }

  protected Nozzle.Type GetNozzleType(string type)
  {
    foreach (Nozzle.Type val in Enum.GetValues(typeof(Nozzle.Type)))
    {
      if (val.ToString() == type)
      {
        return val;
      }
    }

    throw new InvalidOperationException("Nozzle Type not found.");
  }

  // 無視する行か？の判定
  protected virtual bool SholdIgnore(string line)
  {
    return line.StartsWith("#") || string.IsNullOrEmpty(line);
  }
  protected static string[] SplitLine(string line)
  {
    return line.Split(',').Select(item => item.Trim()).ToArray();
  }
}
