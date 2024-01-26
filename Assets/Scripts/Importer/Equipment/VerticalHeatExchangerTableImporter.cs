using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class VerticalHeatExchangerTableImporter : PipingPieceTableImporter
  {
    public VerticalHeatExchangerTableImporter( DataSet dataSet ) : base( dataSet, "VerticalHeatExchanger", (2,3,4) )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var he = doc.CreateEntity<VerticalHeatExchanger>();

      he.EquipNo = cells[0];
      he.Height = double.Parse(cells[5]) / 1000.0;
      he.Diameter = double.Parse(cells[6]) / 1000.0;

      return ( he, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
  }
}