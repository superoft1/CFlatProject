using System;

namespace PID
{
  interface ICrossPageConnectable : IConnectable
  {
    string LinkedPersistentID { get; set; }
  }
}
