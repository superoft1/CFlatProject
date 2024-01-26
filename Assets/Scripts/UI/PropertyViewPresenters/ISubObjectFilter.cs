using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;

namespace Chiyoda.UI.PropertyViewPresenters
{
  public interface ISubObjectFilter
  {
    bool CanChangeCount( IMemorableObject baseObject );
    int GetCount( IMemorableObject baseObject );
    void SetCount( IMemorableObject baseObject, int count );
    object GetSubObject( IMemorableObject baseObject, int index );
  }
}
