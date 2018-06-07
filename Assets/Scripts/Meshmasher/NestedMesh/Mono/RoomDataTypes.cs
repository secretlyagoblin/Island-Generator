﻿using System.Collections;
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

        public RoomCode Blerp(RoomCode a, RoomCode b, RoomCode c, Vector3 weight)
        {
            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                return a;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                return b;
            }
            else
            {
                return c;
            }
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

        public RoomColor Blerp(RoomColor a, RoomColor b, RoomColor c, Vector3 weight)
        {
            var r = a.r * weight.x + b.r * weight.y + c.r * weight.z;
            var g = a.g * weight.x + b.g * weight.y + c.g * weight.z;
            var cb = a.b * weight.x + b.b * weight.y + c.b * weight.z;

            return new RoomColor(new Color(r, g, cb));
            
        }
    }

    struct RoomFloat : IBlerpable<RoomFloat> {

        public float Value;

        public RoomFloat(float value)
        {
            Value = value;
        }

        public RoomFloat Blerp(RoomFloat a, RoomFloat b, RoomFloat c, Vector3 weight)
        {
            return new RoomFloat(a.Value* weight.x + b.Value * weight.y + c.Value * weight.z);
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


        public ZoneBoundary Blerp(ZoneBoundary a, ZoneBoundary b, ZoneBoundary c, Vector3 weight)
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

            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                HighestCellCode = a.RoomCode;
                HighestBarycenter = weight.x;

                if (weight.y >= weight.z)
                {
                    SecondHighestCellCode = b.RoomCode;
                    SecondHighestBarycenter = weight.y;
                }else
                {
                    SecondHighestCellCode = c.RoomCode;
                    SecondHighestBarycenter = weight.z;
                }
            } else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                HighestCellCode = b.RoomCode;
                HighestBarycenter = weight.y;

                if (weight.x >= weight.z)
                {
                    SecondHighestCellCode = a.RoomCode;
                    SecondHighestBarycenter = weight.x;
                }
                else
                {
                    SecondHighestCellCode = c.RoomCode;
                    SecondHighestBarycenter = weight.z;
                }
            }
            else
            {
                HighestCellCode = c.RoomCode;
                HighestBarycenter = weight.z;

                if (weight.x >= weight.y)
                {
                    SecondHighestCellCode = a.RoomCode;
                    SecondHighestBarycenter = weight.x;
                }
                else
                {
                    SecondHighestCellCode = b.RoomCode;
                    SecondHighestBarycenter = weight.y;
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

        public MeshDual Blerp(MeshDual a, MeshDual b, MeshDual c, Vector3 weight)
        {
            var dual = new float[] { weight.x, weight.y, weight.z };

            int highest;
            int secondHighest;

            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                highest = 0;
                secondHighest = weight.y > weight.z ? 1 : 2;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                highest =  1;
                secondHighest = weight.x > weight.z ? 0 : 2;
            }
            else
            {
                highest = 2;
                secondHighest = weight.x > weight.y ? 0 : 1;
            }

            var result = dual[highest] - dual[secondHighest];


            return new MeshDual(result);
        }

    }
}
