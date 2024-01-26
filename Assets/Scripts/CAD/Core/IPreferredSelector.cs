using System;
using System.Collections.Generic;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Core
{
  public interface IPreferredSelector
  {
    IEnumerable<IElement> FindPreferredElements( IElement element );
  }
}
