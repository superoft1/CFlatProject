using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
    public static class LeafUnitExtention
    {
        [UI.Property(UI.PropertyCategory.ComponentName, "Name", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false)]
        public static string GetName(this LeafUnit leafUnit)
        {
            return leafUnit.Name;
        }

        [UI.Property(UI.PropertyCategory.ComponentName, "Name")]
        public static void SetName(this LeafUnit leafUnit, string name)
        {
            leafUnit.Name = name;
        }

        [UI.Property(UI.PropertyCategory.Position, "LocalPosition", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false)]
        public static Vector3d GetLocalPosition(this LeafUnit leafUnit)
        {
            return leafUnit.LocalCod.Origin;
        }

        [UI.Property(UI.PropertyCategory.Position, "LocalPosition")]
        public static void SetLocalPosition(this LeafUnit leafUnit, Vector3d pos)
        {
            leafUnit.LocalCod = new LocalCodSys3d(pos, leafUnit.LocalCod);
        }


        [UI.Property(UI.PropertyCategory.Position, "AreaSize", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false)]
        public static Vector3d GetAreaSize(this LeafUnit leafUnit)
        {
            return leafUnit.AreaSize;
        }

        [UI.Property(UI.PropertyCategory.Position, "AreaSize")]
        public static void SetAreaSize(this LeafUnit leafUnit, Vector3d pos)
        {
            leafUnit.AreaSize = pos;
        }
    }
}
