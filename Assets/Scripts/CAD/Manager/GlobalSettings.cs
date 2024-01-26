using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.CAD.Manager
{
  public class GlobalSettings : MonoBehaviour
  {
    private static GlobalSettings instance = null;

    public static GlobalSettings Instance
    {
      get
      {
        return instance;
      }
    }

    private void Awake()
    {
      if ( instance == null ) {
        instance = this;
      }
    }

    private void OnDestroy()
    {
      instance = null;
    }

    [SerializeField]
    private bool isVertexMergeModeOnDifferentLine = true;

    public bool IsVertexMergeModeOnDifferentLine
    {
      get { return isVertexMergeModeOnDifferentLine; }
      set { isVertexMergeModeOnDifferentLine = value; }
    }

  }
}