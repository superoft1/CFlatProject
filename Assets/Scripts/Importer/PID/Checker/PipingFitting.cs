using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PID
{
  class PipingFitting : IPipingFitting
  {
    // IPipingComponent
    public string ID { get; set; } = "";

    public string Type { get; set; } = "";

    // IPipingFitting
    public string Tag { get; set; } = "";

    public List<string> Diameters { get; } = new List<string>();

    // IConnectable
    public PipingSegment Owner { get; set; } = null;

    public List<( IConnectable component, int index )> Parents { get; } = new List<( IConnectable, int )>();
    public List<( IConnectable component, int index )> Children { get; } = new List<( IConnectable, int )>();

    public override string ToString()
    {
//      var orderedDiameters = Diameters.OrderByDescending( diameters => Regex.Match( diameters, "[0-9.]+" ).Value ).ToArray();
//      return $"{Type}, {Tag}, {string.Join( ", ", orderedDiameters )}";
      return $"{Type}, {Tag}, {string.Join( ", ", Diameters )}";
    }
  }
}
