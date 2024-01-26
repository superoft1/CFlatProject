using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Core
{
  public class PropertyEventArgs : EventArgs
  {
    public INamedProperty Property { get; }

    public PropertyEventArgs( INamedProperty property )
    {
      Property = property;
    }
  }

  /// <summary>
  /// プロパティつき要素。
  /// </summary>
  public interface IPropertiedElement : IElement
  {
    string ObjectName { get ; }

    IEnumerable<INamedProperty> GetProperties() ;

    INamedProperty GetProperty( string propertyName ) ;

    IPropertiedElement GetElementByName( string objectName ) ;

    event EventHandler<PropertyEventArgs> PropertyAdded ;
    event EventHandler<PropertyEventArgs> PropertyRemoved ;
  }

  public static class PropertiedElementExtension
  {
    public static void TriggerChangeProperty( this IPropertiedElement elm, string propertyName, double newValue )
    {
      var prop = elm.GetProperty( propertyName );
      if ( null != prop ) prop.Value = newValue;
    }
  }
}
