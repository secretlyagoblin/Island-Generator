using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;

namespace UnityNURBS.Operators
{

    public class ExtractCurve : Operator, IOperator
    {
        public bool extractU;
        public double uvParameter;

        public ExtractCurve() {}

        public bool Cook()
        {
            // constraints
            if ( uvParameter < 0 || uvParameter > 1 ) {
                errorMessage = "uvParameter needs to be between 0 and 1";
                return false;
            }

            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsPatch ) {
                    var patch = ( NurbsPatch ) primitive;
                    if ( extractU )
                        outputGeometry.Add ( patch.ExtractCurveU ( uvParameter ) ); 
					else
						outputGeometry.Add ( patch.ExtractCurveV ( uvParameter ) );
					// FIXME: support attribute transfer etc
                }
                else
                    outputGeometry.Add ( primitive );
            }

            return true;
        }


    }

}