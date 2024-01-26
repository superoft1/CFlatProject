using System;
using System.Collections.Generic;

namespace PID
{
  interface IConnectable : IPipingComponent
  {
    PipingSegment Owner { get; set; }

    List<( IConnectable component, int index )> Parents { get; }
    List<( IConnectable component, int index )> Children { get; }
  }
}
