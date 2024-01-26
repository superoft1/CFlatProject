using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class StructureElement
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetSpecList()
    {
      var table = Chiyoda.DB.DB.Get<StructureElementTable>();
      Assert.IsNotNull(table);

      // Spec一覧を取得
      {
        var specList = table.GetSpecList();
        Assert.True( specList.Contains("JIS") );
        Assert.True( specList.Contains("ASTM") );
        Assert.True( specList.Contains("BS") );
        Assert.True( specList.Contains("RC") );
        Assert.True( specList.Contains("EURO") );
        Assert.AreEqual( 5,specList.Count() );
      }
    }

    [Test]
    public void GetTypeList()
    {
      var table = Chiyoda.DB.DB.Get<StructureElementTable>();
      Assert.IsNotNull(table);

      // Type一覧を取得
      {
        var typeList = table.GetTypeList();
        Assert.True( typeList.Contains("H") );
        Assert.True( typeList.Contains("C") );
        Assert.True( typeList.Contains("L") );
        Assert.True( typeList.Contains("PIPE") );
        Assert.True( typeList.Contains("RB") );
        Assert.True( typeList.Contains("RC") );
        Assert.True( typeList.Contains("TEE") );
        Assert.True( typeList.Contains("FB") );
        Assert.AreEqual( 8,typeList.Count() );
      }
    }

    [Test]
    public void GetStandardList()
    {
      var table = Chiyoda.DB.DB.Get<StructureElementTable>();
      Assert.IsNotNull(table);

      {
        var standardList = table.GetStandardList( "JIS", "RB" ) ;
        Assert.True( standardList.Contains("RB16") );
        Assert.True( standardList.Contains("RB19") );
        Assert.True( standardList.Contains("RB20") );
        Assert.True( standardList.Contains("RB22") );
        Assert.AreEqual( 4,standardList.Count() );
      }
    }
    
    
    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<StructureElementTable>();
      Assert.IsNotNull(table);
      
      var delta = Tolerance.AssertEqualDelta ;
      
      { // NULLがないケース
        var record = table.Get("EURO", "H", "IPE100");
        Assert.AreEqual(100.0, record.H_D, delta);
        Assert.AreEqual(55.0, record.B, delta);
        Assert.AreEqual(4.1, record.tw_t, delta);
        Assert.AreEqual(5.7, record.tf, delta);
        Assert.AreEqual(10.3, record.AX, delta);
        Assert.AreEqual(171.0, record.Ix, delta);
        Assert.AreEqual(34.2, record.Sx, delta);
        Assert.AreEqual(5.8, record.Sy, delta);
        Assert.AreEqual(4.07, record.Rx, delta);
        Assert.AreEqual(1.24, record.Ry, delta);
        Assert.AreEqual(8.1, record.Weight, delta);
      }

      { // NULLがあるケース
        var record = table.Get("BS", "PIPE", "P-139.7x5.0");
        Assert.AreEqual(139.7, record.H_D, delta);
        Assert.IsFalse(record.B.HasValue);
        Assert.AreEqual(5.0, record.tw_t, delta);
        Assert.IsFalse(record.tf.HasValue);
        Assert.AreEqual(21.2, record.AX, delta);
        Assert.AreEqual(481.0, record.Ix, delta);
        Assert.AreEqual(68.8, record.Sx, delta);
        Assert.IsFalse(record.Sy.HasValue);
        Assert.AreEqual(4.77, record.Rx, delta);
        Assert.IsFalse(record.Ry.HasValue);
        Assert.AreEqual(16.6, record.Weight, delta);
      }

      { // Ixが大きいケース
        var record = table.Get("ASTM", "TEE", "WT7x199");
        Assert.AreEqual(232.3, record.H_D, delta);
        Assert.AreEqual(421.4, record.B, delta);
        Assert.AreEqual(45.0, record.tw_t, delta);
        Assert.AreEqual(72.3, record.tf, delta);
        Assert.AreEqual(377.4, record.AX, delta);
        Assert.AreEqual(10697.15, record.Ix, delta);
        Assert.AreEqual(616.15, record.Sx, delta);
        Assert.AreEqual(2146.71, record.Sy, delta);
        Assert.AreEqual(5.33, record.Rx, delta);
        Assert.AreEqual(10.95, record.Ry, delta);
        Assert.AreEqual(296.15, record.Weight, delta);
      }

      // 存在しない組み合わせを指定するとException
      Assert.Throws<NoRecordFoundException>(() => table.Get("EURO","PIPE","P-76.1x3.1"));
    }
  }
}