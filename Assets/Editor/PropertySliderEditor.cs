using UnityEditor ;
using UnityEditor.UI ;
using UnityEngine.Events ;

namespace Chiyoda.UI
{
  [CanEditMultipleObjects]
  [CustomEditor( typeof( PropertySlider ), true )]
  public class PropertySliderEditor : SliderEditor
  {
    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI() ;
      this.serializedObject.Update() ;
      EditorGUILayout.PropertyField( this.serializedObject.FindProperty( "m_OnSliderDetached" ), false ) ;
      this.serializedObject.ApplyModifiedProperties() ;
    }
  }
}