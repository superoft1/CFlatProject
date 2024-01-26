using System ;
using System.Collections.Generic ;
using System.Linq ;

namespace Chiyoda.CAD.Model.Structure
{
  internal class RcSpecService : ISpecificationService
  {
    private readonly IBaseMaterialSelectionRule _rule ;
    private readonly ElementDatabaseInterface _table ;
    
    public RcSpecService( IBaseMaterialSelectionRule rule )
    {
      _rule = rule ;
      _table = new ElementDatabaseInterface();
    }

    public IStructuralMaterial Column( IStructuralMaterial beam )
    {
      var baseSize = _rule.CalcStandardColumnSize( beam.SubSize ) ;
      return GetStandardMaterial( baseSize, v => v > 0.9 ) ;
    }

    public IStructuralMaterial Beam( double spanWidth )
    {
      var baseSize = _rule.CalcStandardBeamSize( spanWidth ) ;
      return GetStandardMaterial( baseSize, v => v < 0.6 ) ;
    }

    public IList<string> GetList()
    {
      return _table.GetStandardList( "RC", SteelShapeType.Unknown ) ;
    }
    
    public IStructuralMaterial Get( string name )
    {
      return new RcMaterial( GetRecord( name ) ) ;
    }
    
    public IStructurePart GetElement( IStructuralMaterial material, double length )
    {
      var record = GetRecord( material.Name ) ;
      return new HSteel( 0.001*(record.H_D ?? 200), 0.001*(record.B ?? 200), length );
    }

    public double SectionArea( IStructuralMaterial material )
    {
      var record = GetRecord( material.Name ) ;
      return 0.001* (record.H_D ?? 0.0) * 0.001 * (record.B ?? 0.0) ;
    }
    
    private IStructuralMaterial GetStandardMaterial( double baseSize, Predicate<double> isTargetRatioHB )
    {
      var records = _table.GetRecords( "RC", SteelShapeType.Unknown ) ;
      var record = records
        .FirstOrDefault( r => r.H_D > 1.0e3*baseSize && isTargetRatioHB( r.B.Value / r.H_D.Value ) ) ;
      return (record == null) 
        ? new RcMaterial( records.Last() )
        : new RcMaterial( record ) ;
    }

    private DB.StructureElementTable.Record GetRecord( string name )
    {
      return _table.GetRecord( "RC", SteelShapeType.Unknown, name ) ;
    }
  }
}