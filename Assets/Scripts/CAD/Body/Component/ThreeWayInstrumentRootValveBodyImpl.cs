using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class ThreeWayInstrumentRootValveBodyImpl : Body
  {

    [SerializeField]
    GameObject mainValve1;

    [SerializeField]
    GameObject mainValve2;

    [SerializeField]
    GameObject referenceValve;

    public GameObject ReferenceValve
    {
      get
      {
        return referenceValve;
      }

      set
      {
        referenceValve = value;
      }
    }

    public GameObject MainValve1
    {
      get
      {
        return mainValve1;
      }

      set
      {
        mainValve1 = value;
      }
    }

    public GameObject MainValve2
    {
      get
      {
        return mainValve2;
      }

      set
      {
        mainValve2 = value;
      }
    }
  }

}
