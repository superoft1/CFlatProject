using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Core
{
  public class Draggability
  {
    public bool X { get; private set; }
    public bool Y { get; private set; }
    public bool Z { get; private set; }

    public void Update( IEnumerable<IElement> selections )
    {
      X = true;
      Y = true;
      Z = true;

      foreach ( var elm in selections ) {
        switch ( elm ) {
          case Edge e:
            if ( null != elm.Ancestor<BlockEdge>() || e is Route) {
              X = false;
              Y = false;
              Z = false;
              return;
            }
            break;
          
          case CAD.Model.Structure.IFreeDraggablePlacement _ :
            break;

          case ElectricalDevices _ :
            break;

          default:
            X = false;
            Y = false;
            Z = false;
            return;
        }
      }
    }
  }
}
