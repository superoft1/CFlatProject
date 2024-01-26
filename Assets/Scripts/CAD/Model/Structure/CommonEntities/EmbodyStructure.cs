using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.CommonEntities
{
  public abstract class EmbodyStructure : PlacementEntity
  {
    public abstract IEnumerable<IStructurePart> StructureElements { get ; }

    protected EmbodyStructure( Document document ) : base( document )
    {}
  } ;
}