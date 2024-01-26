using UnityEditor ;
using UnityEditor.UI ;

namespace Chiyoda.UI
{
  [CanEditMultipleObjects]
  [CustomEditor( typeof( CommitButton ), true )]
  public class CommitButtonEditor : ButtonEditor
  {
    public override void OnInspectorGUI()
    {
      this.serializedObject.Update() ;
      EditorGUILayout.PropertyField( this.serializedObject.FindProperty( "historyCommit" ), false ) ;
      this.serializedObject.ApplyModifiedProperties() ;
      base.OnInspectorGUI() ;
    }
  }
}