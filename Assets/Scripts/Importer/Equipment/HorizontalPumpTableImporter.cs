using System ;
using System.Collections.Generic ;
using System.Data ;
using System.IO ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class HorizontalPumpTableImporter : PipingPieceTableImporter
  {
    public HorizontalPumpTableImporter( DataSet dataSet) : base( dataSet, "HorizontalPump", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle  )
    {
      var row = Table.Rows.Find(name);
      var cells = TableReader.Row2Array(row);
      HorizontalPump pump;
      if (cells[1] == "HOR.Pump_Top-Top")
      {
        pump = doc.CreateEntity<TopTopTypePump>();
      }
      else if (cells[1] == "HOR.Pump_End-Top")
      {
        pump = doc.CreateEntity<EndTopTypePump>();
      }
      else if (cells[1] == "HOR.Pump_Side-Side")
      {
        pump = doc.CreateEntity<SideSideTypePump>();
      }
      else
      {
        throw new InvalidDataException();
      }

      pump.EquipNo = cells[0];
      var list = new List<double>();
      for (int i = 6; i < 18; ++i)
      {
        list.Add(double.Parse(cells[i]) / 1000d);
      }
      pump.BasePlateWidth = list[0];
      pump.BasePlateLength = list[1];
      pump.BasePlateHeight = list[2];
      pump.ImpellerWidth = list[3];
      pump.ImpellerLength = list[4];
      pump.ImpellerHeight = list[5];
      pump.DriverWidth = list[6];
      pump.DriverLength = list[7];
      pump.MotorWidth = list[8];
      pump.MotorLength = list[9];
      pump.MotorHeight = list[10];
      pump.DriverAxisHeight = list[2] + 0.5 * list[5];
      pump.ImpellerOffset = list[11];

      if (createNozzle)
      {
        for (var i = 18; i < cells.Length; i += 8)
        {
          var kind = GetNozzleKind(cells[i]);
          var length = double.Parse(cells[i + 3]) / 1000d;
          var diameterMm = double.Parse(cells[i + 4]);
          var xFromReferencePoint = double.Parse(cells[i + 5]) / 1000d;
          var yFromReferencePoint = double.Parse(cells[i + 6]) / 1000d;
          var plane = GetPlacementPlane(cells[i + 7]);
          var nozzle = pump.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm), plane, xFromReferencePoint, yFromReferencePoint);
          nozzle.Name = cells[i + 1];
          nozzle.NozzleType = GetNozzleType(cells[i + 2]);
        }
      }
        
      return (pump, ParseOrigin(cells), ParseAngleAxis(cells));
    }


    private static HorizontalPump.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( HorizontalPump.NozzleKind val in Enum.GetValues( typeof( HorizontalPump.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }

    private static HorizontalPump.PlacementPlane GetPlacementPlane( string plane )
    {
      foreach ( HorizontalPump.PlacementPlane val in Enum.GetValues( typeof( HorizontalPump.PlacementPlane ) ) )
      {
        if ( val.ToString() == plane )
        {
          return val;
        }
      }
      throw new InvalidOperationException( "Placement Plane not found." );
    }
  }
}