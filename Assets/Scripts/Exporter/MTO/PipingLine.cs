using System.Collections.Generic;

namespace MTO
{
  class PipingLine
  {
    public string AreaId { get; set; } = "";

    public string LineId { get; set; } = "";

    public string ServiceClass { get; set; } = "";

    public string InsulationType { get; set; } = "";

    public List<List<IPipingComponent>> Components { get; } = new List<List<IPipingComponent>>();
  }
}
