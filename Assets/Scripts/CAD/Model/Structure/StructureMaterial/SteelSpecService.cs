using System ;
using System.Collections.Generic ;
using System.Linq ;

namespace Chiyoda.CAD.Model.Structure
{
  internal class SteelSpecService 
  {
    private class Spec : ISpecificationService
    {
      private readonly SteelShapeType _type ;
      private readonly SteelSpecService _src ;
      public Spec( SteelSpecService src, SteelShapeType type )
      {
        _type = type ;
        _src = src ;
      }
      
      public IStructuralMaterial Column( IStructuralMaterial beam )
      {
        var baseSize = _src._rule.CalcStandardColumnSize( beam.SubSize ) ;
        return GetStandardMaterial( baseSize, v => v > 0.9 ) ;
      }
    
      public IStructuralMaterial Beam( double spanWidth )
        => GetStandardMaterial( _src._rule.CalcStandardBeamSize( spanWidth ), v => v < 0.6 ) ;
      
      public IList<string> GetList() => _src._database.GetStandardList( _src._specKey, _type ) ;
    
      public IStructuralMaterial Get( string name )
      {
        return new SteelMaterial( _src._database.GetRecord( _src._specKey, _type, name ), MaterialRotation.Rot0 ) ;
      }

      private IStructuralMaterial GetStandardMaterial( double baseSize, Predicate<double> isTargetRatioHb )
      {
        var records = _src._database.GetRecords( _src._specKey, _type ) ;
        var record = records
          .FirstOrDefault( r => r.B != null && (r.H_D > 1.0e3*baseSize && isTargetRatioHb( r.B.Value / r.H_D.Value )) ) ;
        return record == null
          ? new SteelMaterial( records.Last(), MaterialRotation.Rot0 )
          : new SteelMaterial( record, MaterialRotation.Rot0 ) ;
      }
    }
    
    private string _specKey  ;
    private readonly IBaseMaterialSelectionRule _rule ;
    private readonly ElementDatabaseInterface _database ;

    public SteelSpecService( SteelSpecificationType type, IBaseMaterialSelectionRule rule )
    {
      _specKey = type.ToSpecKey() ;
      _rule = rule ;
      _database = new ElementDatabaseInterface();
    }
    
    public ISpecificationService Get( SteelShapeType type ) => new Spec( this, type );
    
    public IStructurePart GetElement( IStructuralMaterial material, double length )
    {
      var record = _database.GetRecord( _specKey, material.ShapeType, material.Name ) ;
      if ( ( record.H_D == null ) || ( record.B == null ) ) {
        return new HSteel( 0.1, 0.1, length ) ;
      }
      return new HSteel( 0.001 * record.H_D.Value, 0.001 * record.B.Value, length ) ;
      
    }
    
    public double SteelWeightPerLength( IStructuralMaterial material )
    {
      var record = _database.GetRecord( _specKey, material.ShapeType, material.Name ) ;
      return record.Weight ?? 0.0 ;
    }
    
    public void ChangeSpec( SteelSpecificationType type ) => _specKey = type.ToSpecKey() ;
  }
}