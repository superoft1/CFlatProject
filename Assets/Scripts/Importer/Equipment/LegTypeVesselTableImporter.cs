using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class LegTypeVesselTableImporter : PipingPieceTableImporter
  {
    public LegTypeVesselTableImporter( DataSet dataSet ) : base( dataSet, "LegTypeVessel", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var vessel = doc.CreateEntity<LegTypeVessel>();
      
      vessel.EquipNo = cells[0];
      vessel.HeightOfCylinder = double.Parse(cells[6]) / 1000.0;
      vessel.DiameterOfCylinder = double.Parse(cells[7]) / 1000.0;
      vessel.HeightOfLeg = double.Parse(cells[8]) / 1000.0;
      vessel.LegThickness = double.Parse(cells[9]) / 1000.0;
      vessel.IsLowerCapFlat = (double.Parse(cells[11]) == 0.0);
        
      if (createNozzle) {
        for (var i = 13; i < cells.Length; i += 7) {
          var kind = GetNozzleKind( cells[i] );
          var length = double.Parse( cells[i + 3] ) / 1000d;
          var diameterMm = double.Parse( cells[i + 4] );
          var height = double.Parse( cells[i + 5] ) / 1000d;
          var angle = double.Parse( cells[i + 6] );
          Nozzle nozzle;
          switch (kind) {
            case LegTypeVessel.NozzleKind.UP:
            case LegTypeVessel.NozzleKind.HZ:
            case LegTypeVessel.NozzleKind.DN:
              nozzle = vessel.AddNozzle( kind, length, DiameterFactory.FromNpsMm(diameterMm), height, angle );
              break;
            default:
              throw new InvalidOperationException();
          }
          nozzle.NozzleType = GetNozzleType( cells[i + 2] );
          nozzle.Name = cells[i + 1];
        }
      } 
      return ( vessel, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }

    private static LegTypeVessel.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( LegTypeVessel.NozzleKind val in Enum.GetValues( typeof( Column.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}