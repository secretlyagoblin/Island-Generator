using UnityEngine;
using System.Collections.Generic;

namespace BuildingGenerator {

    public class BuildingRecipe {

        public Vector3[] Vectors
        { get; private set; }

        Floorplate _floorPlate;

        public BuildingRecipe()
        { }

        public void ImportFloorplateJSON(TextAsset JSON)
        {
            _floorPlate = JsonUtility.FromJson<Floorplate>(JSON.text);

            int vertCount = _floorPlate.vectors.Length / 3;

            Vectors = new Vector3[vertCount];

            for (var x = 0; x < vertCount; x++)
            {
                int vertIndex = x * 3;
                Vectors[x] = new Vector3(_floorPlate.vectors[vertIndex], _floorPlate.vectors[vertIndex + 1], _floorPlate.vectors[vertIndex + 2]);
            }
        }

        public void DrawTriangles(Color color)
        {

            for (var x = 0; x < _floorPlate.triangles.Length; x ++)
            {

                var index = _floorPlate.triangles[x] * 3;

                var a = _floorPlate.indices[index];
                var b = _floorPlate.indices[index + 1];
                var c = _floorPlate.indices[index + 2];

                Debug.DrawLine(Vectors[a], Vectors[b], color);
                Debug.DrawLine(Vectors[b], Vectors[c], color);
                Debug.DrawLine(Vectors[c], Vectors[a], color);

            }
        }

        public void DrawQuads(Color color)
        {

            for (var x = 0; x < _floorPlate.quads.Length; x += 2)
            {

                var tri = _floorPlate.quads[x];
                var index = tri * 3;

                var a = _floorPlate.indices[index];
                var b = _floorPlate.indices[index + 1];
                var c = _floorPlate.indices[index + 2];

                Debug.DrawLine(Vectors[a], Vectors[b], color);
                Debug.DrawLine(Vectors[b], Vectors[c], color);
                Debug.DrawLine(Vectors[c], Vectors[a], color);

                tri = _floorPlate.quads[x + 1];
                index = tri * 3;

                a = _floorPlate.indices[index];
                b = _floorPlate.indices[index + 1];
                c = _floorPlate.indices[index + 2];

                Debug.DrawLine(Vectors[a], Vectors[b], color);
                Debug.DrawLine(Vectors[b], Vectors[c], color);
                Debug.DrawLine(Vectors[c], Vectors[a], color);

            }
        }

        class Floorplate {
            public float[] vectors = null; // {x1, y1, z1, x2, y2, z2...}
            public int[] indices = null;   // {1, 2, 3, 4, 5, 6}
            public int[] triangles = null; // {1, 2} <- this indicates two triangles - '1' meanining indices 1, 2, and 3
            public int[] quads = null;     // {1, 2} <- this indicates one quad, made from trianges 1 and 2. (iterate through i+=2)
        }

    }
}