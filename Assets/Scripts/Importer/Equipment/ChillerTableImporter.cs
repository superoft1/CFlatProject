using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class ChillerTableImporter : PipingPieceTableImporter
  {
    public ChillerTableImporter( DataSet dataSet ) : base( dataSet, "Chiller", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var chiller = doc.CreateEntity<Chiller>();

      chiller.EquipNo = cells[0];
      chiller.LengthOfTube = double.Parse(cells[6]) / 1000.0;
      chiller.DiameterOfTube = double.Parse(cells[7]) / 1000.0;
      chiller.HeightOfTip = double.Parse(cells[15]) / 1000.0;
      return ( chiller, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
  }
}