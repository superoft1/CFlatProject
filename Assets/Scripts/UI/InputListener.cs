using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.UI
{
  abstract class InputListener
  {
    public enum InputListenResult
    {
      SuppressOtherEvents,
      ResumeOtherEvents,
    }

    protected abstract bool IsListenStart { get; }
    protected abstract bool IsListening { get; }
    protected abstract bool IsListenEnd { get; }

    protected abstract InputListenResult StartListen();
    protected abstract InputListenResult UpdateListen();
    protected abstract InputListenResult EndListen();

    public InputListenResult Listen()
    {
      if ( IsListenEnd ) {
        return EndListen();
      }
      else if ( IsListening ) {
        return UpdateListen();
      }
      else if ( IsListenStart ) {
        return StartListen();
      }
      else {
        return InputListenResult.ResumeOtherEvents;
      }
    }

    public void ForceEndListen()
    {
      EndListen();
    }
  }
}
