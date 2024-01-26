using System.Linq ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Topology
{
  public abstract class BlockEdge : CompositeEdge
  {
    protected BlockEdge( Document document ) : base( document )
    {
    }
    
    protected internal override Edge DisassembleImpl()
    {
      UnbindAllRules() ;

      var allEdges = EdgeList.ToArray() ;
      
      ReleaseAllEdgesForDisassemble() ;
      
      var group = Document.CreateEntity<Group>() ;
      group.LocalCod = this.LocalCod ;
      using ( CAD.Topology.Group.ContinuityIgnorer( group ) ) {
        foreach ( var edge in allEdges ) {
          group.AddEdge( edge.DisassembleImpl() ) ;
        }
      }

      return group ;
    }

    protected abstract void ReleaseAllEdgesForDisassemble() ;
  }
}