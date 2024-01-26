using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SphericalTypeTankBody : Body
  {
    [SerializeField]
    GameObject tankBody;

    [SerializeField]
    GameObject northLegBody;

    [SerializeField]
    GameObject southLegBody;

    [SerializeField]
    GameObject eastLegBody;

    [SerializeField]
    GameObject westLegBody;

    public SphericalTypeTankBody()
    {
    }

    public GameObject TankBody
    {
      get
      {
        return tankBody;
      }
      set
      {
        tankBody = value;
      }
    }

    public GameObject NorthLegBody
    {
      get
      {
        return northLegBody;
      }

      set
      {
        northLegBody = value;
      }
    }

    public GameObject SouthLegBody
    {
      get
      {
        return southLegBody;
      }

      set
      {
        southLegBody = value;
      }
    }

    public GameObject EastLegBody
    {
      get
      {
        return eastLegBody;
      }

      set
      {
        eastLegBody = value;
      }
    }

    public GameObject WestLegBody
    {
      get
      {
        return westLegBody;
      }

      set
      {
        westLegBody = value;
      }
    }

  }
}
