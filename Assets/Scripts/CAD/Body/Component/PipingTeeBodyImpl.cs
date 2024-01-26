using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class PipingTeeBodyImpl : MonoBehaviour
  {
    [SerializeField]
    GameObject mainPipe;

    [SerializeField]
    GameObject referencePipe;

    public GameObject ReferencePipe
    {
      get
      {
        return referencePipe;
      }

      set
      {
        referencePipe = value;
      }
    }

    public GameObject MainPipe
    {
      get
      {
        return mainPipe;
      }

      set
      {
        mainPipe = value;
      }
    }
  }

}
