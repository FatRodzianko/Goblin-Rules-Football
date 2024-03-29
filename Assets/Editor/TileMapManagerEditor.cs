#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileMapManager))]
public class TileMapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (TileMapManager)target;
        if (GUILayout.Button("Save Map"))
        {
            script.SaveMap();
        }
        if (GUILayout.Button("!!! WARNING CLEAR MAP BELOW"))
        {
            //script.ClearMap();
        }
        if (GUILayout.Button("Clear Map"))
        {
            Debug.Log("TileMapManagerEditor: Clear map: " + Time.time.ToString());
            script.ClearMapFromEditor();
        }
        if (GUILayout.Button("!!! WARNING CLEAR MAP ABOVE"))
        {
            //script.ClearMap();
        }
        if (GUILayout.Button("Load Map"))
        {
            script.LoadMapFromEditor();
        }
    }
}
#endif
