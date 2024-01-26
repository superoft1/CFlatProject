using System;
using System.Collections.Generic;

namespace PID
{
  interface IPipingFitting : IConnectable
  {
    string Tag { get; set; }

    List<string> Diameters { get; }
  }
}
