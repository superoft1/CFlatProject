using System.Collections;
using System.Collections.Generic;
//using Assets.Scripts.CAD.Model.PipeSupport;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{
  public class IDFPipeSupportImpoter : IDFEntityImporter
  {
    private Support Support { get ; set ; }
    public IDFPipeSupportImpoter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements,
      Vector3d _standard, UnitOption option) : base(_type, _legType, _elements, _standard, option)
    {
    }

    public override LeafEdge Import(Chiyoda.CAD.Core.Document doc)
    {
      Support = doc.CreateEntity( IDFEntityType.GetType( FittingType ) ) as Support ;
      return null ;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      var support = Support;
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var position = GetStartPoint(elements);
      var block = Group as BlockPattern;
      if ( block != null ) {
        position = block.LocalCod.LocalizePoint( position );
      }
      var assocPipe = prev.PipingPiece;

      SupportPositionBase pos;
      if ( assocPipe is Pipe ) {
        pos = new PipeSupportPosition( support );
        var assocLeafEdge = assocPipe.Parent as LeafEdge;
        pos.Target = assocLeafEdge;
      }
      else if ( null != assocPipe && null != assocPipe.Parent ) {
        pos = new RelativeSupportPosition( support );
        var assocLeafEdge = assocPipe.Parent as LeafEdge;
        pos.Target = assocLeafEdge;
      }
      else {
        pos = new AbsoluteSupportPosition( support );
      }
      pos.Position = position;
      support.SupportPosition = pos;
      support.SupportType = SymbolKey(elements) == "DUCK" ?  SupportType.Trunnion : SupportType.None;//DuckはTrunnionの事

      var elmWithSupport = (Group as ISupportParentElement) ?? Document;
      elmWithSupport.Supports.Add( support );

      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType,
      string[] columns)
    {
      if (entityState == EntityState.Out)
      {
        return this;
      }
      if (entityState == EntityState.In)
      {
        entityState = EntityState.Out;
      }
      return this;
    }
  }
}