using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Data ;
using System.IO;
using System.Linq;
using System.Text ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Importer.Equipment ;
using UnityEngine;

namespace Chiyoda.Test.Routing
{
  [DataContract]
  public class EquipDefinition
  {
    [DataMember] public string Name { get; set; }
    [DataMember] public Vector3d Pos { get; set; }
    [DataMember] public string NozzleKind { get; set; }
    [DataMember] public string NozzleType { get; set; }
    [DataMember] public double Diameter { get; set; }
    
    [IgnoreDataMember] public Equipment Equipment { get; set; }

    public void Setup(Document document)
    {
      var nozzleLength = 0.5;
      
      DataSet dataSet = new DataSet();
      var path = Path.Combine(ImportManager.InstrumentsPath(), "GenericEquipment.csv");
      dataSet.Tables.Add( TableReader.Load( Path.Combine(AGRUnitView.DemoPath(), path), "GenericEquipment" )  ) ;
      var instrumentTable = PipingPieceTableFactory.Create( BlockPatternType.Type.GenericEquipment, dataSet ) ;

      var (piece, origin, rot) = instrumentTable.Generate( document, "base", false ) ;
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.GenericEquipment ) ;
      var equipment = piece as Equipment ;
      var edge = BlockPatternFactory.CreateInstrumentEdgeVertex( bp, equipment, Pos, rot ) ;
      this.Equipment = equipment ;

      // ノズルを追加
      if (Equipment is GenericEquipment equip)
      {
        var diam = DiameterFactory.FromNpsMm( Diameter.ToMillimeters() );
        var nozzle = equip.AddNozzle(GetNozzleKind(NozzleKind), nozzleLength, diam);
        
        if(NozzleType == "Suction") nozzle.NozzleType = Nozzle.Type.Suction;
        else if (NozzleType == "Discharge") nozzle.NozzleType = Nozzle.Type.Discharge;
        else Debug.Assert(false);
          
        nozzle.Name = NozzleKind;
      }
      document.CreateHalfVerticesAndMakePairs(edge);
    }

    private GenericEquipment.NozzleKind GetNozzleKind(string nozzleKindStr)
    {
      switch (NozzleKind)
      {
        case "HZ1" :return GenericEquipment.NozzleKind.HZ1;
        case "HZ2" :return GenericEquipment.NozzleKind.HZ2;
        case "HZ3" :return GenericEquipment.NozzleKind.HZ3;
        case "HZ4" :return GenericEquipment.NozzleKind.HZ4;
        case "UP" :return GenericEquipment.NozzleKind.UP;
        case "DOWN" :return GenericEquipment.NozzleKind.DOWN;
      }
      Debug.Assert(false);
      return GenericEquipment.NozzleKind.HZ1;
    }
  }
}