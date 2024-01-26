using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Linq ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern
{
  class NoPipesBlockPatternTableImporter
  {
    private DataTable Table { get ; }

    private NoPipesBlockPatternTableImporter(string tableCsv)
    {
      Table = TableReader.Load( tableCsv, "NoPipesBlockPatternTable", needPrimeKey:false ) ;
    }

    private IEnumerable<(BlockPatternType.Type type, IEnumerable<string> instruments)> EachBlockPattern()
    {
      var dv = new DataView(Table);
      var distinctTable = dv.ToTable( true, Table.Columns[ 0 ].ColumnName ) ;
      foreach ( DataRow row in distinctTable.Rows ) {
        var blockName = row[ 0 ].ToString() ;
        List<string> values = new List<string>();
        HashSet<BlockPatternType.Type> checkTypes = new HashSet<BlockPatternType.Type>();
        foreach ( DataRow r in Table.Rows ) {
          if ( r[ 0 ].ToString() != blockName ) continue ;
          checkTypes.Add( BlockPatternType.Parse( r[ 1 ].ToString() ) ) ;
          values.Add( r[ 2 ].ToString() ) ;
        }

        if ( checkTypes.Count != 1 ) {
          throw new InvalidOperationException("Multiple blockPattern type error.");
        }

        yield return (checkTypes.First(), values) ;
      }
    }
    
    public static void ImportAll( DataSet instrumentTables, List<string> csvPathList)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;

      foreach ( var noPipesTable in csvPathList.Select( c => new NoPipesBlockPatternTableImporter( c ) ) ) {
        foreach ( var blocks in noPipesTable.EachBlockPattern() ) {
          var bp = BlockPatternFactory.CreateBlockPattern( blocks.type ) ;
          bool updateBlockPatternLocalCod = true ;
          foreach ( var instName in blocks.instruments ) {
            var instrumentTable = PipingPieceTableFactory.Create( blocks.type, instrumentTables ) ;
            var (instrument, origin, rot) = instrumentTable.Generate( curDoc, instName, createNozzle: true ) ;
            if ( updateBlockPatternLocalCod ) {
              bp.LocalCod = new LocalCodSys3d( origin, rot, false ) ;
            }
            updateBlockPatternLocalCod = false ;

            if ( instrument is GenericEquipment genericEquipment ) {
              bp.Name = genericEquipment.EquipmentType + "Block" ;
            }

            BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
          }
        }
      }
    }
  }
}