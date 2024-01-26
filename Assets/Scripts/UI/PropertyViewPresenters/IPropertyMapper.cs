using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.UI.PropertyViewPresenters
{
  interface IPropertyMapper : IObjectValueMapper
  {
    PropertyCategory Category { get; }

    string Name { get; }
    int Order { get; }

    long UniqueToken { get; }
  }

  interface IListPropertyMapper : IPropertyMapper
  {
    ValueType ElementValueType { get; }
    
    string IndexLabelPrefix { get ; }

    // Listをプロパティとして保持するオブジェクトに対する処理
    int GetCount( object obj );
    void SetCount( object obj, int count );
    bool CanSetCount( object obj );
    object GetElement( object obj, int index );
    void SetElement( object obj, int index, object newValue );

    IObjectValueMapper GetElementMapper( int index );
  }
}
