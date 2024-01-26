using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IDF
{
  public class IDFComponentTable
  {
    public string Proj { get; set; }
    public string FileName { get; set; }
    public string Specification { get; set; }
    public string ComponentClass { get; set; }
    public string StockNumber { get; set; }
    /// <summary>
    /// In, Out, その他の順
    /// </summary>
    public string[] Sizes { get; set; }

    public override string ToString()
    {
      string s = Proj + "," + FileName + "," + Specification + "," + ComponentClass + "," + StockNumber + "," +
                 string.Join(",", Sizes.ToArray());

      return s;
    }

    public static string Header()
    {
      return
        "検証PRJ,Tag_(File名),Specification (Service Class),ComponentClass (Parts Name),StockNumber,In (Size-1),Out (Size-2)";

    }
  }
}