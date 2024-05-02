using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
public class ScriptableObjectIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;

        Object owner = property.serializedObject.targetObject;
        // This is the unity managed GUID of the scriptable object, which is always unique :3
        string unityManagedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner));

        if (property.stringValue != unityManagedGuid)
        {
            property.stringValue = unityManagedGuid;
        }
        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = true;
    }
}
#endif

[CreateAssetMenu(fileName = "New Golf Course", menuName = "Golf Course")]
public class ScriptableCourse : ScriptableObject
{
    [ScriptableObjectId]
    public string id;
    public string CourseName;
    public ScriptableHole[] HolesInCourse;
    public string AddressablePath;
    public bool IsCustomCourse = false;
}
