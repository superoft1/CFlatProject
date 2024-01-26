using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Routing
{
  public interface IBranch : IElement
  {
    IEndPoint TermPoint { get ; }
    bool IsStart { get ; }

    IEnumerable<IBranch> Branches { get ; }
  }
}