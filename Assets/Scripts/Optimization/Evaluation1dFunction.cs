using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System;
using Chiyoda.UI;

namespace Chiyoda.CAD.Plotplan
{
    public abstract class Evaluation1dFunction : EvaluationFunciton
    {

        public abstract double Evaluate(double d);
        public abstract double Different(Entity firstEntity, Entity secondEntity);
        public override double Evaluate(Entity firstEntity, Entity secondEntity)
        {
            double d = Different(firstEntity, secondEntity);
            return Evaluate(d);
        }


    }

}