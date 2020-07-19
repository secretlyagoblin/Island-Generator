using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using WanderingRoad.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WanderingRoad.Procgen.Levelgen
{
    public class RecursiveHexTest : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject BorderPrefab;
        public Mesh PreviewMesh;

        public bool Preview = true;

        private List<HexGroupVisualiser> _gizmosHexGroups;// = new HexGroupVisualiser(PreviewMesh,)

        private string _savePath = "";

        BinaryFormatter _formatter = new BinaryFormatter();

        // Start is called before the first frame update
        void Start()
        {

            
            _gizmosHexGroups = new List<HexGroupVisualiser>() { };//HexGroupVisualiser(PreviewMesh);

            var seed = "I'd kill fill zill";

            //cool seed "4/27/2020 7:45:28 PM"
            // orphan node to test.. "4/27/2020 5:56:43 PM"
            //RNG.Init("3/15/2020 5:58:48 PM");
            // dead end to test... RNG.Init("5/2/2020 5:04:24 PM");
            //RNG.DateTimeInit();
            //RecursiveHex.RandomSeedProperties.Disable();

            var savePath = $"{Application.persistentDataPath}/{seed}";

            if (Directory.Exists(savePath))
            {
                GroupsFromFile();
            }
            else
            {
                HexMapBuilder.BuildHexMap(seed);
            }

            return;
        }

        private List<HexGroup> GroupsFromFile()
        {
            Debug.Log("Known level seed! Skipping file creation and loading from disk!");

            return Directory.GetFiles($"{_savePath}/Chunks")
                .Select(x => {
                    var stream = new FileStream(x, FileMode.Open, FileAccess.Read);
                    return (HexGroup)_formatter.Deserialize(stream);
                }).ToList();
        }

        private void OnDrawGizmos()
        {
            //for (int i = 0; i < _gizmosHexGroups.Count; i++)
            //{
            //    _gizmosHexGroups[i].DrawGizmos();
            //}
            ////_gizmosHexGroup.DrawGizmos();
        }

        IEnumerator FinaliseHexgroup(List<HexGroup> hexGroup, Action<HexGroup> func)
        {
            for (int i = 0; i < hexGroup.Count; i++)
            {
                func(hexGroup[i]);
                yield return null;
            }
        }

        private void Finalise(HexGroup group)
        {
            var gobject = new GameObject();
            gobject.name = "Subregion";
            var renderer = gobject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;
            gobject.AddComponent<MeshFilter>().sharedMesh = group.ToMesh();
            gobject.transform.parent = this.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (Preview)
            {
                for (int i = 0; i < _gizmosHexGroups.Count; i++)
                {
                    _gizmosHexGroups[i].DrawMeshes();
                }
            }
            
            //this.transform.Rotate(Vector3.up, 5f * Time.deltaTime);
        }

        private float NoiseAtIndex(Hex hex)
        {
            var pos = hex.Index.Position2d;
            var perlin = Mathf.PerlinNoise(
                pos.x * 0.25324f,
                pos.y * 0.25324f
                );

            var multiplier = Mathf.InverseLerp(0, 6, hex.Payload.EdgeDistance);

            return perlin*multiplier;

        }

        private float CalculateNoise(float x, float y, HexPayload payload)
        {
            var offset = 0.01332f;
            var scale = 0.0745f;
            x += offset;
            y += offset;

            var noise = Mathf.PerlinNoise(x * scale, y * scale);

            offset = 0.03332f;
            scale = 0.1545f;
            x += offset;
            y += offset;

            var noise2 = Mathf.PerlinNoise(x * scale, y * scale);

            var edgeDistance = Mathf.Max(payload.EdgeDistance + (RNG.CoinToss()?RNG.NextFloat(-0.1f, 0.1f):0) - 0.5f, 0f);
            var distance = Mathf.InverseLerp(0, 2, edgeDistance);
            noise *= distance;
            var distance2 = Mathf.InverseLerp(0, 5, edgeDistance);
            noise += (distance2*noise2*0.3f);

            var height = (payload.Height * 4) + (edgeDistance * 2f) + (noise * 6) ;



            return height;
        }
    }

    //public class PropGen : IDeterminePropRelationships
    //{
    //    public bool GetFarProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite)
    //    {
    //        propDataToWrite.Clear();
    //
    //        foreach (var item in hoods)
    //        {
    //            var payload = item.Value.Center.Payload;
    //            if (payload.EdgeDistance < 0.75f || payload.EdgeDistance >2)
    //                continue;
    //
    //            var data = new PropData()
    //            {
    //                Position = item.Value.Center.Index.Position2d,
    //                HeightGuide = 2,
    //                Rotation = RNG.NextFloat(360),
    //                Yaw = 0,
    //                PropType = PropType.Backdrop
    //            };
    //
    //            propDataToWrite.Add(data);
    //        }
    //
    //        return true;
    //
    //    }
    //
    //    public bool GetCloseProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
