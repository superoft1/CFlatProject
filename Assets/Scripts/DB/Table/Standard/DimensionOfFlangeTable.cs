using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class DimensionOfFlangeTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get ; internal set ; }
      public int NPSmm { get ; internal set ; }
      public double NP { get ; internal set ; }
      public string EndPrepForFace { get ; internal set ; }
      public string EndPrepForPipe { get ; internal set ; }
      public double O { get; set; }
      public double tf { get; set; }
      public double X { get; set; }
      public double Y { get ; internal set ; }
      public double f { get ; internal set ; }
      
      public double Height
      {
        get { return Y + f; }
      }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimFlange in _referenceDB.GetTable<STD_DimensionOfFlange>(context)
           join dimFlangeFace in _referenceDB.GetTable<STD_DimensionOfFlangeFace>(context)
             on new { dimFlange.Standard, dimFlange.NPS, dimFlange.NP } equals
             new { dimFlangeFace.Standard, dimFlangeFace.NPS, dimFlangeFace.NP }
           join dimFlangeHeight in _referenceDB.GetTable<STD_DimensionOfFlangeHeight>(context)
             on new { dimFlange.Standard, dimFlange.NPS, dimFlange.NP } equals
             new { dimFlangeHeight.Standard, dimFlangeHeight.NPS, dimFlangeHeight.NP }
           join nps in _referenceDB.GetTable<STD_NPS>(context) on dimFlange.NPS equals nps.ID
           join sta in _referenceDB.GetTable<STD_Standard>(context) on dimFlange.Standard equals sta.ID
           join typeEP in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on dimFlangeHeight.EndPrep equals typeEP.ID
           join typeEP_F in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on dimFlangeFace.EndPrep equals typeEP_F.ID
           join rating in _referenceDB.GetTable<STD_RatingClass>(context) on dimFlange.NP equals rating.ID

           select new Record()
           {
             Standard = sta.Name,
             NPSmm = nps.mm,
             NP = rating.Rating,
             EndPrepForFace = typeEP_F.Code,
             EndPrepForPipe = typeEP.Code,
             O = dimFlange.O,
             tf = dimFlange.tf,
             X = dimFlange.X,
             Y = dimFlangeHeight.Y,
             f = dimFlangeFace.f
           }).Cast<RecordBase>().ToList();
      }
    }
    public Record Get( int NPSmm, Rating rating = null, string flangeType = "WN", string faceType = "RF" )
    {
      if(rating == null) rating = new Rating();

      var hit = Records.Where( rec => rec.NPSmm == NPSmm && 
                                     rec.Standard == rating.Standard && 
                                     rec.NP == rating.NP && 
                                     rec.EndPrepForPipe == flangeType && 
                                     rec.EndPrepForFace == faceType);
      if (hit.Any()) {
        return hit.First();
      }
      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} in DimensionOfFlangeTable");
    }
  }
}