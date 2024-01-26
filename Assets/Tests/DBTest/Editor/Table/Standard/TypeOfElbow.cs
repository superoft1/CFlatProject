using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class TypeOfElbow
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<TypeOfElbowTable>();
      Assert.IsNotNull(table);

      var codeList = table.GetCodeList();
      Assert.NotNull(codeList);
      Assert.NotZero(codeList.Count);

      Assert.IsTrue(codeList.Contains("90deg"));
      Assert.IsTrue(codeList.Contains("S90deg"));
      Assert.IsTrue(codeList.Contains("45deg"));
    }
  }
}