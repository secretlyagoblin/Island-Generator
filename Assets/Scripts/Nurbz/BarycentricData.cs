using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshMasher.MeshTiling
{
    public struct BarycentricData
    {
        public float u;
        public float v;
        public float w;
        public bool contained;
        public bool bounding;
        public int index;

        public BarycentricData(int index, float u, float v, float w, bool contained, bool bounding)
        {
            this.index = index;
            this.u = u;
            this.v = v;
            this.w = w;
            this.contained = contained;
            this.bounding = bounding;
        }
    }
}
