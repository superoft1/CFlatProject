using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class TypeOfElbowTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Name { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from typeElbow in _referenceDB.GetTable<STD_TypeOfElbow>(context)
           select new Record()
           {
             Name = typeElbow.Name
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<string> GetCodeList()
    {
      return Records.Select(rec => rec.Name).ToList();
    }
  }
}