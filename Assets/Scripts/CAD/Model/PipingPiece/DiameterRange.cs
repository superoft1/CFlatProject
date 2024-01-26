using System ;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using Tolerance = Chiyoda.CAD.Core.Tolerance ;

namespace Chiyoda.CAD.Model
{
  public class DiameterRange
  {
    const int MinDiameterIndex = 0;

    private IList<NPSTable.Record> _recordList;

    private IList<NPSTable.Record> RecordList => _recordList;

    public int GetMinDiameterIndex() => MinDiameterIndex;
    public int GetMaxDiameterIndex() { return RecordList.Count-1; }

    public int MinDiameterNpsMm { get; private set; }

    public int MaxDiameterNpsMm { get; private set; }

    public DiameterRange(int minDiameterNpsMm, int maxDiameterNpsMm)
    {
      ChangeRange(minDiameterNpsMm, maxDiameterNpsMm);
    }

    public void ChangeRange(int minDiameterNpsMm, int maxDiameterNpsMm)
    {
      MinDiameterNpsMm = minDiameterNpsMm;
      MaxDiameterNpsMm = maxDiameterNpsMm;
      _recordList = Chiyoda.DB.DB.Get<NPSTable>().GetRange(minDiameterNpsMm, maxDiameterNpsMm);
    }

    public Diameter FromIndex( int index )
    {
      var rec = RecordList[ index ] ;
      return new Diameter( npsMm: rec.mm, npsInch: rec.Inchi ) ;
    }

    public int GetIndex( Diameter diameter )
    {
      // TODO なんか雑
      var minDist = RecordList.Select(rec => Math.Abs(rec.mm - diameter.NpsMm)).Min();
      var targetRec = RecordList.First(rec => Math.Abs(rec.mm - diameter.NpsMm) < minDist + Tolerance.DistanceTolerance);
      return RecordList.IndexOf(targetRec);
    }

    // ここはNPSの分数表示を返す
    public string GetInchiDisp(int index)
    {
      return RecordList[index].InchiStr;
    }
    
    /// <summary>
    /// ブロックパターンの径の呼び径MM範囲(1.5 inch ~ 20 inch)
    /// これ以外の範囲が必要な場合は調整する事
    /// </summary>
    /// <returns></returns>
    public static (int min, int max) GetBlockPatternNpsMmRange()
    {
      return ( 40, 500 ) ;
    }
  }
}