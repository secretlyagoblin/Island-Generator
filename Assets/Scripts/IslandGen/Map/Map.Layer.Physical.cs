using UnityEngine;
using System.Collections;

namespace Map {

    public partial class Layer {
        public PhysicalMap ToPhysical(Rect rect)
        {
            return new PhysicalMap(this, rect);
        }

    }
}
