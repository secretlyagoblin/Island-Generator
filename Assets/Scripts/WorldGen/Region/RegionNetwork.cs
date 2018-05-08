using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;

namespace WorldGen {

    //Large Scale info on each region, how they connect.
    public class RegionNetwork {

        RegionNetworkSettings _settings;
        Transform _transform;
        List<Region> _regions = new List<Region>();

        public static GameObject NodePrefab;

        public RegionNetwork(Transform root, RegionNetworkSettings settings)
        {
            _settings = settings;
            _transform = root;
            DefaultGraphInit(root);
        }

        public void Simulate(float totalTime, float step)
        {
            Physics2D.autoSimulation = false;
            //Physics2D.autoSimulation = false;
            var t = 0f;
            while (t < totalTime)
            {
                Physics2D.Simulate(step);
                t += step;
                    }

            Physics2D.autoSimulation = true;
        }

        public List<Region> Finalise()
        {
            for (int i = 0; i < _regions.Count; i++)
            {
                _regions[i].Finalise(_settings.DestroyGameObject);
            }

            return _regions;        
        }

        public void DebugDraw(Color color, float duration, bool DisplayInXZSpace)
        {
            for (int i = 0; i < _regions.Count; i++)
            {
                for (int u = 0; u < _regions[i].Regions.Count; u++)
                {
                    if (DisplayInXZSpace)
                    { Debug.DrawLine(_regions[i].XZPos, _regions[i].Regions[u].XZPos, color, duration); }
                    else { Debug.DrawLine(_regions[i].GraphPosition, _regions[i].Regions[u].GraphPosition, color, duration); }
                    
                }
            }
        }

        //Currently just creates a region network in hardcoded fashion - should take a network of some kind as input and simulate.
        void DefaultGraphInit(Transform root)
        {
            var points = new Vector3[]
{
            new Vector3(0,0,RNG.NextFloat(0,2)),
            new Vector3(10,0,RNG.NextFloat(0,2)),
            new Vector3(20,0,RNG.NextFloat(0,2)),
            new Vector3(5,10,RNG.NextFloat(1.5f,3)),
            new Vector3(15,10,RNG.NextFloat(1.5f,3)),
            new Vector3(10,20,RNG.NextFloat(2,3)),
            new Vector3(20,20,RNG.NextFloat(1.5f,3)),
            new Vector3(15,30,RNG.NextFloat(2,4)),
            new Vector3(15,15,RNG.NextFloat(3,4))
};
            for (int i = 0; i < points.Length; i++)
            {
                _regions.Add(CreateNewRegion(points[i], points[i].z));
            }

            CreateChain(_regions[0], _regions[1], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[1], _regions[2], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[3], _regions[4], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[5], _regions[6], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[0], _regions[3], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[3], _regions[5], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[5], _regions[7], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[1], _regions[4], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[8], _regions[6], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[1], _regions[3], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[2], _regions[4], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[8], _regions[5], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[6], _regions[7], RNG.Next(_settings.SpawnedNodeMin, _settings.SpawnedNodeMax), true);
            CreateChain(_regions[4], _regions[8], 1, false);

            //_joints = GetComponentsInChildren<SpringJoint2D>();
            //StartCoroutine(FreezePhysicsAfterTime(TimeCount));
        }

        //Creates gameobject representation of graph to simulate
        Region CreateNewRegion(Vector3 position, float height)
        {
            var obj = new GameObject();

            var region = new Region()
            {
                Height = height,
                Radius = RNG.NextFloat(_settings.RadiusMin,_settings.RadiusMax),
                Object = obj
            };

            obj.name = "Node";
            var rigid = obj.AddComponent<Rigidbody2D>();
            rigid.gravityScale = 0f;
            obj.AddComponent<CircleCollider2D>();
            obj.transform.position = new Vector3(position.x, position.y, 0f);
            obj.transform.localScale = Vector2.one * region.Radius;
            obj.transform.parent = _transform;
            return region;
        }

        void CreateChain(Region p1, Region p2, int divisions, bool regionConnected)
        {
            var chain = new List<Region>();
            chain.Add(p1);

            for (int i = 1; i < divisions; i++)
            {
                var pointOnDomain = Mathf.InverseLerp(0, divisions, i);
                var position = Vector3.Lerp(p1.Object.transform.position, p2.Object.transform.position, pointOnDomain);
                var height = Mathf.Lerp(p1.Height, p2.Height, pointOnDomain);

                chain.Add(CreateNewObjectAndAddLink(position, height, chain[i - 1], regionConnected));
            }

            AddLink(p2, chain[chain.Count - 1], regionConnected);
        }

        Region CreateNewObjectAndAddLink(Vector3 position, float height, Region link, bool regionConnected)
        {
            var region = CreateNewRegion(position, height);
            if(regionConnected)
                _regions.Add(region);

            AddLink(region, link, regionConnected);
            return region;
        }

        void AddLink(Region a, Region b, bool regionConnected)
        {
            var spring = a.Object.AddComponent<SpringJoint2D>();
            spring.connectedBody = b.Object.GetComponent<Rigidbody2D>();
            spring.enableCollision = true;
            spring.distance = 0;

            if (regionConnected)
            {
                a.Regions.Add(b);
                b.Regions.Add(a);
            }
        }
    }
}
