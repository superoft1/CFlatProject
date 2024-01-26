using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Model ;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{

  public static class LeafEdgeCodSysUtils
  {
    public static void LocalizeStraightComponent(LeafEdge leafEdge, Vector3d origin, Vector3d direction)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;
      var vec = leafEdgeCodSys.LocalizeVector(direction);

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      Vector3d dir1;
      Vector3d dir2;
      LocalCodSys3d.CreateOtherDirections(vec, out dir1, out dir2);
      var rot = Quaternion.LookRotation((Vector3)dir2, (Vector3)dir1);

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * rot, beforeMir);
    }

    public static void LocalizeElbow90Component(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var cross = -Vector3d.Cross(axis, reference);
      var rot = Quaternion.LookRotation((Vector3)cross, (Vector3)axis);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }

    public static void LocalizeElbow45Component(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var outer = Vector3d.Cross(axis, reference);
      var upward = Vector3d.Cross(outer, reference);
      var forward = Vector3d.Cross(reference, upward);
      var rot = Quaternion.LookRotation((Vector3)forward, (Vector3)upward);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }

    public static void LocalizeTeeComponent(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var cross = -Vector3d.Cross(axis, reference);
      var rot = Quaternion.LookRotation((Vector3)cross, -(Vector3)reference);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }
    
    /// <summary>
    /// ３つの端点が一つに縮退している場合
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="origin"></param>
    public static void LocalizeeDgeneratedComponent(this StubInReinforcingWeld stub, Vector3d origin)
    {
      var leafEdge = stub.LeafEdge ;
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeCod);
    }

    public static void LocalizeLateralTeeComponent(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var cross = Vector3d.Cross(axis, reference);
      var upword = Vector3d.Cross(cross, axis);
      var rot = Quaternion.LookRotation((Vector3)cross, (Vector3)upword);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }

    public static void LocalizeThreeWayInstrumentRootValveComponent(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var cross = -Vector3d.Cross(axis, reference);
      var rot = Quaternion.LookRotation((Vector3)cross, -(Vector3)reference);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }

    public static void LocalizeEccentricPipingReducerComponent(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;
      axis = leafEdgeCodSys.LocalizeVector(axis);
      reference = leafEdgeCodSys.LocalizeVector(reference);

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var outer = Vector3d.Cross( axis, reference );
      var rot = Quaternion.LookRotation( (Vector3)outer, (Vector3)reference );

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * rot, beforeMir);
    }
    
    public static void LocalizePRVComponent(LeafEdge leafEdge, Vector3d origin, Vector3d inlet, Vector3d outlet)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;
      inlet = leafEdgeCodSys.LocalizeVector(inlet);
      outlet = leafEdgeCodSys.LocalizeVector(outlet);

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;
      
      var outer = Vector3d.Cross( inlet, outlet );
      var rot = Quaternion.LookRotation( (Vector3)outer, (Vector3)outlet );

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * rot, beforeMir);
    }
    
    public static void LocalizeCVComponent(LeafEdge leafEdge, Vector3d origin, Vector3d axis, Vector3d reference)
    {
      var leafEdgeCodSys = leafEdge.GlobalCod;
      var diff = origin - leafEdgeCodSys.Origin;

      var beforeCod = leafEdge.LocalCod ;
      var beforeOrg = beforeCod.Origin + leafEdgeCodSys.LocalizeVector( diff );
      var beforeRot = beforeCod.Rotation;
      var beforeMir = beforeCod.IsMirrorType ;

      var cross = -Vector3d.Cross(axis, reference);
      var rot = Quaternion.LookRotation((Vector3)cross, (Vector3)reference);

      var baseRot = leafEdgeCodSys.Rotation;
      var localRot = Quaternion.Inverse(baseRot) * rot;

      leafEdge.LocalCod = new LocalCodSys3d(beforeOrg, beforeRot * localRot, beforeMir);
    }
  }

}