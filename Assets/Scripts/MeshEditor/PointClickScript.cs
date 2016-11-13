using UnityEngine;
using Nurbz;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PointClickScript : MonoBehaviour {

    public List<Vector3> Vectors = new List<Vector3>();
    public List<Line3> Lines = new List<Line3>();



    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Line3> GetTwoLines(Vector3 backbo)
    {

        var lines = new List<Line3>();

        var pointPool = new List<Vector3>(Vectors);

        Vector3 closest = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        float distance = float.MaxValue;

        foreach (var vecto in pointPool)
        {
            var dist = Vector3.Distance(backbo, vecto);
            if (dist < distance)
            {
                closest = vecto;
                distance = dist;
            }

        }

        pointPool.Remove(closest);

        lines.Add(new Line3(backbo, closest));
        distance = float.MaxValue;

        foreach (var vecto in pointPool)
        {
            var dist = Vector3.Distance(backbo, vecto);
            if (dist < distance)
            {
                closest = vecto;
                distance = dist;
            }

        }

        pointPool.Remove(closest);

        lines.Add(new Line3(backbo, closest));

        return lines;
    }

    public void RecalculateCoolines()
    {

        var lines = new List<Line3>();

        foreach (var vec in Vectors)
        {
            var pointPool = new List<Vector3>(Vectors);
            pointPool.Remove(vec);

            Vector3 closest = vec;
            float distance = float.MaxValue;

            foreach (var vecto in pointPool)
            {
                var dist = Vector3.Distance(vec, vecto);
                if (dist < distance)
                {
                    closest = vecto;
                    distance = dist;
                }

            }

            pointPool.Remove(closest);

            lines.Add(new Line3(vec, closest));
            distance = float.MaxValue;

            foreach (var vecto in pointPool)
            {
                var dist = Vector3.Distance(vec, vecto);
                if (dist < distance)
                {
                    closest = vecto;
                    distance = dist;
                }
            }

            pointPool.Remove(closest);
            lines.Add(new Line3(vec, closest));
            distance = float.MaxValue;

            foreach (var vecto in pointPool)
            {
                var dist = Vector3.Distance(vec, vecto);
                if (dist < distance)
                {
                    closest = vecto;
                    distance = dist;
                }

            }

            pointPool.Remove(closest);

            lines.Add(new Line3(vec, closest));
            distance = float.MaxValue;


        }
        Lines = lines;
    }
}