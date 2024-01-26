using System.Collections.Generic;

namespace Importer.Checker
{
  class PipingComponent
  {
    public string Type { get; set; } = "";

    public string Tag { get; set; } = "";
              
    public List<string> Diameters { get; } = new List<string>();
  }
}
