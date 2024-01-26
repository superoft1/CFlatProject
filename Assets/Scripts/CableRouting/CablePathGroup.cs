using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CableRouting.Math ;

namespace Chiyoda.CableRouting
{
  public class CablePathGroup
  {
    private IList<ICablePath> PRackList { get ; }= new List<ICablePath>();
    private IList<ICablePath> BranchList { get ; } = new List<ICablePath>();
    public CablePathGroup(IList<ICablePath> pipeRackList)
    {
      PRackList = pipeRackList ;
    }
    
    // TODO リファクタリング
    public BranchPath GetOrCreateBranchFrom(Vector3d terminalPoint)
    {
      // TODO 同一の終点に対し、既に存在していたら、作成しない
      {}
      
      if (!PRackList.Any()) return null ;
      
      var pointOnPipeRackPathList = new List<Vector3d>() ;
      foreach ( var pipeRackPath in PRackList ) {
        var rect = pipeRackPath.Rect ;
        pointOnPipeRackPathList.Add(rect.GetClosedTo(terminalPoint)) ;
      }
      // 最も近い点を１つだけ取得
      var closedPoint = pointOnPipeRackPathList.OrderBy( pnt => ( pnt - terminalPoint ).magnitude ).First() ;

      // パイプラックパス上の点と自身をつなぐパスを生成する(z=0)
      var branchPath = new BranchPath(closedPoint, terminalPoint);
      BranchList.Add( branchPath );
      return branchPath ;
    }
  }
}