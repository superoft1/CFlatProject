using System;
using UnityEngine;

namespace Chiyoda.CAD.Body {
  public class KettleTypeShellBody : ShellBody
  {
    [SerializeField]
    GameObject drumBody;

    [SerializeField]
    GameObject tipBody;

    [SerializeField]
    GameObject taperBody;

    public GameObject DrumBody { get => drumBody; set => drumBody = value; }
    public GameObject TipBody { get => tipBody; set => tipBody = value; }
    public GameObject TaperBody { get => taperBody; set => taperBody = value; }
  }
}