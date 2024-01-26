using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Body ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using JetBrains.Annotations ;
using UnityEngine ;
using UnityEngine.EventSystems ;

namespace Chiyoda.UI
{
  /// <summary>
  /// ドラッグ＆ドロップ可能な機器アイコン
  /// </summary>
  public abstract class DragPlacementIcon<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    where T : Entity, IPlacement
  {
    private Camera _mainCamera ;

    private TreeViewMode? _orgTreeViewMode ;
    private T _dragElement ;
    private List<IRegion> _regions ;

    private RegionBody _highlightingRegionBody = null ;
    private GameObject _dragger ;

    public void Start()
    {
      _mainCamera = GameObject.Find( "MainCamera" )?.GetComponent<Camera>() ;
    }

    private void SetupUI( IElement element )
    {
      if ( null == _orgTreeViewMode ) {
        _orgTreeViewMode = PresenterManager.Instance.TreeViewItemPresenter.Mode ;
        PresenterManager.Instance.TreeViewItemPresenter.Mode = TreeViewMode.BasicView ;
      }

      DocumentTreeView.Instance().Target = element ;
    }

    private void UnsetupUI()
    {
      if ( null == _orgTreeViewMode ) return ;
      PresenterManager.Instance.TreeViewItemPresenter.Mode = _orgTreeViewMode.Value ;
      DocumentTreeView.Instance().Target = DocumentTreeView.Instance().Target.Document ;
      _orgTreeViewMode = null ;

      CloseSubmenuView() ;
      CloseDialog() ;
    }

    protected abstract void CreateInitialElement( Document document, Action<T> onFinish ) ;

    protected void UpdateElement( T placement )
    {
      if ( null != _dragElement ) {
        placement.LocalCod = _dragElement.LocalCod ;
        RemoveFromParent( _dragElement ) ;
      }

      _dragElement = placement ;
      SetupUI( _dragElement ) ;
    }

    protected void RemoveDragElement()
    {
      if ( null == _dragElement ) return;
      RemoveFromParent( _dragElement ) ;
    }

    protected void RotateDragElement( bool bPlusRotate )
    {
      if ( ! ( _dragElement is Edge edge ) ) return ;

      var angle = ( bPlusRotate ) ? 90 : -90 ;
      edge.ExtraHorizontalRotationDegree += angle ;
    }

    protected abstract void RemoveFromParent( T placement ) ;

    void IBeginDragHandler.OnBeginDrag( PointerEventData data )
    {
      // 左ボタン以外は無視
      if ( data.pointerId != -1 ) return ;

      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;
      if ( null != _highlightingRegionBody ) _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.None ;

      // ドラッグ開始時に全可視Regionを取得しておく
      _regions = new List<IRegion>( curDoc.Regions ) ;
      foreach ( var wp in curDoc.WorkPlanes ) {
        if ( wp.IsVisible ) {
          _regions.AddRange( wp.Regions ) ;
        }
      }

      // ドラッグ用要素を作成
      _dragger = new GameObject() ;
      _dragger.transform.SetParent( gameObject.transform ) ;
      _dragger.transform.position = data.position ;

      // ドラッグ開始時にドラッグ要素を作成
      CreateInitialElement( DocumentCollection.Instance.Current, elm =>
      {
        UpdateElement( elm ) ;
        ( (IDragHandler) this ).OnDrag( data ) ;
      } ) ;
    }

    void IDragHandler.OnDrag( PointerEventData data )
    {
      // 左ボタン以外は無視
      if ( data.pointerId != -1 ) return ;

      if ( null != _highlightingRegionBody ) _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.None ;

      if ( _dragElement == null ) return ;

      _dragger.transform.position = data.position ;

      if ( ! MouseUtil.IsMouseOnMainView() ) return ;

      var hitInfo = RayCastRegionFromMousePosition( _mainCamera, _dragElement?.Document, _regions ) ;
      _dragElement.LocalCod = new LocalCodSys3d( _dragElement.ParentCod.LocalizePoint( hitInfo.HitPos ), _dragElement.LocalCod ) ;
      if ( hitInfo.Region == null ) return ;

      BodyMap.Instance.TryGetBody( hitInfo.Region as Entity, out var body ) ;
      _highlightingRegionBody = body as RegionBody ;
      if ( _highlightingRegionBody == null ) return ;

      _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.Enabled ;
    }

    void IEndDragHandler.OnEndDrag( PointerEventData data )
    {
      // 左ボタン以外は無視
      if ( data.pointerId != -1 ) return ;

      if ( null != _highlightingRegionBody ) _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.None ;
      _highlightingRegionBody = null ;

      if ( _dragElement == null ) return ;

      Destroy( _dragger ) ;

      CloseSubmenuView() ;

      if ( ! MouseUtil.IsMouseOnMainView() ) {
        CancelPlacement() ;
        return ;
      }

      var hitInfo = RayCastRegionFromMousePosition( _mainCamera, _dragElement.Document, _regions ) ;
      _dragElement.LocalCod = new LocalCodSys3d( _dragElement.ParentCod.LocalizePoint( hitInfo.HitPos ), _dragElement.LocalCod ) ;

      ShowDialog() ;
    }

    protected virtual void ShowDialog()
    {
      // デフォルトでは配置を完了する
      FixPlacement() ;
    }

    protected virtual void CloseDialog()
    {
    }

    protected void CancelPlacement()
    {
      if ( _dragElement == null ) return ;

      if ( null != _highlightingRegionBody ) _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.None ;
      _highlightingRegionBody = null ;

      UnsetupUI() ;

      _dragElement.Document.History.Cancel() ;
      _dragElement = null ;
      _regions = null ;
    }

    protected void FixPlacement()
    {
      if ( _dragElement == null ) return ;

      if ( null != _highlightingRegionBody ) _highlightingRegionBody.RegionVisibility = RegionBody.BodyVisibility.None ;
      _highlightingRegionBody = null ;

      UnsetupUI() ;

      _dragElement.Document.MaintainEdgePlacement() ;
      _dragElement.Document.HistoryCommit() ;
      _dragElement.Document.SelectElement( _dragElement );
      _dragElement = null ;
      _regions = null ;
    }

    private void CloseSubmenuView()
    {
      GetComponentInParent<SubmenuView>()?.Hide() ;
    }

    /// <summary>
    /// マウス座標からレイを飛ばして一番近くのRegionを返す
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="regions"></param>
    /// <returns>衝突したRegionを返す。衝突してなければnullを返す</returns>
    [NotNull]
    private static RegionHitInfo RayCastRegionFromMousePosition( Camera camera, Document doc, IEnumerable<IRegion> regions )
    {
      RegionHitInfo nearInfo = null ;

      // レイの座標と向きを求める
      RayCastUtil.CreateRayFromMousePosition( camera, out var rayPos, out var rayDir ) ;

      // 全Regionとの衝突チェック
      foreach ( var region in regions ) {
        var hitInfo = region.HitTest( rayPos, rayDir ) ;
        if ( hitInfo != null && ( nearInfo == null || hitInfo.HitLength < nearInfo.HitLength ) ) {
          nearInfo = hitInfo ;
        }
      }

      if ( null == nearInfo ) {
        // Document外であれば、画面上でDocumentと最も近い場所を求める
        if ( null == doc ) doc = DocumentCollection.Instance.Current ;
        Vector3 c = doc.Area.Center, s = doc.Area.Size ;

        nearInfo = new RegionHitInfo( null, NearestPos( camera, doc.Area ), 0 ) ;
      }

      return nearInfo ;
    }

    private static Vector3 NearestPos( Camera camera, SquareRegion square )
    {
      Vector3 mousePos = Input.mousePosition ;
      float baseZ = (float)square.GetBaseHeight() ;
      Vector3 c = square.Center, s = square.Size ;
      var points = new[]
      {
        camera.WorldToScreenPoint( new Vector3( c.x - s.x * 0.5f, c.y - s.y * 0.5f, baseZ ) ),
        camera.WorldToScreenPoint( new Vector3( c.x - s.x * 0.5f, c.y + s.y * 0.5f, baseZ ) ),
        camera.WorldToScreenPoint( new Vector3( c.x + s.x * 0.5f, c.y + s.y * 0.5f, baseZ ) ),
        camera.WorldToScreenPoint( new Vector3( c.x + s.x * 0.5f, c.y - s.y * 0.5f, baseZ ) ),
      } ;

      int minIndex = 3 ;
      var minLength2 = GetDistanceSquared( mousePos, points[ 3 ], points[ 0 ], out var minT ) ;
      for ( int i = 0 ; i < 3 ; ++i ) {
        var dist2 = GetDistanceSquared( mousePos, points[ i ], points[ i + 1 ], out var t ) ;
        if ( dist2 < minLength2 ) {
          minT = t ;
          minIndex = i ;
          minLength2 = dist2 ;
        }
      }

      return Vector3.Lerp( points[ minIndex ], points[ ( minIndex + 1 ) % 4 ], minT ) ;
    }

    private static float GetDistanceSquared( Vector3 p, Vector3 from, Vector3 to, out float t )
    {
      p -= from ;
      to -= from ;
      var len2 = to.sqrMagnitude ;
      if ( len2 < Tolerance.FloatEpsilon * Tolerance.FloatEpsilon ) {
        t = 0 ;
        return p.sqrMagnitude ;
      }

      t = Vector3.Dot( p, to ) / len2 ;
      return ( p - t * to ).sqrMagnitude ;
    }
  }
}