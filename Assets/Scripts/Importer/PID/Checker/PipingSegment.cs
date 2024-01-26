using System;
using System.Collections.Generic;

namespace PID
{
  class PipingSegment
  {
    public string ID { get; set; } = "";

    public string Specification { get; set; } = "";

    public string Diameter { get; set; } = "";

    public List<IConnectable> Components { get; } = new List<IConnectable>();

    public class ComponentConnection
    {
      public ( string id, int index ) From { get; set; } = ( "", -1 );
      public ( string id, int index ) To { get; set; } = ( "", -1 );
    }

    public ComponentConnection Connection { get; set; } = new ComponentConnection();
  }
}
