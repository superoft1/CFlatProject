using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Topology
{
  public interface ISupportParentElement : IElement
  {
    ICollection<Support> Supports { get; }
  }
}
