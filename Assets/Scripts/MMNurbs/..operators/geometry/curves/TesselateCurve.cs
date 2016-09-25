using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using UnityNURBS.Render;

namespace UnityNURBS.Operators
{

    public class TesselateCurve : Operator, IOperator
    {
        public int mode;
        public int segments;
        public double length;
        public int lengthFidelity;
        public bool perSegment;  // split order 2 curves in segments (so existing point positions can be preserved)

        private List<double> uCoordinates, uList, curveLengths;
        private int numSamples;

        public TesselateCurve()
        {
            outputs.Add ( "u coordinates", null );
            outputs.Add ( "curve lengths", null );
        }


        private NurbsCurve Tesselate ( NurbsCurve curve )
        {
            mmVector3[] samples = curve.GetPoints ( uCoordinates.ToArray() );
            //get length and running totals by adding lengths of segments together
            double curveLength;
            double segmentLength = 0;
            var runningTotals = new double[ numSamples ];
            var tmp = new mmVector3();

            for ( int j = 0; j < numSamples - 1; j++ ) {
                tmp = samples[ j + 1 ] - samples[ j ];
                runningTotals[ j + 1 ] = runningTotals[ j ] + tmp.magnitude; // FIXME: magnitude = expensive!
            }

            //last one is total length
            curveLength = runningTotals[ numSamples - 1 ];
            curveLengths.Add ( curveLength );

            if ( mode == 0 )
                segmentLength = curveLength / segments; else if ( mode == 1 )

                segmentLength = length; else if ( mode == 2 )

                segmentLength = curveLength / ( System.Math.Floor ( curveLength / length ) );

            var controlPoints = new List<mmVector3>();
            double nextPointLength = segmentLength;
            //first point
            controlPoints.Add ( samples[ 0 ] );
            uList.Add ( uCoordinates[ 0 ] );
            //middle points
            int m = 0;

            while ( nextPointLength < curveLength ) {
                //brute force search
                while ( m < numSamples - 1 )
                    if ( runningTotals[ m++ ] > nextPointLength )
                        break;

                controlPoints.Add ( samples[ m ] );
                uList.Add ( uCoordinates[ m ] );
                nextPointLength += segmentLength;
            }

            //last point
            if ( m < numSamples - 1 ) {
                controlPoints.Add ( samples[ numSamples - 1 ] );
                uList.Add ( uCoordinates[ numSamples - 1 ] );
            }

            var tCurve = new NurbsCurve();
            tCurve.order = 2;
            tCurve.points = controlPoints;
            tCurve.knotVector = NurbsCurve.GenerateKnotVector ( 2, controlPoints.Count );
			
			// FIXME: support attribute transfer etc
			tCurve.material = new mmMaterial ( curve.material );

            return tCurve;
        }

        private NurbsCurve TesselateSplitCurve ( NurbsCurve curve )
        {
            var curves = new List<NurbsCurve>();

            for ( int k = 0; k < curve.numControlPoints - 1; k++ ) {
                var segment = new NurbsCurve();
                segment.color = curve.color;
                segment.order = 2;
                var controlPoints = new List<mmVector3> ();
                controlPoints.Add ( curve.points[ k ] );
                controlPoints.Add ( curve.points[ k + 1 ] );
                segment.knotVector = NurbsCurve.GenerateKnotVector ( 2, 2 );
                segment.points = controlPoints;
                curves.Add ( Tesselate ( segment ) );
            }

            var uListRedo = new List<double>();
            var tmpPoints = new List<mmVector3>();
            int uCounter = 0;

            for ( int i = 0; i < curves.Count - 1; i++ ) {
                int numPoints = curves[ i ].points.Count;
                tmpPoints.AddRange ( curves[ i ].points );
                uListRedo.AddRange ( uList.GetRange ( uCounter, numPoints ) );

                if ( curves[ i ].points[ numPoints - 1 ] == curves[ i + 1 ].points[ 0 ] ) { // line segments touching, so remove last point
                    tmpPoints.RemoveAt ( tmpPoints.Count - 1 );
                    uListRedo.RemoveAt ( uListRedo.Count - 1 );
                }

                uCounter += numPoints;
            }

            tmpPoints.AddRange ( curves[ curves.Count - 1 ].points );
            uListRedo.AddRange ( uList.GetRange ( uCounter, curves[ curves.Count - 1 ].points.Count ) );
            var tCurve = new NurbsCurve();
            tCurve.order = 2;
            tCurve.points = tmpPoints;
			tCurve.knotVector = NurbsCurve.GenerateKnotVector ( 2, tmpPoints.Count );
			
			// FIXME: support attribute transfer etc
			tCurve.material = new mmMaterial ( curve.material );

            return tCurve;
        }


        public bool Cook()
        {
            // some constraints
            if ( lengthFidelity < 10 ) {
                errorMessage = "Length fidelity must be 10 or greater.";
                return false;
            }

            if ( length <= 0 ) {
                errorMessage = "length need to be greater than zero";
                return false;
            }

            if ( segments < 0 ) {
                errorMessage = "number of segments needs to be positive";
                return false;
            }

            if ( numSamples != segments * lengthFidelity ) {
                numSamples = segments * lengthFidelity;
                uCoordinates = new List<double>();

                for ( double i = 0; i < numSamples; i++ )
                    uCoordinates.Add ( i / ( numSamples - 1 ) );
            }

            uList = new List<double>();
            curveLengths = new List<double>();

            // foreach(Primitive primitive in Primitive.Flatten (inputGeometry)) {  FIXME  better support for groups (preserve them!)
            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsCurve ) {
                    var curve = primitive as NurbsCurve;

                    if ( curve.order == 2 && perSegment )
                        outputGeometry.Add ( TesselateSplitCurve ( curve ) );
                    else outputGeometry.Add ( Tesselate ( curve ) );
                }
                else
                    outputGeometry.Add ( primitive );
            }

            outputs[ "u coordinates" ] = uList;
            outputs[ "curve lengths" ] = curveLengths;
            return true;
        }



    }

}