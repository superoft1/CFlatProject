using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class CatalogOfReducerTable : TableBaseCached 
  {
    public class Record : RecordOfCatalog
    {
      public int    NPS_L_mm            { get; internal set; }
      public int    NPS_S_mm            { get; internal set; }
      public string EndPrep             { get; internal set; }
      public string IdentificationNote  { get; internal set; }
      public int?   Schedule            { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from cor in _referenceDB.GetTable<CatalogOfReducer>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on cor.Standard equals sta.ID
           join ptn in _referenceDB.GetTable<STD_PipeThickness>(context) on cor.PipeThickness equals ptn.ID
           join nps_l in _referenceDB.GetTable<STD_NPS>(context) on ptn.NPS equals nps_l.ID
           join nps_s in _referenceDB.GetTable<STD_NPS>(context) on cor.NPS_S equals nps_s.ID
           join edp in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on cor.EndPrep equals edp.ID

           select new Record()
           {
             ShortCode = cor.ShortCode,
             Standard = sta.Name,
             IdentificationNote = ptn.IdentificationNote,
             NPS_L_mm = nps_l.mm,
             NPS_S_mm = nps_s.mm,
             Schedule = ptn.Schedule_No,
             EndPrep = edp.Code,
             IdentCode = cor.IdentCode
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPS_L_mm, string endPrep, string identificationNote, string standard = "ASME")
    {
      var rtn = Records.Where(rec =>  rec.NPS_L_mm == NPS_L_mm &&
                                      rec.EndPrep == endPrep &&
                                      rec.IdentificationNote == identificationNote &&
                                      rec.Standard == standard).ToList();
      if (rtn.Any()) return rtn.ToList();
      throw new NoRecordFoundException($"No record found for NPS_L_mm = {NPS_L_mm} and EndPrep = {endPrep} and IdentificationNotein = {identificationNote} In CatalogOfTee");
    }

  }
}