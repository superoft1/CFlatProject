using System ;
using System.Linq;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.ConcentricPipingReducerCombination )]
  public class ConcentricPipingReducerCombination : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      LargeTerm,
      SmallTerm,
    }

    public ConnectPoint LargeTermConnectPoint => GetConnectPoint( (int) ConnectPointType.LargeTerm );
    public ConnectPoint SmallTermConnectPoint => GetConnectPoint( (int) ConnectPointType.SmallTerm );

    public IEnumerable<( Vector3d point, Diameter diameter )> InnerConnectPoints => _innerConnectPoints;

    private readonly Memento<double> _length;
    private readonly Memento<bool> _enableChangeSizeLevelPropagation;
    private readonly Memento<double> _insulationThickness;
    private readonly MementoList<( Vector3d point, Diameter diameter )> _innerConnectPoints;

    private double _designatedLength;
    private readonly List<string> _stockNumbers = new List<string>();

    public ConcentricPipingReducerCombination( Document document ) : base( document )
    {
      _length = new Memento<double>( this );
      _enableChangeSizeLevelPropagation = new Memento<bool>( this, true );
      _insulationThickness = new Memento<double>( this, 0 );
      _innerConnectPoints = new MementoList<( Vector3d point, Diameter diameter )>( this );

      ComponentName = "ConcentricPipingReducer";
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.LargeTerm );
      AddNewConnectPoint( (int) ConnectPointType.SmallTerm );
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects();

      LargeTermConnectPoint.AfterNewlyDiameterChanged += OnDiameterChanged;
      SmallTermConnectPoint.AfterNewlyDiameterChanged += OnDiameterChanged;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as ConcentricPipingReducerCombination;
      _length.CopyFrom( entity._length.Value );
      _enableChangeSizeLevelPropagation.CopyFrom( entity.EnableChangeSizeLevelPropagation );
      _insulationThickness.CopyFrom( entity._insulationThickness.Value );
      _innerConnectPoints.CopyFrom( entity._innerConnectPoints );
      _designatedLength = entity._designatedLength;
      _stockNumbers.AddRange( entity._stockNumbers );
    }

    public override void ChangeSizeNpsMm( int connectPointNumber, int newDiameterNpsMm )
    {
      if ( EnableChangeSizeLevelPropagation )
      {
        var cp = GetConnectPoint( connectPointNumber );
        var beforeDiameter = cp.Diameter.OutsideMeter;
        var afterDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ).OutsideMeter;
        _designatedLength *= (afterDiameter / beforeDiameter);
        base.ChangeSizeNpsMm( connectPointNumber, newDiameterNpsMm );
      }
      else
      {
        GetConnectPoint( connectPointNumber ).Diameter = DiameterFactory.FromNpsMm( newDiameterNpsMm );
        // 他のConnectPointには伝播しない
      }
    }

    public bool EnableChangeSizeLevelPropagation
    {
      get => _enableChangeSizeLevelPropagation.Value;
      set => _enableChangeSizeLevelPropagation.Value = value;
    }

    public double Length
    {
      get { return _length.Value; }
      set
      {
        _designatedLength = value;
        UpdateCombination();
      }
    }

    public double LargeDiameter
    {
      get
      {
        return LargeTermConnectPoint.Diameter.OutsideMeter;
      }
      set
      {
        LargeTermConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( value );
      }
    }

    public double SmallDiameter
    {
      get
      {
        return SmallTermConnectPoint.Diameter.OutsideMeter;
      }
      set
      {
        SmallTermConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( value );
      }
    }

    public int CombinationCount => _innerConnectPoints.Count() + 1;

    public IEnumerable<( ( Vector3d point, Diameter diameter ) largeTerm,
                         ( Vector3d point, Diameter diameter ) smallTerm )> CombinationConnectPoints
    {
      get
      {
        var connectPoints = new List<( Vector3d point, Diameter diameter )>();

        connectPoints.Add( ( LargeTermConnectPoint.Point, LargeTermConnectPoint.Diameter ) );
        foreach ( var innerConnectPoint in InnerConnectPoints ) {
          connectPoints.Add( ( innerConnectPoint.point, innerConnectPoint.diameter ) );
          connectPoints.Add( ( innerConnectPoint.point, innerConnectPoint.diameter ) );
        }
        connectPoints.Add( ( SmallTermConnectPoint.Point, SmallTermConnectPoint.Diameter ) );

        return connectPoints.Select( ( diameter, i ) => new { value = diameter, index = i } )
                            .GroupBy( entry => entry.index / 2, entry => entry.value )
                            .Select( group => ( group.First(), group.Last() ) );
      }
    }

    [UI.Property( UI.PropertyCategory.ComponentName, "ID", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.Hidden )]
    public new string StockNumber { get; set; } // Componentで定義されているPropertyを非表示へ

    [UI.Property( UI.PropertyCategory.StockNumber, "ID", Visibility = UI.PropertyVisibility.Editable )]
    public IList<string> StockNumbers => _stockNumbers;

    [UI.Property( UI.PropertyCategory.Dimension, "Diameter", Visibility = UI.PropertyVisibility.ReadOnly )]
    private IList<string> CombinationStr
    {
      get
      {
        var combinationStr = new List<string>();
        var table = DB.DB.Get<DB.NPSTable>();
        foreach ( var connectPoint in CombinationConnectPoints ) {
          var largeRecord = table.Records.First( record => record.mm == connectPoint.largeTerm.diameter.NpsMm );
          var smallRecord = table.Records.First( record => record.mm == connectPoint.smallTerm.diameter.NpsMm );
          combinationStr.Add( $"{largeRecord.InchiStr} x {smallRecord.InchiStr} in" );
        }
        return combinationStr;
      }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Thickness", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double InsulationThickness
    {
      get { return _insulationThickness.Value; }
      set { _insulationThickness.Value = value < 0.0 ? 0.0 : value; }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Code", ValueType = UI.ValueType.Text, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string InsulationCode { get; set; }

    public override Bounds GetBounds()
    {
      var size = new Vector3d( Length, LargeDiameter + InsulationThickness * 2, LargeDiameter + InsulationThickness * 2 );
      return new Bounds( (Vector3)Origin, (Vector3)size );
    }

    private void OnDiameterChanged( object sender, EventArgs e )
    {
      UpdateCombination();
    }

    private void UpdateCombination()
    {
      try {
        var table = DB.DB.Get<DB.DimensionOfReducerTable>();
        var records = table.Get( LargeTermConnectPoint.Diameter.NpsMm, SmallTermConnectPoint.Diameter.NpsMm );

        var length = records.Sum( record => record.Height ).Millimeters();
        UpdateLength( length );

        _innerConnectPoints.Clear();
        var prevPoint = LargeTermConnectPoint.Point;
        foreach ( var record in records.Skip( 1 ) ) {
          var point = prevPoint - record.Height.Millimeters() * Axis;
          var diameter = DiameterFactory.FromNpsMm( record.NPS_L_mm );
          _innerConnectPoints.Add( ( point, diameter ) );
          prevPoint = point;
        }

        _stockNumbers.Clear();
        _stockNumbers.AddRange( Enumerable.Repeat( "", records.Count ) ); // FIXME: StockNumberをデータベースより取得する
      }
      catch ( DB.NoRecordFoundException ex ) {
//        if ( 0 != LargeTermConnectPoint.Diameter.NpsMm && 0 != SmallTermConnectPoint.Diameter.NpsMm ) {
//          var table = DB.DB.Get<DB.NPSTable>();
//          var largeRecord = table.Records.First( record => record.mm == LargeTermConnectPoint.Diameter.NpsMm );
//          var smallRecord = table.Records.First( record => record.mm == SmallTermConnectPoint.Diameter.NpsMm );
//          Debug.LogError( $"Valid concentric reducer combination not found. {largeRecord.InchiStr} x {smallRecord.InchiStr} in" );
//        }

        if ( LargeTermConnectPoint.Diameter.NpsMm == SmallTermConnectPoint.Diameter.NpsMm ) {
          UpdateLength( 0.0 );
        }
        else {
          UpdateLength( _designatedLength ); // Reducerの組み合わせが存在しない ひとまず旧アルゴリズムで表現する
        }

        _innerConnectPoints.Clear();

        _stockNumbers.Clear();
        _stockNumbers.Add( "NoValidCombination" );
      }
    }

    private void UpdateLength( double length )
    {
      LargeTermConnectPoint.SetPointVector( 0.5 * length * Axis );
      SmallTermConnectPoint.SetPointVector( -0.5 * length * Axis );

      _length.Value = length;
    }
  }
}