using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chiyoda.UI
{
  public class DataEditAutoRouting : MonoBehaviour
  {
    [SerializeField] ModalDialog ErrorDialog;
    
    [SerializeField]
    TreeView treeView = null;

    [SerializeField] 
    VtpAutoRouting.AutoRoutingMgr routingMgr = null;

    #region OnClick
    // 選択されたEntityからルートを作る
    public void CreateRoute()
    {
      var curDoc = DocumentCollection.Instance.Current ;
      if ( curDoc == null ) return ;

      var selectedEntities = curDoc.SelectedElements.ToList() ;
      if ( selectedEntities.Count() < 2 ) {
        ErrorDialog.Show("Route create error", "Vertices are insufficient.\nPrepare for both Suction and Discharge directions." ) ;
        return ;
      }
      
      // Vertex格納用
      var vertEntityTypeMap = new Dictionary<HalfVertex, HalfVertex.FlowType>() ;

      // LeafEdgeからVertexを取得
      foreach ( var entity in selectedEntities ) {
        switch ( entity ) {
          case Nozzle nz :
            if ( entity.Parent is Equipment eq ) {
              AddMap( eq, nz, nz.NozzleNumber, vertEntityTypeMap ) ;
            } else if (entity.Parent is NozzleArray array) {
              eq = array.Parent as Equipment ;
              AddMap( eq, nz, array.ConnectPointIndex(nz), vertEntityTypeMap ) ;
            }
            break ;
          case Chiyoda.CAD.Model.Component comp :
          {
            var freeVertices = comp.LeafEdge.GetFreeVertex().ToList() ;
            if ( ! freeVertices.Any() || freeVertices.Count != 1 ) {
              ErrorDialog.Show( "Route create error", "Free vertex does not exist.\nSelect more than 2 vertices." ) ;
              return ;
            }
            var freeVertex = freeVertices[ 0 ] ;
            // 未追加の場合
            if ( ! vertEntityTypeMap.ContainsKey( freeVertex ) ) {
              vertEntityTypeMap.Add( freeVertex, freeVertex.Flow ) ;
            }

            // 独立配管の場合
            AddIndependentOtherVertices( freeVertex, vertEntityTypeMap ) ;
          }
            break ;

          case HalfVertex freeVertex:
            if (!vertEntityTypeMap.ContainsKey(freeVertex))
            {
              // TODO: halfvertexを表示している時点で流れ方向をも設定されているべきと思うが、とりあえずここで設定
              if ( freeVertex.Flow == HalfVertex.FlowType.Undefined ) {
                if ( freeVertex.LeafEdge?.PipingPiece is Equipment equip ) {
                  var nzl = equip.GetNozzle( freeVertex.ConnectPointIndex ) ;
                  if ( nzl != null ) {
                    freeVertex.Flow = nzl.NozzleType == Nozzle.Type.Discharge
                      ? HalfVertex.FlowType.FromThisToAnother
                      : HalfVertex.FlowType.FromAnotherToThis ;
                  }
                }         
              }
              vertEntityTypeMap.Add(freeVertex, freeVertex.Flow);
            }

            // 独立配管の場合
            AddIndependentOtherVertices( freeVertex, vertEntityTypeMap ) ;
            break;
        }
      }

      if ( ! CheckVerticesFlow(vertEntityTypeMap) ) {
        ErrorDialog.Show("Route create error", "Vertices are insufficient.\nPrepare for both Suction and Discharge directions." ) ;
        return ;
      }

      var route = routingMgr.CreateRoute( curDoc, vertEntityTypeMap ) ;
      curDoc.SelectElement( route ) ;
    }

    private bool CheckVerticesFlow(Dictionary<HalfVertex, HalfVertex.FlowType> vertEntityTypeMap)
    {
      // SuctionとDischargeの両方の向きがある場合 true
      if ( vertEntityTypeMap.ContainsValue( HalfVertex.FlowType.FromAnotherToThis ) &&
           vertEntityTypeMap.ContainsValue( HalfVertex.FlowType.FromThisToAnother ) ) {
        return true;
      }

      return false;
    }

    void AddMap(Equipment eq, Nozzle nz, int cpIndex, Dictionary<HalfVertex, HalfVertex.FlowType> vertEntityTypeMap)
    {
      var freeVertex = eq.LeafEdge.GetVertex( cpIndex ) ;
      if ( freeVertex.Partner != null ) {
        ErrorDialog.Show( "Route create error", "Free vertex does not exist.\nSelect more than 2 vertices." ) ;
        return ;
      }
      // Suction，DischargeをもとにType設定
      var type = nz.NozzleType == Nozzle.Type.Suction ? HalfVertex.FlowType.FromAnotherToThis : HalfVertex.FlowType.FromThisToAnother ;
      vertEntityTypeMap.Add( freeVertex, type ) ;
    }

    private void AddIndependentOtherVertices(HalfVertex freeVertex, Dictionary<HalfVertex, HalfVertex.FlowType> vertEntityTypeMap)
    {
      // ペアを検索してRouteに自動追加
      var blockEdge = freeVertex.Closest<CompositeBlockPattern>() ?? (BlockEdge) freeVertex.Closest<BlockPattern>() ;
      if ( blockEdge == null ) return ;
      foreach ( var v in blockEdge.Vertices.Where( x => x.Name == freeVertex.Name ) ) {
        // 同名 & 未追加の場合に追加
        if ( ! vertEntityTypeMap.ContainsKey( v ) ) {
          vertEntityTypeMap.Add( v, v.Flow ) ;
        }
      }
    }

    // ツリー上で選択されているルートに対し自動ルーティングを作成
    public void AutoRouting()
    {
      routingMgr.Execute( DocumentTreeView.Instance().GetSelectedItem<Route>() );
    }

    // 結果の削除（未実装）
    public void DeleteResult()
    {
//      throw new System.NotImplementedException();
//      routingMgr.DeleteResult(DocumentTreeView.Instance().GetSelectedItem<Route>());
    }

    public void AllAutoRouting()
    {
      VtpAutoRouting.AutoRoutingMgr.Instance().Execute();
    }

    public void DeleteAllResult()
    {
      VtpAutoRouting.AutoRoutingMgr.Instance().DeleteResult();
    }
    
    #endregion
    
    /* 表示/非表示関係は別で実装中
    public void HideRoute()
    {
      var selected = ((TreeViewItemChildrenArea)treeView.Items).GetSelectedAllDescendants()
                                                               .Where(item => item.GetTreeViewItemSource() is Route);
      if (treeView.Items.Count() > 0)
      {
        foreach (var item in treeView.Items.First().Items)
        {
          if (selected.Contains(item)) continue;
          if (item.GetTreeViewItemSource() is Route route)
          {
            route.IsVisible = false;
          }
        }
      }
    }
    */
  }
}
