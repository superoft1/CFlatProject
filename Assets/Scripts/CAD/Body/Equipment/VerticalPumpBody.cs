using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class VerticalPumpBody : Body
  {
    [SerializeField]
    GameObject body;

    public GameObject Body
    {
      get
      {
        return body;
      }

      set
      {
        body = value;
      }
    }
  }
}
