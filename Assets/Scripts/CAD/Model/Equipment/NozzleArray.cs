using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
    [Entity( EntityType.Type.NozzleArray )]
    public abstract class NozzleArray : Entity, IDiameterRange
    {
        private readonly MementoList<Nozzle> _nozzles;
        private readonly Memento<string> _prefixId ;
        private readonly Memento<Nozzle.Type> _type ;
        private readonly Memento<int> _nozzleCount ;
        private readonly Memento<Diameter> _nozzleDiameter ;
        private readonly Memento<double> _nozzleLength ;
        private readonly Memento<int> _initialConnectPointIndex ;
        private readonly Memento<DiameterRange> _diameterRange;
        
        DiameterRange IDiameterRange.DiameterRange => _diameterRange.Value;

        protected NozzleArray( Document document ) : base( document )
        {
            _nozzles = new MementoList<Nozzle>( this ) ;
            _nozzles.AfterNewlyItemChanged += ( sender, e ) =>
            {
                OnNewlyChildrenChanged( e.Convert<IElement>( pair => pair ) ) ;
                OnAfterNewlyValueChanged() ;
            } ;
            _nozzles.AfterHistoricallyItemChanged += ( sender, e ) =>
            {
                OnHistoricallyChildrenChanged( e.Convert<IElement>( pair => pair ) ) ;
                OnAfterHistoricallyValueChanged() ;
            } ;
            
            _prefixId = CreateMementoAndSetupValueEvents( "" ) ;
            _type = CreateMementoAndSetupValueEvents( Nozzle.Type.Suction ) ;
            _nozzleCount = CreateMementoAndSetupValueEvents( 0 ) ;
            _nozzleDiameter = CreateMementoAndSetupValueEvents(DiameterFactory.FromOutsideMeter(0.0) ) ;
            _nozzleLength = CreateMementoAndSetupValueEvents( 0.0 ) ;
            _initialConnectPointIndex = CreateMementoAndSetupValueEvents( 0 ) ;
            _diameterRange = CreateMementoAndSetupValueEvents(new DiameterRange(40, 500));
        }
        
        public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
        {
            base.CopyFrom( another, storage );

            var entity = another as NozzleArray;
            _nozzles.CopyFrom( entity._nozzles.Select( pair => pair ) ) ;
            _prefixId.CopyFrom( entity._prefixId.Value );
            _type.CopyFrom( entity._type.Value );
            _nozzleCount.CopyFrom( entity._nozzleCount.Value );
            _nozzleLength.CopyFrom( entity._nozzleLength.Value );
            _nozzleDiameter.CopyFrom( entity._nozzleDiameter.Value );
            _initialConnectPointIndex.CopyFrom( entity._initialConnectPointIndex.Value );
            _diameterRange.CopyFrom(entity._diameterRange.Value);
        }
        
        void IDiameterRange.ChangeRange(int minDiameterNpsMm, int maxDiameterNpsMm)
        {
            _diameterRange.Value.ChangeRange(minDiameterNpsMm, maxDiameterNpsMm);
        }

        public IEnumerable<Nozzle> Nozzles
        {
            get
            {
                foreach ( var nozzle in _nozzles ) {
                    yield return nozzle ;
                }
            }

            set
            {
                _nozzles.Clear();
                foreach ( var nozzle in value ) {
                    _nozzles.Add( nozzle );
                }
            }
        }
        
        public virtual int NozzleCount
        {
            get
            {
                return _nozzleCount.Value ;
            }
            set
            {
                var parent = Parent as Equipment ;
                for ( int i = 0 ; i < _nozzleCount.Value ; ++i ) {
                   parent.RemoveConnectPoint( _initialConnectPointIndex.Value + i ) ;
                }
                _nozzleCount.Value = value ;
                _nozzles.Clear();
                for ( int i = 0 ; i < value ; ++i ) {
                    var nozzle = this.Document.CreateEntity<NozzleOnPlane>() ;
                    nozzle.Length = this.NozzleLength ;
                    nozzle.Diameter = this.Diameter ;
                    nozzle.Name = this.PrefixId + (i+1) ;
                    nozzle.NozzleType = this.NozzleType ;
                    _nozzles.Add( nozzle ) ;

                    var cp = parent.AddNewConnectPoint( _initialConnectPointIndex.Value + i ) ;
                    cp.Diameter = this.Diameter ;
                }
            }
        }


        [UI.Property(UI.PropertyCategory.BaseData, "NozzleLength", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
        public double NozzleLength
        {
            get
            {
                return _nozzleLength.Value ;
            }
            set
            {
                _nozzleLength.Value = value ;
                foreach ( var nozzle in _nozzles ) {
                    nozzle.Length = value ;
                }
            }
        }
        
    [UI.Property(UI.PropertyCategory.BaseData, "Diameter", ValueType = UI.ValueType.DiameterRange, Visibility = UI.PropertyVisibility.Editable)]
    public int DiameterNpsMm
    {
      get
      {
        return Diameter.NpsMm;
      }

      set
      {
        Diameter = DiameterFactory.FromNpsMm(value);
      }
    }

    public Diameter Diameter
    {
      get
      {
        return _nozzleDiameter.Value;
      }
      set
      {
        _nozzleDiameter.Value = value;
        foreach ( var nozzle in _nozzles ) {
          nozzle.Diameter = value ;
        }
      }
     }

        public int InitialConnectPointIndex
        {
            get { return _initialConnectPointIndex.Value ; }
            set { _initialConnectPointIndex.Value = value ; }
        }

        public int ConnectPointIndex(Nozzle nozzle)
        {
            var index = Array.IndexOf( Nozzles.ToArray(), nozzle ) ;
            return _initialConnectPointIndex.Value + index ;
        }
        
        public bool ExistsNozzle( int nozzleNumber )
        {
            return nozzleNumber >= InitialConnectPointIndex && nozzleNumber < (InitialConnectPointIndex + NozzleCount) ;
        }

        public Nozzle GetNozzle( int nozzleNumber )
        {
            return _nozzles[ nozzleNumber - InitialConnectPointIndex ] ;
        }
        
        public string PrefixId
        {
            get { return _prefixId.Value ; }
            set
            {
                _prefixId.Value = value ;
                for (int i = 0 ; i < _nozzles.Count; ++i) {
                    var nozzle = _nozzles[ i ] ;
                    nozzle.Name = value + (i+1) ;
                }
            }
        }

        [UI.Property(UI.PropertyCategory.BaseData, "NozzleType", ValueType = UI.ValueType.Select, Visibility = UI.PropertyVisibility.Editable)]
        public Nozzle.Type NozzleType
        {
            get { return _type.Value ; }
            set
            {
                _type.Value = value ;
                foreach ( var nozzle in _nozzles ) {
                    nozzle.NozzleType = value ;
                }
            }
        }
        
        public override Bounds? GetGlobalBounds()
        {
            return _nozzles
                .Select(nozzle => {
                    BodyMap.Instance.TryGetBody( nozzle, out var body ) ;
                    return body?.GetGlobalBounds() ;
                })
                .UnionBounds();
        }
        
        public override IEnumerable<IElement> Children
        {
            get
            {
                foreach ( var nozzle in _nozzles ) yield return nozzle ;
            }
        }
    }

}