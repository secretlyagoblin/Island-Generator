using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCollection : MonoBehaviour {

    Dictionary<MapType, MapPair> _maps = new Dictionary<MapType, MapPair>();

    public Map GetMap(MapType type) { 
            if (_maps.ContainsKey(type))
            {
                return new Map(_maps[type].Map);
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

    public void SetRect(Rect rect)
    {
        foreach (var item in _maps)
        {
            item.Value.SetRect(rect);
        }
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

    public void AddMap(MapType type, Map map)
    {
       _maps[type] = new MapPair(map);

    }

    class MapPair {
        public Map Map { get; private set; }
        Rect _rect;

        public MapPair(Map map)
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


