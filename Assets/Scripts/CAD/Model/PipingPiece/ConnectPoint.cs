using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology ;
using MaterialUI ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [System.Serializable]
  public sealed class ConnectPoint : MemorableObjectBase, ICopyable
  {
    public override History History => Parent.Document.History ;

    public IElement Parent { get; }

    public event EventHandler AfterNewlyDiameterChanged;

    private readonly Memento<Vector3d> _point;
    private readonly Memento<Vector3d> _vector ;
    private readonly Memento<Diameter> _diameter;
    private readonly Memento<string> _tag;
    private readonly Memento<int> _connectPointNumber;

    public ConnectPoint( PipingPiece pp, int connectPointNumber ) : this( pp, connectPointNumber, Vector3d.zero, Model.Diameter.Default() )
    {
    }

    public ConnectPoint( PipingPiece pp, int connectPointNumber, in Vector3d point, Diameter diameter ) : this( pp, connectPointNumber, point, point, diameter )
    {
    }

    public ConnectPoint( PipingPiece pp, int connectPointNumber, in Vector3d point, in Vector3d vector, Diameter diameter )
    {
      Parent = pp ;

      _point = CreateMementoAndSetupValueEvents( point ) ;
      _vector = CreateMementoAndSetupValueEvents( vector ) ;
      _diameter = CreateMementoAndSetupValueEvents( diameter ) ;
      _tag = CreateMementoAndSetupValueEvents<string>() ;
      _connectPointNumber = new Memento<int>( this, connectPointNumber ) ;

      _diameter.AfterNewlyValueChanged += ( sender, e ) => AfterNewlyDiameterChanged?.Invoke( this, EventArgs.Empty ) ;
    }

    public void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var cp = another as ConnectPoint ;
      _point.CopyFrom( cp._point.Value ) ;
      _vector.CopyFrom( cp._vector.Value ) ;
      _diameter.CopyFrom( cp._diameter.Value ) ;
      _tag.CopyFrom( cp._tag.Value ) ;
      _connectPointNumber.CopyFrom( cp._connectPointNumber.Value ) ;
    }

    public int ConnectPointNumber
    {
      get => _connectPointNumber.Value ;
    }

    public Vector3d Point
    {
      get { return _point.Value; }
    }

    public Vector3d Vector
    {
      get { return _vector.Value ; }
    }

    /// <summary>
    /// ConnectPointの向きはpointを正規化した値を登録
    /// </summary>
    /// <param name="point"></param>
    public void SetPointVector( Vector3d point )
    {
      SetPointVector( point, point.normalized );
    }

    public void SetPointVector( Vector3d point, Vector3d vector )
    {
      _point.Value = point ;
      _vector.Value = vector ;
    }

    public Vector3d GlobalPoint
    {
      get
      {
        if ( Parent.Parent is LeafEdge le ) {
          return le.GlobalCod.GlobalizePoint( Point ) ;
        }
        else {
          return Point ;
        }
      }
    }

    public Diameter Diameter
    {
      get { return _diameter.Value; }

      set { _diameter.Value = value; }
    }

    public string Tag { get => _tag.Value; set => _tag.Value = value; }

    public static double GetDiameterOfNpsMm(int diameterNpsMm)
    {
      return DiameterFactory.FromNpsMm(diameterNpsMm).OutsideMeter;
    }

    public Bounds? GetGlobalBounds()
    {
      return new Bounds( (Vector3)GlobalPoint, Vector3.zero ) ;
    }
  }
}