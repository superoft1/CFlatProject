using System.Collections.Generic ;
using System.Linq;

namespace Chiyoda.CableRouting
{
  public class CableRouter
  {
    public void Execute(IReadOnlyList<ICable> cableList)
    {
      foreach ( var cable in cableList ) {
        cable.ClearPoints() ;
        /* ケーブルパスを無視して、最短でつなぐ
        cable.Add(cable.From) ;
        cable.Add(cable.To) ;
        */
      }
    }

    public IList<BranchPath> GetOrCreateBranchPath(IList<ICable> cableList, IList<ICablePath> initCablePathList)
    {
      IList<BranchPath> rtn = new List<BranchPath>();
      
      // 端点からケーブルトレイのパスを作成
      var cablePathGroup = new CablePathGroup(initCablePathList) ;
      foreach (var terminalPoint in cableList.Select(cbl => cbl.From)) {
        rtn.Add( cablePathGroup.GetOrCreateBranchFrom(terminalPoint) ) ;
      }
      
      // TODO Toをクラスタリング
      
      // Toからからケーブルトレイパスを生成
      foreach (var terminalPoint in cableList.Select(cbl => cbl.To)) {
        rtn.Add( cablePathGroup.GetOrCreateBranchFrom(terminalPoint) ) ;
      }

      return rtn;
    }
    
    public CablePathConnected Connect(IReadOnlyList<ICable> cableList, IReadOnlyList<ICablePath> cablePath)
    {
      return null;
    }
  }
}