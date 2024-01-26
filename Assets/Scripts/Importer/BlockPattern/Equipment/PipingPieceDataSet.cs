using System.Data ;
using System.IO ;
using Importer.Equipment ;

namespace Importer.BlockPattern.Equipment
{
  static class PipingPieceDataSet
  {
    public static DataSet GetPipingPieceDataSet()
    {
      DataSet dataSet = new DataSet() ;
      var table = TableReader.Load( Path.Combine( ImportManager.InstrumentsPath(), @"InstrumentTable.csv" ), "instruments" ) ;
      for ( int i = 0 ; i < table.Rows.Count ; ++i ) {
        dataSet.Tables.Add( TableReader.Load(
          Path.Combine( ImportManager.InstrumentsPath(), table.Rows[ i ][ 1 ].ToString() ),
          table.Rows[ i ][ 0 ].ToString() ) ) ;
      }

      return dataSet ;
    }
  }
}
