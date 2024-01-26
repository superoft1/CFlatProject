using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class PipingClassTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Name { get; internal set; }
      public int    Rating { get; internal set; }
      public double  CollosionAllowance { get; internal set; }
      public string Description { get; internal set; }
      public string Standard { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from pipingClass in _referenceDB.GetTable<PipingClass>(context)
           join standard in _referenceDB.GetTable<STD_Standard>(context) on pipingClass.Standard equals standard.ID
           join rating in _referenceDB.GetTable<STD_RatingClass>(context) on pipingClass.Rating equals rating.ID
           select new Record()
           {
             Name = pipingClass.Name,
             Rating = rating.Rating,
             CollosionAllowance = pipingClass.CA,
             Description = pipingClass.ServiceDescription,
             Standard = standard.Name
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<string> GetNameList()
    {
      return Records.Select(rec => rec.Name ).ToList();
    }

    public IList<string> GetNameList(string standard)
    {
      return Records.Where(rec => rec.Standard == standard)
                    .Select(rec => rec.Name).ToList();
    }

    public IList<CatalogOfPipeTable.Record> GetPipes(string pipingClass)
    {
      var catalog = DB.Get<CatalogOfPipeTable>();

      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context_PDR = new DataContext(dbcon))
      {
        var catalogIDList =
        (from stock in _referenceDB.GetTable<PipingClassStock>(context_PDR)
         join pipingclass in _referenceDB.GetTable<PipingClass>(context_PDR) on stock.PipingClass equals pipingclass.ID
         where stock.ShortCode == "PIP" && pipingclass.Name == pipingClass
         select stock.Catalog).ToList();

        return catalog.Get(catalogIDList);
      }
    }

    /// <summary>
    /// エルボのカタログを返す
    /// </summary>
    /// <param name="pipingClass"></param>
    /// <returns></returns>
    public IList<CatalogOfElbowTable.Record> GetElbows(string pipingClass)
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context_PDR = new DataContext(dbcon))
      {
        var catalogIDList =(
          from ppcs in _referenceDB.GetTable<PipingClassStock>(context_PDR)
          join ppc in _referenceDB.GetTable<PipingClass>(context_PDR) on ppcs.PipingClass equals ppc.ID
          join coe in _referenceDB.GetTable<CatalogOfElbow>(context_PDR) on ppcs.Catalog equals coe.ID
          join sta in _referenceDB.GetTable<STD_Standard>(context_PDR) on coe.Standard equals sta.ID
          join ptn in _referenceDB.GetTable<STD_PipeThickness>(context_PDR) on coe.PipeThickness equals ptn.ID
          join nps in _referenceDB.GetTable<STD_NPS>(context_PDR) on ptn.NPS equals nps.ID
          join edp in _referenceDB.GetTable<STD_TypeOfEndPrep>(context_PDR) on coe.EndPrep equals edp.ID
          where (ppcs.ShortCode == "90E" || ppcs.ShortCode == "45E") && ppc.Name == pipingClass
          select new CatalogOfElbowTable.Record()
            {
              Standard = sta.Name,
              ShortCode = coe.ShortCode,
              NPSmm = nps.mm,
              EndPrep = edp.Code,
              IdentificationNote = ptn.IdentificationNote,
              Schedule = ptn.Schedule_No,
              IdentCode = coe.IdentCode
            }
        ).ToList();

        if (catalogIDList.Any()) return catalogIDList.ToList();
        throw new NoRecordFoundException($"No record found In PipingClass and PipingClassStock");

      }
    }

    /// <summary>
    /// ティーのカタログを返す
    /// </summary>
    /// <param name="pipingClass"></param>
    /// <returns></returns>
    public IList<CatalogOfTeeTable.Record> GetTees(string pipingClass)
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context_PDR = new DataContext(dbcon))
      {
        var catalogList = (
          from pcs  in _referenceDB.GetTable<PipingClassStock>(context_PDR)
          join pc   in _referenceDB.GetTable<PipingClass>(context_PDR)    on pcs.PipingClass    equals pc.ID
          join cot  in _referenceDB.GetTable<CatalogOfTee>(context_PDR)   on pcs.Catalog        equals cot.ID
          join sta  in _referenceDB.GetTable<STD_Standard>(context_PDR)      on cot.Standard       equals sta.ID
          join ptn  in _referenceDB.GetTable<STD_PipeThickness>(context_PDR) on cot.PipeThickness  equals ptn.ID
          join npsh in _referenceDB.GetTable<STD_NPS>(context_PDR)           on ptn.NPS            equals npsh.ID
          join npsb in _referenceDB.GetTable<STD_NPS>(context_PDR)           on cot.NPS_B          equals npsb.ID
          join edp  in _referenceDB.GetTable<STD_TypeOfEndPrep>(context_PDR) on cot.EndPrep        equals edp.ID
          where pcs.ShortCode == "TEE" && pc.Name == pipingClass
          select new CatalogOfTeeTable.Record()
          {
            Standard = sta.Name,
            ShortCode = cot.ShortCode,
            NPS_H_mm = npsh.mm,
            NPS_B_mm = npsb.mm,
            EndPrep = edp.Code,
            IdentificationNote = ptn.IdentificationNote,
            Schedule = ptn.Schedule_No,
            IdentCode = cot.IdentCode
          }
        ).ToList();

        if (catalogList.Any()) return catalogList.ToList();
        throw new NoRecordFoundException($"No record found In PipingClass and PipingClassStock");

      }
    }

    /// <summary>
    /// レデューサのカタログを返す
    /// </summary>
    /// <param name="pipingClass"></param>
    /// <returns></returns>
    public IList<CatalogOfReducerTable.Record> GetReducers(string pipingClass)
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        var catalogList = (
          from pcs in _referenceDB.GetTable<PipingClassStock>(context)
          join pc in _referenceDB.GetTable<PipingClass>(context) on pcs.PipingClass equals pc.ID
          join cor in _referenceDB.GetTable<CatalogOfReducer>(context) on pcs.Catalog equals cor.ID
          join sta in _referenceDB.GetTable<STD_Standard>(context) on cor.Standard equals sta.ID
          join ptn in _referenceDB.GetTable<STD_PipeThickness>(context) on cor.PipeThickness equals ptn.ID
          join npsl in _referenceDB.GetTable<STD_NPS>(context) on ptn.NPS equals npsl.ID
          join npss in _referenceDB.GetTable<STD_NPS>(context) on cor.NPS_S equals npss.ID
          join edp in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on cor.EndPrep equals edp.ID
          where (pcs.ShortCode == "CRE" || pcs.ShortCode == "ERE") && pc.Name == pipingClass
          select new CatalogOfReducerTable.Record()
          {
            Standard = sta.Name,
            ShortCode = cor.ShortCode,
            NPS_L_mm = npsl.mm,
            NPS_S_mm = npss.mm,
            EndPrep = edp.Code,
            IdentificationNote = ptn.IdentificationNote,
            Schedule = ptn.Schedule_No,
            IdentCode = cor.IdentCode
          }
        ).ToList();

        if (catalogList.Any()) return catalogList.ToList();
        throw new NoRecordFoundException($"No record found In PipingClass and PipingClassStock");

      }
    }

  }
}
