using System.Collections.Generic ;

namespace Chiyoda.CableRouting
{
  public class CablePathUtil
  {
    public static IList<ICablePath> CollectFrom(IList<ICablePathAvailable> cablePathAvailableList)
    {
      IList<ICablePath> rtn = new List<ICablePath>();
      // CablePathを取得
      // ひとまず、ラックの各レイヤからは１つずつとする
      foreach (var cablePathAvailable in cablePathAvailableList) {
        if ( cablePathAvailable != null )
          foreach ( var cablePath in cablePathAvailable.GetCablePath() ) {
            rtn.Add( cablePath ) ;
          }
      }
      return rtn;
    }
  }
}