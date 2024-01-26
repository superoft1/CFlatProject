using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Core
{
  class EntityAttribute : Attribute
  {
    public Model.EntityType.Type EntityType { get; private set; }

    public EntityAttribute( Model.EntityType.Type type )
    {
      EntityType = type;
    }
  }
}
