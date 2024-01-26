using System ;

namespace Chiyoda.CAD.Core
{
  public interface IRelocatable : IPlacement
  {
    event EventHandler LocalCodChanged;
  }
}