using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chiyoda.UI
{
  class MouseDragEventArgs : EventArgs
  {
    public bool SuspendOtherInputs { get; set; }
    public bool Finished { get; set; }
    public Vector3 StartPos { get; private set; }
    public Vector3 LastPos { get; private set; }
    public Vector3 CurrentPos { get; private set; }

    public MouseDragEventArgs( Vector3 startPos, Vector3 lastPos, Vector3 currentPos )
    {
      StartPos = startPos;
      LastPos = lastPos;
      CurrentPos = currentPos;
    }
  }

  enum ModifierKeyState
  {
    Down,
    Up,
    Any,
  }

  class MouseDragListener : InputListener
  {
    private readonly int _buttonType;
    private readonly CameraOperator _cameraOperator;
    private readonly Camera _worldCamera;
    private bool _isStarted = false;
    private Vector3 _startPos;
    private Vector3 _lastPos;

    public event EventHandler<MouseDragEventArgs> Update;

    protected virtual void OnUpdate( MouseDragEventArgs e )
    {
      if ( null != Update ) Update( this, e );
    }

    public MouseDragListener( int buttonType, CameraOperator cameraOperator ) : this( buttonType, cameraOperator, null ) { }

    public MouseDragListener( int buttonType, CameraOperator cameraOperator, Camera worldCamera )
    {
      _buttonType = buttonType;
      _cameraOperator = cameraOperator;
      _worldCamera = worldCamera;
    }

    public ModifierKeyState ShiftKeyState { get; set; }
    public ModifierKeyState CtrlCmdKeyState { get; set; }
    public ModifierKeyState AltOptionKeyState { get; set; }

    private bool IsShiftInState
    {
      get
      {
        return IsInState( ShiftKeyState, InputKeyModifier.IsShiftDown );
      }
    }
    private bool IsCtrlCmdInState
    {
      get
      {
        return IsInState( CtrlCmdKeyState, InputKeyModifier.IsCtrlOrCmdDown );
      }
    }
    private bool IsAltOptionInState
    {
      get
      {
        return IsInState( AltOptionKeyState, InputKeyModifier.IsAltOrOptionDown );
      }
    }
    private bool IsAllModifierKeysInState
    {
      get { return IsShiftInState && IsCtrlCmdInState && IsAltOptionInState; }
    }

    private static bool IsInState( ModifierKeyState keyState, bool anyDown )
    {
      switch ( keyState ) {
        case ModifierKeyState.Down: return anyDown;
        case ModifierKeyState.Up: return !anyDown;
        default: return true;
      }
    }

    protected override bool IsListenStart
    {
      get
      {
        if ( _isStarted ) {
          EndListen(); // マウスイベントに不整合が生じているので初期化して回避する
        }
        return (Input.GetMouseButtonDown( _buttonType ) && IsAllModifierKeysInState && _cameraOperator.IsInViewArea() && !ModalDialog.IsOpened && !MouseUtil.IsMouseOnImage());
      }
    }

    protected override bool IsListening
    {
      get { return _isStarted && Input.GetMouseButton( _buttonType ); }
    }

    protected override bool IsListenEnd
    {
      get { return _isStarted && Input.GetMouseButtonUp( _buttonType ); }
    }

    protected override InputListenResult StartListen()
    {
      _isStarted = true;
      _startPos = Input.mousePosition;
      if ( null != _worldCamera ) _startPos = _worldCamera.ScreenToWorldPoint( _startPos );
      _lastPos = _startPos;

      return InputListenResult.ResumeOtherEvents;
    }

    protected override InputListenResult UpdateListen()
    {
      var newPos = Input.mousePosition;
      if ( null != _worldCamera ) newPos = _worldCamera.ScreenToWorldPoint( newPos );

      var e = new MouseDragEventArgs( _startPos, _lastPos, newPos );
      _lastPos = newPos;

      OnUpdate( e );

      if ( e.Finished ) {
        EndListen();
      }

      if ( e.SuspendOtherInputs ) {
        return InputListenResult.SuppressOtherEvents;
      }

      return InputListenResult.ResumeOtherEvents;
    }

    protected override InputListenResult EndListen()
    {
      _isStarted = false;
      return InputListenResult.ResumeOtherEvents;
    }
  }
}
