using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;

namespace Chiyoda.UI.PropertyViewPresenters
{
  interface IPropertyViewPresenter
  {
    object Target { get; }

    int PropertyMapperCount { get; set; }
    bool CanChangePropertyMapperCount { get; }
    IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers { get; }
    IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers { get; }
    IObjectValueMapper GetPropertyMapper( int index );

    PropertyVisibility GetVisibility( IObjectValueMapper mapper );
    INamedProperty GetProperty( IObjectValueMapper mapper );
    object GetValue( IObjectValueMapper mapper );
    void SetValue( IObjectValueMapper mapper, object newValue );
    IEnumerable GetValueList( IObjectValueMapper mapper );

    IPropertyViewPresenter GetSubPresenter( IObjectValueMapper mapper );
  }
}
