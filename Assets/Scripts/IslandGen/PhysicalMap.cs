using UnityEngine;
using System.Collections;

public class PhysicalMap {

	Map _map;

	public PhysicalMap(Map map, Rect bounds){
		_map = map;
	}

	public Map ToMap(){
		return _map;
	}


}
