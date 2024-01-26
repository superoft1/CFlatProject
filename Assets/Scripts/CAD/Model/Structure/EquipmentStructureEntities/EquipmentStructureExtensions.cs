using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal static class EquipmentStructureExtensions
  {
    public static string ToName( this Vector3Int key )
    {
      return $"({key.x}, {key.y}, {key.z})" ;
    }
  }
}