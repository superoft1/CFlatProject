using System;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Presenter;
using Chiyoda.CAD.Topology;
using UnityEngine;

public static class BlockPatternFactory {

  public static BlockPattern CreateBlockPattern(BlockPatternType.Type type, bool isBlockPatternArrayChild = false)
  {
    var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
    var bp = curDoc.CreateEntity<BlockPattern>();
    if (bp.Type != BlockPatternType.Type.Unknown && bp.Type != type)
    {
      throw new InvalidOperationException();
    }
    bp.Type = type;
    bp.Name = type + "Block";
    if ( !isBlockPatternArrayChild ) {
      bp.Document.AddEdge( bp );
    }
    return bp;
  }

  public static LeafEdge CreateInstrumentEdgeVertex( Chiyoda.CAD.Topology.BlockPattern bp, Chiyoda.CAD.Model.Equipment instrument, Vector3d origin, Quaternion rot )
  {
    var bpCod = bp.LocalCod ;
    var location = bpCod.LocalizePoint( origin ) ;
    var localRot = rot * Quaternion.Inverse( bp.LocalCod.Rotation ) ;
    var localCod = new LocalCodSys3d( location, localRot, bpCod.IsMirrorType ) ;
    return bp.CreateEquipmentEdgeVertex( instrument, localCod ) ;
  } 
}
