using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using UnityNURBS.Render;

namespace UnityNURBS.Operators
{

    public class SplitCurve : Operator, IOperator
    {
        public double u;
        public bool outputSplitPoints;

        public SplitCurve() {}

        public bool Cook()
        {
            if ( u < 0 || u > 1 ) {
                errorMessage = "u needs to be between (and not including) 0 and 1";
                return false;
            }

            var pointList = new PositionsPrimitive();
            pointList.points = new List<mmVector3>();

            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsCurve ) {
                    if ( outputSplitPoints ) {
                        var curve = ( NurbsCurve ) primitive;
                        pointList.points.Add ( curve.GetPoint ( u ) );
                    }
					else {
						// FIXME: support attribute transfer etc
                        var thisCurve = ( NurbsCurve ) primitive;
                        outputGeometry.Add ( thisCurve );

                        if ( u != 0 & u != 1 ) {  // if U == 0 or 1, just output orig curve
                            var newCurve = new NurbsCurve();
                            thisCurve.Split ( u, newCurve );
							newCurve.material = new mmMaterial ( thisCurve.material );
                            outputGeometry.Add ( newCurve );
                        }
                    }
                }
                else
                    outputGeometry.Add ( primitive );
            }

            if ( outputSplitPoints )
                outputGeometry.Add ( pointList );

            return true;
        }



    }

}