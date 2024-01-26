using UnityEngine ;

namespace Importer.CSV.AutoRouting
{
  internal class DefaultInterpreter : IPositionInterpreter
  {
    private readonly Vector3d _diff ;
    public DefaultInterpreter( Vector3d diff )
    {
      _diff = diff ;
    }

    public Vector3d GetPosition( string strId, string strX, string strY, string strZ,
      double diameter, string serviceType )
    {
      var x = double.Parse( strX ) / 1000.0 + _diff.x ;
      var y = -double.Parse( strY ) / 1000.0 - _diff.y ;
      var z = double.Parse( strZ ) / 1000.0 + _diff.z ;
      return new Vector3d( x, y, z ) ;
    }

    public Vector3d GetDirection( string strDir )
    {
      switch ( strDir ) {
        case "+X" : return new Vector3d( 1.0, 0.0, 0.0 ) ;
        case "-X" : return new Vector3d( -1.0, 0.0, 0.0 ) ;
        // unity座標系ではYが反対になる
        case "+Y" : return new Vector3d( 0.0, -1.0, 0.0 ) ;
        case "-Y" : return new Vector3d( 0.0, 1.0, 0.0 ) ;
        case "+Z" : return new Vector3d( 0.0, 0.0, 1.0 ) ;
        case "-Z" : return new Vector3d( 0.0, 0.0, -1.0 ) ;
      }

      return new Vector3d() ;
    }
  }
}