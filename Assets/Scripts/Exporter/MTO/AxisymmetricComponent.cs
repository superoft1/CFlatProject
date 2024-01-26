using UnityEngine;

namespace MTO
{
  public abstract class AxisymmetricComponent : IPipingComponent
  {
    public Vector3d Axis => Vector3d.zero;
    public Vector3d Reference => Vector3d.zero;
    public abstract Vector3d[] ConnectPoints { get; }
    public abstract double[] Diameters { get; }

    public abstract double Length { get; }
    public abstract double Weight { get; }

    public bool IsAxisymmetric => true;
  }
}
