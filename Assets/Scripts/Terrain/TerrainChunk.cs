using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {

    public Terrain Terrain;
    public Maps.Map Map;
    public Matrix4x4[][] Props;
    
    public TerrainChunk(Terrain terrain, Matrix4x4[][] props)
    {
        Terrain = terrain;
        Props = props;
    }

    void SortProps(Matrix4x4[] props)
    {
        //need to sort out a whole prop thing here, instanced vs not, etc
    }


}
