using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology;
using Chiyoda.DB ;
using Tolerance = Chiyoda.CAD.Core.Tolerance;

namespace Chiyoda.CAD.Model
{
  public static class DiameterFactory
  {
    private static NPSTable Table
    {
      get
      {
        if (DocumentCollection.Instance.Current == null)
        {
          throw new InvalidOperationException();
        }
        return Chiyoda.DB.DB.Get<NPSTable>();
      }
    }

    private static IList<NPSTable.Record> Records => Table.Records;

    public static int MinDiameterNPSMm => Records[0].mm;
    public static int MaxDiameterNPSMm => Records[Records.Count-1].mm;

    public static Diameter FromNpsMm(double npsMm)
    {
      // TODO なんか雑
      var minDist = Records.Select(rec => Math.Abs(rec.mm - npsMm)).Min();
      var targetRec = Records.First(rec => Math.Abs(rec.mm - npsMm) < minDist + Tolerance.DistanceTolerance);
      return new Diameter( npsMm: targetRec.mm, npsInch: targetRec.Inchi);
    }

    public static Diameter FromNpsInch(double npsInch)
    {
      // TODO なんか雑
      var minDist = Records.Select(rec => Math.Abs(rec.Inchi - npsInch)).Min();
      var targetRec = Records.First(rec => Math.Abs(rec.Inchi - npsInch) < minDist + Tolerance.DistanceTolerance);
      return new Diameter(npsMm: targetRec.mm, npsInch: targetRec.Inchi);
    }

    /// <summary>
    /// 外形(ミリメートル）から変換
    /// </summary>
    /// <param name="outsideDiameter"></param>
    /// <returns></returns>
    public static Diameter FromOutsideMm(double outsideDiameter)
    {
      var closestNPSmm = Chiyoda.DB.DB.Get<OutsideDiameterTable>().GetClosestTo( outsideDiameter ).NPSmm ;
      var targetRec = Records.First(rec => rec.mm == closestNPSmm);
      return new Diameter(npsMm: targetRec.mm, npsInch: targetRec.Inchi);
    }

    /// <summary>
    /// 外形(メートル）から変換
    /// </summary>
    /// <param name="outsideDiameterMeter"></param>
    /// <returns></returns>
    public static Diameter FromOutsideMeter(double outsideDiameterMeter)
    {
      return FromOutsideMm(outsideDiameterMeter.ToMillimeters());
    }

    /// <summary>
    /// 外形(ミリメートル）を取得
    /// </summary>
    /// <param name="diameter"></param>
    /// <returns></returns>
    public static double GetOutsideDiameterMm(Diameter diameter)
    {
      var minDist = Records.Select(rec => Math.Abs(rec.mm - diameter.NpsMm)).Min();
      var targetRec = Records.First(rec => Math.Abs(rec.mm - diameter.NpsMm) < minDist + Tolerance.DistanceTolerance);
      return Chiyoda.DB.DB.Get<OutsideDiameterTable>().GetOne( targetRec.mm ).OutsideDiameter ;
    }

    public static int GetDiffIndexBetween(int NPSFrom_mm, int NPSTo_mm)
    {
      return Table.GetDiffIndexBetween(NPSFrom_mm, NPSTo_mm);
    }

    public static Diameter Default()
    {
      return new Diameter(0,0);
    }

    public static DiameterRange GetDiameterRangeFromLine(Line line)
    {
      // FIXME: lineからサービスクラスを取得する
      // 現状は暫定の範囲を返している
      return new DiameterRange(MinDiameterNPSMm, MaxDiameterNPSMm);
    }
  }
}
