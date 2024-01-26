using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Core
{
  public interface IMemorableObject : IO.ISerializableObject
  {
    event EventHandler AfterNewlyValueChanged ;
    event EventHandler AfterHistoricallyValueChanged ;

    History History { get; }
  }
}
