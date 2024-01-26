using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Plotplan
{
    public class BoundaryDistanceFromOuterToInner : BoundaryDistanceEvaluatorOnXYplane
    {
        public override double Different(Entity firstEntity, Entity secondEntity)
        {
            var firstLeafUnit = (LeafUnit)firstEntity;
            var secondLeafUnit = (LeafUnit)secondEntity;
            var firstInner = firstLeafUnit.OuterBoundary;
            var secondInner = secondLeafUnit.InnerBoundary;
            return BoundaryDistance(firstInner, secondInner);
        }
        public override double Evaluate(double d)
        {
            return d;
        }
    }
}
