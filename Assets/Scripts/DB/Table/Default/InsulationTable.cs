using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Default ;
using Chiyoda.DB.Entity.Standard ;


namespace Chiyoda.DB
{
  public class InsulationTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Code { get ; internal set ; }
      public double NPS { get ; internal set ; }
      public double Temperature { get ; internal set ; }
      public double Thickness { get ; internal set ; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from insuThickness in _referenceDB.GetTable<InsulationThickness>(context)
            join insuGroup in _referenceDB.GetTable<InsulationGroup>(context) on insuThickness.GroupID equals insuGroup.ID
            join insuCode in _referenceDB.GetTable<InsulationCode>(context) on insuGroup.ID equals insuCode.ThicknessGroupID
            join nps in _referenceDB.GetTable<STD_NPS>(context) on insuThickness.NPS equals nps.ID
            select new Record()
              {
                Code = insuCode.Code,
                NPS = nps.mm,
                Temperature = insuThickness.Temperature,
                Thickness = insuThickness.Thickness
              }
          ).Cast<RecordBase>().ToList() ;
      }
    }
    
    public IList<string> GetCodeList()
    {
      return Records.Select( rec => rec.Code ).Distinct().OrderBy( rec => rec ).ToList() ;
    }

    public double GetThickness( string code, int NPS, double temperature )
    {
      var cacheInCode = Records.Where( rec => rec.Code == code ) ;
      if(!cacheInCode.Any()) throw new NoRecordFoundException("Insulation code {code} does not exist.");
      var NPSList = cacheInCode.Select( rec => rec.NPS ).Distinct().ToList() ;
      var temperatureList = cacheInCode.Select( rec => rec.Temperature ).Distinct().ToList() ;

      var NPSCandidates = NPSList.Where( rec => rec >= NPS ) ;
      var tempCandidates = temperatureList.Where( rec => rec >= temperature ) ;

      var NPSToEval = NPSCandidates.Any() ? NPSCandidates.Min() : NPSList.Max() ;
      var tempToEval = tempCandidates.Any() ? tempCandidates.Min() : temperatureList.Max() ;

      var hit = cacheInCode.Where( item => (item.NPS == NPSToEval) && (item.Temperature == tempToEval) ) ;
      if ( hit.Any() ) return hit.First().Thickness ;
      throw new NoRecordFoundException($"No record found for code = {code} and NPS = {NPS} in InsulationTable");
    }
  }
}