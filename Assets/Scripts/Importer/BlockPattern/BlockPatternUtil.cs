using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using JetBrains.Annotations ;
using UnityEngine ;
using BP = Chiyoda.CAD.Topology.BlockPattern;

namespace Importer.BlockPattern
{
  public static class BlockPatternUtil
  {
    /// <summary>
    /// <see cref="BP"/> 内でX/Y/Zの各方向に伸びている各 <see cref="Pipe"/> 同士の <see cref="Pipe.FlexRatio"/> を、
    /// 現在の (<see cref="Pipe.Length"/> - <see cref="PipeManager.GetMinimumLength( Pipe )"/>) に応じて設定します。
    /// （<see cref="Pipe.FlexRatio"/> が0より大きいもののみ）
    /// </summary>
    /// <remarks>
    /// <para>呼び出しタイミングは、以下を満たすこと：</para>
    /// <list type="bullet">
    /// <item><description><see cref="Pipe"/> の <see cref="Pipe.MinimumLengthRatioByDiameter"/> を設定後（ <see cref="Pipe.MinimumLengthRatioByDiameter"/> の値を変更するため）</description></item>
    /// <item><description><see cref="LeafEdge"/> のインデックスを解決後（本関数内で <see cref="LeafEdge"/> を挿入するため）</description></item>
    /// <item><description>最初の <see cref="Document.MaintainEdgePlacement()"/> を呼び出す前（呼び出した際に形状が変更される可能性があるため）</description></item>
    /// </list>
    /// </remarks>
    /// <param name="bp">対象ブロックパターン</param>
    public static void UpdateFlexRatiosByLength( BP bp )
    {
      SetFlexRatiosByLength( bp.GetAllComponents().OfType<Pipe>().Where( pipe => 0 < pipe.FlexRatio ) ) ;
    }

    /// <summary>
    /// <see cref="BP"/> 内でX/Y/Zの各方向に伸びている各 <see cref="Pipe"/> 同士の <see cref="Pipe.FlexRatio"/> を、
    /// 現在の (<see cref="Pipe.Length"/> - <see cref="PipeManager.GetMinimumLength( Pipe )"/>) に応じて設定します。
    /// </summary>
    /// <remarks>
    /// <para>呼び出しタイミングは、以下を満たすこと：</para>
    /// <list type="bullet">
    /// <item><description><see cref="Pipe"/> の <see cref="Pipe.MinimumLengthRatioByDiameter"/> を設定後（ <see cref="Pipe.MinimumLengthRatioByDiameter"/> の値を変更するため）</description></item>
    /// <item><description><see cref="LeafEdge"/> のインデックスを解決後（本関数内で <see cref="LeafEdge"/> を挿入するため）</description></item>
    /// <item><description>最初の <see cref="Document.MaintainEdgePlacement()"/> を呼び出す前（呼び出した際に形状が変更される可能性があるため）</description></item>
    /// </list>
    /// </remarks>
    /// <param name="pipes">対象パイプ</param>
    public static void SetFlexRatiosByLength( params Pipe[] pipes )
    {
      SetFlexRatiosByLength( (IEnumerable<Pipe>) pipes ) ;
    }

    /// <summary>
    /// <see cref="BP"/> 内でX/Y/Zの各方向に伸びている各 <see cref="Pipe"/> 同士の <see cref="Pipe.FlexRatio"/> を、
    /// 現在の (<see cref="Pipe.Length"/> - <see cref="PipeManager.GetMinimumLength( Pipe )"/>) に応じて設定します。
    /// </summary>
    /// <remarks>
    /// <para>呼び出しタイミングは、以下を満たすこと：</para>
    /// <list type="bullet">
    /// <item><description><see cref="Pipe"/> の <see cref="Pipe.MinimumLengthRatioByDiameter"/> を設定後（ <see cref="Pipe.MinimumLengthRatioByDiameter"/> の値を変更するため）</description></item>
    /// <item><description><see cref="LeafEdge"/> のインデックスを解決後（本関数内で <see cref="LeafEdge"/> を挿入するため）</description></item>
    /// <item><description>最初の <see cref="Document.MaintainEdgePlacement()"/> を呼び出す前（呼び出した際に形状が変更される可能性があるため）</description></item>
    /// </list>
    /// </remarks>
    /// <param name="pipes">対象パイプ</param>
    public static void SetFlexRatiosByLength( IEnumerable<Pipe> pipes )
    {
      var pipesX = new List<Pipe>() ;
      var pipesY = new List<Pipe>() ;
      var pipesZ = new List<Pipe>() ;

      foreach ( var pipe in pipes ) {
        var le = pipe?.LeafEdge ;
        var bp = le?.Closest<BP>() ;
        if ( null == bp ) throw new InvalidOperationException( "Pipe must have a BlockPattern as its ancestor." ) ;

        if ( le.GetMinimumLength() <= 0 ) continue ;

        var axis = le.GetCodSysInAncestor( bp ).GlobalizeVector( pipe.Axis ) ;
        if ( axis.IsParallelTo( Vector3d.right ) ) {
          pipesX.Add( pipe ) ;
        }
        else if ( axis.IsParallelTo( Vector3d.up ) ) {
          pipesY.Add( pipe ) ;
        }
        else if ( axis.IsParallelTo( Vector3d.forward ) ) {
          pipesZ.Add( pipe ) ;
        }
      }

      SetFlexRatiosByLengthInDirection( pipesX ) ;
      SetFlexRatiosByLengthInDirection( pipesY ) ;
      SetFlexRatiosByLengthInDirection( pipesZ ) ;
    }

    private static void SetFlexRatiosByLengthInDirection( IEnumerable<Pipe> pipes )
    {
      foreach ( var pipe in pipes ) {
        var minLength = pipe.GetMinimumLength() ;
        var proportionalLength = Math.Max( 0, pipe.Length - minLength ) ;
        if ( 0 < minLength ) {
          SplitForSystemMinLength( pipe ) ;
        }

        pipe.FlexRatio = proportionalLength ;
      }
    }

    private static void SplitForSystemMinLength( Pipe pipe )
    {
      var le = pipe.LeafEdge ;

      var length = pipe.MinimumLengthWithoutOletRadius ;
      var maxOletWidth1 = le.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term1 ) ;
      var maxOletWidth2 = le.GetMaxOletWidth( (int) Pipe.ConnectPointType.Term2 ) ;

      if ( 0 < maxOletWidth1 ) {
        if ( 0 < maxOletWidth2 ) {
          // Term1側・Term2側を両方分割
          SplitForSystemMinLength( le, (int) Pipe.ConnectPointType.Term1, 0.5, maxOletWidth1 + length / 2, "_FixedAreaBefore" ) ;
          SplitForSystemMinLength( le, (int) Pipe.ConnectPointType.Term2, 0.5, maxOletWidth2 + length / 2, "_FixedAreaAfter" ) ;
        }
        else {
          // Term1側のみ分割
          SplitForSystemMinLength( le, (int) Pipe.ConnectPointType.Term1, 1.0, maxOletWidth1 + length, "_FixedAreaBefore" ) ;
        }
      }
      else {
        // Term2側のみ分割
        SplitForSystemMinLength( le, (int) Pipe.ConnectPointType.Term2, 1.0, maxOletWidth2 + length, "_FixedAreaAfter" ) ;
      }
    }

    private static void SplitForSystemMinLength( LeafEdge leafEdge, int connectPointIndex, double clonedPipeMinLenRatioByDia, double splitLength, string namePostfix )
    {
      var pipe = (Pipe) leafEdge.PipingPiece ;

      using ( var copyObjectStorage = new CopyObjectStorage() ) {
        var clonedLeafEdge = leafEdge.Clone( copyObjectStorage ) ;
        var clonedPipe = (Pipe) clonedLeafEdge.PipingPiece ;

        pipe.MinimumLengthRatioByDiameter = 0 ;
        pipe.Length -= splitLength ;

        clonedPipe.MinimumLengthRatioByDiameter = clonedPipeMinLenRatioByDia ;
        clonedPipe.Length = 0 ;
        if ( ! string.IsNullOrEmpty( clonedPipe.ObjectName ) ) {
          clonedPipe.ObjectName += namePostfix ;
        }

        if ( ! string.IsNullOrEmpty( clonedLeafEdge.ObjectName ) ) {
          clonedLeafEdge.ObjectName += namePostfix ;
        }

        InsertPipe( leafEdge, connectPointIndex, clonedLeafEdge ) ;
      }
    }

    private static void InsertPipe( LeafEdge leafEdge, int connectPointIndex, LeafEdge anotherLeafEdge )
    {
      int anotherConnectPointIndex ;
      switch ( (Pipe.ConnectPointType) connectPointIndex ) {
        case Pipe.ConnectPointType.Term1 :
          anotherConnectPointIndex = (int) Pipe.ConnectPointType.Term2 ;
          break ;

        case Pipe.ConnectPointType.Term2 :
          anotherConnectPointIndex = (int) Pipe.ConnectPointType.Term1 ;
          break ;

        default :
          throw new InvalidOperationException() ;
      }

      var v = leafEdge.GetVertex( connectPointIndex ) ;
      var orgPartner = v.Partner ;
      v.Partner = null ;

      var group = (IGroup) leafEdge.Parent ;
      using ( Group.ContinuityIgnorer( group ) ) {
        group.AddEdge( anotherLeafEdge ) ;
      }

      anotherLeafEdge.GetVertex( anotherConnectPointIndex ).Partner = v ;
      anotherLeafEdge.GetVertex( connectPointIndex ).Partner = orgPartner ;
    }
  }
}