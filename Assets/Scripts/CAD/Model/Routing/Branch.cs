using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Routing
{
  public class Branch : Entity, IBranch
  {
    private readonly Memento<IEndPoint> _term ;
    private readonly MementoList<IBranch> _branches ;

    public Branch( Document document ) : base( document )
    {
      _term = CreateMementoAndSetupChildrenEvents<IEndPoint>() ;
      _branches = CreateMementoListAndSetupChildrenEvents<IBranch>() ;
    }

    public void Initialize( IEndPoint p, bool isStart )
    {
      _term.Value = p ;
      IsStart = isStart ;
    }

    public void AddBranch( IBranch b )
    {
      _branches.Add( b ) ;
      b.TermPoint.Name =  b.IsStart ? "From" : "To" ;
      if ( b is Entity e ) {
        e.Name = $"Branch-{_branches.Count}" ;
      }
    } 

    public IEndPoint TermPoint => _term.Value ;
    
    public bool IsStart { get ; private set ; }

    public IEnumerable<IBranch> Branches => _branches ;

    public override IEnumerable<IElement> Children
    {
      get
      {
        if ( null != TermPoint ) {
          yield return TermPoint ;
        }
        foreach ( var b in _branches ) {
          yield return b ;
        }
      }
    }

    public override Bounds? GetGlobalBounds() => _term.Value.GetGlobalBounds() ;
  }
}