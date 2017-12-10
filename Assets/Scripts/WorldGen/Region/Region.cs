using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {
    public class Region {

        public float Height;
        public float Radius;
        public GameObject Object;
        public List<Region> Regions = new List<Region>();
        public Vector2 GraphPosition;
        public Vector3 XZPos { get { return new Vector3(GraphPosition.x, 0, GraphPosition.y); } }

        public void Finalise()
        {
            var pos = Object.transform.position;
            GraphPosition = new Vector2(pos.x, pos.y);
            GameObject.Destroy(Object);
        }
        


    }
}
