using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class LegTypeVesselBody : Body
  {
    [SerializeField]
    GameObject cylinderBody;

    [SerializeField]
    GameObject northLegBody;

    [SerializeField]
    GameObject southLegBody;

    [SerializeField]
    GameObject eastLegBody;

    [SerializeField]
    GameObject westLegBody;

    [SerializeField]
    GameObject upperCapBody;

    [SerializeField]
    GameObject lowerCapBody;

    public LegTypeVesselBody()
    {
    }

    public GameObject CylinderBody
    {
      get
      {
        return cylinderBody;
      }

      set
      {
        cylinderBody = value;
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

    public GameObject UpperCapBody
    {
      get
      {
        return upperCapBody;
      }

      set
      {
        upperCapBody = value;
      }
    }

    public GameObject LowerCapBody
    {
      get
      {
        return lowerCapBody;
      }

      set
      {
        lowerCapBody = value;
      }
    }
  }
}
