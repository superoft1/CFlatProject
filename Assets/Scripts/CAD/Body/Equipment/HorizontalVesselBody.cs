using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class HorizontalVesselBody : Body
  {
    [SerializeField]
    GameObject drumBody;

    [SerializeField]
    GameObject saddleBody1;

    [SerializeField]
    GameObject saddleBody2;

    [SerializeField]
    GameObject capBody1;

    [SerializeField]
    GameObject capBody2;

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

    public GameObject CapBody1
    {
      get
      {
        return capBody1;
      }

      set
      {
        capBody1 = value;
      }
    }

    public GameObject CapBody2
    {
      get
      {
        return capBody2;
      }

      set
      {
        capBody2 = value;
      }
    }
  }
}
