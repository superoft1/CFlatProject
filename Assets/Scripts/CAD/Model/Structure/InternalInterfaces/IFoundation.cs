using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface IFoundation : IRelocatable
  {
    bool IsShallow { get ; set ; }
    double Capacity { get ; set ; }
    
    double Load { get ; set ; }
  }
}