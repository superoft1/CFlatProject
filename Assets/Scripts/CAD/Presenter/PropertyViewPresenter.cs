using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.UI;

namespace Chiyoda.CAD.Presenter
{
  class PropertyViewPresenter : IEntityPresenter
  {
    private readonly PropertyView _propertyView;

    public PropertyViewPresenter( PropertyView propertyView )
    {
      _propertyView = propertyView;
    }

    public bool IsRaised( IElement element )
    {
      return object.ReferenceEquals( element.Document, _propertyView.Document );
    }

    public void Raise( IElement element )
    {
      if ( element is Document doc ) {
        _propertyView.Document = doc;
      }
    }

    public void Update( IElement element )
    {
      _propertyView.UpdatePropertyValues();
    }

    public void TransformUpdate( IElement element )
    {
      _propertyView.UpdatePropertyValues();
    }

    public void Destroy( IElement element )
    {
      if ( object.ReferenceEquals( element, _propertyView.Document ) ) {
        _propertyView.Document = null;
      }
    }

    public void Processed()
    {
    }
  }
}
