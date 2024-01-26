using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class NPSTable : TableBaseCached 
  {
    public class Record : RecordBase
    { 
      public int mm { get ; internal set ; }
      public double Inchi { get ; internal set ; }
      public string InchiStr { get ; internal set ; }
    }

    public IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using ( IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef) )
      using ( var context = new DataContext( dbcon ) ) {
        return
          (from nps in _referenceDB.GetTable<STD_NPS>( context )
          select new Record()
          {
            mm = nps.mm,
            Inchi = nps.Inchi,
            InchiStr = nps.InchiDisp,
          } ).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> GetRange(int MinNPS, int maxNPS)
    {
      return Records.Where(rec => rec.mm >= MinNPS && rec.mm <= maxNPS).OrderBy(rec => rec.mm).ToList();

      // TODO 異常系のテスト
    }

    public int GetDiffIndexBetween(int NPSFrom_mm, int NPSTo_mm)
    {
      // インデックス比較対象リスト
      var targetList = Records.Select(rec => rec.mm).ToList();

      var idxOfFrom = targetList.IndexOf(NPSFrom_mm);
      var idxOfTo   = targetList.IndexOf(NPSTo_mm);
      if (idxOfFrom == -1 || idxOfTo == -1){
        throw new NoRecordFoundException($"No record found for NPSFrom = {NPSFrom_mm} of NPSTo = {NPSTo_mm} in NPS");
      }
      return idxOfTo - idxOfFrom;
    }
  }
}