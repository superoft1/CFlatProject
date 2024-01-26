using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class PlateTypeHeatExchangerTableImporter : PipingPieceTableImporter
  {
    public PlateTypeHeatExchangerTableImporter( DataSet dataSet ) : base( dataSet, "PlateTypeHeatExchanger", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var he = doc.CreateEntity<PlateTypeHeatExchanger>();

      he.EquipNo = cells[0];
      he.Width = double.Parse(cells[6]) / 1000.0;
      he.Length = double.Parse(cells[7]) / 1000.0;
      he.Height = double.Parse(cells[8]) / 1000.0;
        
      for (var i = 9; i < cells.Length; i += 5)
      {
        var kind = GetNozzleKind(cells[i]);
        var length = double.Parse(cells[i + 3]) / 1000d;
        var diameterMm = double.Parse(cells[i + 4]);
        Nozzle nozzle = he.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm));

        nozzle.NozzleType = GetNozzleType(cells[i + 2]);
        nozzle.Name = cells[i + 1];
      }

      return ( he, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }

    private static PlateTypeHeatExchanger.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( PlateTypeHeatExchanger.NozzleKind val in Enum.GetValues( typeof( PlateTypeHeatExchanger.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}