using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Plotplan;
using UnityEngine;

namespace Chiyoda.UI
{
  public class UnitContentManager : MonoBehaviour
  {
    private static UnitContentManager Instance { get; set; }

    public List<UnitTypeInfo> unitTypeList;

    [Serializable]
    public class UnitTypeInfo
    {
      public UnitType.Type unitType;
      public CompositeUnit.Type compositeUnitType;
      public Texture2D texture;
      public Vector3 size;
      public List<UnitModelInfo> modelInfo;
    }
    
    [Serializable]
    public class UnitModelInfo
    {
      public GameObject model;
      public Vector2 rate;
    }

    private void Awake()
    {
      Instance = this;
    }

    public static CompositeUnit.Type GetCompositeUnitType(UnitType.Type type)
    {
      return Instance == null
        ? CompositeUnit.Type.None
        : (from typeInfo in Instance.unitTypeList where typeInfo.unitType == type select typeInfo.compositeUnitType)
        .FirstOrDefault();
    }

    public static Texture2D GetTexture(UnitType.Type type)
    {
      return Instance == null
        ? null
        : (from typeInfo in Instance.unitTypeList where typeInfo.unitType == type select typeInfo.texture)
        .FirstOrDefault();
    }

    public static Vector3 GetSize(UnitType.Type type)
    {
      return Instance == null
        ? new Vector3()
        : (from typeInfo in Instance.unitTypeList where typeInfo.unitType == type select typeInfo.size)
        .FirstOrDefault();
    }

    public static GameObject GetModel(UnitType.Type type)
    {
      if (Instance == null) return null;

      var modelInfos = (from typeInfo in Instance.unitTypeList where typeInfo.unitType == type select typeInfo.modelInfo).FirstOrDefault();
      return modelInfos?.Select(modelInfo => modelInfo.model).FirstOrDefault();
    }
  }
}