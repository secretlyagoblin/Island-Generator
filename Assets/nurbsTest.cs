using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nurbz;

public class nurbsTest : MonoBehaviour {

    float _perlinScale = 0.2213423f;

    // Use this for initialization
    void Start()
    {

        RNG.DateTimeInit();

        var startLine = new Line2(Vector2.zero, Vector2.one.normalized);
        startLine.DrawDebugView();
        var sample = startLine.middle;
        //var noise = Mathf.PerlinNoise((sample.x * _perlinScale) + Random.Range(0, 10000f), (sample.y * _perlinScale) + Random.Range(0, 10000f));

        var sampleNormal = startLine.orientation;
        sampleNormal = new Vector2(sampleNormal.x, -sampleNormal.y);

        //var angle = 

        //var angle = noise * Mathf.PI * 2;
        //var newVec = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

        var line = new Line2(sample, sample + sampleNormal.normalized);
        //line.DrawDebugView();

        var topline = new Line2(startLine.start, startLine.start + sampleNormal.normalized);
        var bottomLine = new Line2(startLine.end, startLine.end + sampleNormal.normalized);

        var normal = line.orientation;
        normal = new Vector2(line.orientation.y, -line.orientation.x);
        var nextLine = new Line2(line.end, line.end + normal.normalized);

        var p1 = startLine.start;
        var p2 = startLine.end;

        var p3 = topline.IntersectionPoint(nextLine);
        var p4 = bottomLine.IntersectionPoint(nextLine);

        var lineA = new Line2(p1, p3);
        var lineB = new Line2(p2, p4);
        var lineC = new Line2(p3, p4);

        lineA.DrawDebugView();
        lineB.DrawDebugView();
        lineC.DrawDebugView();

        var cell = new Cell();

        cell.Points[0] = p1;
        cell.Points[1] = p2;
        cell.Points[2] = p3;
        cell.Points[3] = p4;


        for (int i = 0; i < 190; i++)
        {
            cell = cell.AddNewCell();
        }









        //Debug.DrawRay(sample, newVec, Color.white, 100f);








    }
	
	// Update is called once per frame
	void Update () {
		
	}

    Vector2 RotateVector2d(Vector2 vec, float rotate)
    {
        var x = vec.x * Mathf.Cos(rotate) - vec.y * Mathf.Sin(rotate);
        var y = vec.x * Mathf.Sin(rotate) + vec.y * Mathf.Cos(rotate);

        return new Vector2(x, y);
        
    }

    private class Cell {

        public Vector2[] Points = new Vector2[4];

        public Cell[,] Context = new Cell[3, 3];

        public Coord Coord;
        

        public Cell()
        {
            Context[1, 1] = this;
            Coord = new Coord(0, 0);
        }

        public Cell(Cell daddy, Vector2 adjacency)
        {
            Context[1, 1] = this;
            var coord = new Coord(daddy.Coord.x + (int)adjacency.x, daddy.Coord.y + (int)adjacency.y);

        }

        public Cell AddNewCell()
        {
            Line2 startLine = new Line2(Points[2], Points[3]);

            switch (RNG.Next(0, 4)){
                case 0:
                    startLine = new Line2(Points[1], Points[3]);
                    break;
                case 1:
                    startLine = new Line2(Points[0], Points[2]);
                    break;
                case 2:
                    startLine = new Line2(Points[1], Points[0]);
                    break;
                default:
                    break;

            }
            startLine.DrawDebugView();
            var sample = startLine.middle;

            var sampleNormal = startLine.orientation;
            sampleNormal = new Vector2(sampleNormal.x, -sampleNormal.y);

            var line = new Line2(sample, sample + sampleNormal.normalized);
            //line.DrawDebugView();

            var topline = new Line2(startLine.start, startLine.start + sampleNormal.normalized);
            var bottomLine = new Line2(startLine.end, startLine.end + sampleNormal.normalized);

            var normal = line.orientation;
            normal = new Vector2(line.orientation.y, -line.orientation.x);
            var nextLine = new Line2(line.end, line.end + normal.normalized);

            var p1 = startLine.start;
            var p2 = startLine.end;

            var p3 = topline.IntersectionPoint(nextLine);
            var p4 = bottomLine.IntersectionPoint(nextLine);

            var lineA = new Line2(p1, p3);
            var lineB = new Line2(p2, p4);
            var lineC = new Line2(p3, p4);

            lineA.DrawDebugView();
            lineB.DrawDebugView();
            lineC.DrawDebugView();

            var cell = new Cell(this, Vector2.up);

            cell.Points[0] = p1;
            cell.Points[1] = p2;
            cell.Points[2] = p3;
            cell.Points[3] = p4;

            cell.HarvestContext(this);

            return cell;

        }

        public void HarvestContext(Cell other)
        {
            var offset = other.Coord - Coord;

            var xMin = 0 - offset.x < 0?0: offset.x;
            var yMin = 0 - offset.y < 0 ? 0 : offset.y;

            var xMax = 3 - offset.x > 3 ? 3 : 3 - offset.x;
            var yMax = 3 - offset.y > 3 ? 3 : 3 - offset.y;

            //Here we put all the bits together, and add to the context

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {

                }
            }
        }

    }
}
