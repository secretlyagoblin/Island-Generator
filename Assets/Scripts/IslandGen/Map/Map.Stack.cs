using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map {

    public class Stack {

        Dictionary<MapType, MapPair> _maps = new Dictionary<MapType, MapPair>();

        Rect _rect;

        public Stack(Rect rect)
        {
            _rect = rect;
        }

        public Layer GetMap(MapType type)
        {
            if (_maps.ContainsKey(type))
            {
                return new Layer(_maps[type].Map);
            }
            else
            {
                return null;
            }
        }

        public PhysicalMap GetPhysicalMap(MapType type)
        {
            if (_maps.ContainsKey(type))
            {
                return _maps[type].GetPhysicalMap();
            }
            else
            {
                return null;
            }
        }

        void SetRect()
        {
            foreach (var item in _maps)
            {
                item.Value.SetRect(_rect);
            }
        }

        public void SetRect(Rect rect)
        {
            _rect = rect;
            SetRect();
        }

        public PhysicalMap this[MapType type]
        {
            get
            {
                if (_maps.ContainsKey(type))
                {
                    return _maps[type].GetPhysicalMap();
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddMap(MapType type, Layer map)
        {
            _maps[type] = new MapPair(map);
            SetRect();
        }

        class MapPair {
            public Layer Map { get; private set; }
            Rect _rect;

            public MapPair(Layer map)
            {
                Map = map;
                _rect = new Rect(Vector2.zero, Vector2.one);
            }

            public void SetRect(Rect rect)
            {
                _rect = rect;

            }

            public PhysicalMap GetPhysicalMap()
            {
                return Map.ToPhysical(_rect);
            }
        }
    }
}