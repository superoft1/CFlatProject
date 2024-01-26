using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model
{
  public class Pit : WorkPlane
  {
    private readonly Memento<PitRegion> _region ;

    public override IEnumerable<IRegion> Regions
    {
      get { yield return _region.Value ; }
    }

    public Pit( Document document ) : base( document )
    {
      _region = new Memento<PitRegion>( this ) ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;

      _region.Value = Document.CreateEntity<PitRegion>() ;
    }

    public PitRegion Shape => _region.Value ;
  }
}