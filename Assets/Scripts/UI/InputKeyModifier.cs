using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.UI
{
  static class InputKeyModifier
  {
    private static readonly KeyCode[] SHIFT_KEYS = new[] { KeyCode.RightShift, KeyCode.LeftShift };
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
    private static readonly KeyCode[] CTRL_CMD_KEYS = new[] { KeyCode.RightCommand, KeyCode.LeftCommand };
    private static readonly KeyCode[] ALT_OPTION_KEYS = new[] { KeyCode.RightAlt, KeyCode.LeftAlt };
#else
    private static readonly KeyCode[] CTRL_CMD_KEYS = new[] { KeyCode.RightControl, KeyCode.LeftControl };
    private static readonly KeyCode[] ALT_OPTION_KEYS = new[] { KeyCode.RightAlt, KeyCode.LeftAlt };
#endif

    public static bool IsShiftDown
    {
      get { return AnyDown( SHIFT_KEYS ); }
    }

    public static bool IsCtrlOrCmdDown
    {
      get { return AnyDown( CTRL_CMD_KEYS ); }
    }

    public static bool IsAltOrOptionDown
    {
      get { return AnyDown( ALT_OPTION_KEYS ); }
    }

    private static bool AnyDown( KeyCode[] keys )
    {
      foreach(var key in keys ) {
        if ( Input.GetKey( key ) ) return true;
      }
      return false;
    }
  }
}
