using System.Collections.Generic ;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class CableBody : Body
  {
    [SerializeField]
    List<GameObject> cables = new List<GameObject>();

    public List<GameObject> Cables { get => cables; }
  }
}