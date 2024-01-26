using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class TypeOfEndPrep
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetCodeList()
    {
      var table = Chiyoda.DB.DB.Get<TypeOfEndPrepTable>() ;
      Assert.IsNotNull( table );

      var codeList = table.GetCodeList();
      Assert.NotNull(codeList);
      Assert.NotZero(codeList.Count);

      Assert.IsTrue(codeList.Contains("BW"));
      Assert.IsTrue(codeList.Contains("NPT"));
      Assert.IsTrue(codeList.Contains("RF"));
      Assert.IsTrue(codeList.Contains("SW"));
      Assert.IsTrue(codeList.Contains("PE"));
      Assert.IsTrue(codeList.Contains("BE"));
      Assert.IsTrue(codeList.Contains("FF"));
      Assert.IsTrue(codeList.Contains("RJ"));
      Assert.IsTrue(codeList.Contains("WN"));
    }
  }
}