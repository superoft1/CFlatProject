using System.Collections ;

namespace Chiyoda.Common
{
  /// <summary>
  /// <see cref="System.Collections.ICollection"/> ではないものを <see cref="System.Collections.ICollection"/> のように扱うためのプロキシ・インターフェースです。
  /// </summary>
  public interface ICollectionProxy : ICollection
  {
    /// <summary>
    /// ベースとなる <see cref="System.Collections.IEnumerable"/> を取得します。
    /// </summary>
    IEnumerable BaseCollection { get ; }
  }
}