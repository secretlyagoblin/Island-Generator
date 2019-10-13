using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RecursiveHex : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();

        //var code = 0;

        //var finalMeshes = new HexGroup()
        //    .Subdivide()
        //    .ForEach(x => { x.Height = RNG.NextFloat(10); x.Code = code; code++; })
        //    .Subdivide()
        //    .Subdivide(x => x.Code)
        //    .ForEachHexGroup(x => x.Subdivide())
        //    .ForEachHexGroup(x => x.ToMesh());

        var layer1 = new HexGroup();
        var layer2 = layer1.DebugSubdivideRowOnly().Subdivide();//.Subdivide();//.Subdivide().Subdivide();

        var mesh = layer2.ToMesh();

        this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        //var gob = new GameObject();
        //gob.AddComponent<MeshFilter>().sharedMesh = layer1.ToMesh();
        //gob.AddComponent<MeshRenderer>();





    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class HexGroup
{
    private Dictionary<Vector2Int, Hex> _inside;
    private Dictionary<Vector2Int, Hex> _border;

    private static readonly Vector2Int[] _offsets = new Vector2Int[]
        {
            new Vector2Int(+1,+0),
            new Vector2Int(+0,-1),
            new Vector2Int(-1,-1),
            new Vector2Int(-1,+0),
            new Vector2Int(-1,+1),
            new Vector2Int(+0,+1)
        };



    public HexGroup ForEach(Func<Hex,Hex> action)
    {
        foreach (var item in _inside)
        {
            _inside[item.Key] = action(item.Value);
        }
        return this;
    }

    public HexGroup ForEach(Action<Hex> action)
    {
        foreach (var item in _inside)
        {
            action(item.Value);
        }
        return this;
    }

    private Neighbourhood[] GetNeighbourhoods()
    {
        var hood = new Neighbourhood[_inside.Count];

        var count = 0;

        foreach (var item in _inside)
        {
            var hexes = new Hex[6];

            

            for (int i = 0; i < 6; i++)
            {
                var key = item.Key + _offsets[i];
                if (_border.ContainsKey(key))
                {
                    hexes[i] = _border[key];
                }
                else if(_inside.ContainsKey(key))
                {
                    hexes[i] = _inside[key];
                }
                else
                {
                    //Debug.LogError("Hey, Remove Null Neighbours Being Possible");
                    hexes[i] = new Hex();
                }
            }

            hood[count] = new Neighbourhood
            {
                Center = item.Value,
                N0 = hexes[0],
                N1 = hexes[1],
                N2 = hexes[2],
                N3 = hexes[3],
                N4 = hexes[4],
                N5 = hexes[5]
            };

            count++;
        }

        return hood;
    }

    private void AddBorderHex(Hex hex)
    {
        _border.Add(hex.Index, hex);
    }

    private void AddHex(Hex hex)
    {
        _inside.Add(hex.Index, hex);
    }

    /// <summary>
    /// Creates a single hex cell at 0,0 with 6 border cells
    /// </summary>
    public HexGroup()
    {
        _inside = new Dictionary<Vector2Int, Hex>(1);
        _border = new Dictionary<Vector2Int, Hex>(6);

        AddHex(new Hex(0, 0));

        for (int i = 0; i < _offsets.Length; i++)
        {
            AddBorderHex(new Hex(_offsets[i].x, _offsets[i].y));
        }
    }

    public HexGroup Subdivide()
    {
        return new HexGroup(this);
    }

    public HexGroup DebugSubdivideRowOnly()
    {
        return new HexGroup(this, true);
    }

    public HexGroup[] Subdivide(Func<Hex,object> func)
    {
        throw new NotImplementedException();
    }

    private HexGroup(HexGroup parent)
    {
        var hoods = parent.GetNeighbourhoods();
        _inside = new Dictionary<Vector2Int, Hex>(parent._inside.Count*19);
        _border = new Dictionary<Vector2Int, Hex>(parent._border.Count * 10);

        for (int i = 0; i < hoods.Length; i++)
        {
            var hood = hoods[i];
            var cells = hood.Subdivide();
                
            for (int u = 0; u < cells.Length; u++)
            {
                if (hood.IsBorder)
                    this.AddBorderHex(cells[u]);
                else
                    this.AddHex(cells[u]);
            }            
        }
    }

    private HexGroup(HexGroup parent, bool yee)
    {
        var hoods = parent.GetNeighbourhoods();
        _inside = new Dictionary<Vector2Int, Hex>(parent._inside.Count * 19);
        _border = new Dictionary<Vector2Int, Hex>(parent._border.Count * 10);

        for (int i = 0; i < hoods.Length; i++)
        {
            var hood = hoods[i];
            var cells = hood.DebugSubdivideRowOnly();

            for (int u = 0; u < cells.Length; u++)
            {
                if (hood.IsBorder)
                    this.AddBorderHex(cells[u]);
                else
                    this.AddHex(cells[u]);
            }
        }
    }

    private static Mesh ToMesh(Dictionary<Vector2Int, Hex> dict)
    {
        var verts = new Vector3[dict.Count * 3 * 6];
        var tris = new int[dict.Count * 6 * 3];

        var material = new Material(Shader.Find("Standard"));
        
        material.color = RNG.NextColor();


        var count = 0;

        foreach (var item in dict)
        {
            var center = new Vector3(item.Key.x,0, item.Key.y* Hex.ScaleY);
            var isOdd = item.Key.y % 2 != 0;

            

           var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
           
           if (item.Key.x % 2 != 0)
           {
               obj.GetComponent<MeshRenderer>().sharedMaterial = material;
           }
           
           obj.name = item.Key.ToString();
           obj.transform.position = center;

            for (int i = 0; i < 6; i++)
            {
                var index = (count * 3 * 6) + (i * 3);
                verts[index] = center;
                verts[index + 1] = item.Value.GetCorner3(i);
                verts[index + 2] = item.Value.GetCorner3(i + 1);

                if (isOdd)
                {
                    verts[index].x += 0.5f;
                    verts[index + 1].x += 0.5f;
                    verts[index + 2].x += 0.5f;
                    obj.transform.position = center + new Vector3(0.5f,0,0);
                }

                tris[index] = index;
                tris[index + 1] = index + 1;
                tris[index + 2] = index + 2;
            }
            count++;
        }

        return new Mesh()
        {
            vertices = verts,
            triangles = tris
        };
    }

    public Mesh ToMesh()
    {
        return HexGroup.ToMesh(_inside);
    }

    public Mesh ToMeshBorder()
    {
        return HexGroup.ToMesh(_border);
    }
}

public struct Neighbourhood
{
    public Hex Center;
    public Hex N0;
    public Hex N1;
    public Hex N2;
    public Hex N3;
    public Hex N4;
    public Hex N5;

    public bool IsBorder;

    private static readonly Vector2Int[] _3x3ChildrenOffsets = new Vector2Int[]
    {
        //Center
        new Vector2Int(0,0),

        //One
        new Vector2Int(+0,+1),
        new Vector2Int(+1,+1),
        new Vector2Int(+0,-1),
        new Vector2Int(+1,-1),
        new Vector2Int(-1,+0),
        new Vector2Int(+1,+0),
        //Two
        new Vector2Int(-1,-2),
        new Vector2Int(+0,-2),
        new Vector2Int(+1,-2),

        new Vector2Int(-1,+2),
        new Vector2Int(+0,+2),
        new Vector2Int(+1,+2),

        new Vector2Int(+2,+1),
        new Vector2Int(+2,+0),
        new Vector2Int(+2,-1),

        new Vector2Int(-1,+1),
        new Vector2Int(-2,+0),
        new Vector2Int(-1,-1),
        //new Vector2Int(+1,-2),
        //new Vector2Int(+2,0)

    };

    private static readonly Vector2Int[] _2x2ChildrenOffsets = new Vector2Int[]
    {
        //Center
        new Vector2Int(0,0),

        new Vector2Int(+1,+0),
        new Vector2Int(+0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1,+0),
        new Vector2Int(-1,+1),
        new Vector2Int(+0,+1)
    };

    private static readonly Vector2Int[] _DebugOffsets = new Vector2Int[]
{
        //Center
        //new Vector2Int(0,0),
        //
        //new Vector2Int(+1,+0),
        //new Vector2Int(+0,-1),
        //new Vector2Int(-1,-1),
        //new Vector2Int(-1,+0),
        //new Vector2Int(-1,+1),
        //new Vector2Int(+0,+1),
        //
        //    new Vector2Int(+0,+2),
        //        new Vector2Int(+0,-2),
        //
        //        new Vector2Int(+0,+3),
        //        new Vector2Int(-1,+3),
        //                        new Vector2Int(+0,+4),
        //                                        new Vector2Int(+0,-3),
        //        new Vector2Int(-1,-3),
        //                                        new Vector2Int(+0,-4),
        new Vector2Int(0,0),
        new Vector2Int(0,1),
        new Vector2Int(0,2),
        new Vector2Int(0,3),
        new Vector2Int(0,4),
        new Vector2Int(0,5),
        new Vector2Int(0,6),



};

    public Hex[] Subdivide()
    {
        var children = new Hex[_2x2ChildrenOffsets.Length];

        for (int i = 0; i < _2x2ChildrenOffsets.Length; i++)
        {
            children[i] = Interpolate(_2x2ChildrenOffsets[i]);
        }

        return children;
    }

    public Hex[] DebugSubdivideRowOnly()
    {

            var children = new Hex[_DebugOffsets.Length];

            for (int i = 0; i < _DebugOffsets.Length; i++)
            {
                children[i] = Interpolate(_DebugOffsets[i]);
            }

            return children;        
    }

    private Hex Interpolate(Vector2Int offset) {

        var xGridOffset = new Vector2(2.5f, 1);
        var yGridOffset = new Vector2(0.5f, 3);

        var everyOtherY = new Vector2(-2, 2);

        var inUseIndex = this.Center.Index;

        //if (inUseIndex.y < 0)
        //{
        //    inUseIndex.x++;// -= inUseIndex.y;
        //}

        var halfCount = inUseIndex.y * 0.5f;
        var yGridEvenCount = Mathf.Floor(halfCount);
        var yGridOddCount = Mathf.Ceil(halfCount);


        var shiftedIndex = (inUseIndex.x * xGridOffset) +
            (inUseIndex.y * yGridOffset);
            //(yGridEvenCount * yGridOffset) +
            //(yGridOddCount * everyOtherY)
            //;


        var evenX = this.Center.Index.x % 2 == 0;
        var evenY = this.Center.Index.y % 2 == 0;
        //
        //var offsetX = Mathf.FloorToInt(this.Center.Index.x * 3);
        //var offsetY = this.Center.Index.y * 3;

        var offsetX = Mathf.FloorToInt(shiftedIndex.x);
        var offsetY = Mathf.FloorToInt(shiftedIndex.y);

        if(evenX && evenY)
        {

        }
        else if(evenX && !evenY)
        {
            if (offset.y % 2 != 0)
            {
                offsetX++;
            }
        } else if (!evenX && evenY)
        {
            if(offset.y % 2 != 0)
            {
                offsetX++;
            }
        }
        else if (!evenX && !evenY)
        { 

        }

            //if (evenY && offset.y % 2 != 0)
            //{
            //    offsetX--;
            //}




            //if(offsetY+offset.y % 2 != 0)
            //{
            //    offsetX+= 1;
            //}

            //var offsetY = evenX ? 0 : 1;

            //if ((offsetY + offset.y) % 2 == 0)
            //{
            //    offsetX++;
            //}



            //offsetX += evenY ? 1 : 0;

            var x = offsetX + offset.x;// + modificationX;
        var y = offsetY + offset.y;// + modificationY;

        return new Hex(x, y);
    }

}

public struct Hex
{
    public float Height;
    public Vector2Int Index;
    public int Code;

    public static readonly float ScaleY = 0.866025f;
    public static readonly float HalfHex = 0.55f;

    public Hex(int x, int y)
    {
        Index = new Vector2Int(x, y);
        Height = 0;
        Code = 0;
    }

    public Vector2 GetCorner2(int i)
    {
        var angle_deg = 60f * i - 30f;
        var angle_rad = Mathf.PI / 180f * angle_deg;
        return new Vector2(Index.x+Hex.HalfHex * Mathf.Cos(angle_rad),
                     (Index.y * Hex.ScaleY )+ Hex.HalfHex * Mathf.Sin(angle_rad));
    }

    public Vector3 GetCorner3(int i)
    {
        var angle_deg = 60f * i - 30f;
        var angle_rad = Mathf.PI / 180f * angle_deg;
        return new Vector3(Index.x + Hex.HalfHex * Mathf.Cos(-angle_rad),Height,
                     (Index.y * Hex.ScaleY)+Hex.HalfHex * Mathf.Sin(-angle_rad));
    }


}

public static class HexUtils{
    public static HexGroup[] ForEachHexGroup(this HexGroup[] array, Action<HexGroup> action)
    {
        for (int i = 0; i < array.Length; i++)
        {
            action(array[i]);
        }

        return array;
    }

    public static T[] ForEachHexGroup<T>(this HexGroup[] array, Func<HexGroup,T> action)
    {
        var output = new T[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            output[i] = action(array[i]);
        }

        return output;
    }
}