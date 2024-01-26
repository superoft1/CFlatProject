using UnityEngine ;

namespace Importer.CSV.AutoRouting
{
  internal interface IPositionInterpreter
  {
    Vector3d GetPosition( string strId, string strX, string strY, string strZ, double diameter, string serviceType ) ;

    Vector3d GetDirection( string strDir ) ;
  }
}