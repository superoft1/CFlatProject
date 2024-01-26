using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Chiyoda.CAD.Topology;
using Chiyoda.CAD.BP;

public class TopologyObjectMap
{

  Dictionary<Edge, EdgeObject> edgeBodyMap = new Dictionary<Edge, EdgeObject>();
  Dictionary<HalfVertex, VertexObject> vertexBodyMap = new Dictionary<HalfVertex, VertexObject>();

  public void Add( Edge edge, EdgeObject leafEdgeObject )
  {
    edgeBodyMap.Add( edge, leafEdgeObject );
  }

  public void Add( HalfVertex vertex, VertexObject vertexObject )
  {
    vertexBodyMap.Add( vertex, vertexObject );
  }

  public EdgeObject GetEdgeObject( Edge edge )
  {
    if ( edge == null ) return null;

    EdgeObject obj;
    if ( edgeBodyMap.TryGetValue( edge, out obj ) ) {
      return obj;
    }
    else {
      return null;
    }
  }
  
  public VertexObject GetVertexObject( HalfVertex vertex )
  {
    if ( vertex == null ) return null;

    VertexObject obj;
    if ( vertexBodyMap.TryGetValue( vertex , out obj ) ) {
      return obj;
    }
    else {
      return null;
    }
  }
  
  public void Remove( Edge edge )
  {
    if ( edge == null ) return;

    edgeBodyMap.Remove( edge );
  }

  public void Remove( HalfVertex vertex )
  {
    if ( vertex == null ) return;

    vertexBodyMap.Remove( vertex );
  }
}
