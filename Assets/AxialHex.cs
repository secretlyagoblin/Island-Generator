using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AxialHex : MonoBehaviour
{
    public GameObject SpawnObject;

    public HashSet<NastyHex> _map = new HashSet<NastyHex>();

    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();

        PopulateMap(4);

        var newMap = new HashSet<NastyHex>();

        var colour = RNG.NextColor();
        var innerColour = RNG.NextColor();

        foreach (var item in _map)
        {
            var newNasty = (item * 3) + (item.Rotate60()*2);

            //var colour = RNG.NextColor();
            //var innerColour = RNG.NextColor();

            //newMap.Add(item * 5);

            var hood = GetNeigbours(newNasty, 2);
            var hoodz = HoodDispatch(hood);
            var innerHood = hoodz[0];
            var outerHood = hoodz[1];

            //hood.ForEach(x => newMap.Add(x));

            //hood.ForEach(x => SpawnGameObject(x, colour));

            innerHood.ForEach(x => {
                var jitterColour = RNG.SimilarColor(colour, 0.1f);
                SpawnGameObject(x, jitterColour);
            });

            outerHood.ForEach(x => {
                var jitterColour = RNG.SimilarColor(innerColour, 0.1f);
                SpawnGameObject(x, jitterColour);
            });
        }

        return;

        /*
        var blastMap = new HashSet<NastyHex>();

        foreach (var item in newMap)
        {
            var newNasty = (item * 3) + (item.Rotate60() * 2);

            //var colour = RNG.NextColor();
            

            //newMap.Add(item * 5);

            var hood = GetNeigbours(newNasty, 2);

            hood.ForEach(x => blastMap.Add(x));

           // hood.ForEach(x => {
           //     var jitterColour = RNG.SimilarColor(colour, 0.1f);
           //     SpawnGameObject(x, jitterColour);
           //
           // });            
        }


        foreach (var item in blastMap)
        {
            var newNasty = (item * 3) + (item.Rotate60() * 2);

            var colour = RNG.NextColor();


            //newMap.Add(item * 5);

            var hood = GetNeigbours(newNasty, 2);

            //hood.ForEach(x => blastMap.Add(x));

            hood.ForEach(x => {
                var jitterColour = RNG.SimilarColor(colour, 0.1f);
                SpawnGameObject(x, jitterColour);
           
            });            
        }

    */

    }

    // Update is called once per frame
    void Update()
    {

    }

    List<List<NastyHex>> HoodDispatch(List<NastyHex> toSplit)
    {
        //var count = 10;
        //var end = toSplit.Count - count;
        //
        //return toSplit.GetRange(count, end);

        var edge = new List<NastyHex>()
        {
            toSplit[0],
            //toSplit[1],
            toSplit[2],
            toSplit[3],
            toSplit[6],
            toSplit[7],
            toSplit[11],
            toSplit[12],
            toSplit[15],
            toSplit[16],
            //toSplit[17],
            toSplit[18],
        };

        var center = new List<NastyHex>()
        {
            toSplit[4],
            toSplit[5],
            toSplit[8],
            toSplit[9],
            toSplit[10],
            toSplit[13],
            toSplit[14],
        };

        return new List<List<NastyHex>>()
        {
            edge,center
        };
    }

    void SpawnGameObject(NastyHex hex, Color color)
    {
        var index = NastyToNormal(hex);
        var pos = NormalToIngame(index);
        var gob = Instantiate(SpawnObject, pos, Quaternion.identity, this.transform);
        gob.name = $"{hex.x},{hex.y},{hex.z}";
        //gob.transform.localScale = new Vector3(2, 2, 2);

        var meshRenderer = gob.GetComponent<MeshRenderer>();
        var newMaterial = new Material(meshRenderer.sharedMaterial);
        newMaterial.color = color;
        meshRenderer.sharedMaterial = newMaterial;
    }

    Vector2Int NastyToNormal(NastyHex nasty)
    {
        var col = nasty.x + (nasty.z - (nasty.z & 1)) / 2;
        var row = nasty.z;
        return new Vector2Int(col, row);
    }

    Vector3 NormalToIngame(Vector2Int hex)
    {
        var center = new Vector3(
                    hex.x,
                    0,
                    //item.Value.Payload.Height, 
                    hex.y * RecursiveHex.Hex.ScaleY);
        var isOdd = hex.y % 2 != 0;

        if (isOdd)
        {
            center += new Vector3(0.5f, 0, 0);
        }

        return center;
    }

    void PopulateMap(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                _map.Add(new NastyHex(q, r, -q - r));
            }
        }
    }

    List<NastyHex> GetNeigbours(NastyHex seed, int radius)
    {
        var returnList = new List<NastyHex>();
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                returnList.Add(new NastyHex(q, r, -q - r)+seed);
            }
        }

        return returnList;
    }
}

public struct NastyHex
{
    public int x;
    public int y;
    public int z;
        public NastyHex(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

    public static NastyHex operator +(NastyHex a, NastyHex b)
    {
        return new NastyHex(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static NastyHex operator -(NastyHex a, NastyHex b)
    {
        return new NastyHex(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static NastyHex operator *(NastyHex a, int b)
    {
        return new NastyHex(a.x * b, a.y * b, a.z * b);
    }

    public static NastyHex operator *(NastyHex a, NastyHex b)
    {
        return new NastyHex(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public NastyHex Rotate60()
    {
        return new NastyHex(-y, -z, -x);
    }
        
}
