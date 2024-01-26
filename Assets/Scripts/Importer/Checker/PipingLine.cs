using System.Collections.Generic;

namespace Importer.Checker
{
  class PipingLine
  {
    public string LineID { get; set; } = "";

    public string Specification { get; set; } = "";

    public List<PipingComponent> Components { get; } = new List<PipingComponent>();
  }
}
