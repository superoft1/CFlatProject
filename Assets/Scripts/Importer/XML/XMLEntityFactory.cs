using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.Importer;

public class XMLEntityFactory {

  public static Entity ImportEntity(Chiyoda.CAD.Core.Document doc, IEntityDictionary dic, EntityType.Type entityType, System.Xml.XmlElement element, IGroup group, LeafEdge leafEdge)
  {
    var importer = CreateEntityImporter(entityType, element);
    if (importer == null) return null;
    importer.EntityDictionary = dic;
    importer.Group = group;
    importer.ParentLeafEdge = leafEdge;
    importer.Document = doc ;
    return importer.Import(doc);
  }

  static XMLEntityImporter CreateEntityImporter(EntityType.Type entityType, System.Xml.XmlElement element)
  {
    switch (entityType)
    {
      case EntityType.Type.Pipe : return new XMLPipeImporter(entityType, element);
      case EntityType.Type.PipingElbow90 : return new XMLPipingElbow90Importer(entityType, element);
      case EntityType.Type.PipingElbow45 : return new XMLPipingElbow45Importer(entityType, element);
      case EntityType.Type.WeldNeckFlange : return new XMLWeldNeckFlangeImporter(entityType, element);
      case EntityType.Type.BlindFlange : return new XMLBlindFlangeImporter(entityType, element);
      case EntityType.Type.SlipOnFlange : return new XMLSlipOnFlannge(entityType, element);
      case EntityType.Type.Flange : return new XMLFlangeImporter(entityType, element);
      case EntityType.Type.GateValve : return new XMLGateValveImporter(entityType, element);
      case EntityType.Type.ButterflyValve : return new XMLButterflyValveImpoter(entityType, element);
      case EntityType.Type.CheckValve : return new XMLCheckValveImporter(entityType, element);
      case EntityType.Type.BallValve : return new XMLBallValveImporter(entityType, element);
      case EntityType.Type.PipingTee : return new XMLPipingTeeImporter(entityType, element);
      case EntityType.Type.ConcentricPipingReducerCombination : return new XMLConcentricPipingReducerImporter(entityType, element);
      case EntityType.Type.EccentricPipingReducerCombination : return new XMLEccentricPipingReducerImporter(entityType, element);
      case EntityType.Type.PipingLateralTee : return new XMLPipingLateralTeeImporter(entityType, element);
      case EntityType.Type.PipingCap : return new XMLPipingCapImporter(entityType, element);
      case EntityType.Type.PipingPlug : return new XMLPipingPlugImporter(entityType, element);
      case EntityType.Type.PipingCoupling : return new XMLPipingCouplingImporter(entityType, element);
      case EntityType.Type.WeldOlet : return new XMLWeldOletImporter(entityType, element);
      case EntityType.Type.SockOlet : return new XMLSockOletImporter(entityType, element);
      case EntityType.Type.StubInReinforcingWeld : return new XMLStubInReinforcingWeldImporter(entityType, element);
      case EntityType.Type.AngleTStrainer : return new XMLAngleTStrainerImporter(entityType, element);
      case EntityType.Type.OpenSpectacleBlank : return new XMLOpenSpectacleBlankImporter(entityType, element);
      case EntityType.Type.BlankSpectacleBlank : return new XMLBlankSpectacleBlankImporter(entityType, element);
      case EntityType.Type.ThreeWayInstrumentRootvalve : return new XMLThreeWayInstrumentRootValve(entityType, element);
      case EntityType.Type.OrificePlate : return new XMLOrificePlateImporter(entityType, element);
      case EntityType.Type.Support : return new XMLPipeSupportImporter(entityType, element);
    }

    return null;
  }

}
