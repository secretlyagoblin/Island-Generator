using UnityEngine;
using System.Collections;

public partial class Map
{
	public PhysicalMap ToPhysical(Rect rect){
		return new PhysicalMap(this, rect);
	}

}
