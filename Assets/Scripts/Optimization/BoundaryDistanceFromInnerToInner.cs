using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Plotplan
{
    public class BoundaryDistanceFromInnerToInner : BoundaryDistanceEvaluatorOnXYplane
    {
        
        public override double Different(Entity firstEntity, Entity secondEntity) 
        {
            var firstLeafUnit = (LeafUnit)firstEntity;
            var secondLeafUnit = (LeafUnit)secondEntity;
            var firstInner = firstLeafUnit.InnerBoundary;
            var secondInner = secondLeafUnit.InnerBoundary;
            return BoundaryDistance(firstInner, secondInner);
        }
        public override double Evaluate(double d)
        {
            return d;
        }
    }
}
