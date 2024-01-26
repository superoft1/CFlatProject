using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class EdgeSubPresenter : SubPresenter<Edge>
    {
      public EdgeSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter ) { }

      protected override bool IsRaised( Edge edge )
      {
        return (null != TopologyObjectMap.GetEdgeObject( edge ));
      }

      protected override void Raise( Edge edge )
      {
        var go = new GameObject();
        go.name = edge.Name;

        var edgeObject = go.AddComponent<EdgeObject>();
        edgeObject.Edge = edge;
        TopologyObjectMap.Add( edge, edgeObject );

        go.transform.SetParent( RootGameObject.transform );
      }

      protected override void Update( Edge edge )
      {
        var edgeObject = TopologyObjectMap.GetEdgeObject( edge );
        if ( null == edgeObject ) return;

        var go = edgeObject.gameObject;
        var parentObject = TopologyObjectMap.GetEdgeObject( edge.Group as Edge );
        if ( edgeObject.Parent != parentObject ) {
          if ( null != parentObject ) {
            go.transform.SetParent( parentObject.transform );
          }
          else {
            go.transform.SetParent( RootGameObject.transform );
          }

          edgeObject.Parent = parentObject;
          if ( null != parentObject ) {
            parentObject.Children.Add( edgeObject );
          }
        }

        go.transform.SetLocalCodSys( edge.LocalCod ) ;
      }

      protected override void TransformUpdate( Edge edge )
      {
        var edgeObject = TopologyObjectMap.GetEdgeObject( edge );
        if ( null == edgeObject ) return ;
        
        var go = edgeObject.gameObject;
        go.transform.SetLocalCodSys( edge.LocalCod ) ;
      }

      protected override void Destroy( Edge edge )
      {
        var edgeObject = TopologyObjectMap.GetEdgeObject( edge );
        if ( null == edgeObject ) return;

        var parentObject = edgeObject.Parent;
        if ( null != parentObject ) {
          parentObject.Children.Remove( edgeObject );
          edgeObject.Parent = null;
        }

        edgeObject.transform.SetParent( null );
        TopologyObjectMap.Remove( edge );
        GameObject.Destroy( edgeObject.gameObject );
      }
    }
  }
}