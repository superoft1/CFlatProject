using System ;
using System.Collections.Generic ;
using System.Data ;
using System.IO ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class GenericEquipmentTableImporter : PipingPieceTableImporter
  {
    public GenericEquipmentTableImporter( DataSet dataSet) : base( dataSet, "GenericEquipment", (4,5,6), 3 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle  )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var genericEquip = doc.CreateEntity<GenericEquipment>() ;

      genericEquip.EquipNo = cells[ 0 ] ;
      genericEquip.EquipmentType = cells[ 2 ] ;
      genericEquip.LengthOfEquipment = double.Parse( cells[ 7 ] ) / 1000.0 ;
      genericEquip.WidthOfEquipment = double.Parse( cells[ 8 ] ) / 1000.0 ;
      genericEquip.HeightOfEquipment = double.Parse( cells[ 9 ] ) / 1000.0 ;

      if ( createNozzle ) {
        for ( var i = 10 ; i < cells.Length ; i += 5 ) {
          var kind = GetNozzleKind( cells[ i ] ) ;
          var length = double.Parse( cells[ i + 3 ] ) / 1000d ;
          var diameterMm = double.Parse( cells[ i + 4 ] ) ;
          var nozzle = genericEquip.AddNozzle( kind, length, DiameterFactory.FromNpsMm(diameterMm)) ;
          nozzle.NozzleType = GetNozzleType( cells[ i + 2 ] ) ;
          nozzle.Name = cells[ i + 1 ] ;
        }
      }

      return ( genericEquip, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }

    private static GenericEquipment.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( GenericEquipment.NozzleKind val in Enum.GetValues( typeof( GenericEquipment.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}