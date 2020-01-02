﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshMasher
{
    public struct Barycenter
    {
        public float u;
        public float v;
        public float w;
        public bool contained;
        public bool bounding;
        public bool isEdge;

        public static Barycenter center { get { return new Barycenter(THIRD, THIRD, THIRD, true, false); } }

        public Barycenter(
            //int index, 
            float u, float v, float w, bool contained, bool bounding)
        {
            //this.index = index;
            this.u = u;
            this.v = v;
            this.w = w;
            this.contained = contained;
            this.bounding = bounding;
            isEdge = (!contained || bounding);


        }

        private const float THIRD = 1f / 3f;

        
    }
}