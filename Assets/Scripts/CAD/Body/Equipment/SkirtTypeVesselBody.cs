using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SkirtTypeVesselBody : Body
  {
    [SerializeField]
    GameObject skirtBody;

    [SerializeField]
    GameObject towerBody;

    [SerializeField]
    GameObject upperCapBody;

    [SerializeField]
    GameObject lowerCapBody;

    public SkirtTypeVesselBody()
    {
    }

    public GameObject SkirtBody
    {
      get
      {
        return skirtBody;
      }

      set
      {
        skirtBody = value;
      }
    }

    public GameObject TowerBody
    {
      get
      {
        return towerBody;
      }

      set
      {
        towerBody = value;
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
