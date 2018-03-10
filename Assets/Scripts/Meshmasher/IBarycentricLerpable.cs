using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshMasher {

    public interface IBarycentricLerpable<T> {
        T Lerp(T a, T b, T c, Vector3 weight);
    }
}
