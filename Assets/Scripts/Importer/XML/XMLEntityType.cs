using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

public class XMLEntityType {

  public static EntityType.Type GetType(string name) {
    switch(name) {
      case "Pipe" : return EntityType.Type.Pipe;
      case "PipeBend" : return EntityType.Type.PipingElbow90;
      case "PipingElbow90" : return EntityType.Type.PipingElbow90;
      case "PipingElbow45" : return EntityType.Type.PipingElbow45;
      case "PipingTee" : return EntityType.Type.PipingTee;
      case "PipingCoupling" : return EntityType.Type.PipingCoupling;
      case "PipingPlug" : return EntityType.Type.PipingPlug;
      case "PipingCap" : return EntityType.Type.PipingCap;
      case "BallValve" : return EntityType.Type.BallValve;
      case "GateValve" : return EntityType.Type.GateValve;
      case "CheckValve" : return EntityType.Type.CheckValve;
      case "AngleCheckValve" : return EntityType.Type.AngleTStrainer;
      case "ButterflyValve" : return EntityType.Type.ButterflyValve;
      case "ThreeWayInstrumentRootvalve" : return EntityType.Type.ThreeWayInstrumentRootvalve;
      case "ConcentricPipingReducer" : return EntityType.Type.ConcentricPipingReducerCombination;
      case "EccentricPipingReducer" : return EntityType.Type.EccentricPipingReducerCombination;
      case "Olet" : return EntityType.Type.WeldOlet;
      case "WeldOlet" : return EntityType.Type.WeldOlet;
      case "SockOlet" : return EntityType.Type.SockOlet;
      case "ElbOlet" : return EntityType.Type.WeldOlet;
      case "Flange" : return EntityType.Type.Flange;
      case "BlindFlange" : return EntityType.Type.BlindFlange;
      case "SlipOnFlange" : return EntityType.Type.SlipOnFlange;
      case "WeldNeckFlange" : return EntityType.Type.WeldNeckFlange;
      case "OpenSpectacleBlank" : return EntityType.Type.OpenSpectacleBlank;
      case "StubInReinforcingWeld" : return EntityType.Type.StubInReinforcingWeld;
      case "UNKNOWN_COMPONENT" : return EntityType.Type.UNKNOWN_COMPONENT;
    }
    return EntityType.Type.UNKNOWN_COMPONENT;
  }
	
}
