using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class DimensionOfReducerTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get ; internal set ; }
      public int NPS_S_mm { get ; internal set ; }
      public int NPS_L_mm { get ; internal set ; }
      public double Height { get ; internal set ; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimReducer in _referenceDB.GetTable<STD_DimensionOfReducer>(context)
           join stan in _referenceDB.GetTable<STD_Standard>(context) on dimReducer.Standard equals stan.ID
           join npsL in _referenceDB.GetTable<STD_NPS>(context) on dimReducer.NPS_L equals npsL.ID
           join npsS in _referenceDB.GetTable<STD_NPS>(context) on dimReducer.NPS_S equals npsS.ID
           select new Record()
           {
             Standard = stan.Name,
             NPS_L_mm = npsL.mm,
             NPS_S_mm = npsS.mm,
             Height = dimReducer.H
           }).Cast<RecordBase>().ToList();
      }
    }
    
    public IList<Record> Get( int NPSmm1, int NPSmm2, string standard = "ASME" )
    {
      // 引数の数値のいずれかが0やマイナス、または引数の両方が同じ数値の場合は処理しない
      if ((NPSmm1 > 0 || NPSmm2 > 0 ) && NPSmm1 != NPSmm2)
      {
        var NPS_L = Math.Max(NPSmm1, NPSmm2);
        var NPS_S = Math.Min(NPSmm1, NPSmm2);

        var hit1 = Records.Where(rec => rec.Standard== standard && rec.NPS_L_mm == NPS_L && rec.NPS_S_mm == NPS_S );
        if (hit1.Count() == 1) return hit1.ToList();

        //レデューサ1つで径を落としきれないならば組み合わせを探す
        else
        {
          // 1つ目の部品を取得:NPS_Lから変換できる最もNPS_Sが小さいレデューサ
          IList<Record> parts = Records.Where(rec => rec.Standard == standard &&
                                                     rec.NPS_L_mm == NPS_L &&
                                                     rec.NPS_S_mm == (Records.Where(min => min.NPS_L_mm == NPS_L).Min(min => min.NPS_S_mm))
                                             ).ToList();
          var next_NPS_L = parts.First().NPS_S_mm;//次の部品を探すためのキー情報として最小NPS_Sを保持

          // 2つ目以降の部品を取得
          // 前の部品のNPS_Sと一致するNPS_Lを持つレデューサが無くなる迄を限度としてループ処理
          for (; (Records.Where(rec => rec.Standard == standard && rec.NPS_L_mm == next_NPS_L).ToList().Count()) > 0; )
          {
            //抽出した部品リストのNPS_Sに目的の落としたい径が含まれているか否か
            IList<Record> next_parts = Records.Where(rec => rec.Standard == standard && rec.NPS_L_mm == next_NPS_L).ToList();
            if (next_parts.Where(rec => rec.NPS_S_mm == NPS_S).Count() > 0)
            {
              //落としたい径が含まれている場合はその部品を次の組み合わせとして保持して処理を終了
              parts.Add(next_parts.Where(rec => rec.NPS_S_mm == NPS_S).ToList().First());
              return parts.ToList();
            }
            else
            {
              //落としたい径が含まれていないならば、それらの中で最も小さいNPS_Sの部品を取得し次の部品の抽出ループへ
              var min_NPS_S = next_parts.Min(min => min.NPS_S_mm);
              parts.Add(next_parts.Where(rec => rec.NPS_S_mm == min_NPS_S).ToList().First());
              next_NPS_L = min_NPS_S;//次の部品を探すためのキー情報として最小NPS_Sを保持
            }
          }
          //部品の組み合わせを見つけられずにループを終えてしまったのであれば、径を落とす事ができない組み合わせであるとする
        }
      }
      //径が落とせないパターン、引数値が0又はマイナス、引数値同士が同数値である場合・・・NoRecordFoundExceptionを返す
      throw new NoRecordFoundException($"No record found for NPS1 = {NPSmm1} and NPS2 = {NPSmm2} in DimensionOfReducerTable");
    }
  }
}