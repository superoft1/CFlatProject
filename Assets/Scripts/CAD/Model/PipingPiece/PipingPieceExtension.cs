using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  static class PipingPieceExtension
  {
    [UI.Property( UI.PropertyCategory.Position, "LocalPosition", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public static Vector3d GetLocalPosition( this PipingPiece pipingPiece )
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if ( null == edge ) {
        throw new NullReferenceException();
      }

      return edge.LocalCod.Origin;
    }

    [UI.Property( UI.PropertyCategory.Position, "LocalPosition" )]
    public static void SetLocalPosition( this PipingPiece pipingPiece, Vector3d pos )
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if ( null == edge ) {
        throw new NullReferenceException();
      }
      edge.LocalCod = new LocalCodSys3d( pos, edge.LocalCod );
    }

    [UI.Property(UI.PropertyCategory.Position, "WorldPosition", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public static Vector3d GetWorldPosition(this PipingPiece pipingPiece)
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if (null == edge)
      {
        throw new NullReferenceException();
      }

      return edge.GlobalCod.Origin;
    }

    [UI.Property(UI.PropertyCategory.Position, "WorldPosition")]
    public static void SetWorldPosition(this PipingPiece pipingPiece, Vector3d pos)
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if (null == edge)
      {
        throw new NullReferenceException();
      }
      pos = edge.ParentCod.LocalizePoint(pos);
      edge.LocalCod = new LocalCodSys3d(pos, edge.LocalCod);
    }

    [UI.Property( UI.PropertyCategory.Position, "Rotation Angle", IsEditableForBlockPatternChildren = false )]
    public static double GetHorizontalRotationDegree( this PipingPiece pipingPiece )
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if ( null == edge ) {
        throw new NullReferenceException();
      }

      return edge.ExtraHorizontalRotationDegree;
    }

    [UI.Property( UI.PropertyCategory.Position, "Rotation Angle" )]
    public static void SetHorizontalRotationDegree( this PipingPiece pipingPiece, double angleDegree )
    {
      var edge = pipingPiece.Parent as LeafEdge;
      if ( null == edge ) {
        throw new NullReferenceException();
      }

      edge.ExtraHorizontalRotationDegree = angleDegree;
    }

    //[UI.Property( UI.PropertyCategory.Dimension, "Diameter", ValueType = UI.ValueType.DiameterLevel, Visibility = UI.PropertyVisibility.Editable )]
    //public static int GetHorizontalRotation( this PipingPiece pipingPiece )
    //{
    //  var edge = pipingPiece.Parent as LeafEdge;
    //  if ( null == edge ) {
    //    throw new NullReferenceException();
    //  }

    //  return 8;
    //}

    //[UI.Property( UI.PropertyCategory.Dimension, "Diameter" )]
    //public static void SetHorizontalRotation( this PipingPiece pipingPiece, int dlv )
    //{
    //}

  }
}
