using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMasher.NodeData.Types {

    public struct ZoneBoundary : IBlerpable<ZoneBoundary> {

        public int RoomCode { get { return _roomCode; } set { _roomCode = value; } }
        public bool IsEdgeNode;
        public int[] Links { get { return _links; } set { _links = value; } }

        int _roomCode;
        int[] _links;

        const float _threshold = 0.6f;
        const int _linkMaxCount = 5;

        public ZoneBoundary(int roomCode)
        {
            _roomCode = roomCode;
            IsEdgeNode = false;
            _links = new int[] { roomCode };
        }

        public ZoneBoundary(int roomCode, bool partOfBoundary, int[] links)
        {
            _roomCode = roomCode;
            IsEdgeNode = partOfBoundary;
            _links = links;
        }


        public ZoneBoundary Blerp(ZoneBoundary a, ZoneBoundary b, ZoneBoundary c, Barycenter weight)
        {
            var roomCode = a._roomCode.Blerp(a._roomCode, b._roomCode, c._roomCode, weight);

            int[] links;

            if (roomCode == a._roomCode)
                links = a._links;
            else if (roomCode == b._roomCode)
                links = b._links;
            else
                links = c._links;

            //return fine if everything is matching

            var allLinks = a._links;
            var other1 = false;
            var other2 = false;

            for (int i = 0; i < allLinks.Length; i++)
            {
                if (b.RoomCode == allLinks[i])
                    other1 = true;
                if (c.RoomCode == allLinks[i])
                    other2 = true;

                if (other1 && other2)
                    return new ZoneBoundary(roomCode, false, (int[])links.Clone());
            }

            //calculate second highest thing

            int HighestCellCode, SecondHighestCellCode;
            float HighestBarycenter, SecondHighestBarycenter;

            if (weight.u >= weight.v && weight.u >= weight.w)
            {
                HighestCellCode = a.RoomCode;
                HighestBarycenter = weight.u;

                if (weight.v >= weight.w)
                {
                    SecondHighestCellCode = b.RoomCode;
                    SecondHighestBarycenter = weight.v;
                }
                else
                {
                    SecondHighestCellCode = c.RoomCode;
                    SecondHighestBarycenter = weight.w;
                }
            }
            else if (weight.v >= weight.w && weight.v >= weight.u)
            {
                HighestCellCode = b.RoomCode;
                HighestBarycenter = weight.v;

                if (weight.u >= weight.w)
                {
                    SecondHighestCellCode = a.RoomCode;
                    SecondHighestBarycenter = weight.u;
                }
                else
                {
                    SecondHighestCellCode = c.RoomCode;
                    SecondHighestBarycenter = weight.w;
                }
            }
            else
            {
                HighestCellCode = c.RoomCode;
                HighestBarycenter = weight.w;

                if (weight.u >= weight.v)
                {
                    SecondHighestCellCode = a.RoomCode;
                    SecondHighestBarycenter = weight.u;
                }
                else
                {
                    SecondHighestCellCode = b.RoomCode;
                    SecondHighestBarycenter = weight.v;
                }
            }

            var inSameZone = false;

            for (int i = 0; i < links.Length; i++)
            {
                if (SecondHighestCellCode == links[i])
                {
                    inSameZone = true;
                    continue;
                }

            }

            var withinThreshold = (Mathf.Abs(HighestBarycenter - SecondHighestBarycenter) > _threshold);

            if (inSameZone || withinThreshold)
                return new ZoneBoundary(roomCode, false, (int[])links.Clone());
            else
                return new ZoneBoundary(roomCode, true, (int[])links.Clone());
        }

        bool CheckConnection(int roomCodeA, int roomCodeB, float weightA, float weightB)
        {
            if (roomCodeA == roomCodeB)
            {
                return true;
            }
            else
            {
                if (weightA > _threshold || weightB > _threshold)
                    return true;
                return false;
            }
        }
    }

    public struct MeshDual : IBlerpable<MeshDual> {

        public float Value;

        public MeshDual(float value)
        {
            Value = value;
        }

        public MeshDual Blerp(MeshDual a, MeshDual b, MeshDual c, Barycenter weight)
        {
            var dual = new float[] { weight.u, weight.v, weight.w };

            int highest;
            int secondHighest;

            if (weight.u >= weight.v && weight.u >= weight.w)
            {
                highest = 0;
                secondHighest = weight.v > weight.w ? 1 : 2;
            }
            else if (weight.v >= weight.w && weight.v >= weight.u)
            {
                highest = 1;
                secondHighest = weight.u > weight.w ? 0 : 2;
            }
            else
            {
                highest = 2;
                secondHighest = weight.u > weight.v ? 0 : 1;
            }

            var result = dual[highest] - dual[secondHighest];


            return new MeshDual(result);
        }

    }

    public struct CliffData : IBlerpable<CliffData> {

        public float Distance;
        public bool FuzzyBoundary;

        public CliffData(float distance, bool walkable)
        {
            Distance = distance;
            FuzzyBoundary = walkable;
        }


        public CliffData Blerp(CliffData a, CliffData b, CliffData c, Barycenter barycenter)
        {
            return new CliffData()
            {
                Distance = Distance.Blerp(a.Distance, b.Distance, c.Distance, barycenter),
                FuzzyBoundary = FuzzyBoundary.Blerp(a.FuzzyBoundary, b.FuzzyBoundary, c.FuzzyBoundary, barycenter)
            };
        }
    }

    public static class Utils {

        public static float Blerp(this float f, float a, float b, float c, Barycenter weight)
        {
            return a * weight.u + b * weight.v + c * weight.w;
        }

        public static int Blerp(this int i, int a, int b, int c, Barycenter weight)
        {
            if (weight.u >= weight.v && weight.u >= weight.w)
            {
                return a;
            }
            else if (weight.v >= weight.w && weight.v >= weight.u)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        public static bool Blerp(this bool boo, bool a, bool b, bool c, Barycenter weight)
        {
            if (weight.u >= weight.v && weight.u >= weight.w)
            {
                return a;
            }
            else if (weight.v >= weight.w && weight.v >= weight.u)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        public static Color Blerp(this Color col, Color a, Color b, Color c, Barycenter weight)
        {
            var r = a.r * weight.u + b.r * weight.v + c.r * weight.w;
            var g = a.g * weight.u + b.g * weight.v + c.g * weight.w;
            var bee = a.b * weight.u + b.b * weight.v + c.b * weight.w;

            return new Color(r, g, bee);



        }
    }
}
