using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;

namespace UnityNURBS.Operators
{

    public class RefinePatch : Operator, IOperator
    {
        public RefinePatch() {}

        public bool Cook()
        {
            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsPatch ) {
                    var np = primitive as NurbsPatch;
					np.RefineU();
					// FIXME: support attribute transfer etc
                    outputGeometry.Add ( np );
                }
                else
                    outputGeometry.Add ( primitive );
            }

            return true;
        }
    }
}