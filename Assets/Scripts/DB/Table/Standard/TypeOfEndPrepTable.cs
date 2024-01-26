using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class TypeOfEndPrepTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Code { get; set; }
      public string Name { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from typeEndPrep in _referenceDB.GetTable<STD_TypeOfEndPrep>(context)
           select new Record()
           {
             Code = typeEndPrep.Code,
             Name = typeEndPrep.Name
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<string> GetCodeList()
    {
      return Records.Select(rec => rec.Code).ToList();
    }
  }
}