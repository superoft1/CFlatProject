using System;
using System.Collections ;

namespace Chiyoda.CAD.IO
{
  /// <summary>
  /// ファイル保存対象オブジェクト
  /// </summary>
  public interface ISerializableObject
  {
  }

  /// <summary>
  /// ファイル保存対象リスト (array形式で保存するもの)
  /// </summary>
  public interface ISerializableList : IEnumerable
  {
    Type ItemType { get ; }
    
    void Clear() ;

    void Add( object item ) ;
  }
}