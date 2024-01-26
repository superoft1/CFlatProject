using System.Collections.Generic;

namespace Chiyoda.CableRouting
{
  public interface ICablePathAvailable
  {
    IList<ICablePath> GetCablePath();
  }
}