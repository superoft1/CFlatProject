using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;

namespace Chiyoda.UI.PropertyViewPresenters
{
  /// <summary>
  /// オブジェクトのプロパティ操作を行なうオブジェクトのインターフェース。
  /// </summary>
  interface IObjectValueMapper
  {
    string Label { get; }
    ValueType ValueType { get; }

    bool IsEditableForBlockPatternChildren { get; }

    INamedProperty GetPropertyInfo( object obj ) ;
    object GetObjectValue( object obj );
    void SetObjectValue( object obj, object value );
    IEnumerable GetValueList( object obj );

    PropertyVisibility GetVisibility( object obj );
  }
}
