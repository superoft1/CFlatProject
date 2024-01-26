using System;
using System.Data;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Importer.Equipment
{
  internal class FilterTableImporter : PipingPieceTableImporter
  {
    public FilterTableImporter(DataSet dataSet) : base(dataSet, "Filter", (3, 4, 5), 2)
    {
    }

    public DataRow FindPattern(string name)
    {
      return Table.Rows.Find(name);
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate(Document doc, string name, bool createNozzle)
    {
      var row = Table.Rows.Find(name);
      var cells = TableReader.Row2Array(row);
      var filter = doc.CreateEntity<Filter>();

      filter.EquipNo = cells[0];
      filter.HeightOfCylinder = double.Parse(cells[6]) / 1000.0;
      filter.DiameterOfFilter = double.Parse(cells[7]) / 1000.0;
      filter.DiameterOfFlange = double.Parse(cells[8]) / 1000.0;
      filter.HeightOfFlange = double.Parse(cells[9]) / 1000.0;
      filter.HeightOfLeg = double.Parse(cells[10]) / 1000.0;
      filter.LegThickness = double.Parse(cells[11]) / 1000.0;
      filter.LengthOfCap = double.Parse(cells[12]) / 1000.0;

      if (createNozzle)
      {
        for (var i = 13; i < cells.Length; i += 8)
        {
          var kind = GetNozzleKind(cells[i]);
          var diameterMm = double.Parse(cells[i + 3]);
          var length = double.Parse(cells[i + 4]) / 1000d;
          var param1 = double.Parse(cells[i + 5]);
          var param2 = double.Parse(cells[i + 6]);
          var placement = GetPlacement(cells[i + 7]);
          var nozzle = filter.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm), placement, param1, param2);
          nozzle.NozzleType = GetNozzleType(cells[i + 2]);
          nozzle.Name = cells[i + 1];
        }
      }

      return (filter, ParseOrigin(cells), ParseAngleAxis(cells));
    }

    private static Filter.NozzleKind GetNozzleKind(string kind)
    {
      foreach (Filter.NozzleKind val in Enum.GetValues(typeof(Filter.NozzleKind)))
      {
        if (val.ToString() == kind)
        {
          return val;
        }
      }
      throw new InvalidOperationException("Nozzle Kind not found.");
    }

    private static Filter.Placement GetPlacement(string placement)
    {
      foreach (Filter.Placement val in Enum.GetValues(typeof(Filter.Placement)))
      {
        if (val.ToString() == placement)
        {
          return val;
        }
      }
      throw new InvalidOperationException("Placement not found.");
    }
  }
}