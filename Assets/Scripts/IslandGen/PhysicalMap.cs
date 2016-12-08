using UnityEngine;
using System.Collections;

public class PhysicalMap {

	Map _map;

	public Map ToMap(){
		return _map;
	}



	protected Rect _rect;

	protected Vector2 _bottomLeft;
	protected Vector2 _topLeft;
	protected Vector2 _topRight;
	protected Vector2 _bottomRight;


	protected NumberRange _xRange;
	protected NumberRange _yRange;

	protected Vector2 _size;

	public PhysicalMap (Map map, Rect rect){

		_map = map;

		_rect = rect;
		_bottomLeft = rect.position;
		_topLeft = new Vector2 (_rect.xMax, rect.position.y);
		_bottomRight = new Vector2 (rect.position.x, _rect.yMax);
		_topRight = _rect.max;

		_xRange = new NumberRange (rect.position.x, _topRight.x);
		_yRange = new NumberRange (rect.position.y, _topRight.y);
	}

	public bool Overlaps(PhysicalMap other){
		return _rect.Overlaps (other._rect);
	}

	public void DrawShape(Color color){
		Debug.DrawLine (_topLeft, _topRight, color);
		Debug.DrawLine (_topRight, _bottomRight, color);
		Debug.DrawLine (_bottomRight, _bottomLeft, color);
		Debug.DrawLine (_bottomLeft, _topLeft, color);
	}

	public PhysicalMap PhysicalAverage(PhysicalMap other){

		if (!Overlaps (other))
			return this;

		var boundsA = new NormalisedRectArray (this, other);
		var boundsB = new NormalisedRectArray (other, this);

		Debug.Log ("--------------------");
		boundsA.GetSubArray (_map.FloatArray);
		boundsB.GetSubArray (other._map.FloatArray);
		Debug.Log ("--------------------");

		return this;
	}

	public static Rect GetOverlappingRect(PhysicalMap a, PhysicalMap b){

		if (!a.Overlaps (b))
			return new Rect();

		var xRange = NumberRange.GetOverlappingBounds (a._xRange, b._xRange);
		var yRange = NumberRange.GetOverlappingBounds (a._yRange, b._yRange);

		return new Rect(new Vector2(xRange.Min,yRange.Min),new Vector2(xRange.Size,yRange.Size));
	}

	protected struct NumberRange{

		public float Min;
		public float Max;

		public float Size{get{return Max - Min; }}

		public NumberRange(float min, float max){
			Min = min;
			Max = max;
		}

		public float GetValueInRange(float value){
			return Mathf.InverseLerp (Min, Max,value);			
		}


		static public NumberRange GetOverlappingBounds(NumberRange a, NumberRange b){

			NumberRange left;
			NumberRange right;

			if (a.Min < b.Min) {
				left = a;
				right = b;
			} else {
				left = b;
				right = a;
			}

			var newMin = right.Min;
			var newMax = left.Max < right.Max ? left.Max : right.Max;

			return new NumberRange (newMin, newMax);
		}
	}

	protected struct NormalisedRectArray{

		public NumberRange XBounds;
		public NumberRange YBounds;

		public NormalisedRectArray(PhysicalMap parent, PhysicalMap other){

			var xRange = NumberRange.GetOverlappingBounds (parent._xRange, other._xRange);
			var yRange = NumberRange.GetOverlappingBounds (parent._yRange, other._yRange);

			XBounds = new NumberRange (parent._xRange.GetValueInRange (xRange.Min), parent._xRange.GetValueInRange (xRange.Max));
			YBounds = new NumberRange (parent._yRange.GetValueInRange (yRange.Min), parent._yRange.GetValueInRange (yRange.Max));

			//Debug.Log("X Bounds: " + XBounds.Min + ", " + XBounds.Max);
			//Debug.Log("Y Bounds: " + YBounds.Min + ", " + YBounds.Max);
		}

		public float[,] GetSubArray(float[,] array){

			var sizeX = array.GetLength (0);
			var xMin = Mathf.RoundToInt(sizeX * XBounds.Min);
			var xMax = Mathf.RoundToInt(sizeX * XBounds.Max);

			var sizeY = array.GetLength (1);
			var yMin = Mathf.RoundToInt(sizeY * YBounds.Min);
			var yMax = Mathf.RoundToInt(sizeY * YBounds.Max);


			Debug.Log("X Sub-Array Bounds: " + xMin + ", " + xMax);
			Debug.Log("Y Sub-Array Bounds: " + yMin + ", " + yMax);

			var returnArray = new float[xMax - xMin, yMax - yMin];

			for (int x = xMin; x < xMax; x++) {
				for (int y = yMin; y < yMax; y++) {
					returnArray[x-xMin,y-yMin] = array[x,y];					
				}	
			}
			return returnArray;						
		}
	}
}
