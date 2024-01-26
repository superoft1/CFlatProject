using System ;
using System.Data ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Importer.Equipment
{
  internal class AirFinCoolerTableImporter : PipingPieceTableImporter
  {
    public AirFinCoolerTableImporter( DataSet dataSet ) : base( dataSet, "AirFinCooler", (3,4,5), 2 )
    {
    }

    public override (Chiyoda.CAD.Model.PipingPiece piece, Vector3d origin, Quaternion rot) Generate( Document doc, string name, bool createNozzle )
    {
      var row = Table.Rows.Find( name ) ;
      var cells = TableReader.Row2Array( row ) ;
      var cooler = doc.CreateEntity<AirFinCooler>() ;

      cooler.EquipNo = cells[ 0 ] ;
      cooler.LengthOfAirCooler = double.Parse( cells[ 6 ] ) / 1000.0 ;
      cooler.WidthOfAirCooler = double.Parse( cells[ 7 ] ) / 1000.0 ;
      cooler.HeightOfAirCooler = double.Parse( cells[ 8 ] ) / 1000.0 ;

      var nozzleArrayList = cooler.Children.ToList() ;
      if ( nozzleArrayList.Count == 2 ) {
        var sucNozzleArray = nozzleArrayList[ 0 ] as AFCNozzleArray ;
        sucNozzleArray.NozzleType = Nozzle.Type.Suction ;
        sucNozzleArray.PrefixId = cells[ 10 ] ;
        sucNozzleArray.Placement = (AirFinCooler.PlacementPlane)Enum.Parse(typeof(AirFinCooler.PlacementPlane), cells[ 11 ]) ;
        sucNozzleArray.NozzleCount = int.Parse(cells[ 12 ]) ;
        sucNozzleArray.NozzleLength = double.Parse( cells[ 13 ] ) / 1000.0 ;
        sucNozzleArray.Diameter = DiameterFactory.FromOutsideMeter( double.Parse( cells[ 14 ] ) / 1000.0 );
        sucNozzleArray.Margin = double.Parse( cells[ 15 ] ) / 1000.0 ;
        sucNozzleArray.Interval = double.Parse( cells[ 16 ] ) / 1000.0 ;
        
        var disNozzleArray = nozzleArrayList[ 1 ] as AFCNozzleArray;
        disNozzleArray.NozzleType = Nozzle.Type.Discharge ;
        disNozzleArray.PrefixId = cells[ 18 ] ;
        disNozzleArray.Placement = (AirFinCooler.PlacementPlane)Enum.Parse(typeof(AirFinCooler.PlacementPlane), cells[ 19 ]) ;
        disNozzleArray.NozzleCount = int.Parse(cells[ 20 ]) ;
        disNozzleArray.NozzleLength = double.Parse( cells[ 21 ] ) / 1000.0 ;
        disNozzleArray.Diameter = DiameterFactory.FromOutsideMeter( double.Parse( cells[ 22 ] ) / 1000.0) ;
        disNozzleArray.Margin = double.Parse( cells[ 23 ] ) / 1000.0 ;
        disNozzleArray.Interval = double.Parse( cells[ 24 ] ) / 1000.0 ;
      }

      return ( cooler, ParseOrigin( cells ), ParseAngleAxis( cells ) ) ;
    }

    private static AirFinCooler.NozzleKind GetNozzleKind( string kind )
    {
      foreach ( AirFinCooler.NozzleKind val in Enum.GetValues( typeof( AirFinCooler.NozzleKind ) ) ) {
        if ( val.ToString() == kind ) {
          return val ;
        }
      }
      throw new InvalidOperationException( "Nozzle Kind not found." ) ;
    }
  }
}