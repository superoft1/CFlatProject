using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DimensionOfTeesTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get; internal set; }
      public int NPS_H_mm { get; internal set; }
      public int NPS_B_mm { get; internal set; }
      public double CenterToEnd_Run_C { get; set; }
      public double CenterToEnd_Outlet_M { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimTees in _referenceDB.GetTable<STD_DimensionOfTees>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on dimTees.Standard equals sta.ID
           join npsh in _referenceDB.GetTable<STD_NPS>(context) on dimTees.NPS_H equals npsh.ID
           join npsb in _referenceDB.GetTable<STD_NPS>(context) on dimTees.NPS_B equals npsb.ID
           select new Record()
           {
             Standard = sta.Name,
             NPS_H_mm = npsh.mm,
             NPS_B_mm = npsb.mm,
             CenterToEnd_Run_C = dimTees.CenterToEnd_Run_C,
             CenterToEnd_Outlet_M = dimTees.CenterToEnd_Outlet_M
           }).Cast<RecordBase>().ToList();
      }
    }

    public Record GetOne(int NPS_H_mm, int NPS_B_mm, string standard = "ASME")
    {
      var hit = Records.Where(rec => rec.Standard == standard &&
                                    rec.NPS_H_mm == NPS_H_mm &&
                                    rec.NPS_B_mm == NPS_B_mm);
      if (hit.Any()) {
        return hit.First();
      }

      // 暫定！組み合わせが存在しない場合
      // Branchのほうが大きい場合は不正とする
      // Run : BranchのOutletには依存しないので、存在するNPS_Runに対応するRun寸法を採用する
      // Outlet:
      // 2インチ(50mm)未満は、Rと同じ
      // 2インチ(50mm)以上は、入力されたNPS_Runを条件にNPS_OutletとOutletの組を抽出して、線形モデルを作る
      if(NPS_H_mm > NPS_B_mm) {
        var hit2 = Records.Where(rec => rec.Standard == standard &&
                                      rec.NPS_H_mm == NPS_H_mm);
        if (hit2.Any()) {
          var run = hit2.First().CenterToEnd_Run_C;

          // 単回帰式 // TODO Mathに入れる
          var x = hit2.Select(rec => (double)rec.NPS_B_mm).ToArray();
          var y = hit2.Select(rec => rec.CenterToEnd_Outlet_M).ToArray();

          var xAve = x.Average();
          var yAve = y.Average();

          var beta1Denom = 0.0;
          var beta1Numer = 0.0;
          for (int i = 0; i < x.Length; i++)
          {
            beta1Numer += (y[i] - yAve) * (x[i] - xAve);
            beta1Denom += (x[i] - xAve) * (x[i] - xAve);
          }
          var beta1 = beta1Numer / beta1Denom;
          var beta0 = yAve - beta1 * xAve;
          var outlet = beta0 + beta1 * NPS_B_mm;
          outlet = System.Math.Round(outlet);
          return new Record()
          {
            NPS_H_mm = NPS_H_mm,
            NPS_B_mm = NPS_B_mm,
            Standard = standard,
            CenterToEnd_Run_C = run,
            CenterToEnd_Outlet_M = outlet
          };
        }
      }

      throw new NoRecordFoundException($"No record found for NPS_H = {NPS_H_mm} and NPS_B = {NPS_B_mm} in DimensionOfTees");
    }
  }
}