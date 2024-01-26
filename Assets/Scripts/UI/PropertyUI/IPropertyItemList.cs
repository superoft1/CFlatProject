using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.UI.PropertyUI
{
  interface IPropertyItemList : IEnumerable<PropertyItemView>
  {
    PropertyItemView this[int index] { get; }
    int Count { get; }
    void Add( PropertyItemView item );
  }
}
