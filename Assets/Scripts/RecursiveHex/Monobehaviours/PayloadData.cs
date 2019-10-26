using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RecursiveHex;

public class PayloadData : MonoBehaviour
{
    public Dictionary<string, object> KeyValuePairs;
    public string NeighbourhoodData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    List<Vector3> _lines = null;
    List<Vector3> _innerLines = null;

    private void OnDrawGizmosSelected()
    {
        if (_lines == null)
        {
            _lines = NeighbourhoodData.Split('\n').Select(str =>
            {
                var nums = str.Trim(new char[] { '(', ')' }).Split(',');
                var x = int.Parse(nums[0]);
                var y = int.Parse(nums[1]);

                var hex = new Hex(new Vector2Int(x, y), new HexPayload());
                var point = hex.GetNestedHexIndexFromOffset(Vector2Int.zero);



                var isOdd = y % 2 != 0;

                var center = new Vector3(point.x, 0, point.y * Hex.ScaleY);

                if (isOdd)
                {
                    center.x += 0.5f;
                }

                return center;
            }).ToList();
        }

        if (_innerLines == null)
        {
            var nums = this.gameObject.name.Trim(new char[] { '(', ')' }).Split(',');
            var x = int.Parse(nums[0]);
            var y = int.Parse(nums[1]);
            var test = new Vector2Int(x, y);

            _innerLines = Neighbourhood.GetNeighbours(test).Select(n =>
            {

                var p = n + test;

                var isOdd = p.y % 2 != 0;

                var center = new Vector3(p.x, 0, p.y * Hex.ScaleY);

                if (isOdd)
                {
                    center.x += 0.5f;
                }

                return center;
            }).ToList();
        }



            _lines.ForEach(x =>
            {
                Debug.DrawLine(this.transform.position, x, Color.green);
            });

            _innerLines.ForEach(x =>
            {
                Debug.DrawLine(this.transform.position, x, Color.red);
            });
        
    }
}
