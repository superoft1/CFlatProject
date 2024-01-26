using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using JetBrains.Annotations ;

namespace Chiyoda.CAD.Core
{
  [IO.CustomSerializer( typeof( Serializers.PropertyCollectionSerializer ) )]
  public class PropertyCollection : IEnumerable<INamedProperty>
  {
    private readonly IndexableDictionary<string, INamedProperty> _properties = new IndexableDictionary<string, INamedProperty>( StringComparer.InvariantCultureIgnoreCase );

    /// <summary>
    /// プロパティを追加します。
    /// </summary>
    /// <param name="property">追加したいプロパティ。</param>
    /// <exception cref="ArgumentNullException">プロパティが null です。</exception>
    /// <returns>既に同名のプロパティがある場合は false 。そうでない場合は true 。</returns>
    public bool Add( [NotNull] INamedProperty property )
    {
      if ( null == property ) throw new ArgumentNullException( nameof( property ) ) ;

      if ( _properties.ContainsKey( property.PropertyName ) ) return false ;

      _properties.Add( property.PropertyName, property ) ;
      return true ;
    }

    /// <summary>
    /// プロパティを削除し、削除したプロパティを返します。
    /// </summary>
    /// <param name="propertyName">削除したいプロパティのプロパティ名。</param>
    /// <returns>削除したプロパティが存在する場合はそのプロパティ。存在しなかった場合は null 。</returns>
    public INamedProperty Remove( string propertyName )
    {
      var property = this[propertyName] ;
      if ( null == property ) return null ;

      _properties.Remove( propertyName ) ;

      return property ;
    }

    /// <summary>
    /// 名前からプロパティを取得します。
    /// </summary>
    /// <param name="propertyName">プロパティ名。</param>
    public INamedProperty this[ string propertyName ] => _properties.TryGetValue( propertyName, out var prop ) ? prop : null ;

    /// <summary>
    /// インデックスからプロパティを取得します。
    /// </summary>
    /// <param name="index">インデックス。</param>
    public INamedProperty this[ int index ] => _properties[ index ] ;

    /// <summary>
    /// 登録されたプロパティの個数を取得します。
    /// </summary>
    public int Count => _properties.Count ;

    public IEnumerable<IUserDefinedNamedProperty> UserDefinedProperties => _properties.Values.OfType<IUserDefinedNamedProperty>() ;

    
    public IEnumerator<INamedProperty> GetEnumerator()
    {
      return _properties.Values.GetEnumerator() ;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator() ;
    }
  }
}