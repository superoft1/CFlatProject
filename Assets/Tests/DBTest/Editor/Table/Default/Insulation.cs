using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class Insulation
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetTypeList()
    {
      var table = Chiyoda.DB.DB.Get<InsulationTable>() ;
      Assert.NotNull( table );
      
      var typeList = table.GetCodeList() ;
      Assert.NotNull( typeList );
      Assert.NotZero( typeList.Count );
      
      Assert.IsTrue( typeList.Contains( "HC1G" ) );
      Assert.IsTrue( typeList.Contains( "AS1D" ) );
    }
    
    [Test]
    public void GetThickness()
    {
      var table = Chiyoda.DB.DB.Get<InsulationTable>() ;
      Assert.NotNull( table );
 
      Assert.AreEqual( 30.0, table.GetThickness( "HC1G",  25, 50f ) );
      Assert.AreEqual( 30.0, table.GetThickness( "HC1G",  25, 99f ) );
      Assert.AreEqual( 30.0, table.GetThickness( "HC1G",  25, 100f ) );
      Assert.AreEqual( 50.0, table.GetThickness( "HC1G",  25, 101f ) );
      Assert.AreEqual( 50.0, table.GetThickness( "HC1G",  25, 102f ) );
      Assert.AreEqual( 150.0, table.GetThickness( "HC1G",  25, 600f ) );
      Assert.AreEqual( 150.0, table.GetThickness( "HC1G",  25, 601f ) );
      
      Assert.AreEqual( 50.0, table.GetThickness( "CC1G", 700, 51f ) ) ;
      Assert.AreEqual( 50.0, table.GetThickness( "CC1G", 700, 50f ) ) ;
      Assert.AreEqual( 210.0, table.GetThickness( "CC1G", 700, -149f ) ) ;
      Assert.AreEqual( 210.0, table.GetThickness( "CC1G", 700, -150f ) ) ;
      Assert.AreEqual( 220.0, table.GetThickness( "CC1G", 700, -151f ) ) ;
      Assert.AreEqual( 220.0, table.GetThickness( "CC1G", 700, -152f ) ) ;
      Assert.AreEqual( 220.0, table.GetThickness( "CC1G", 700, -170f ) ) ;
      Assert.AreEqual( 220.0, table.GetThickness( "CC1G", 700, -171f ) ) ;
      
      // 不可能な組み合わせの場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetThickness( "NoType", 50, 40.0f ));
    }
  }
}