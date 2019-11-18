using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshMasher {

    public interface IBlerpable<T> {
        T Blerp(T a, T b, T c, Barycenter barycenter);
    }


}
