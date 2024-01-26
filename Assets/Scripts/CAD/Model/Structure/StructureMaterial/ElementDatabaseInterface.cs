using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.DB ;

namespace Chiyoda.CAD.Model.Structure
{
  internal class ElementDatabaseInterface
  {
    private class TypeListCache
    {
      private readonly Dictionary<SteelShapeType, IList<StructureElementTable.Record>> _listCache 
        = new Dictionary<SteelShapeType, IList<StructureElementTable.Record>>();
      
      public (bool, IList<StructureElementTable.Record>) GetStandardList( SteelShapeType type )
      {
        return _listCache.TryGetValue( type, out var list )
          ? ( true, list ) : ( false, new List<StructureElementTable.Record>() ) ;
      }

      public void Cache( SteelShapeType type, IList<StructureElementTable.Record> records )
      {
        _listCache.Add( type, records );
      }
    }

    private class NameRecordCache
    {
      private readonly Dictionary<string, IList<StructureElementTable.Record>> _nameRecords 
        = new Dictionary<string, IList<StructureElementTable.Record>>();

      public (bool, StructureElementTable.Record) GetRecord( string name, SteelShapeType type, string spec )
      {
        if ( !_nameRecords.TryGetValue( name, out var list ) ) {
          return ( false, null ) ;
        }

        foreach ( var r in list ) {
          if ( ( r.Spec == spec ) && ( r.Type == type.ToKey() ) ) {
            return ( true, r ) ;
          } 
        }
        return ( false, null ) ;
      }

      public void Register( StructureElementTable.Record r )
      {
        if ( _nameRecords.ContainsKey( r.Standard ) ) {
          _nameRecords[r.Standard].Add( r );
          return ;
        }
        _nameRecords.Add( r.Standard, new List<StructureElementTable.Record>() { r } );
      }
    }

    private readonly StructureElementTable _table ;
    private readonly Dictionary<string, TypeListCache> _recordTable = new Dictionary<string, TypeListCache>();
    private readonly NameRecordCache _recordCache = new NameRecordCache();

    public ElementDatabaseInterface()
    {
      _table = DB.DB.Get<DB.StructureElementTable>() ;      
    }

    public IList<string> GetStandardList( string spec, SteelShapeType type )
    {
      return GetRecords( spec, type ).Select( r => r.Standard ).ToList() ;
    }
    
    public IList<StructureElementTable.Record> GetRecords( string spec, SteelShapeType type )
    {
      var listCache = GetListService( spec ) ;
      var (success, l) = listCache.GetStandardList( type ) ;
      if ( success ) {
        return l ;
      }

      var typeKey = type.ToKey() ;
      var list = new List<StructureElementTable.Record>() ;
      foreach ( var r in _table.GetStandardList( spec, typeKey )
        .Select( s => _table.Get( spec, typeKey, s ) )
        .OrderBy( r => r.H_D ) ) {
        list.Add( r );
        _recordCache.Register( r );
      }
      listCache.Cache( type, list ) ;
      return list ;
    }

    public StructureElementTable.Record GetRecord( string spec, SteelShapeType type, string name )
    {
      var (success, r ) = _recordCache.GetRecord( name, type, spec ) ;
      if ( success ) {
        return r ;
      }
      var record = _table.Get( spec, type.ToKey(), name ) ;
      _recordCache.Register( record );
      return record ;
    }
    private TypeListCache GetListService( string spec )
    {
      if ( _recordTable.TryGetValue( spec, out var s ) ) {
        return s ;
      } 
      var cache = new TypeListCache();
      _recordTable.Add( spec, cache );
      return cache ;
    }
  }
}