using System.IO ;
using UnityEditor ;
using UnityEngine ;

#if UNITY_EDITOR
public class CustomMenueScript
{
  /// <summary>
  /// LeafEdgeのローカル座標
  /// </summary>
  [MenuItem( "CustomMenu/Develop/WriteSelectedLeafEdgeLocal" )]
  static void DevelopLocal()
  {
    foreach ( Transform transform in Selection.transforms ) {
      var t = transform ;
      while ( t.name != "LeafEdge" ) {
        t = t.parent ;
      }

      var child = t.GetChild( 0 ) ;
      var parent = child.parent ;
      var path = Application.dataPath + "/outputs/selected_leafedge.txt" ;
      var localPosition = parent.localPosition ;
      var pos = $"{child.name}, {localPosition.x}, {localPosition.y}, {localPosition.z}\n" ;
      File.AppendAllText( path, pos ) ;
    }
  }

  /// <summary>
  /// 所属している親のブロックパターンからの相対座標
  /// </summary>
  [MenuItem( "CustomMenu/Develop/WriteSelectedLeafEdgeRelative" )]
  static void DevelopRelative()
  {
    foreach ( Transform transform in Selection.transforms ) {
      var t = transform ;
      while ( t.name != "LeafEdge" ) {
        t = t.parent ;
      }

      var child = t.GetChild( 0 ) ;
      var leafEdge = child.parent ;
      var idfBlock = leafEdge.parent ;
      var block = idfBlock.parent ;
      var path = Application.dataPath + "/outputs/selected_leafedge.txt" ;
      var globalPosition = leafEdge.position ;
      var relativePosition = block.transform.InverseTransformPoint( globalPosition ) ;
      var pos = $"{child.name}, {relativePosition.x}, {relativePosition.y}, {relativePosition.z}\n" ;
      File.AppendAllText( path, pos ) ;
    }
  }

  /// <summary>
  /// 所属している親のブロックパターンからの相対座標
  /// </summary>
  [MenuItem( "CustomMenu/Develop/LeafEdgeProperty" )]
  static void DevelopLeafEdgeProperty()
  {
    foreach ( Transform transform in Selection.transforms ) {
      var t = transform ;
      var edgeObject = t.GetComponent<EdgeObject>() ;
      if ( edgeObject == null ) {
        edgeObject = t.parent.GetComponent<EdgeObject>() ;
      }
      if ( edgeObject == null ) {
        edgeObject = t.parent.parent.GetComponent<EdgeObject>() ;
      }
      if ( edgeObject != null ) {
        Debug.Log( "Edge Name is " + edgeObject.Edge.ObjectName ) ;
      }
    }
  }

  /// <summary>
  /// BlockPattern作成用に、所属しているグループ内で選択LeafEdgeが何番目かをログに出力
  /// </summary>
  [MenuItem( "GameObject/BlockPattern/LeafEdge Index", false, 0 )]
  private static void LeafEdgeIndex()
  {
    foreach ( Transform transform in Selection.transforms ) {
      var t = transform ;
      while ( t.name != "LeafEdge" ) {
        t = t.parent ;
      }
      var child = t.GetChild( 0 ) ;
      var parent = t.parent ;
      for ( int i = 0 ; i < parent.childCount ; ++i ) {
        if ( child != parent.GetChild( i ).GetChild( 0 ) ) continue ;
        Debug.Log($"{child.name} index = {i} in {parent.name}"  );
        break ;
      }
    }
  }
}

#endif
