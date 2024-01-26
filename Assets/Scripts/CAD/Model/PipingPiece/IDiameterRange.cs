using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chiyoda.CAD.Model
{
  public interface IDiameterRange
  {
    DiameterRange DiameterRange { get; }

    void ChangeRange(int minDiameterNpsMm, int maxDiameterNpsMm);
  }
}
