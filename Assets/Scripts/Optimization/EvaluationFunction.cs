using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System;
using Chiyoda.UI;

namespace Chiyoda.CAD.Plotplan
{
    public abstract class EvaluationFunciton
    {
        public enum ValueType
        {
            Cost,
            Safty,
            Attractive,
            OtherWorth
        };

        public abstract double Evaluate(Entity firstEntity, Entity secondEntity);
  
    }
   
}
