using System;
using System.Data;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Importer.Equipment
{
  internal class PressureReliefValveTableImporter : PipingPieceTableImporter
  {
    public PressureReliefValveTableImporter(DataSet dataSet) : base(dataSet, "PressureReliefValve", (3, 4, 5), 2)
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate(Document doc, string name, bool createNozzle)
    {
      var row = Table.Rows.Find(name);
      var cells = TableReader.Row2Array(row);
      var valve = doc.CreateEntity<PressureReliefValve>();

      valve.InletLength = double.Parse( cells[ 6 ] ) / 1000.0 ;
      valve.InletDiameter = double.Parse( cells[ 7 ] ) / 1000.0 ;
      valve.OutletLength = double.Parse(cells[ 8 ] ) / 1000.0 ;
      valve.OutletDiameter = double.Parse(cells[9] ) / 1000.0 ;
      valve.BonnetLength = double.Parse(cells[ 10 ] ) / 1000.0 ;
      valve.BonnetDiameter = double.Parse(cells[ 11 ] ) / 1000.0 ;
      valve.CapLength = double.Parse(cells[ 12 ] ) / 1000.0 ;
      valve.CapDiameter = double.Parse(cells[ 13 ] ) / 1000.0 ;

      return (valve, ParseOrigin(cells), ParseAngleAxis(cells));
    }
  }
}