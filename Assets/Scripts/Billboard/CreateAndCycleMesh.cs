using UnityEngine;
using System.Collections;

public class CreateAndCycleMesh : MonoBehaviour {

    public int SpriteXCount;
    public int SpriteYCount;

    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;

    Mesh[] _meshArray;


    // Use this for initialization
    void Start () {

        _meshArray = new Mesh[SpriteXCount * SpriteYCount];

        var index = -1;


        var vertices = new Vector3[]{
            new Vector3(0,0,0),
               new Vector3(-1,0,0),
            new Vector3(-1,1,0),
                     new Vector3(0,1,0),

        };

        var triangles = new int[] {0,1,2,0,2,3 };

        for (int y = SpriteYCount-1; y >0; y--)
           
        {
            for (int x = 0; x < SpriteXCount; x++)
            {
                index++;

                float minX = x * (1f / SpriteXCount);
                float maxX = (x+1) * (1f / SpriteXCount);

                float minY= y * (1f / SpriteYCount);
                float maxY = (y + 1) * (1f / SpriteYCount);

                var bottomLeftUV = new Vector2(minX, minY);
                var bottomRightUV = new Vector2(maxX, minY);
                var topLeftUV = new Vector2(minX, maxY);
                var topRightUV = new Vector2(maxX, maxY);

                var mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = new Vector2[]
        {
            bottomLeftUV,
               bottomRightUV,
            topRightUV,
               topLeftUV
        };

                mesh.RecalculateNormals();
                _meshArray[index] = mesh;


            }
        }

        StartCoroutine(Animate(new int[] { 0, 1, 2, 1,2,2,2,3,4,3,4,5,5,5 }, 0.3f));

	
	}
	
	// Update is called once per frame
	void Update () {

        //MeshFilter.mesh = _meshArray[Random.Range(0, 3)];

    }

    IEnumerator Animate(int[] cycle, float time)
    {
        var count = 0;
        var frame = 0;
        while (true)
        {
            MeshFilter.mesh = _meshArray[cycle[frame]];
            frame++;
            count++;
            if (count >= cycle.Length)
            {
                count = 0;
                frame = 0;
            }

            yield return new WaitForSeconds(time);
        }
    }
}
