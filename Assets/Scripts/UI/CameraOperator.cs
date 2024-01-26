using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  public class CameraOperator : MonoBehaviour
  {
    private enum CameraDirection
    {
      XPlus,
      YPlus,
      XMinus,
      YMinus,
      ZPlus,  // Zは上のみ考慮
    }

    private const float BASE_SIZE = 5f;
    private static readonly float ISOMETRIC_VERT_ANGLE = Mathf.Atan( Mathf.Sqrt( 0.5f ) ) * Mathf.Rad2Deg;

    private InputListenerManager _inputListenerManager = null;
    private float _horzAngle = 0;
    private float _vertAngle = 0;
    private float _logSize = 0;
    private Bounds? _bounds = null;
    private Animator _animator = null;
    private Vector2 _rightMouseDownPosition;

    [SerializeField]
    private RectTransform _headerArea;
    [SerializeField]
    private RectTransform _footerArea;
    [SerializeField]
    private RectTransform _leftSideArea;
    [SerializeField]
    private RectTransform _rightSideArea;
    [SerializeField]
    private SizeAdjuster _leftSizeAdjuster;
    [SerializeField]
    private SizeAdjuster _rightSizeAdjuster;
    [SerializeField]
    private bool _limitedRotation = true;
    [SerializeField]
    private GameObject _contextMenu;

    public Camera TargetCamera;
    public Camera PickableCamera;

    private Transform targetCameraTransform;
    private Transform pickableCameraTransform;
    
    public float CameraDistance { get; set; } = 5000;
    public float MinLog { get; set; } = -100;
    public float MaxLog { get; set; } = 200;
    
    private void Start()
    {
      _inputListenerManager = CreateListenerManager();

      TargetCamera.orthographic = true;
      TargetCamera.nearClipPlane = 0;
      TargetCamera.farClipPlane = CameraDistance * 2;
      TargetCamera.transform.position -= TargetCamera.transform.forward * CameraDistance;

      PickableCamera.orthographic = true;
      PickableCamera.nearClipPlane = 0;
      PickableCamera.farClipPlane = CameraDistance * 2;
      PickableCamera.transform.position -= PickableCamera.transform.forward * CameraDistance;

      targetCameraTransform = TargetCamera.transform;
      pickableCameraTransform = PickableCamera.transform;

      ElementDragManager.Instance.DraggerCamera = TargetCamera;

      SetDirection( 0, 90, false );
      UpdateZoom();
    }

    private InputListenerManager CreateListenerManager()
    {
      var manager = new InputListenerManager();

      manager.AddListener( CreateMouseDragger() );
      manager.AddListener( CreateMouseZoomer() );
      manager.AddListener( CreateMouseRotater() );

      return manager;
    }

    private InputListener CreateMouseDragger()
    {
      // 左ドラッグによる表示範囲移動
      var dragger = new MouseDragListener( 0, this, TargetCamera )
      {
        ShiftKeyState = ModifierKeyState.Up,
        CtrlCmdKeyState = ModifierKeyState.Up,
        AltOptionKeyState = ModifierKeyState.Up,
      };
      dragger.Update += ( sender, e ) =>
      {
        if ( ElementDragManager.Instance.IsDragging ) {
          ElementDragManager.Instance.Drag( e.CurrentPos - e.LastPos );
        }
        else {
          TargetCamera.transform.position -= (e.CurrentPos - e.StartPos);
        }
      };
      return dragger;
    }
    private InputListener CreateMouseZoomer()
    {
      // Ctrl＋右ドラッグによる表示サイズ変更
      var zoomer = new MouseDragListener( 1, this )
      {
        ShiftKeyState = ModifierKeyState.Up,
        CtrlCmdKeyState = ModifierKeyState.Down,
        AltOptionKeyState = ModifierKeyState.Up,
      };
      zoomer.Update += ( sender, e ) =>
      {
        var zoomRatio = (e.CurrentPos.y - e.LastPos.y) / (Screen.height / 4); // 高さの1/4を移動するごとに倍率2倍
        _logSize -= zoomRatio * 30.103f;  //  log_10(2) * 100%
        UpdateZoom();
      };
      return zoomer;
    }
    private InputListener CreateMouseRotater()
    {
      // 右ドラッグによる回転
      var rotater = new MouseDragListener( 1, this )
      {
        ShiftKeyState = ModifierKeyState.Up,
        CtrlCmdKeyState = ModifierKeyState.Up,
        AltOptionKeyState = ModifierKeyState.Up,
      };
      rotater.Update += ( sender, e ) =>
      {
        if ( _limitedRotation )
        {
          var rotX = (e.CurrentPos.x - e.LastPos.x) / (Screen.height / 2); // 高さの半分を移動するごとに90度X回転
          var rotY = (e.CurrentPos.y - e.LastPos.y) / (Screen.height / 2); // 高さの半分を移動するごとに90度Y回転
          _horzAngle -= rotX * 90;
          _vertAngle -= rotY * 90;
          TargetCamera.transform.position += TargetCamera.transform.forward * CameraDistance;
          UpdateDirection();
          TargetCamera.transform.position -= TargetCamera.transform.forward * CameraDistance;
        }
        else
        {
          Vector3 diff = e.CurrentPos - e.LastPos;
          Vector3 axis = Vector3.Cross( Vector3.forward, diff );
          float angle = Mathf.Atan2( diff.magnitude, Screen.height / 2 );
          TargetCamera.transform.position += TargetCamera.transform.forward * CameraDistance;
          TargetCamera.transform.rotation *= Quaternion.AngleAxis( Mathf.Rad2Deg * angle, axis );
          TargetCamera.transform.position -= TargetCamera.transform.forward * CameraDistance;

          Matrix4x4 mat = Matrix4x4.Rotate( TargetCamera.transform.rotation );
          _horzAngle = Mathf.Rad2Deg * Mathf.Atan2( -mat.m10, mat.m00 );
          _vertAngle = Mathf.Rad2Deg * Mathf.Atan2( -mat.m22, mat.m21 );
        }
      };
      return rotater;
    }

    private void OnDestroy()
    {
      _inputListenerManager = null;
    }

    private void Update()
    {
      _inputListenerManager.Listen();

      if ( null != _animator ) {
        _animator.Update();
        if ( _animator.IsFinished ) {
          _animator = null;
        }
        return;
      }

      if (ModalDialog.IsOpened)
      {
        return;
      }

      if ( Input.touchCount == 2 ) {
        Touch touchZero = Input.GetTouch( 0 );
        Touch touchOne = Input.GetTouch( 1 );

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        SetZoom( difference * 0.01f );
      }

      if (MouseUtil.IsMouseInScreen())
      {
        // マウスホイールでズーム
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (!Mathf.Approximately(mouseScroll, 0f) && !MouseUtil.IsMouseOnScrollView())
        {
          SetZoom(mouseScroll * 25f);
        }

        // コンテキストメニューの表示非表示
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
          // コンテキストメニュー外で左クリックしたら非表示
          if (_contextMenu.activeSelf && !MouseUtil.IsMouseOnContextMenu())
          {
            _contextMenu.SetActive(false);
          }
        }
        else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(2))
        {
          // 左ボタンと中ボタンを押してない場合のみ右ボタンでコンテキストメニュー表示
          if (Input.GetMouseButtonDown(1))
          {
            // 右ボタンを押したら座標を覚えておく。表示中だったらいったん非表示にする。
            _rightMouseDownPosition = Input.mousePosition;
            _contextMenu.SetActive(false);
          }
          else if (Input.GetMouseButtonUp(1) && MouseUtil.IsMouseInContextMenuPopupRect())
          {
            // 右ボタンを押した場所からあまり動いてなければコンテキストメニューを表示する
            if (((Vector2) Input.mousePosition - _rightMouseDownPosition).sqrMagnitude < 1f)
            {
              _contextMenu.SetActive(true);
            }
          }
        }
      }

      // Pickableカメラをメインカメラに同期させる
      pickableCameraTransform.position = targetCameraTransform.position;
      pickableCameraTransform.localScale = targetCameraTransform.localScale;
      pickableCameraTransform.localRotation = targetCameraTransform.localRotation;
      PickableCamera.orthographicSize = TargetCamera.orthographicSize;
    }

    public void SetBoundary( Bounds? bounds )
    {
      if ( null == bounds ) return;

      FitTo( TargetCamera, bounds.Value );
    }

    private void FitTo( Camera camera, Bounds bounds )
    {
      _bounds = bounds;

      var uiSize = GetUISize();
      var extent = GetCurrentExtent( TargetCamera, bounds );

      float screenWidth = Screen.width, screenHeight = Screen.height, viewWidth = screenWidth - uiSize.x * 2, viewHeight = screenHeight - uiSize.y * 2;

      float size = Mathf.Max( extent.y, extent.x * viewHeight / viewWidth ) * (screenHeight / viewHeight) * 1.05f;  // 表示範囲は1.05倍

      var orgLogSize = _logSize;
      var newLogSize = Mathf.Log10( size / BASE_SIZE ) * 100;
      newLogSize = Mathf.Clamp( newLogSize, MinLog, MaxLog );

      var orgPos = camera.transform.position;
      var newPos = bounds.center - camera.transform.forward * CameraDistance;
      SetupAnimator( t =>
      {
        camera.transform.position = Vector3.Lerp( orgPos, newPos, t );
        _logSize = Mathf.Lerp( orgLogSize, newLogSize, t );
        UpdateZoom();
      }, 0.25f );
    }

    private static Vector3 GetCurrentExtent( Camera camera, Bounds bounds )
    {
      var transform = camera.transform;
      var extent = bounds.extents;

      var xDir = transform.TransformVector( extent.x, 0, 0 );
      var yDir = transform.TransformVector( 0, extent.y, 0 );
      var zDir = transform.TransformVector( 0, 0, extent.z );

      return new Vector3(
        Mathf.Abs( xDir.x ) + Mathf.Abs( yDir.x ) + Mathf.Abs( zDir.x ),
        Mathf.Abs( xDir.y ) + Mathf.Abs( yDir.y ) + Mathf.Abs( zDir.y ),
        Mathf.Abs( xDir.z ) + Mathf.Abs( yDir.z ) + Mathf.Abs( zDir.z )
      );
    }

    public void SetDirectionXPlus()
    {
      SetDirection( 270, 0 );
    }
    public void SetDirectionXMinus()
    {
      SetDirection( 90, 0 );
    }
    public void SetDirectionYPlus()
    {
      SetDirection( 180, 0 );
    }
    public void SetDirectionYMinus()
    {
      SetDirection( 0, 0 );
    }
    public void SetDirectionZPlus()
    {
      var angle = ( TargetCamera.transform.rotation.eulerAngles.z + 180 ) % 360;
      var dir = 0;
      for (int d = 45; d < 360; d += 90)
      {
        if (angle < d)
        {
          dir = d - 45;
          break;
        }
      }
      SetDirection( dir, 90 );
    }
    public void SetDirectionZMinus()
    {
      SetDirection( 0, -90 );
    }
    public void SetDirectionNearestIsometric()
    {
      var newHorzAngle = Mathf.Floor( _horzAngle / 90f ) * 90f + 45f;
      var newVertAngle = ISOMETRIC_VERT_ANGLE;
      SetDirection( newHorzAngle, newVertAngle );
    }

    private void SetDirection( float horzAngle, float vertAngle, bool withAnimator = true )
    {
      var orgHorzAngle = _horzAngle;
      var orgVertAngle = _vertAngle;

      if ( orgHorzAngle + 180 < horzAngle ) horzAngle -= 360;
      else if ( orgHorzAngle > horzAngle + 180 ) horzAngle += 360;

      if ( withAnimator ) {
        var center = TargetCamera.transform.position + TargetCamera.transform.forward * CameraDistance;
        var newCenter = _bounds.HasValue ? _bounds.Value.center : Vector3.zero;

        SetupAnimator( t =>
        {
          TargetCamera.transform.position += TargetCamera.transform.forward * CameraDistance;
          _horzAngle = Mathf.Lerp( orgHorzAngle, horzAngle, t );
          _vertAngle = Mathf.Lerp( orgVertAngle, vertAngle, t );
          UpdateDirection();
          TargetCamera.transform.position = Vector3.Lerp( center, newCenter, t );
          TargetCamera.transform.position -= TargetCamera.transform.forward * CameraDistance;
        }, 0.25f );
      }
      else {
        _horzAngle = horzAngle;
        _vertAngle = vertAngle;
        UpdateDirection();
      }
    }

    private void UpdateDirection()
    {
      _horzAngle = (_horzAngle - Mathf.Floor( _horzAngle / 360 ) * 360);
      _vertAngle = Mathf.Clamp( _vertAngle, -90, 90 );

      float th = Mathf.Deg2Rad * -_horzAngle, costh = Mathf.Cos( th ), sinth = Mathf.Sin( th );
      float phi = Mathf.Deg2Rad * _vertAngle, cosphi = Mathf.Cos( phi ), sinphi = Mathf.Sin( phi );
      var dirZ = new Vector3( sinth * sinphi, -costh * sinphi, cosphi );
      var dirY = new Vector3( sinth * cosphi, -costh * cosphi, -sinphi );

      TargetCamera.transform.rotation = Quaternion.LookRotation( dirY, dirZ );
    }

    public void Zoom( float multiplier )
    {
      float logFrom = _logSize;
      float logTo = logFrom - Mathf.Log10( multiplier ) * 100f;

      SetupAnimator( t =>
      {
        _logSize = Mathf.Lerp( logFrom, logTo, t );
        UpdateZoom();
      }, 0.25f );
    }

    private void SetZoom( float increment )
    {
      _logSize -= increment;

      UpdateZoom();
    }

    private void UpdateZoom()
    {
      _logSize = Mathf.Clamp( _logSize, MinLog, MaxLog );

      TargetCamera.orthographicSize = BASE_SIZE * Mathf.Pow( 10, _logSize * 0.01f );
    }

    private void SetupAnimator( Action<float> action, float timeSpan )
    {
      if ( null != _animator ) {
        _animator.Finish();
      }
      _animator = new Animator( action, timeSpan );
    }

    public bool IsInViewArea()
    {
      var rect = GetViewRect();

      return rect.Contains( Input.mousePosition ) && !_leftSizeAdjuster.IsMouseOver && !_rightSizeAdjuster.IsMouseOver;
    }

    private Rect GetViewRect()
    {
      float min_x = 0, min_y = 0, max_x = Screen.width, max_y = Screen.height;
      if ( null != _leftSideArea ) {
        min_x += _leftSideArea.rect.width;
      }
      if ( null != _rightSideArea ) {
        max_x -= _rightSideArea.rect.width;
      }
      if ( null != _footerArea ) {
        min_y += _footerArea.rect.height;
      }
      if ( null != _headerArea ) {
        max_y -= _headerArea.rect.height;
      }

      return new Rect( min_x, min_y, max_x - min_x, max_y - min_y );
    }

    private Vector2 GetUISize()
    {
      float x = 0, y = 0;
      if ( null != _leftSideArea ) {
        x = Mathf.Max( x, _leftSideArea.rect.width );
      }
      if ( null != _rightSideArea ) {
        x = Mathf.Max( x, _rightSideArea.rect.width );
      }
      if ( null != _footerArea ) {
        y = Mathf.Max( y, _footerArea.rect.height );
      }
      if ( null != _headerArea ) {
        y = Mathf.Max( y, _headerArea.rect.height );
      }

      return new Vector2( x, y );
    }



    private class Animator
    {
      private readonly Action<float> _action;
      private readonly Func<float, float> _filter;
      private readonly float _startTime;
      private readonly float _timeSpan;
      private float _t;

      public bool IsFinished
      {
        get { return (1 <= _t); }
      }

      public Animator( Action<float> action, float timeSpan )
        : this( action, timeSpan, null )
      {
      }

      public Animator( Action<float> action, float timeSpan, Func<float, float> filter )
      {
        _action = action;
        _filter = filter;
        _startTime = Time.fixedTime;
        _timeSpan = timeSpan;
        _t = 0;
      }

      public void Update()
      {
        if ( _timeSpan <= 0 ) {
          if ( Time.fixedTime <= _startTime ) {
            _t = 0;
          }
          else {
            _t = 1f;
          }
        }
        else {
          _t = Mathf.Clamp01( (Time.fixedTime - _startTime) / _timeSpan );
        }
        if ( null != _filter ) {
          _action( _filter( _t ) );
        }
        else {
          _action( _t );
        }
      }

      public void Finish()
      {
        if ( null != _filter ) {
          _action( _filter( 1f ) );
        }
        else {
          _action( 1f );
        }
      }
    }
  }
}
