using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public interface IPlacement : IElement
  {
    LocalCodSys3d LocalCod { get ; set ; }
    LocalCodSys3d ParentCod { get ; }
    LocalCodSys3d GlobalCod { get ; }
  }
}