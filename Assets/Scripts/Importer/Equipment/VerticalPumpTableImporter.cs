using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class VerticalPumpTableImporter : PipingPieceTableImporter
  {
    public VerticalPumpTableImporter( DataSet dataSet ) : base( dataSet, "VerticalPump", (2,3,4) )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var pump = doc.CreateEntity<VerticalPump>();

      pump.EquipNo = cells[0];
      pump.Height = double.Parse(cells[5]) / 1000.0;
      pump.Diameter = double.Parse(cells[6]) / 1000.0;

      for (var i = 7; i < cells.Length; i += 5)
      {
        var kind = GetNozzleKind(cells[i]);
        var length = double.Parse(cells[i + 3]) / 1000d;
        var diameterMm = double.Parse(cells[i + 4]);
        var nozzle = pump.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm));
        nozzle.NozzleType = GetNozzleType(cells[i + 2]);
        nozzle.Name = cells[i + 1];
      }
      return ( pump, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }

    private static VerticalPump.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( VerticalPump.NozzleKind val in Enum.GetValues( typeof( VerticalPump.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}