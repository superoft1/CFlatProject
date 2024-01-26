using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Default ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class StructureElementTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Spec { get; internal set; }
      public string Type { get; internal set; }
      public string Standard { get; internal set; }
      public double? H_D { get; internal set; }
      public double? B { get; internal set; }	
      public double? tw_t	{ get; internal set; }
      public double? tf	{ get; internal set; }
      public double? AX	{ get; internal set; }
      public double? Ix	{ get; internal set; }
      public double? Sx	{ get; internal set; }
      public double? Sy	{ get; internal set; }
      public double? Rx	{ get; internal set; }
      public double? Ry	{ get; internal set; }
      public double? Weight { get; internal set; }    
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from record in _referenceDB.GetTable<StructureElement>(context)
          join eleType in _referenceDB.GetTable<StructureElementType>(context) on record.Type equals eleType.ID
          join stan in _referenceDB.GetTable<STD_Standard>(context) on record.Spec equals stan.ID
          select new Record()
          {
            Spec = stan.Name,
            Type = eleType.Type,
            Standard = record.Standard,
            H_D = record.H_D,
            B = record.B,
            tw_t = record.tw_t,
            tf = record.tf,
            AX = record.AX,
            Ix = record.Ix,
            Sx = record.Sx,
            Sy = record.Sy,
            Rx = record.Rx,
            Ry = record.Ry,
            Weight = record.Weight
          }).Cast<RecordBase>().ToList();
      }
    }

    public Record Get(string spec, string type, string standard)
    {
      var result = Records.FirstOrDefault(rec => rec.Spec == spec && rec.Type == type && rec.Standard == standard);
      if ( result != null ) return result ;
      throw new NoRecordFoundException($"No record found for Spec = {spec}, type = {type} and standard = {standard} in DimensionOfReducerTable");
    }

    public IList<string> GetSpecList()
    {
      return Records.Select( rec => rec.Spec ).Distinct().ToList() ;
    }

    public IList<string> GetTypeList()
    {
      return Records.Select( rec => rec.Type ).Distinct().ToList() ;
    }

    public IList<string> GetStandardList( string spec, string type )
    {
      var result = Records.Where(rec => rec.Spec == spec && rec.Type == type);
      if ( result.Any() ) return result.Select( rec => rec.Standard ).ToList() ;
      throw new NoRecordFoundException($"No record found for Spec = {spec}, type = {type} in DimensionOfReducerTable");
    }
  }
}