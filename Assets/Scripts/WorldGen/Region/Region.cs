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
        public int RoomId = -1;
        public int RoomSize = 0;

        public void Finalise(bool destroyGameObject)
        {
            var pos = Object.transform.position;
            GraphPosition = new Vector2(pos.x, pos.y);

            var thisPos = XZPos;

            //for (int i = 0; i < Regions.Count; i++)
            //{
            //    var otherPos = new Vector3(Regions[i].Object.transform.position.x, 0, Regions[i].Object.transform.position.y);
            //
            //    Debug.DrawLine(thisPos, otherPos, RNG.GetRandomColor(), 100f);
            //}

            if(destroyGameObject)
                GameObject.Destroy(Object);
        }
        


    }
}
