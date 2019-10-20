using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PayloadData))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myTarget = (PayloadData)target;

        if(myTarget.KeyValuePairs == null)
        {
            EditorGUILayout.HelpBox("No properties on this object.",MessageType.Info);
        }

        foreach (var item in myTarget.KeyValuePairs)
        {
            EditorGUILayout.LabelField(item.Key, item.Value.ToString());
        }
    }
}