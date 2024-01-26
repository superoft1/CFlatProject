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
  internal class HorizontalVesselTableImporter : PipingPieceTableImporter
  {
    public HorizontalVesselTableImporter( DataSet dataSet) : base( dataSet, "HorizontalVessel", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle  )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var vessel = doc.CreateEntity<HorizontalVessel>();

      vessel.EquipNo = cells[0];
      vessel.LengthOfDrum = double.Parse(cells[6]) / 1000.0;
      vessel.DiameterOfDrum = double.Parse(cells[7]) / 1000.0;

      for (var i = 13; i < cells.Length; i += 6)
      {
        var kind = GetNozzleKind(cells[i]);
        var length = double.Parse(cells[i + 3]) / 1000d;
        var diameterMm = double.Parse(cells[i + 4]);
        var directionFromBase = double.Parse(cells[i + 5]) / 1000d;
        Nozzle nozzle = vessel.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm), directionFromBase);

        nozzle.NozzleType = GetNozzleType(cells[i + 2]);
        nozzle.Name = cells[i + 1];
      }

      return ( vessel, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }


    private static HorizontalVessel.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( HorizontalVessel.NozzleKind val in Enum.GetValues( typeof( HorizontalVessel.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}