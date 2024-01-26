using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using UnityEngine;

public class CSVEndTopTypePumpImporter : EquipmentImporter
{
  public override List<(Equipment, LocalCodSys3d)> ImportData(Chiyoda.CAD.Core.Document doc, string path, LocalCodSys3d parentCod, bool createNozzle)
  {
    var pumpList = new List<(Equipment, LocalCodSys3d)>();
    //using (var sr = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
    //{
    //  while (!sr.EndOfStream)
    //  {
    //    var pump = doc.CreateEntity<EndTopTypePump>();

    //    var line = sr.ReadLine();
    //    if (SholdIgnore(line)) continue;
    //    var cells = SplitLine(line);

    //    pump.EquipNo = cells[0];
    //    var list = new List<double>();
    //    for (int i = 3; i < 20; ++i) {
    //      list.Add(double.Parse(cells[i]) / 1000d);
    //    }
    //    pump.PList = list;

    //    pump.P5 = -list[4];
    //    pump.P6 = list[5];
    //    pump.P11 = list[10];
    //    pump.FoundationSize = new Vector3d(list[1] + list[2], list[0], list[3]);
    //    pump.RotatingEquipSize = new Vector3d(list[7] + list[8], list[5] + list[6], list[9] + list[10]);
    //    pump.DriverSize = new Vector3d(list[12], list[11], list[12]);
    //    pump.MotorSize = new Vector3d(list[14] + list[15], list[13], list[16]);

    //    var location = parentCod.LocalizePoint(Origin(cells));

    //    var rot = Quaternion.AngleAxis(float.Parse(cells[24]), Vector3.forward);
    //    var localRot = rot * Quaternion.Inverse(parentCod.Rotation);
    //    var rotation = localRot;

    //    var localCod = new LocalCodSys3d(location, rotation);
    //    pumpList.Add( (pump, localCod) );
    //  }
    //}
    return pumpList;
  }


  protected override Vector3d Origin( string[] cells )
  {
    return new Vector3d( -double.Parse( cells[21] ), double.Parse( cells[22] ), double.Parse( cells[23] ) ) / 1000d;
  }

  protected override Quaternion AngleAxis( string[] cells )
  {
    float.TryParse( cells[24], out var rotAngle );
    return Quaternion.AngleAxis( rotAngle, Vector3.forward );
  }
}
