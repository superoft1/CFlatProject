using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class ColumnTableImporterImporter : PipingPieceTableImporter
  {
    public ColumnTableImporterImporter( DataSet dataSet ) : base(dataSet, "Column", (3,4,5), 2)
    { }
  
    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var column = doc.CreateEntity<Column>() ;

      column.EquipNo = cells[ 0 ] ;
      column.HeightOfTower = double.Parse( cells[ 6 ] ) / 1000.0 ;
      column.DiameterOfTower = double.Parse( cells[ 7 ] ) / 1000.0 ;
      column.HeightOfSkirt = double.Parse( cells[ 10 ] ) / 1000.0 ;
      column.IsLowerCapFlat = double.Parse( cells[ 12 ] ) == 0.0 ;

      if ( createNozzle ) {
        for ( var i = 13 ; i < cells.Length ; i += 7 ) {
          var kind = GetNozzleKind( cells[ i ] ) ;
          var length = double.Parse( cells[ i + 3 ] ) / 1000d ;
          var diameterMm = double.Parse( cells[ i + 4 ] ) ;
          var height = double.Parse( cells[ i + 5 ] ) / 1000d ;
          var angle = double.Parse( cells[ i + 6 ] ) ;
          Nozzle nozzle ;
          switch ( kind ) {
            case Column.NozzleKind.UP :
            case Column.NozzleKind.DOWN :
              nozzle = column.AddNozzle( kind, length, DiameterFactory.FromNpsMm(diameterMm)) ;
              break ;
            case Column.NozzleKind.HZ1 :
            case Column.NozzleKind.HZ2 :
            case Column.NozzleKind.HZ3 :
            case Column.NozzleKind.HZ4 :
            case Column.NozzleKind.HZ5 :
            case Column.NozzleKind.HZ6 :
            case Column.NozzleKind.HZ7 :
            case Column.NozzleKind.HZ8 :
            case Column.NozzleKind.HZ9 :
            case Column.NozzleKind.HZ10 :
              nozzle = column.AddNozzle( kind, length, DiameterFactory.FromNpsMm(diameterMm), height, angle ) ;
              break ;
            default :
              throw new InvalidOperationException() ;
          }
          nozzle.NozzleType = GetNozzleType( cells[ i + 2 ] ) ;
          nozzle.Name = cells[ i + 1 ] ;
        }
      }
      return ( column, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
  
    private static Column.NozzleKind GetNozzleKind(string kind)
    {
      foreach (Column.NozzleKind val in Enum.GetValues(typeof(Column.NozzleKind)))
      {
        if (val.ToString() == kind)
        {
          return val;
        }
      }
      throw new InvalidOperationException("Nozzle Kind not found.");
    }
  }
}
