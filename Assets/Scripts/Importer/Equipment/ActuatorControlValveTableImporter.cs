using System;
using System.Data;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Importer.Equipment
{
    public class ActuatorControlValveTableImporter : PipingPieceTableImporter
    {
        public ActuatorControlValveTableImporter(DataSet dataSet) : base(dataSet, "ActuatorControlValve", (3, 4, 5), 2)
        {
        }

        public DataRow FindPattern(string name)
        {
            return Table.Rows.Find(name);
        }

        public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate(Document doc, string name, bool createNozzle)
        {
            Debug.Log("--Generate name: " + name);
            var row = FindPattern(name);
            var cells = TableReader.Row2Array(row);
            var valve = doc.CreateEntity<ActuatorControlValve>();

            valve.Length = double.Parse(cells[6]) / 1000.0;
            valve.Diameter = double.Parse(cells[7]) / 1000.0;
            valve.DiaphramLength = double.Parse(cells[8]) / 1000.0;
            valve.DiaphramDiameter = double.Parse(cells[9]) / 1000.0;

            return (valve, ParseOrigin(cells), ParseAngleAxis(cells));
        }
    }
}