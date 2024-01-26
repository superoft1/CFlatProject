using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Topology;
using UnityEngine;

public class VertexObject : MonoBehaviour {

  [SerializeField]
  EdgeObject leafEdge;

  [SerializeField]
  VertexObject partnerVertex;

  public EdgeObject LeafEdge
  {
    get => leafEdge ;
    set => leafEdge = value ;
  }

  public VertexObject Partner
  {
    get => partnerVertex ;
    set => partnerVertex = value ;
  }
}
