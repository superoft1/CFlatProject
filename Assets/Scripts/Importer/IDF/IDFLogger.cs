using System.IO;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace IDF
{
  internal class DebugLogger
  {
    private readonly string m_entityEdgeFile;
    private readonly string m_entityOrderFile;
    private readonly Line m_Line;
    private readonly bool m_write;

    public DebugLogger(Line line, string path, bool write = false)
    {
      m_entityOrderFile = string.Format(Application.dataPath + "/outputs/IDFEntityOrder_{0}.txt", Path.GetFileNameWithoutExtension(path));
      if (File.Exists(m_entityOrderFile)) File.Delete(m_entityOrderFile);

      m_entityEdgeFile = string.Format(Application.dataPath + "/outputs/IDFEntityEdge_{0}.txt", Path.GetFileNameWithoutExtension(path));
      if (File.Exists(m_entityEdgeFile)) File.Delete(m_entityEdgeFile);

      m_Line = line;
      m_write = write;
    }

    public void WriteEntityOrder(string col0, LeafEdge leafEdge)
    {
      if (!m_write) {return;}
      var fittingType = IDFRecordType.GetFittingType(col0);
      if (fittingType == IDFRecordType.FittingType.Unknown) return;
      var entityType = IDFEntityType.GetType(fittingType);
      if (entityType == EntityType.Type.NoEntity) return;
      var legType = IDFRecordType.GetLegType(col0);
      var path = m_entityOrderFile;
      using (var sw = File.AppendText(path))
      {
        if ( leafEdge == null ) {
          sw.WriteLine(entityType + " : " + legType);
        }
        else {
          sw.WriteLine(entityType + " : " + legType + $": {leafEdge.PipingPiece.Name}");
        }
      }
    }

    public void WriteEntityEdge()
    {
      if (!m_write) {return;}
      using (var sw = new StreamWriter(m_entityEdgeFile, false))
      {
        foreach (var e in m_Line.LeafEdges) {
          if (e.PipingPiece == null) {
            continue;
          }
          sw.WriteLine("{0}({1}) Vtx={2}", e.PipingPiece.Name, e.PipingPiece.GetType().Name, e.VertexCount);
          foreach ( var v in e.Vertices ) {
            if ( v?.LeafEdge != null ) {
              sw.WriteLine( "  edge1: {0}({1})", v.LeafEdge.PipingPiece.Name, v.LeafEdge.PipingPiece.GetType().Name ) ;
            }

            var v2 = v.Partner ;
            if ( v2?.LeafEdge != null ) {
              sw.WriteLine( "  edge2: {0}({1})", v2.LeafEdge.PipingPiece.Name, v2.LeafEdge.PipingPiece.GetType().Name ) ;
            }
          }

          sw.WriteLine(string.Empty);
        }
      }
    }
  }
}