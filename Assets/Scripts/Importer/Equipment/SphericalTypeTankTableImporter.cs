using System ;
using System.Data ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class SphericalTypeTankTableImporter : PipingPieceTableImporter
  {
    public SphericalTypeTankTableImporter( DataSet dataSet ) : base( dataSet, "SphericalTypeTank", ( 3,4,5 ), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc,
      string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var tank = doc.CreateEntity<SphericalTypeTank>() ;

      tank.EquipNo = cells[ 0 ] ;
      tank.DiameterOfCylinder = double.Parse( cells[ 6 ] ) / 1000.0 ;
      tank.HeightOfP1FromBase = double.Parse( cells[ 8 ] ) / 1000.0 ;
      tank.LegThickness = double.Parse( cells[ 9 ] ) / 1000.0 ;

      return ( tank, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }
  }
}