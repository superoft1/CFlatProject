using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;

namespace Chiyoda.Importer
{
  public class GroupInfo
  {
    public readonly Dictionary<Line, Group> Line2Group = new Dictionary<Line, Group>();

    public Document Document { get; }
    public IGroup IGroup { get; }
    private bool AppendDirectlyToGroup { get ; }
      
    private string Path { get ; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="group"></param>
    /// <param name="path"></param>
    /// <param name="appendDirectlyToGroup">中間階層のGroupを作成せずに、直接コンストラクタで渡したgroup直下にLeafEdgeを追加する</param>
    public GroupInfo( Document doc, IGroup group, string path, bool appendDirectlyToGroup = false)
    {
      Document = doc;
      IGroup = group;
      AppendDirectlyToGroup = appendDirectlyToGroup ;
      Path = System.IO.Path.GetFileName( path ) ;
    }

    public (Line, IGroup) GetLineAndGroup( string lineId )
    {
      var line = Document.FindOrCreateLine( lineId );
      if ( AppendDirectlyToGroup ) {
        return ( line, IGroup ) ;
      }
      if ( Line2Group.TryGetValue( line, out var value ) ) {
        return (line, value);
      }
      else {
        var group = Document.CreateEntity<Group>();
        group.Name = Path ;
        Line2Group.Add( line, group );
        IGroup.AddEdge( group );
        return (line, group);
      }
    }
  }

}


