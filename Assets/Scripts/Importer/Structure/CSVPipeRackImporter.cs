using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;

public class CSVPipeRackImporter : StructureImporter
{
  public override void ImportData(string path, Document doc)
  {
    using (var sr = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
    {
      while (!sr.EndOfStream)
      {
        var rack = StructureFactory.CreatePipeRack(doc, PipeRackFrameType.Single );

        var line = sr.ReadLine();
        if (string.IsNullOrEmpty(line))
        {
          continue;
        }
        var cells = line.Split(',');

        var heights = new List<double>();
        var maxLayerCnt = 6;

        var structureId = cells[0];
        double.TryParse(cells[2], out var rotation);
        double.TryParse(cells[6], out var width);
        int.TryParse(cells[8], out var intervalNo);
        double.TryParse(cells[9], out var beamInterval);
        {
          for (int i = 0; i < maxLayerCnt; i++)
          {
            var colIdx = 11 + i;
            var height = 0.0;
            if (cells.Length > colIdx && double.TryParse(cells[colIdx], out height) && height != 0.0) heights.Add(height);
            else break;
          }
        }

        if ( rack is Entity e ) {
          e.Name = structureId ;
        }
        rack.Rotation = rotation;
        rack.Position = Origin(cells);
        
        rack.IntervalCount = intervalNo;
        rack.SetWidthAndStandardMaterials( width / 1000.0, beamInterval / 1000.0 ) ;
        rack.IsHalfDownSideBeam = ( cells[ 10 ] == "A" ) ;

        rack.FloorCount = heights.Count();
        {
          var preHeight = 0.0;
          int i = 0;
          foreach (var height in heights) {
            rack.SetFloorHeight( i++, ( height - preHeight ) / 1000 ) ;
            preHeight = height;
          }
        }        
      }
    }
  }
}
