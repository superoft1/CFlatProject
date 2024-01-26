using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public interface IUserDefinedRule
  {
    void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ) ;
  }

  public struct ComponentDiameterChangeInfo
  {
    public string EdgeName { get ; }
    public int ConnectPointNumber { get ; }
    public int OffsetDiameterLevel { get ; }

    public static implicit operator ComponentDiameterChangeInfo( (string, int) edgeNameAndConnectPointNumber )
    {
      return new ComponentDiameterChangeInfo( edgeNameAndConnectPointNumber.Item1, edgeNameAndConnectPointNumber.Item2 ) ;
    }
    public static implicit operator ComponentDiameterChangeInfo( (string, int, int) edgeNameConnectPointNumberAndOffsetDiameterLevel )
    {
      return new ComponentDiameterChangeInfo( edgeNameConnectPointNumberAndOffsetDiameterLevel.Item1, edgeNameConnectPointNumberAndOffsetDiameterLevel.Item2, edgeNameConnectPointNumberAndOffsetDiameterLevel.Item3 ) ;
    }

    public ComponentDiameterChangeInfo( string edgeName, int connectPointNumber )
      : this( edgeName, connectPointNumber, 0 )
    {
    }
    public ComponentDiameterChangeInfo( string edgeName, int connectPointNumber, int offsetDiameterLevel )
    {
      EdgeName = edgeName ;
      ConnectPointNumber = connectPointNumber ;
      OffsetDiameterLevel = offsetDiameterLevel ;
    }
  }

  internal class KeepProperty
  {
    public KeepProperty( string propertyName )
    {
      PropertyName = propertyName ;
    }
    /// <summary>
    /// 子要素のプロパティの場合は'.'で指定する
    /// 2段以上は未対応
    /// </summary>
    public string PropertyName { get ; }
    public double? DefaultValue { get ; set ; }
  }

  public class KeepPropertyHandler
  {
    private readonly List<KeepProperty> _keepProperties ;

    public KeepPropertyHandler(params string[] keepProperties)
    {
      _keepProperties = keepProperties.Select( k => new KeepProperty( k ) ).ToList() ;
    }

    public void Save(BlockPattern blockPattern)
    {
      foreach ( var keepProperty in _keepProperties ) {
        var prop = keepProperty.PropertyName.Split( '.' ).ToList() ;
        if ( prop.Count > 2 ) {
          // 2段以上の要素のネストは未対応
          throw new InvalidOperationException() ;
        }

        if ( prop.Count == 1 ) {
          keepProperty.DefaultValue = blockPattern.GetProperty( prop[ 0 ] )?.Value ;
        }
        else if ( prop.Count == 2 ) {
          var element = blockPattern.GetElementByName( prop[ 0 ] ) ;
          if ( element != null ) {
            keepProperty.DefaultValue = element.GetProperty( prop[ 1 ] )?.Value ;
          }
        }
      }
    }

    public void Load(BlockPattern blockPattern)
    {
      foreach ( var keepProperty in _keepProperties ) {
        var prop = keepProperty.PropertyName.Split( '.' ).ToList() ;
        if ( prop.Count > 2 ) {
          // 2段以上の要素のネストは未対応
          throw new InvalidOperationException() ;
        }

        if ( prop.Count == 1 && keepProperty.DefaultValue.HasValue ) {
          blockPattern.GetProperty( prop[ 0 ] ).Value = keepProperty.DefaultValue.Value ;
        }
        else if ( prop.Count == 2 ) {
          var element = blockPattern.GetElementByName( prop[ 0 ] ) ;
          if ( element != null && keepProperty.DefaultValue.HasValue ) {
            element.GetProperty( prop[ 1 ] ).Value = keepProperty.DefaultValue.Value ;
          }
        }
      }
    }
  }

  public class InterChangeablePipingRule : IUserDefinedRule
  {
    private readonly Nozzle _nozzle;
    private readonly List<(string name, string nozzle, Edge pattern)> _pipes;
    private readonly string _interChangeableName;
    private readonly KeepPropertyHandler _keepPropertyHandler ;

    public InterChangeablePipingRule(string interChangeableName, Nozzle nozzle, List<(string name, string nozzle, Edge pattern)> pipes, 
     params string[] keepProperties)
    {
      _nozzle = nozzle;
      _pipes = pipes;
      _interChangeableName = interChangeableName;
      _keepPropertyHandler = new KeepPropertyHandler(keepProperties) ;
    }

    public void Run(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (!(owner is BlockPattern ownerBlock)) return;

      var equipment = ownerBlock.Equipments.FirstOrDefault(e => e.GetConnectPointIndex(_nozzle.Name).HasValue);
      if (equipment == null) {
        Debug.LogError($"Equipment having {_nozzle.Name} is not found!!");
        return;
      }
      
      // プロパティが変化していないとConnectionMaintainerが発動しない。
      // （プロパティ設定により）Clone元の形状がおかしい場合があるので、ConnectionMaintainerが発動するようにRegisterConnectionMaintenanceを実施する
      ownerBlock.Document.RegisterConnectionMaintenance( equipment.LeafEdge );

      Edge blockPatternToRemove = GetBlockPatternToRemove(ownerBlock);
      var nzlVtx = GetNozzleVertex(equipment);
      if (nzlVtx.Partner != null) {
        var block = nzlVtx.Partner.Ancestor<BlockPattern>() ;
        _keepPropertyHandler.Save(block);
        nzlVtx.Partner = null;
      }

      var clonedBlockPattern = GetClonedBlockPattern(property);
      ownerBlock.ReplaceEdge( blockPatternToRemove, clonedBlockPattern ) ;
      if ( clonedBlockPattern.EdgeCount > 0 ) {
        nzlVtx.Partner = GetFreeVertexOfClonedInterchangeableElement(clonedBlockPattern);
        clonedBlockPattern.MoveLocalPos(nzlVtx.GlobalPoint - nzlVtx.Partner.GlobalPoint);

        // 配管径のプロパティ値を更新
        var prop = clonedBlockPattern.GetProperty("Diameter");
        if (prop != null) {
          prop.Value = _nozzle.Diameter.NpsMm;
        }
        _keepPropertyHandler.Load(clonedBlockPattern);
      }
     
      clonedBlockPattern.Name = GetBlockPatternName() ;
      clonedBlockPattern.ObjectName = GetBlockPatternName() ;
      clonedBlockPattern.RuleList.BindChangeEvents(true);
      ownerBlock.RuleList.BindChangeEvents(true);
    }

    private string GetBlockPatternName()
    {
      return $"{_nozzle.Name}Block" ;
    }

    private HalfVertex GetFreeVertexOfClonedInterchangeableElement(BlockPattern clonedBlockPattern)
    {
      if (!(clonedBlockPattern.GetElementByName(_interChangeableName) is LeafEdge inter)) {
        throw new InvalidOperationException();
      }
      var vs = inter.GetFreeVertex().ToList();
      if (vs.Count != 1) {
        throw new InvalidOperationException();
      }

      return vs[0];
    }

    private HalfVertex GetNozzleVertex(Equipment equipment)
    {
      var connectPointIndex = equipment.GetConnectPointIndex(_nozzle.Name);
      if (!connectPointIndex.HasValue) {
        throw new InvalidOperationException();
      }

      return equipment.LeafEdge.GetVertex(connectPointIndex.Value);
    }

    private BlockPattern GetClonedBlockPattern(IUserDefinedNamedProperty property)
    {
      Edge g;
      using (var storage = new CopyObjectStorage()) {
        g = _pipes[(int)property.Value].pattern.Clone(storage);
      }
      if ( g == null ) {
        return DocumentCollection.Instance.Current.CreateEntity<BlockPattern>();
      }
      return g as BlockPattern;
    }

    private Edge GetBlockPatternToRemove(BlockPattern ownerBlock)
    {
      return ownerBlock.EdgeList.FirstOrDefault( e => e.Name == GetBlockPatternName() ) ;
    }
  }
  
  
  public class AllComponentDiameterRangeRule : IUserDefinedRule
  {
    private readonly ComponentDiameterChangeInfo[] _edgeNameAndConnectPointNumbers;

    public AllComponentDiameterRangeRule(params ComponentDiameterChangeInfo[] edgeNameAndConnectPointNumbers)
    {
      _edgeNameAndConnectPointNumbers = edgeNameAndConnectPointNumbers;
    }

    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property )
    {
      foreach ( var ecn in _edgeNameAndConnectPointNumbers ) {
        var elm = owner.GetElementByName( ecn.EdgeName ) ;
        if ( elm is LeafEdge edge ) {
          RunByEdge( edge, ecn.ConnectPointNumber, (int)property.Value + ecn.OffsetDiameterLevel ) ;
        }
        else if ( elm is PipingPiece pp ) {
          RunByEdge( pp.LeafEdge, ecn.ConnectPointNumber, (int)property.Value + ecn.OffsetDiameterLevel ) ;
        }
        else {
          Debug.LogError( $"LeafEdge `{ecn.EdgeName}' is not found!!" ) ;
        }
      }
    }

    private static void RunByEdge(LeafEdge edge, int connectPointNumber, int newNpsMm)
    {
      var pp = edge.PipingPiece;
      if (null == pp) return;

      pp.ChangeSizeNpsMm(connectPointNumber, newNpsMm);

      var blockEdge = edge.Closest<BlockEdge>();

      foreach (var v in edge.Vertices)
      {
        if (v.ConnectPointIndex == connectPointNumber) continue;
        var v2 = v.Partner;
        if (null == v2) continue;
        
        var vertexDiameterNpsMm = v.ConnectPoint.Diameter.NpsMm;
        if (vertexDiameterNpsMm == v2.ConnectPoint.Diameter.NpsMm) continue;

        var nextLeafEdge = v2.LeafEdge;
        if ( null == nextLeafEdge ) {
          continue ;
        }
        if ( !(nextLeafEdge.PipingPiece is Equipment) && nextLeafEdge.Closest<BlockEdge>() != blockEdge ) {// 相手が機器の場合はノズルなので、ノズルの径は変更したい
          continue ; // BlockPatternから出た場合は終了
        }

        RunByEdge(nextLeafEdge, v2.ConnectPointIndex, vertexDiameterNpsMm);
      }
    }
  }
}