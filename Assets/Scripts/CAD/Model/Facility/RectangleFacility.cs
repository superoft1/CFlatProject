using System.Collections ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.Plotplan
{

    [Entity(EntityType.Type.LeafUnit)]//暫定
    public class RectangleFacility : Facility
  {
    public Vector3 Origin = Vector3.zero ;
    public Vector3 Size = Vector3.zero ;

    protected RectangleFacility( Document document ) : base( document )
    {
    }
  }
}