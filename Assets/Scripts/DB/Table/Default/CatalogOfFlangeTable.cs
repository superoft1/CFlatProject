using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class CatalogOfFlangeTable : TableBaseCached 
  {
    public class Record : RecordOfCatalog
    {
      public int    NPSmm     { get; internal set; }
      public int    Rating    { get; internal set; }
      public string EndPrep   { get; internal set; }
      public string FlangeFace{ get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from catalog in _referenceDB.GetTable<CatalogOfFlange>(context)
           join standard in _referenceDB.GetTable<STD_Standard>(context) on catalog.Standard equals standard.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on catalog.NPS equals nps.ID
           join endPrep in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on catalog.EndPrep equals endPrep.ID
           join flangeFace in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on catalog.FlangeFace equals flangeFace.ID
           join rating in _referenceDB.GetTable<STD_RatingClass>(context) on catalog.Rating equals rating.ID

           select new Record()
           {
             Standard = standard.Name,
             ShortCode = catalog.ShortCode,
             NPSmm = nps.mm,
             Rating = rating.Rating,
             EndPrep = endPrep.Code,
             FlangeFace = flangeFace.Code,
             IdentCode = catalog.IdentCode
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPSmm, int rating, string endPrep, string flangeFace, string standard = "ASME")
    {
      return Records.Where(rec => rec.NPSmm == NPSmm &&
                                  rec.Rating == rating &&
                                  rec.EndPrep == endPrep &&
                                  rec.FlangeFace == flangeFace &&
                                  rec.Standard == standard).ToList();
    }
  }
}
