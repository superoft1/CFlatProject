using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class StraightTypeShellBody : ShellBody
  {
    [SerializeField]
    GameObject shellBody;

    public GameObject ShellBody { get => shellBody; set => shellBody = value; }
  }
}