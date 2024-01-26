using System.Text.RegularExpressions ;
using NUnit.Framework ;
using Assert = UnityEngine.Assertions.Assert ;

namespace Tests.EditorMode.Editor.Importer.BlockPattern.Equipment
{
  public class EndTopTypePumpBlockPatternImporterTest
  {

    [Test]
    public void RegexTest()
    {
      var pattern = @"([^0-9]+)([0-9]?)$";
      Assert.AreEqual("DischargeMinLengthPipe1", Regex.Replace("DischargeMinLength1", pattern, "$1Pipe$2"))  ;
      Assert.AreEqual("DischargeMinLengthPipe2", Regex.Replace("DischargeMinLength2", pattern, "$1Pipe$2"))  ;
      Assert.AreEqual("SuctionEndPipe", Regex.Replace("SuctionEnd", pattern, "$1Pipe$2")) ;
    }
  }
}