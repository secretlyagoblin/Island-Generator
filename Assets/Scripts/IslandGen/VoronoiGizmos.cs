using UnityEngine;
using System.Collections.Generic;

public class VoronoiGizmos : MonoBehaviour {

    float[,] values;

    public Rect rect = new Rect(new Vector2(21.1234f, 14.23123f), new Vector2(32f, 32f));

    public int offset;
    Vector2[,] largerGrid;

    public Vector3 testPoint;


    [Range(0.0f, 10.0f)]
    public float largerGridScale = 3.445f;


    void Start()
    {




    }

    void OnDrawGizmos()
    {

        rect.position = new Vector2(transform.localPosition.x, transform.localPosition.z);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 1f, rect.size.y));
        Gizmos.color = Color.white;

        int xStart = (int)(Mathf.Floor(rect.position.x / largerGridScale));
        int yStart = (int)(Mathf.Floor(rect.position.y / largerGridScale));

        int gridSizeX = (int)(Mathf.Ceil(rect.size.x / largerGridScale));
        int gridSizeY = (int)(Mathf.Ceil(rect.size.y / largerGridScale));

        largerGrid = new Vector2[gridSizeX + (offset * 2) + 1, gridSizeY + (offset * 2) + 1];

        var xIteration = -1;
        var yIteration = -1;

        var seedX = 2323.23423454f;
        var seedY = 121233.678523f;


        for (float x = xStart - offset; x <= xStart + gridSizeX + offset; x++)
        {
            xIteration++;
            for (float y = yStart - offset; y <= yStart + gridSizeY + offset; y++)
            {
                yIteration++;

                var gridPos = new Vector2((x) * largerGridScale, (y) * largerGridScale);

                gridPos.x += (((Mathf.PerlinNoise(gridPos.x + seedX, gridPos.y + seedX))) * (largerGridScale));
                gridPos.y += (((Mathf.PerlinNoise(gridPos.x + seedY, gridPos.y + seedY))) * (largerGridScale));

                largerGrid[xIteration, yIteration] = gridPos;
            }
            yIteration = -1;
        }

        for (int x = 0; x < largerGrid.GetLength(0); x++)
        {
            for (int y = 0; y < largerGrid.GetLength(1); y++)
            {

                Gizmos.DrawWireCube(new Vector3(largerGrid[x, y].x, 0, largerGrid[x, y].y), Vector3.one);

            }
        }

        int smallerSize = 50;

        for (int x = 0; x < smallerSize; x++)
        {
            for (int y = 0; y < smallerSize; y++)
            {

                var floatX = Mathf.InverseLerp(0, smallerSize - 1, x);
                var floatY = Mathf.InverseLerp(0, smallerSize - 1, y);

                floatX = Mathf.Lerp(rect.position.x, rect.max.x, floatX);
                floatY = Mathf.Lerp(rect.position.y, rect.max.y, floatY);

                var sample = new Vector2(floatX, floatY);

                var point = GetNeighbourhood(sample);

                var distances = new List<float>();



                for (int u = -1; u <= 1; u++)
                {
                    for (int v = -1; v <= 1; v++)
                    {

                        distances.Add(Vector2.Distance(largerGrid[(int)(point.x + u), (int)(point.y + v)], sample));
                    }
                }

                distances.Sort();


                var val = distances[2] - distances[1];

                var color = (Mathf.InverseLerp(0, largerGridScale, val)) * 2;

                Gizmos.color = new Color(color, color, color);

                Gizmos.DrawRay(new Vector3(sample.x, 0, sample.y), Vector3.up * val);

            }
        }
    }

    Vector2 GetNeighbourhood(Vector2 vector)
    {

        var moddedPosition = vector - rect.position;

        int xPos = (int)((Mathf.RoundToInt((moddedPosition.x) / largerGridScale)));
        int yPos = (int)((Mathf.RoundToInt((moddedPosition.y) / largerGridScale)));

        return new Vector2(xPos + offset, yPos + offset);
    }



}