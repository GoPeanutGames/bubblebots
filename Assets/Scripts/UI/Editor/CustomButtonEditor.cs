using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CustomButton), true)]
public class CustomButtonEditor : ButtonEditor
{
    private int _choice = 0;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty stringId = serializedObject.FindProperty("buttonId");
        List<string> availableIds = TypeUtilities.GetAllPublicConstantValues<string>(typeof(ButtonId));

        EditorGUILayout.LabelField("Button Id");
        _choice = availableIds.IndexOf(stringId.stringValue);
        if (_choice == -1)
        {
            _choice = availableIds.IndexOf(ButtonId.DefaultId);
        }
        _choice = EditorGUILayout.Popup(_choice, availableIds.ToArray());
        stringId.stringValue = availableIds.ToArray()[_choice];

        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}

