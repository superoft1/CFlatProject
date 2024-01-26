using UnityEngine;

namespace MTO
{
  public interface IPipingComponent
  {
    Vector3d Axis { get; }
    Vector3d Reference { get; }
    Vector3d[] ConnectPoints { get; }
    double[] Diameters { get; }

    double Length { get; }
    double Weight { get; }

    bool IsAxisymmetric { get; }
  }
}
