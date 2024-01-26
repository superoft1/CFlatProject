using System.Runtime.Serialization;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using UnityEngine;

namespace Chiyoda.Test.Routing
{
  [DataContract]
  public class RackDefinition
  {
    [DataMember] public string Name { get; set; }
    [DataMember] public Vector3d Position { get; set; }
    [DataMember] public double Rotation{ get; set; }
    [DataMember] public double SideBeamThickness{ get; set; }
    [DataMember] public double BeamThickness{ get; set; }
    [DataMember] public double ColumnThickness{ get; set; }
    [DataMember] public double BeamInterval{ get; set; }
    [DataMember] public int IntervalNumber{ get; set; }
    [DataMember] public double Width{ get; set; }
    [DataMember] public List<double> Floors{ get; set; }
    [DataMember] public List<string> LinkedRacks{ get; set; }

    [IgnoreDataMember] internal IStructure PipeRack { get ; set ; }
    public RackDefinition()
    {
      Name = "PipeRack";
      Position = Vector3d.zero;
      Rotation = 0.0;
      SideBeamThickness = 0.2;
      BeamThickness = 0.2;
      ColumnThickness = 0.2;
      BeamInterval = 6.0f;
      IntervalNumber = 12;
      Width = 6.0;
      Floors = new List<double>() {4.5, 2.0, 1.5, 2.0};
    }

    public void Setup(Document document)
    {
      PipeRack = StructureFactory.CreatePipeRack(document, PipeRackFrameType.Single);
      if ( PipeRack is Entity e ) {
        e.Name = Name ;
      }
      PipeRack.Position = Position;
      PipeRack.Rotation = Rotation;
      PipeRack.IntervalCount = IntervalNumber;
      PipeRack.SetWidthAndStandardMaterials( Width, BeamInterval );
      PipeRack.FloorCount = Floors.Count;
      for(int i = 0; i< Floors.Count; i++) {
        PipeRack.SetFloorHeight( i, Floors[ i ] ) ; 
      }
    }
  }
}