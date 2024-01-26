using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DimensionOfPipeGuideTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public int NPSmm { get; internal set; }
      public string GuideType { get; set; }
      public double Height { get; set; }
      public double Width { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimPipeGuide in _referenceDB.GetTable<DimensionOfPipeGuide>(context)
           join nps in _referenceDB.GetTable<STD_NPS>(context) on dimPipeGuide.NPS equals nps.ID
           select new Record()
           {
             NPSmm = nps.mm,
             GuideType = dimPipeGuide.GuideType,
             Height = dimPipeGuide.Height,
             Width = dimPipeGuide.Width
           }).Cast<RecordBase>().ToList();
      }
    }

    public Record GetOne(int NPSmm, string guideType)
    {
      var hit = Records.Where(rec => rec.NPSmm == NPSmm &&
                                     rec.GuideType == guideType);
      if (hit.Any()) {
        return hit.First();
      }

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} and GuideType = {guideType} in DimensionOfPipeGuide");
    }
  }
}