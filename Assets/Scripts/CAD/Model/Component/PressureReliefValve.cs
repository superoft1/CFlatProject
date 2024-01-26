using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using IDF ;
using UnityEngine ;


namespace Chiyoda.CAD.Model
{
    [Entity( EntityType.Type.PressureReliefValve )]
    public class PressureReliefValve : Component
    {
        public enum ConnectPointType
        {
            InletTerm,
            OutletTerm,
        }

        public ConnectPoint InletTermConnectPoint => GetConnectPoint( (int) ConnectPointType.InletTerm ) ;
        public ConnectPoint OutletTermConnectPoint => GetConnectPoint( (int) ConnectPointType.OutletTerm ) ;

        private readonly Memento<double> inletLength ;
        private readonly Memento<double> outletLength ;
        private readonly Memento<double> bonnetLength ;
        private readonly Memento<double> bonnetDiameter ;
        private readonly Memento<double> capLength ;
        private readonly Memento<double> capDiameter ;

        public override bool IsEndOfStream => true ;


        public PressureReliefValve( Document document ) : base( document )
        {
            inletLength = new Memento<double>( this ) ;
            outletLength = new Memento<double>( this ) ;
            bonnetLength = new Memento<double>( this ) ;
            bonnetDiameter = new Memento<double>( this ) ;
            capLength = new Memento<double>( this ) ;
            capDiameter = new Memento<double>( this ) ;

            ComponentName = "PressureReliefValve" ;
        }

        protected internal override void InitializeDefaultObjects()
        {
            base.InitializeDefaultObjects() ;
            AddNewConnectPoint( (int) ConnectPointType.InletTerm ) ;
            AddNewConnectPoint( (int) ConnectPointType.OutletTerm ) ;
        }

        public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
        {
            base.CopyFrom( another, storage ) ;

            var entity = another as PressureReliefValve ;
            inletLength.CopyFrom( entity.inletLength.Value ) ;
            outletLength.CopyFrom( entity.outletLength.Value ) ;
            bonnetLength.CopyFrom( entity.bonnetLength.Value ) ;
            bonnetDiameter.CopyFrom( entity.bonnetDiameter.Value ) ;
            capLength.CopyFrom( entity.capLength.Value ) ;
            capDiameter.CopyFrom( entity.capDiameter.Value ) ;
        }

        public override void ChangeSizeNpsMm( int connectPointNumber, int newDiameterNpsMm )
        {
            var cp = GetConnectPoint( connectPointNumber ) ;
            var beforeDiameter = cp.Diameter.OutsideMeter ;
            var afterDiameter = ConnectPoint.GetDiameterOfNpsMm( newDiameterNpsMm ) ;
            var rate = afterDiameter / beforeDiameter ;
            InletLength *= rate ;
            InletDiameter *= rate ;
            OutletLength *= rate ;
            OutletDiameter *= rate ;
            BonnetLength *= rate ;
            BonnetDiameter *= rate ;
            CapLength *= rate ;
            CapDiameter *= rate ;

            base.ChangeSizeNpsMm( connectPointNumber, newDiameterNpsMm ) ;
        }


        public double InletLength
        {
            get { return inletLength.Value ; }
            set
            {
                inletLength.Value = value ;
                InletTermConnectPoint.SetPointVector( -Axis * inletLength.Value ) ;
            }
        }

        public double InletDiameter
        {
            get { return InletTermConnectPoint.Diameter.OutsideMeter ; }
            set { InletTermConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( value ) ; }
        }

        public double OutletLength
        {
            get { return outletLength.Value ; }
            set
            {
                outletLength.Value = value ;
                OutletTermConnectPoint.SetPointVector( SecondAxis * outletLength.Value ) ;
            }
        }

        public double OutletDiameter
        {
            get { return OutletTermConnectPoint.Diameter.OutsideMeter ; }
            set { OutletTermConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( value ) ; }
        }

        public double BonnetLength
        {
            get { return bonnetLength.Value ; }
            set { bonnetLength.Value = value ; }
        }

        public double BonnetDiameter
        {
            get { return bonnetDiameter.Value ; }
            set { bonnetDiameter.Value = value ; }
        }

        public double CapLength
        {
            get { return capLength.Value ; }
            set { capLength.Value = value ; }
        }

        public double CapDiameter
        {
            get { return capDiameter.Value ; }
            set { capDiameter.Value = value ; }
        }


        public override Bounds GetBounds()
        {
            var bounds = new Bounds( (Vector3) Origin, Vector3.zero ) ;
            bounds.Encapsulate( (Vector3) InletTermConnectPoint.Point ) ;
            bounds.Encapsulate( (Vector3) ( Axis * ( CapLength + BonnetLength ) ) ) ;
            bounds.Encapsulate( (Vector3) OutletTermConnectPoint.Point ) ;

            var maxDiameter = ( InletDiameter > BonnetDiameter ) ? InletDiameter : BonnetDiameter ;
            if ( CapDiameter > maxDiameter ) maxDiameter = CapDiameter ;
            var maxRadius = maxDiameter / 2 ;
            bounds.Encapsulate( (Vector3) ( SecondAxis * maxRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( -SecondAxis * maxRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( ThirdAxis * maxRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( -ThirdAxis * maxRadius ) ) ;

            var outletRadius = OutletDiameter / 2 ;
            bounds.Encapsulate( (Vector3) ( Axis * outletRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( -Axis * outletRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( ThirdAxis * outletRadius ) ) ;
            bounds.Encapsulate( (Vector3) ( -ThirdAxis * outletRadius ) ) ;

            return bounds ;
        }
    }
}
