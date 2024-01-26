using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DiameterTable : TableBase
  {
    public class Record : RecordBase
    {
      public string PipingClass { get; internal set; }
      public int NPSmm { get; internal set; }
      public double NPSInchi { get; internal set; }
      public string NPSInchiStr { get; internal set; }
      public double OutsideDiameter { get; internal set; }
      public double WallThickness { get; set; }
//      public string IdentificationNote { get; internal set; }
//      public int? ScheduleNo { get; set; }
    }

    protected override void Init()
    {
      // Do nothing
    }

    public IList<Record> GetList(string pipingClass)
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        var rtnList = 
          (from pipingclass in _referenceDB.GetTable<PipingClass>(context) 
           where pipingclass.Name == pipingClass
           join stock in _referenceDB.GetTable<PipingClassStock>(context) on pipingclass.ID equals stock.PipingClass
           where stock.ShortCode == "PIP"
           join catalogOfPipe in _referenceDB.GetTable<CatalogOfPipe>(context) on stock.Catalog equals catalogOfPipe.ID
           where  pipingclass.Standard == catalogOfPipe.Standard
           join pipeThickeness in _referenceDB.GetTable<STD_PipeThickness>(context) on catalogOfPipe.PipeThickness equals pipeThickeness.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on pipeThickeness.NPS equals nps.ID
           join outsidediameter in _referenceDB.GetTable<STD_OutsideDiameter>(context) on nps.ID equals outsidediameter.NPS
           where outsidediameter.Standard == pipingclass.Standard
           select new Record()
           {
             PipingClass = pipingclass.Name,
             NPSmm = nps.mm,
             NPSInchi = nps.Inchi,
             NPSInchiStr = nps.InchiDisp,
             OutsideDiameter = outsidediameter.mm,
             WallThickness = pipeThickeness.WallThickness_mm
           }).ToList();
        if (rtnList.Any()) return rtnList;
      }
      throw new NoRecordFoundException($"No record found for pipingClass = {pipingClass} In Diameter.GetList");
    }
    public Record GetSizeUp(string pipingClass, int currentNPSmm)
    {
      var list = GetList(pipingClass);
      var idx = GetIndex(list, currentNPSmm);
      if (idx != -1 && idx + 1 < list.Count) return list[idx + 1];
      throw new NoRecordFoundException($"No record found for pipingClass = {pipingClass} and currentNPSmm = {currentNPSmm} In Diameter.GetSizeUp");
    }

    public Record GetSizDown(string pipingClass, int currentNPSmm)
    {
      var list = GetList(pipingClass);
      var idx = GetIndex(list, currentNPSmm);
      if (idx != -1 && idx - 1 >= 0) return list[idx - 1];
      throw new NoRecordFoundException($"No record found for pipingClass = {pipingClass} and currentNPSmm = {currentNPSmm} In Diameter.GetSizeUp");
    }

    private static int GetIndex(IList<Record> list, int currentNPSmm)
    {
      var current = list.Where(rec => rec.NPSmm == currentNPSmm);
      if (current.Any())
      {
        System.Diagnostics.Debug.Assert(current.Count() == 1);
        return list.IndexOf(current.First());
      }
      return -1;
    }

    /// <summary>
    /// 入力された外径に最も近い規格のNPSレコードを返す
    /// </summary>
    /// <param name="outsideDiameter"></param>
    /// <returns>NPSのレコード</returns>
    public Record GetClosestTo(string pipingClass, double outsideDiameter)
    {
      if (outsideDiameter >= 0.0)
      {
        return GetList(pipingClass).OrderBy(rec => System.Math.Abs(outsideDiameter - rec.OutsideDiameter)).First();
      }
      throw new NoRecordFoundException($"No record found for for pipingClass = {pipingClass} and OutsideDiameter = {outsideDiameter} in Diameter.GetClosestTo");
    }

  }
}
