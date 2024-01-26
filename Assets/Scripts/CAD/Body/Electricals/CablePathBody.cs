using System.Collections.Generic ;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class CablePathBody : Body
  {
    [SerializeField]
    GameObject planeBody;

    public GameObject PlaneBody
    {
      get
      {
        return planeBody;
      }

      set
      {
        planeBody = value;
      }
    }
  }
}