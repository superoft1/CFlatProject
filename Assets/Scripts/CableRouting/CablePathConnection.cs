using System.Collections.Generic;

namespace Chiyoda.CableRouting
{
  public class CablePathConnected
  {
    public IDictionary<ICablePath, IList<ICablePath>> Connection { get; }
  }
}