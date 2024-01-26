using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Model;

namespace Chiyoda.Importer
{
  public interface IEntityDictionary
  {
    Entity GetEntityFromID( string id );
  }
}
