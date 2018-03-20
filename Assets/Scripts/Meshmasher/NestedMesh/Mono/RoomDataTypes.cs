using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMasher.NodeDataTypes {

    struct RoomCode : IBarycentricLerpable<RoomCode> {

        public int Value;

        public RoomCode(int value)
        {
            Value = value;
        }

        public RoomCode Lerp(RoomCode a, RoomCode b, RoomCode c, Vector3 weight)
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

    struct RoomColor : IBarycentricLerpable<RoomColor> {

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

        public RoomColor Lerp(RoomColor a, RoomColor b, RoomColor c, Vector3 weight)
        {
            var r = a.r * weight.x + b.r * weight.y + c.r * weight.z;
            var g = a.g * weight.x + b.g * weight.y + c.g * weight.z;
            var cb = a.b * weight.x + b.b * weight.y + c.b * weight.z;

            return new RoomColor(new Color(r, g, cb));
            
        }
    }

    struct RoomFloat : IBarycentricLerpable<RoomFloat> {

        public float Value;

        public RoomFloat(float value)
        {
            Value = value;
        }

        public RoomFloat Lerp(RoomFloat a, RoomFloat b, RoomFloat c, Vector3 weight)
        {
            return new RoomFloat(a.Value* weight.x + b.Value * weight.y + c.Value * weight.z);
        }
    }

}
