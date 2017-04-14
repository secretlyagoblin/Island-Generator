using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    private float width = 1500;
    private float lenght = 1500;
    private float height = 300;

    private static Vector2 tileAmount = Vector2.one;

    private int heightmapResoltion = 256 +1;
    private int detailResolution = 256;
    private int detailResolutionPerPatch = 8;
    private int controlTextureResolution = 256;
    private int baseTextureReolution = 1024;

    public DetailObjectPool[] DetailObjectPools;

    public SplatCollection SplatCollection;
    public DetailObjectCollection Details;

    public GameObject RockToCreateSteep;
    public GameObject RockToCreateShallow;


    Terrain.TerrainData _heightMap;

    // Use this for initialization
    void Start()
    {
        CreateTerrain();

    }

    // Update is called once per frame

    bool duh = false;

    void Update()
    {
        if (!duh)
        {
            duh = true;
            CreateMyToys();
        }
    }

    private void CreateTerrain()
    {
        //GameObject parent = Instantiate(new GameObject("Boostr"));
        //parent.transform.position = new Vector3(0, 0, 0);

        _heightMap = Terrain.TerrainData.RegionIsland(heightmapResoltion, new Rect());
        _heightMap.HeightMap.Remap(0, height);


        TerrainData terrainData = new TerrainData();

        terrainData.size = new Vector3(width / 8f,
                                        height,
                                        lenght / 8f);

        //terrainData.

        terrainData.baseMapResolution = baseTextureReolution;
        terrainData.heightmapResolution = heightmapResoltion;
        terrainData.alphamapResolution = controlTextureResolution;
        terrainData.SetDetailResolution(detailResolution, detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _heightMap.HeightMap.CreateTerrainMap().Normalise().FloatArray);
        //terrainData.set
        terrainData.splatPrototypes = SplatCollection.GetSplatPrototypes();
        terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(_heightMap.WalkableMap.Clone().Resize(256, 256)));
        terrainData.detailPrototypes = Details.GetDetailPrototypes();



        Details.SetDetails(terrainData, _heightMap.WalkableMap);


        //terrainData.name = name;
        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        var terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        terrain.detailObjectDistance = 100f;

        //terrain.name = name;
        //terrain.transform.parent = parent.transform;
        terrainObj.transform.position = new Vector3(0, 0, 0);

        var parent = new GameObject();

        var propCount = 130f;


        for (int x = 1; x < propCount; x++)
        {
            for (int y = 1; y < propCount; y++)
            {
                var locX = (x ) / propCount;
                var locY = (y) / propCount;

                var innerHeight = terrainData.GetInterpolatedHeight(locX, locY);

                if (_heightMap.WalkableMap.BilinearSampleFromNormalisedVector2(new Vector2(locX, locY)) > 0.5f && innerHeight > 0.3f)
                {
                    

                    var normal = terrainData.GetInterpolatedNormal(x / propCount, y / propCount);
                    var forward = new Vector3(normal.x, 0, normal.z);
                    var steepness = terrainData.GetSteepness(x / propCount, y / propCount);

                    var RockToCreate = steepness < 62 | RNG.Next(0,50)<1 ? RockToCreateShallow : RockToCreateSteep;
                    //steepness = Util.InverseLerpUnclamped(0f, 150f, steepness);
                    //innerHeight -= (steepness*40);

                    forward.Normalize();

                    var rotation = Quaternion.identity;

                    if(Vector3.Dot(normal,Vector3.up) > 0.05f)
                    {
                        rotation = Quaternion.LookRotation(Vector3.Lerp(normal, forward, 0.5f), Vector3.up);

                        //rotation = Quaternion.LookRotation(forward, Vector3.up);

                        //rotation = Quaternion.Euler(0, RNG.NextFloat(-180f, 180f), 0);

                        var obj = Instantiate(RockToCreate, new Vector3(locX * width, innerHeight, locY * lenght), rotation, parent.transform);

                        //obj.transform.RotateAroundLocal(obj.transform.up, RNG.NextFloat(-180f, 180f));


                        var scaleX = Random.Range(1.3f, 2.2f);

                        if (RNG.Next(0, 20) < 1)
                        {
                            scaleX = Random.Range(2.8f, 4.1f);
                        } else if (RNG.Next(0, 20) < 3)
                        {
                            scaleX = Random.Range(1.8f, 3.5f);
                        }




                        obj.transform.localScale = new Vector3(scaleX, scaleX * 3, scaleX);

                        obj.transform.Translate(new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f) - (1f * scaleX * 2), Random.Range(-0.2f, 0.2f)));
                        //obj.transform.Translate(Vector3.down * 15f);

                    }




                }



            }
        }





    }

    private void CreateMyToys()
    {
        for (int i = 0; i < DetailObjectPools.Length; i++)
        {
            DetailObjectPools[i].SetPhysicalMap(_heightMap);
            DetailObjectPools[i].InitPositions();
        }
    }
}