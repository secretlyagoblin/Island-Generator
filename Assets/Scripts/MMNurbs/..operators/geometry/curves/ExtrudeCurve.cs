using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;

namespace UnityNURBS.Operators
{

    public class ExtrudeCurve : Operator, IOperator
    {
        public mmVector3 extrusion;
        public int order;
        public int nrOfSpans;
        public ExtrudeCurve() {}

        public bool Cook()
        {
            if ( order < 2 ) {
                errorMessage = "order needs to be minimum 2";
                return false;
            }

            if ( nrOfSpans + 1 < order ) {
                errorMessage = "number of spans needs to be bigger than order-2";
                return false;
            }

            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsCurve ) {
                    var curve = primitive as NurbsCurve;
					outputGeometry.Add ( curve.ExtrudePatchV ( extrusion, order, nrOfSpans ) );
					// FIXME: support attribute transfer etc
                }
                else
                    outputGeometry.Add ( primitive );
            }

            return true;
        }


    }

}