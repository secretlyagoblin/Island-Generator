using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UnityTerrainHelpers {

    public static List<BoundingCube> PropSample(Maps.Map sampleMap, Maps.Map fallOffMap, int divisions)
    {
        var output2d = new BoundingCube[divisions, divisions];
        var boxTypeMap = new Maps.Map(divisions,divisions);

        var output = new List<BoundingCube>();

        var iterationSize = 1f / (divisions+1);

        var xCount = -1;


        for (float x = 0f; x < 1- iterationSize; x+= iterationSize)
        {
            xCount++;
            var yCount = -1;
            for (float y = 0f; y < 1 - iterationSize; y += iterationSize)
            {
                yCount++;
                var nums = new float[4];

                nums[0] = sampleMap.BilinearSampleFromNormalisedVector2(new Vector2(x, y));
                nums[1] = sampleMap.BilinearSampleFromNormalisedVector2(new Vector2(x+ iterationSize, y));
                nums[2] = sampleMap.BilinearSampleFromNormalisedVector2(new Vector2(x, y+ iterationSize));
                nums[3] = sampleMap.BilinearSampleFromNormalisedVector2(new Vector2(x+ iterationSize, y+ iterationSize));

                var min = nums.Min();
                var max = nums.Max();

                output2d[xCount, yCount] = new BoundingCube(new Vector3(x, min, y), new Vector3(iterationSize, max - min, iterationSize));
            }
        }

        for (int x = 0; x < divisions - 2; x++)
        {
            for (int y = 0; y < divisions - 2; y++)
            {

                if(fallOffMap.BilinearSampleFromNormalisedVector2(output2d[x,y].SamplePoint)<0.5f)
                    continue;

                if (boxTypeMap[x, y] == 1 | boxTypeMap[x, y] == 4)
                    continue;

                if (Random.Range(0, 12) < 10)
                    continue;

                boxTypeMap[x, y] = 4;
                boxTypeMap[x + 1, y] = 3;
                boxTypeMap[x, y + 1] = 3;
                boxTypeMap[x + 1, y + 1] = 3;

                boxTypeMap[x + 2, y] = 2;
                boxTypeMap[x, y + 2] = 2;
                boxTypeMap[x + 2, y+1] = 2;
                boxTypeMap[x+1, y + 2] = 2;
                boxTypeMap[x + 2, y + 2] = 2;

            }
        }


        for (int x = 0; x < divisions-1; x++)
        {
            for (int y = 0; y < divisions-1; y++)
            {
                if (boxTypeMap[x, y] == 1 | boxTypeMap[x, y] == 4  | boxTypeMap[x, y] == 3)
                    continue;

                if (Random.Range(0, 10) < 8)
                    continue;

                boxTypeMap[x, y] = 1;
                boxTypeMap[x+1, y] = 2;
                boxTypeMap[x, y+1] = 2;
                boxTypeMap[x+1, y + 1] = 2;

            }
        }

        for (int x = 0; x < divisions; x++)
        {
            for (int y = 0; y < divisions; y++)
            {
                if (boxTypeMap[x, y] == 0)
                {
                    output.Add(output2d[x, y]);
                }
                if (boxTypeMap[x, y] == 1)
                {
                    output.Add(new BoundingCube(output2d[x, y], output2d[x + 1, y], output2d[x, y + 1], output2d[x + 1, y + 1],2));
                }
                if (boxTypeMap[x, y] == 4)
                {
                    output.Add(new BoundingCube(output2d[x, y], output2d[x + 2, y], output2d[x, y + 2], output2d[x + 2, y + 2],3));
                }
            }
        }


        return output;
    }

    //public static List<BoundingCube> PropMerging(List<BoundingCube>)

    public class BoundingCube {

        public Vector3 Min;
        public Vector3 Size;
        public Vector3 Center;
        public Vector2 SamplePoint;
        public int Scale;

        public BoundingCube(Vector3 min, Vector3 size)
        {
            Min = min;
            Size = size;
            Center = Vector3.Lerp(Min, Min + Size, 0.5f);
            SamplePoint = new Vector2(Center.x, Center.z);
            Scale = 1;
        }


        public BoundingCube(BoundingCube a, BoundingCube b, BoundingCube c, BoundingCube d, int size)
        {
            var list = new List<BoundingCube>() { a, b, c, d };

            list.Sort((x, y) => x.Min.y.CompareTo(y.Min.y));

            var lowestY = list.First().Min.y;
            var highestY = list.Last();

            var height = highestY.Min.y - lowestY + highestY.Size.y;
            //var size = 


            Min = new Vector3(a.Min.x, lowestY, a.Min.z);
            Size = new Vector3(a.Size.x * size, height, a.Size.z * size);
            Center = Vector3.Lerp(Min, Min + Size, 0.5f);
            SamplePoint = new Vector2(Center.x, Center.z);
            Scale = size;
        }

    }


}
