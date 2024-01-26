using System;
using System.Collections.Generic;

namespace PID
{
  class PropertyBreak : IConnectable
  {
    // IPipingComponent
    public string ID { get; set; } = "";

    public string Type { get; set; } = "";

    // IConnectable
    public PipingSegment Owner { get; set; } = null;

    public List<( IConnectable component, int index )> Parents { get; } = new List<( IConnectable, int )>();
    public List<( IConnectable component, int index )> Children { get; } = new List<( IConnectable, int )>();
  }
}
