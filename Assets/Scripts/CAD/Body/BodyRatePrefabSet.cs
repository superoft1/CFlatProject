using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  
  [System.Serializable]
  public class BodyRatePrefabSet : System.IComparable<BodyRatePrefabSet>
  {
    [SerializeField]
    float rate;

    [SerializeField]
    GameObject prefab = null;

    BodyRatePrefabSetList setList;

    public int CompareTo(BodyRatePrefabSet other)
    {
      var a = System.Math.Abs(this.rate - SetList.RateForDifferent);
      var b = System.Math.Abs(other.rate - SetList.RateForDifferent);

      if (a > b) {
        return 1;
      } else if (a < b) {
        return -1;
      } else {
        return 0;
      }
    }

    public float Rate
    {
      get
      {
        return rate;
      }

      set
      {
        rate = value;
      }
    }

    public GameObject Prefab
    {
      get
      {
        return prefab;
      }

      set
      {
        prefab = value;
      }
    }

    public BodyRatePrefabSetList SetList
    {
      get
      {
        return setList;
      }

      set
      {
        setList = value;
      }
    }
  }

}