using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.CommonEntities
{
  internal abstract class EmbodyStructureWithUIProperties : EmbodyStructure
  {
     protected abstract IEnumerable<IStructurePart> GetElements() ;
    
     protected EmbodyStructureWithUIProperties( Document doc ) : base( doc ) 
     {}  
     
     [Chiyoda.UI.Property( UI.PropertyCategory.StructureId, "ID", ValueType = UI.ValueType.Label,
       Visibility = UI.PropertyVisibility.ReadOnly )]
     public string NameProp => Name ;
    
     [Chiyoda.UI.Property( UI.PropertyCategory.Position, "Rotation", ValueType = UI.ValueType.Rotation,
       Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
     public double Rotation
     {
       get => LocalCod.Rotation.eulerAngles.z ;
       set => LocalCod = new LocalCodSys3d( LocalCod.Origin, Quaternion.AngleAxis( (float) value, Vector3.forward ), LocalCod.IsMirrorType ) ;
     }

     [Chiyoda.UI.Property( UI.PropertyCategory.Position, "World Position", ValueType = UI.ValueType.Position,
       Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
     public Vector3d Position
     {
       get => LocalCod.Origin ;
       set => LocalCod = new LocalCodSys3d( value, LocalCod ) ;
     }

     public override IEnumerable<IStructurePart> StructureElements => GetElements() ;
  }
}