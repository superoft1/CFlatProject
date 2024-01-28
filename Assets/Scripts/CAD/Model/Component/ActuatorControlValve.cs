using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
    [Entity( EntityType.Type.ActuatorControlValve )]
    public class ActuatorControlValve : Component//, ILinearComponent
    {
        public enum ConnectPointType
        {
            Term1,
            Term2,
        }

        public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
        public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;

        private readonly Memento<double> oper_Dim_A;
        private readonly Memento<double> oper_Dim_B;
        private readonly Memento<double> oper_Dim_C;
        private readonly Memento<double> oper_Dim_D;

        public ActuatorControlValve( Document document ) : base( document )
        {
            oper_Dim_A = CreateMementoAndSetupValueEvents(0.0) ;
            oper_Dim_B = CreateMementoAndSetupValueEvents(0.0) ;
            oper_Dim_C = CreateMementoAndSetupValueEvents(0.0) ;
            oper_Dim_D = CreateMementoAndSetupValueEvents(0.0);

            ComponentName = "ActuatorControlValve";
        }
        protected internal override void InitializeDefaultObjects()
        {
            base.InitializeDefaultObjects();
            AddNewConnectPoint( (int) ConnectPointType.Term1 );
            AddNewConnectPoint( (int) ConnectPointType.Term2 );
        }

        public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
        {
            base.CopyFrom(another, storage);

            var entity = another as ActuatorControlValve;
            oper_Dim_A.CopyFrom(entity.oper_Dim_A.Value);
            oper_Dim_B.CopyFrom(entity.oper_Dim_B.Value);
            oper_Dim_C.CopyFrom(entity.oper_Dim_C.Value);
            oper_Dim_D.CopyFrom(entity.oper_Dim_D.Value);
        }

        public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
        {
            var cp = GetConnectPoint(connectPointNumber);
            var beforeDiameter = cp.Diameter.OutsideMeter;
            var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
            var rate = afterDiameter / beforeDiameter;
            Oper_Dim_A *= rate;
            Oper_Dim_B *= rate;
            Oper_Dim_C *= rate;
            Oper_Dim_D *= rate;
            base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
        }

        public double TermLenght
        {
            get { return Term1ConnectPoint.Diameter.OutsideMeter; }

            set
            {
                Term1ConnectPoint.Diameter = DiameterFactory.FromOutsideMeter(value); ;
                Term2ConnectPoint.Diameter = Term1ConnectPoint.Diameter;

                ApplyTermConnectPoint();
            }
        }

        public double Oper_Dim_A
        {
            get { return oper_Dim_A.Value; }
            set 
            { 
                oper_Dim_A.Value = value;
                ApplyTermConnectPoint();
            }
        }

        public double Oper_Dim_B
        {
            get { return oper_Dim_B.Value; }
            set { oper_Dim_B.Value = value; }
        }

        public double Oper_Dim_C
        {
            get { return oper_Dim_C.Value; }
            set { oper_Dim_C.Value = value; }
        }

        public double Oper_Dim_D
        {
            get { return oper_Dim_D.Value; }
            set { oper_Dim_D.Value = value; }
        }

        private void ApplyTermConnectPoint()
        {
            Term1ConnectPoint.SetPointVector(new Vector3d(-TermLenght / 2, -Oper_Dim_A + 0.3f, 0.26f));
            Term2ConnectPoint.SetPointVector(new Vector3d(TermLenght / 2, -Oper_Dim_A + 0.3f, 0.26f));
        }

        public override Bounds GetBounds()
        {
            var bounds = new Bounds((Vector3)Origin, Vector3.zero);
            bounds.Encapsulate((Vector3)Term1ConnectPoint.Point);
            bounds.Encapsulate((Vector3)Term2ConnectPoint.Point);
            bounds.Encapsulate((Vector3)(SecondAxis * TermLenght));

            var flowRadius = TermLenght / 2;
            bounds.Encapsulate((Vector3)(SecondAxis * flowRadius));
            bounds.Encapsulate((Vector3)(-SecondAxis * flowRadius));
            bounds.Encapsulate((Vector3)(ThirdAxis * flowRadius));
            bounds.Encapsulate((Vector3)(-ThirdAxis * flowRadius));

            var diaphramRadius = (Oper_Dim_B + Oper_Dim_C) / 2;
            bounds.Encapsulate((Vector3)(Axis * diaphramRadius));
            bounds.Encapsulate((Vector3)(-Axis * diaphramRadius));
            bounds.Encapsulate((Vector3)(ThirdAxis * diaphramRadius));
            bounds.Encapsulate((Vector3)(-ThirdAxis * diaphramRadius));
            return bounds;
        }
    }
}