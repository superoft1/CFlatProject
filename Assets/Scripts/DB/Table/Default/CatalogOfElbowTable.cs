using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class CatalogOfElbowTable : TableBaseCached 
  {
    public class Record : RecordOfCatalog
    {
      public int NPSmm { get; internal set; }
      public string EndPrep { get; internal set; }
      public string IdentificationNote { get; internal set; }
      public int? Schedule { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from coe in _referenceDB.GetTable<CatalogOfElbow>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on coe.Standard equals sta.ID
           join ptn in _referenceDB.GetTable<STD_PipeThickness>(context) on coe.PipeThickness equals ptn.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on ptn.NPS equals nps.ID
           join edp in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on coe.EndPrep equals edp.ID

           select new Record()
           {
             Standard = sta.Name,
             ShortCode = coe.ShortCode,
             NPSmm = nps.mm,
             EndPrep = edp.Code,
             IdentificationNote = ptn.IdentificationNote,
             Schedule = ptn.Schedule_No,
             IdentCode = coe.IdentCode
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPSmm, string endPrep, string identificationNote, string elbow = "90E", string standard = "ASME")
    {
      var rtn = Records.Where(rec =>  rec.NPSmm == NPSmm &&
                                      rec.EndPrep == endPrep &&
                                      rec.ShortCode == elbow &&
                                      rec.IdentificationNote == identificationNote &&
                                      rec.Standard == standard).ToList();
      if (rtn.Any()) return rtn.ToList();
      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} and EndPrep = {endPrep} and IdentificationNotein = {identificationNote} and ShortCode = {elbow} In CatalogOfElbow");
    }

  }
}