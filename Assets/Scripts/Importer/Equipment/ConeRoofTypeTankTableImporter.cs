using System;
using System.Collections.Generic;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Importer.Equipment
{
  internal class ConeRoofTypeTankTableImporter : PipingPieceTableImporter
  {
    public ConeRoofTypeTankTableImporter( DataSet dataSet ) : base(dataSet, "ConeRoofTypeTank", (3,4,5), 2)
    { }
  
    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var tank = doc.CreateEntity<ConeRoofTypeTank>();

      tank.EquipNo = cells[0];
      tank.HeightOfCylinder = double.Parse(cells[6]) / 1000.0;
      tank.DiameterOfCylinder = double.Parse(cells[7]) / 1000.0;
      return ( tank, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
  }
}
