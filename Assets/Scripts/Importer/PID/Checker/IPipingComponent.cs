using System;

namespace PID
{
  interface IPipingComponent
  {
    string ID { get; set; }

    string Type { get; set; }
  }
}
