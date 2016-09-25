using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;

namespace UnityNURBS.Operators
{

    public class InsertKnot : Operator, IOperator
    {
        public double u;
        public InsertKnot() {}

        public bool Cook()
        {
            // constraints
            if ( u < 0 || u > 1 ) {
                errorMessage = "u needs to be between 0 and 1";
                return false;
            }
			
			// FIXME: support attribute transfer etc
            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsCurve ) {
                    var nc = primitive as NurbsCurve;
                    nc.InsertKnot ( u );
                    outputGeometry.Add ( nc );
                }
                else if ( primitive is NurbsPatch ) {
                    var np = primitive as NurbsPatch;
                    np.InsertKnotU ( u, 1 );
                    outputGeometry.Add ( np );
                }
                else
                    outputGeometry.Add ( primitive );
            }

            return true;
        }



    }

}