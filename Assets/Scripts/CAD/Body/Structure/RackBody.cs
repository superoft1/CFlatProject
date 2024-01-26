using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class RackBody : Body
  {
    [SerializeField]
    List<GameObject> columns = new List<GameObject>();

    public List<GameObject> Columns { get => columns; }
  }
}