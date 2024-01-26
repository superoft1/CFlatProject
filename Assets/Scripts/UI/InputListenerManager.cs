using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.UI
{
  class InputListenerManager
  {
    private readonly List<InputListener> _listeners = new List<InputListener>();

    public void AddListener( InputListener listener )
    {
      _listeners.Remove( listener );
      _listeners.Add( listener );
    }
    public void RemoveListener( InputListener listener )
    {
      _listeners.Remove( listener );
    }

    public void Listen()
    {
      foreach ( var listener in _listeners ) {
        if ( InputListener.InputListenResult.SuppressOtherEvents == listener.Listen() ) return;
      }
    }
  }
}
