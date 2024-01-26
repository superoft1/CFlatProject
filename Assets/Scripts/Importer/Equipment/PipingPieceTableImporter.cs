using System ;
using System.Data ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  public abstract class PipingPieceTableImporter
  {
    protected DataTable Table { get ; }
    private (int x, int y, int z) OriginIndex { get ; }
    private int? AngleAxisIndex { get ; }

    protected PipingPieceTableImporter(DataSet dataSet, string tableName, (int x, int y, int z) originIndex, int? angleAxisIndex = null)
    {
      Table = dataSet.Tables[ tableName ] ;
      OriginIndex = originIndex;
      AngleAxisIndex = angleAxisIndex ;
    }

    public abstract (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name,
      bool createNozzle ) ;

    public LocalCodSys3d ParseLocalCod( string name )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      return new LocalCodSys3d(ParseOrigin( cells ), ParseAngleAxis( cells ), false);
    }
  
    protected Vector3d ParseOrigin( string[] cells )
    {
      return new Vector3d( -double.Parse( cells[OriginIndex.x] ), double.Parse( cells[OriginIndex.y] ), double.Parse( cells[OriginIndex.z] ) ) / 1000d;
    }

    protected Quaternion ParseAngleAxis( string[] cells )
    {
      if ( ! AngleAxisIndex.HasValue ) {
        return Quaternion.identity;
      }
      float.TryParse( cells[AngleAxisIndex.Value], out var rotAngle );
      return Quaternion.AngleAxis( rotAngle, Vector3.forward );
    }

    protected Nozzle.Type GetNozzleType(string type)
    {
      foreach (Nozzle.Type val in Enum.GetValues(typeof(Nozzle.Type)))
      {
        if (val.ToString() == type)
        {
          return val;
        }
      }

      throw new InvalidOperationException("Nozzle Type not found.");
    }
  }
}