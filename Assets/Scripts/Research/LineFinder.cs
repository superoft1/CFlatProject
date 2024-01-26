using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Chiyoda.CAD.Topology;
using System.IO;
using Chiyoda.CAD.Core;

public class LineFinder : MonoBehaviour
{

  public void LineFind()
  {
    var filePath = Path.Combine( ImportManager.InstrumentsPath(), "YamaguchiLineNumber.csv" );
    var lines = LineNumberImporter.ImportData( filePath );
    LineFind( lines );
  }

  public struct LineSet
  {
    public string lineId;
    public Vector3d from;
    public Vector3d to;
  }

  void LineFind( List<string> lineIds )
  {
    var doc = DocumentCollection.Instance.Current;

    var sets = new List<LineSet>();
    foreach ( var lineId in lineIds ) {
      var line = doc.FindOrCreateLine( lineId );
      if ( null == line ) continue;

      var vertices = GetVertices( line ).Distinct().ToList();
      Debug.Log( "line : " + line + "," + vertices.Count );

      vertices.Sort();

      if ( vertices.Count < 2 ) {
        Debug.Log( "ng line : " + line + "," + vertices.Count );
        continue;
      }

      var maxList = new List<HalfVertex>();
      maxList.Add( vertices[0] );
      var d = vertices[0].ConnectPoint.Diameter.OutsideMeter;
      for ( int i = 1 ; i < vertices.Count ; ++i ) {
        var d2 = vertices[i].ConnectPoint.Diameter.OutsideMeter;
        if ( System.Math.Abs( d - d2 ) < 0.001 ) {
          maxList.Add( vertices[i] );
        }
        else {
          break;
        }
      }

      int fromIndex = 0;
      int toIndex = 1;
      double distance = 0;
      if ( maxList.Count > 2 ) {
        for ( int i = 0 ; i < maxList.Count ; ++i ) {
          var p1 = maxList[ i ].GlobalPoint ;
          for ( int j = 0 ; j < maxList.Count ; ++j ) {
            var p2 = maxList[ j ].GlobalPoint ;
            var dis = Vector3d.Distance( p1, p2 ) ;
            if ( distance < dis ) {
              distance = dis;
              fromIndex = i;
              toIndex = j;
            }
          }
        }
      }

      var set = new LineSet();
      set.from =  maxList[ fromIndex ].GlobalPoint ;
      set.to = maxList[ toIndex ].GlobalPoint ;
      set.lineId = lineId;
      sets.Add( set );
    }

    foreach ( var set in sets ) {
      var from = GameObject.CreatePrimitive( PrimitiveType.Sphere );
      from.gameObject.name = set.lineId + "_from";
      from.transform.position = (Vector3)set.from;
      from.transform.localScale = Vector3.one * 0.1f;
      var to = GameObject.CreatePrimitive( PrimitiveType.Sphere );
      to.gameObject.name = set.lineId + "_to";
      to.transform.position = (Vector3)set.to;
      to.transform.localScale = Vector3.one * 0.1f;
    }

    LineListExporter.ExportResult( sets );
  }

  IEnumerable<HalfVertex> GetVertices( Line line )
  {
    return line.LeafEdges.SelectMany( le => le.Vertices ).Where( v => null == v.Partner ) ;
  }
}
