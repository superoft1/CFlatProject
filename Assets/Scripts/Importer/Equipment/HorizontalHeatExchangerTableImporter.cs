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
  internal class HorizontalHeatExchangerTableImporter : PipingPieceTableImporter
  {
    public HorizontalHeatExchangerTableImporter( DataSet dataSet) : base( dataSet, "HorizontalHeatExchanger", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle  )
    {
      var row = Table.Rows.Find(name);
      var cells = TableReader.Row2Array(row);

      var he = doc.CreateEntity<HorizontalHeatExchanger>();
      he.EquipNo = cells[0];

      var shell = GenerateShell(doc, cells);
      he.Shell = shell;
      var frontEnd = GenerateFrontEnd(doc, cells);
      he.FrontEnd = frontEnd;
      var rearEnd = GenerateRearEnd(doc, cells);
      he.RearEnd = rearEnd;

//      return (he, Vector3d.zero/*ParseOrigin( cells )*/, Quaternion.identity);

      if (createNozzle)
      {
        for (var i = 22; i < cells.Length; i += 5)
        {
          var kind = GetNozzleKind(cells[i]);
          var length = double.Parse(cells[i + 3]) / 1000d;
          var diameterMm = double.Parse(cells[i + 4]);
          var nozzle = he.AddNozzle(kind, length, DiameterFactory.FromNpsMm(diameterMm));
          nozzle.NozzleType = GetNozzleType(cells[i + 2]);
          nozzle.Name = cells[i + 1];
        }
      }
      return (he, ParseOrigin(cells), ParseAngleAxis(cells));
    }

    Shell GenerateShell(Document doc, string[] cells)
    {
      if (cells[6] == "Straight")
      {
        return GenerateStraightTypeShell(doc, cells);
      }
      else if (cells[6] == "Kettle")
      {
        return GenerateKettleTypeShell(doc, cells);
      }
      else
      {
        return null;
      }
    }

    Shell GenerateStraightTypeShell(Document doc, string[] cells)
    {
      var shell = doc.CreateEntity<StraightTypeShell>();
      shell.Length = ToMeter(cells[9]);
      shell.Diameter = ToMeter(cells[7]);
      shell.WidthOfSaddle = ToMeter(cells[14]);
      shell.HeightOfSaddle = ToMeter(cells[15]);
      shell.LengthOfSaddle = ToMeter(cells[13]);
      shell.DistanceOf1stSaddle = ToMeter(cells[11]);
      shell.DistanceOf2ndSaddle = ToMeter(cells[12]);
      return shell;
    }

    Shell GenerateKettleTypeShell(Document doc, string[] cells)
    {
      var shell = doc.CreateEntity<KettleTypeShell>();
      shell.DiameterOfTip = ToMeter(cells[7]);
      shell.DiameterOfDrum = ToMeter(cells[8]);
      shell.LengthOfTip = ToMeter(cells[9]);
      shell.LengthOfTube = ToMeter(cells[10]);
      shell.WidthOfSaddle = ToMeter(cells[14]);
      shell.HeightOfSaddle = ToMeter(cells[15]);
      shell.LengthOfSaddle = ToMeter(cells[13]);
      shell.DistanceOf1stSaddle = ToMeter(cells[11]);
      shell.DistanceOf2ndSaddle = ToMeter(cells[12]);
      return shell;
    }

    FrontEnd GenerateFrontEnd(Document doc, string[] cells)
    {
      if (cells[18] == "Flat")
      {
        return GenerateFlatTypeFrontEnd(doc, cells);
      }
      else if (cells[18] == "Cap")
      {
        return GenerateCapTypeFrontEnd(doc, cells);
      }
      else
      {
        return null;
      }
    }

    FrontEnd GenerateFlatTypeFrontEnd(Document doc, string[] cells)
    {
      var frontEnd = doc.CreateEntity<FlatTypeFrontEnd>();
      frontEnd.Length = ToMeter(cells[19]);
      return frontEnd;
    }

    FrontEnd GenerateCapTypeFrontEnd(Document doc, string[] cells)
    {
      var frontEnd = doc.CreateEntity<CapTypeFrontEnd>();
      frontEnd.LengthOfTube = ToMeter(cells[19]);
      return frontEnd;
    }

    RearEnd GenerateRearEnd(Document doc, string[] cells)
    {
      if (cells[20] == "Flat")
      {
        return GenerateFlatTypeRearEnd(doc, cells);
      }
      else if (cells[20] == "Cap")
      {
        return GenerateCapTypeRearEnd(doc, cells);
      }
      else
      {
        return null;
      }
    }

    RearEnd GenerateFlatTypeRearEnd(Document doc, string[] cells)
    {
      var rearEnd = doc.CreateEntity<FlatTypeRearEnd>();
      rearEnd.Length = ToMeter(cells[21]);
      return rearEnd;
    }

    RearEnd GenerateCapTypeRearEnd(Document doc, string[] cells)
    {
      var rearEnd = doc.CreateEntity<CapTypeRearEnd>();
      rearEnd.LengthOfTube = ToMeter(cells[21]);
      return rearEnd;
    }

    static double ToMeter( string val )
    {
      return double.Parse( val ) / 1000.0;
    }

    private static HorizontalHeatExchanger.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( HorizontalHeatExchanger.NozzleKind val in Enum.GetValues( typeof( HorizontalHeatExchanger.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}