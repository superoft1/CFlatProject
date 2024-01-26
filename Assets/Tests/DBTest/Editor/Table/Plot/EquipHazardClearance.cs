using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class EquipHazardClearance
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetClearance()
    {
      var table = Chiyoda.DB.DB.Get<EquipHazardClearanceTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      Assert.AreEqual( 9.15, table.GetClearance( EquipHazardGroup.Compressors, EquipHazardGroup.Compressors), delta ) ;
      Assert.AreEqual( 4.575, table.GetClearance( EquipHazardGroup.AirCooledHeatExchanger, EquipHazardGroup.HeatExchanger), delta ) ;
      Assert.AreEqual( 4.575, table.GetClearance( EquipHazardGroup.HeatExchanger, EquipHazardGroup.AirCooledHeatExchanger), delta ) ;
      Assert.AreEqual( 0.0, table.GetClearance( EquipHazardGroup.AnalyzerRooms, EquipHazardGroup.AnalyzerRooms), delta ) ;
    }
  }
}