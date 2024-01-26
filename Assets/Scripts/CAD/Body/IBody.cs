using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Body
{
  public interface IBody : IBoundary
  {
    Model.Entity Entity { get; set; }

    void RemoveFromView();
  }
}
