using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMasher.NodeDataTypes {

    struct RoomCode : IBlerpable<RoomCode> {

        public int Value;

        public static bool operator ==(RoomCode a, RoomCode b) { return a.Value == b.Value; }
        public static bool operator !=(RoomCode a, RoomCode b) { return a.Value != b.Value; }

        public RoomCode(int value)
        {
            Value = value;
        }

        public RoomCode Blerp(RoomCode a, RoomCode b, RoomCode c, Barycenter weight)
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

        public override bool Equals(object obj)
        {
            if (!(obj is RoomCode))
            {
                return false;
            }

            var code = (RoomCode)obj;
            return Value == code.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }
    }

    struct RoomColor : IBlerpable<RoomColor> {

        public Color Value;

        public float r
        {
            get
            {
                return Value.r;
            }
            set
            {
                Value.r = value;
            }
        }

        public float g
        {
            get
            {
                return Value.g;
            }
            set
            {
                Value.g = value;
            }
        }

        public float b
        {
            get
            {
                return Value.b;
            }
            set
            {
                Value.b = value;
            }
        }

        public RoomColor(Color value)
        {
            Value = value;
        }

        public RoomColor Blerp(RoomColor a, RoomColor b, RoomColor c, Barycenter weight)
        {
            var r = a.r * weight.u + b.r * weight.u + c.r * weight.u;
            var g = a.g * weight.v + b.g * weight.v + c.g * weight.v;
            var bee = a.b * weight.w + b.b * weight.w + c.b * weight.w;

            return new RoomColor(new Color(r, g, bee));
            
        }
    }

    struct RoomFloat : IBlerpable<RoomFloat> {

        public float Value;

        public RoomFloat(float value)
        {
            Value = value;
        }

        public RoomFloat Blerp(RoomFloat a, RoomFloat b, RoomFloat c, Barycenter weight)
        {
            return new RoomFloat(a.Value* weight.u + b.Value * weight.v + c.Value * weight.w);
        }
    }

    struct ZoneBoundary : IBlerpable<ZoneBoundary> {

        public int RoomCode { get { return _roomCode.Value; } set { _roomCode.Value = value; } }
        public float Distance;
        public int[] Links { get { return _links; } set { _links = value; } }

        RoomCode _roomCode;
        int[] _links;

        const float _threshold = 0.6f;
        const int _linkMaxCount = 5;

        public ZoneBoundary(int roomCode)
        {
            _roomCode = new RoomCode(roomCode);
            Distance = 0;
            _links = new int[] {roomCode};
        }

        public ZoneBoundary(int roomCode, float distance, int[] links)
        {
            _roomCode = new RoomCode(roomCode);
            Distance = distance;
            _links = links;
        }


        public ZoneBoundary Blerp(ZoneBoundary a, ZoneBoundary b, ZoneBoundary c, Barycenter weight)
        {
            var roomCode = a._roomCode.Blerp(a._roomCode, b._roomCode, c._roomCode, weight).Value;

            int[] links;

            if (roomCode == a._roomCode.Value)
                links = a._links;
            else if (roomCode == b._roomCode.Value)
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

                if(other1 && other2)
                    return new ZoneBoundary(roomCode, 1, (int[])links.Clone());
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
                }else
                {
                    SecondHighestCellCode = c.RoomCode;
                    SecondHighestBarycenter = weight.w;
                }
            } else if (weight.v >= weight.w && weight.v >= weight.u)
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

            if(inSameZone || withinThreshold)
                return new ZoneBoundary(roomCode, 1, (int[])links.Clone());
            else
                return new ZoneBoundary(roomCode, 0, (int[])links.Clone());
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

    struct MeshDual : IBlerpable<MeshDual> {

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
                highest =  1;
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
}
