using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    private float width = 1500;
    private float lenght = 1500;
    private float height = 300;

    private static Vector2 tileAmount = Vector2.one;

    private int heightmapResoltion = 256 +1;
    private int detailResolution = 1024;
    private int detailResolutionPerPatch = 8;
    private int controlTextureResolution = 256;
    private int baseTextureReolution = 1024;

    public DetailObjectPool[] DetailObjectPools;

    public Texture2D main;
    public Texture2D nurm;
    public Texture2D main2;
    public Texture2D nurm2;



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

        var splat = new SplatPrototype();
        splat.texture = main;
        splat.normalMap = nurm;

        var splat2 = new SplatPrototype();
        splat2.texture = main2;
        splat2.normalMap = nurm2;


        for (int x = 1; x <= tileAmount.x; x++)
        {
            for (int y = 1; y <= tileAmount.y; y++)
            {

                TerrainData terrainData = new TerrainData();

                terrainData.size = new Vector3(width/8f,
                                                height,
                                                lenght / 8f);

                terrainData.baseMapResolution = baseTextureReolution;
                terrainData.heightmapResolution = heightmapResoltion;
                terrainData.alphamapResolution = controlTextureResolution;
                terrainData.SetDetailResolution(detailResolution, detailResolutionPerPatch);
                terrainData.SetHeights(0, 0, _heightMap.HeightMap.CreateTerrainMap().Normalise().FloatArray);
                //terrainData.set
                terrainData.splatPrototypes = new SplatPrototype[] { splat, splat2 };
                //terrainData.SetAlphamaps


                //terrainData.name = name;
                GameObject terrain = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);

                //terrain.name = name;
                //terrain.transform.parent = parent.transform;
                terrain.transform.position = new Vector3(0,0,0);


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