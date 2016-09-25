using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(PointClickScript))]
public class PointClickEditorScript : Editor {

    // Use this for initialization
    void Start()
    {

    }

    Vector3 currentPos = new Vector3(0, 0, 0);

    // Update is called once per frame
    void OnSceneGUI()
    {

        var clicker = (PointClickScript)target;


        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane xz = new Plane(clicker.transform.up, clicker.transform.position);
        float what;
        var ig = xz.Raycast(ray, out what);
        var point = ray.GetPoint(what);

        var lins = clicker.GetTwoLines(clicker.transform.InverseTransformPoint(point));
        foreach (var line in lins)
        {
            Handles.DrawLine(clicker.transform.TransformPoint(line.start), clicker.transform.TransformPoint(line.end));
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
        {

            ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            xz = new Plane(clicker.transform.up, clicker.transform.position);
            ig = xz.Raycast(ray, out what);
            point = ray.GetPoint(what);
            clicker.Vectors.Add(clicker.transform.InverseTransformPoint(point));
            clicker.RecalculateCoolines();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.command)
        {

            clicker.Vectors = new List<Vector3>();
            clicker.RecalculateCoolines();
        }

        foreach (var vecbo in clicker.Lines)
        {

            Handles.DrawLine(clicker.transform.TransformPoint(vecbo.start), clicker.transform.TransformPoint(vecbo.end));
        }

        Selection.activeGameObject = clicker.transform.gameObject;

        if ((Vector3)Event.current.mousePosition != currentPos)
        {
            currentPos = Event.current.mousePosition;
            SceneView.RepaintAll();
        }

    }
}