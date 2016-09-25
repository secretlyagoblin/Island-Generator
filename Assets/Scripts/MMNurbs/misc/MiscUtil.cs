using UnityEngine;
using System.Collections;
using UnityNURBS.Types;

namespace UnityNURBS.Util
{

    public class MiscUtil
    {

        public static Vector3[] DoubleVectorArrayToFloatVectorArray(mmVector3[] inputArray)
        {
            var length = inputArray.Length;
            var outputArray = new Vector3[length];

            for (var x = 0; x < length; x++)
            {
                outputArray[x] = new Vector3((float)inputArray[x].x, (float)inputArray[x].y, (float)inputArray[x].z);
            }


            return outputArray;
        }
    }
}
