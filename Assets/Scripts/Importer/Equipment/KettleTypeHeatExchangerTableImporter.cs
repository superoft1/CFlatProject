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
  internal class KettleTypeHeatExchangerTableImporter : PipingPieceTableImporter
  {
    public KettleTypeHeatExchangerTableImporter( DataSet dataSet) : base( dataSet, "KettleTypeHeatExchanger", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle  )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;

      var he = doc.CreateEntity<HorizontalHeatExchanger>();
      he.EquipNo = cells[0];
      var shell = doc.CreateEntity<KettleTypeShell>();  
      shell.LengthOfTube = double.Parse(cells[6]) / 1000.0;
      shell.LengthOfTip = 0.0;
      shell.DiameterOfDrum = double.Parse(cells[7]) / 1000.0;
      shell.DiameterOfTip = shell.DiameterOfDrum * 0.3;
      shell.WidthOfSaddle = double.Parse(cells[8]) / 1000.0;
      shell.HeightOfSaddle = double.Parse(cells[11]) / 1000.0;
      shell.LengthOfSaddle = shell.DiameterOfDrum / 2.0;
      shell.WidthOfSaddle = 0.3;
      shell.HeightOfSaddle = 0.3;
      he.Shell = shell;

      var frontEnd = doc.CreateEntity<CapTypeFrontEnd>();
      frontEnd.LengthOfTube = shell.LengthOfTube / 4.0;
      he.FrontEnd = frontEnd;

      var rearEnd = doc.CreateEntity<CapTypeRearEnd>();
      rearEnd.LengthOfTube = 0.0;
      he.RearEnd = rearEnd;

      return ( he, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
    
    private static KettleTypeHeatExchanger.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( KettleTypeHeatExchanger.NozzleKind val in Enum.GetValues( typeof( KettleTypeHeatExchanger.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}