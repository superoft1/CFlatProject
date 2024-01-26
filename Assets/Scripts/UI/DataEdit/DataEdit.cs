using System ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using System.Linq;
using Chiyoda.CAD.Model.Structure ;
using UnityEngine;

namespace Chiyoda.UI
{
  public class DataEdit : MonoBehaviour
  {
    [SerializeField]
    TreeView treeView = null;

    public void Duplicate()
    {
      var curDoc = DocumentCollection.Instance.Current;
      if ( curDoc == null ) {
        return;
      }

      using ( var storage = new CopyObjectStorage() ) {
        foreach ( var item in ((TreeViewItemChildrenArea)treeView.Items).GetSelectedAllDescendants().Where( IsRootChild ) ) {
          switch ( item.GetTreeViewItemSource() ) {
            case Edge edge: {
                var cloned = edge.Clone( storage );
                var lc = cloned.GlobalCod ;
                var delta = (edge.GetGlobalBounds().Value.size.x + 1d) * Vector3d.right;
                cloned.LocalCod = new LocalCodSys3d( lc.Origin + delta, lc );
                curDoc.AddEdge( cloned );
                curDoc.SelectElement( cloned );
              }
              break;

            case CAD.Model.Electricals.Electricals electricalEntity :
              throw new NotImplementedException();
              break ;
            
            case CAD.Model.Structure.CommonEntities.PlacementEntity structure: {
                var cloned = structure.Clone( storage );
                var lc = cloned.LocalCod;
                var delta = (structure.GetGlobalBounds().Value.size.x + 1d) * Vector3d.right;
                cloned.LocalCod = new LocalCodSys3d( lc.Origin + delta, lc );
                curDoc.Structures.Add( cloned );
                curDoc.SelectElement( cloned );
              }
              break;

            default: break;
          }
        }
      }
    }

    public void Delete()
    {
      var curDoc = DocumentCollection.Instance.Current;
      if ( curDoc == null ) {
        return;
      }

      foreach ( var item in ( (TreeViewItemChildrenArea) treeView.Items ).GetSelectedAllDescendants().Where( IsRootChild ) ) {
        switch ( item.GetTreeViewItemSource() ) {
          case Line line :
            curDoc.RemoveLine( line.LineId, true ) ;
            break ;

          case HydraulicStream stream :
            curDoc.Streams.Remove( stream ) ;
            break ;

          case Equipment instrument :
            // 未対応
            break ;

          case CAD.Model.Electricals.Electricals electricalEntity :
            curDoc.Electrical.Remove( electricalEntity ) ;
            break ;

          case CAD.Model.Structure.CommonEntities.PlacementEntity structure :
            curDoc.Structures.Remove( structure ) ;
            break ;

          case Edge edge :
            if ( null == edge.Parent?.Closest<BlockEdge>() ) {
              edge.Unlink() ;
            }
            break ;
        }
      }
    }

    /// <summary>
    /// Rootの直接の子か？（孫は対象外)
    /// </summary>
    /// <param name="tv"></param>
    /// <returns></returns>
    private bool IsRootChild( TreeViewItem tv )
    {
      int generation = 0;
      for ( var p = tv.Parent ; p != null ; p = p.Parent ) {
        ++generation;
      }

      return generation == 1;
    }
  }
}
