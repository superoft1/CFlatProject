using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.CAD
{
  public interface IBoundary
  {
    Bounds? GetGlobalBounds();
  }

  public static class Boundary
  {
    public static Bounds? GetBounds( object obj )
    {
      if ( obj is IBoundary boundary ) {
        return boundary.GetGlobalBounds();
      }

      if ( obj is IEnumerable enumerable ) {
        return enumerable.Cast<object>().Select( GetBounds ).UnionBounds();
      }

      if ( obj is Bounds bounds ) {
        return bounds;
      }

      return null;
    }
  }
}
