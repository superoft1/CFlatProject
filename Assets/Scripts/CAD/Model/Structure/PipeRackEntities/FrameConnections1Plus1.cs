using System.Linq ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  
  [Entity( EntityType.Type.FrameConnection1Plus1 )]
  internal class FrameConnections1Plus1 : FrameConnections<Connection1Plus1>
  {
    private TransverseFrame<FrameUnit1Plus1> _frame ; 

    public FrameConnections1Plus1( Document document ) : base( document )
    {}

    public override void Initialize( ITransverseFrame frame )
    {
      _frame = new TransverseFrame<FrameUnit1Plus1>( Document.History, new FrameUnit1Plus1( Document.History ), 6.0 ) ;
      base.Initialize( frame ) ;
    }

    public double LeftWidth
    {
      get => _frame.Width ;
      set => _frame.Width = value ;
    }

    public double RightWidth
    {
      get => _frame.Width ;
      set => _frame.Width = value ;
    }

    public double ConnectionWidth
    {
      get => Item.ConnectionLength ;
      set => _frame.Items.OfType<FrameUnit1Plus1>().ForEach( f => f.ConnectionLength = value ) ;
    }

    private FrameUnit1Plus1 Item => ( (FrameUnit1Plus1) _frame.Items.First() ) ;
  }
}