using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  [System.Serializable]
  public class BodyRatePrefabSetList
  {
    [SerializeField]
    List<BodyRatePrefabSet> setList;

    float rateForDifferent;

    public List<BodyRatePrefabSet> SetList
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

    public float RateForDifferent
    {
      get
      {
        return rateForDifferent;
      }

      set
      {
        rateForDifferent = value;
      }
    }

    public void Initialize()
    {
      foreach (var set in setList) {
        set.SetList = this;
      }
    }

    public GameObject GetPrefab(float rate)
    {
      rateForDifferent = rate;
      return setList.Min().Prefab;
    }
  }

}