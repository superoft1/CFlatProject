using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class KettleTypeHeatExchangerBody : Body
  {
    [SerializeField]
    GameObject drumBody;

    [SerializeField]
    GameObject tipBody;

    [SerializeField]
    GameObject saddleBody1;

    [SerializeField]
    GameObject saddleBody2;

    [SerializeField]
    GameObject drumCapBody;

    [SerializeField]
    GameObject tipCapBody;

    [SerializeField]
    GameObject taperBody;

    public GameObject DrumBody
    {
      get
      {
        return drumBody;
      }

      set
      {
        drumBody = value;
      }
    }

    public GameObject TipBody
    {
      get
      {
        return tipBody;
      }

      set
      {
        tipBody = value;
      }
    }

    public GameObject SaddleBody1
    {
      get
      {
        return saddleBody1;
      }

      set
      {
        saddleBody1 = value;
      }
    }

    public GameObject SaddleBody2
    {
      get
      {
        return saddleBody2;
      }

      set
      {
        saddleBody2 = value;
      }
    }

    public GameObject DrumCapBody
    {
      get
      {
        return drumCapBody;
      }

      set
      {
        drumCapBody = value;
      }
    }

    public GameObject TipCapBody
    {
      get
      {
        return tipCapBody;
      }

      set
      {
        tipCapBody = value;
      }
    }

    public GameObject TaperBody
    {
      get
      {
        return taperBody;
      }

      set
      {
        taperBody = value;
      }
    }
  }
}
