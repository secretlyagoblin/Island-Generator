
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using Ayam.Nurbs;

namespace Ayam.Nurbs
{
    public class NurbsLibCurve
    {


        /**
            @property curveHullColor		default color for rendering curve hulls
        **/
        public static Color curveHullColor = new Color ( 0.5f, 0.05f, 0.5f, 1.0f );

        /**
            @property curveHullColor		default color for rendering curves
        **/
        public static Color curveColor = new Color ( 1.0f, 0.05f, 0.5f, 1.0f );

        public static mmVector3 CurvePoint4D ( NurbsCurve nc, double u )
        {
            return CurvePoint4D ( nc.numControlPoints, nc.order, nc.knotVector, NurbsLibCore.PointsAndWeightsToArray4D ( nc.points, nc.pointWeights ), u );
        }

        /**
            Calculate a 4d point at u, from a set of nurbs curves parameters
            @method CurvePoint4D
            @param n				number of control points
            @param p				order of curve
            @param U				knot vector
            @param Pw			control points (xyzw)
            @param u				the u paramater for which the point is to be found
            @return double[ 4 ]	The calculated 4d point
        **/
        public static double[] CurvePoint4D ( int n, int p, double[] U, double[] Pw, double u )
        {
            n--;
            p--;
            int span, j, k;
            double[] Cw = new double[ 4 ];
            double[] Ct = new double[ 4 ];
            span = NurbsLibCore.FindSpan ( n, p, u, U );
            double[] N = NurbsLibCore.BasisFuns ( span, u, p, U );

            for ( j = 0; j <= p; j++ ) {
                k = ( span - p + j ) * 4;
                Cw[ 0 ] = Cw[ 0 ] + N[ j ] * Pw[ k ]     * Pw[ k + 3 ];
                Cw[ 1 ] = Cw[ 1 ] + N[ j ] * Pw[ k + 1 ] * Pw[ k + 3 ];
                Cw[ 2 ] = Cw[ 2 ] + N[ j ] * Pw[ k + 2 ] * Pw[ k + 3 ];
                Cw[ 3 ] = Cw[ 3 ] + N[ j ] * Pw[ k + 3 ];
            }

            Ct[ 0 ] = Cw[ 0 ] / Cw[ 3 ];
            Ct[ 1 ] = Cw[ 1 ] / Cw[ 3 ];
            Ct[ 2 ] = Cw[ 2 ] / Cw[ 3 ];
            Ct[ 3 ] = Cw[ 3 ];
            return Ct;
        }

        public static mmVector3 CurvePoint3D ( NurbsCurve nc, double u )
        {
            return CurvePoint3D ( nc.numControlPoints, nc.order, nc.knotVector, nc.points, u );
        }

        /**
            Calculate a 3d point at u, from a set of nurbs curves parameters
            @method CurvePoint3D
            @param n				number of control points
            @param p				order of curve
            @param U				knot vector
            @param P				control points (xyz)
            @param u				the u paramater for which the point is to be found
            @return double[ 3 ]	The calculated 3d point
        **/
        public static mmVector3 CurvePoint3D ( int n, int p, double[] U, List<mmVector3> P, double u )
        {
            n--;
            p--;
            int span, j, k;
            mmVector3 curvePoint = new mmVector3 ();
            span = NurbsLibCore.FindSpan ( n, p, u, U );
            double[] N = NurbsLibCore.BasisFuns ( span, u, p, U );

            for ( j = 0; j <= p; j++ ) {
                k = ( span - p + j );
                curvePoint.x = curvePoint.x + N[ j ] * P[ k ].x;
                curvePoint.y = curvePoint.y + N[ j ] * P[ k ].y;
                curvePoint.z = curvePoint.z + N[ j ] * P[ k ].z;
            }

            return curvePoint;
        }






        /*  AYAM - insert knot u into 3D non-rational curve (np, p, UP[], P[])
            r times; k: knot span, s: already present knot multiplicity (np >= r+s!)
            nq: new length, newKnots: new knots, Q: new controls (both allocated outside!)

            @methodInsertKnotCurve3D
            @param UQ = newKnots
            @param Q = newControls
            @param UP = oldKnots;
            @param P = oldControls
            @param np = numControlPoints  (was numcontrolpoints - 1 in Ayam)
            @param p = degree (was order -1 in Ayam)
            @param k = knotIndex (insert new knot after this index - ie which span to insert into)
            @param s = mult (pre existing multiplicity)
            @param r = numInserts (how many times to insert the knot)
            nq = is new NumControlPoints - this is no longer passed in as a parameter, as it is not needed in a non C context
        */


        public static void InsertKnotCurve3D ( int numControlPoints, int order, double[] oldKnots, double[] oldControls,
                                               double u, int knotIndex, int mult, int numInserts,
                                               double[] newKnots, double[] newControls )
        {
            numControlPoints --;		//ayam expected provided arguments to be pre-decremented
            int degree = order - 1;
            int i, j, L = 0, i1, i2;
            double mp = 0.0, alpha = 0.0;
            ;
            mp = numControlPoints + degree + 1;		//mp is the same as knotvector length?
            double[] R = new double[ ( degree + 1 ) * 3 ];		//used as temporary storage when working on controls

            // create new knot vector

            for ( i = 0; i <= knotIndex; i++ )
                newKnots[ i ] = oldKnots[ i ];

            for ( i = 1; i <= numInserts; i++ )
                newKnots[ knotIndex + i ] = u;

            for ( i = knotIndex + 1; i <= mp; i++ )
                newKnots[ i + numInserts ] = oldKnots[ i ];

            // save unaltered control points in newControls

            for ( i = 0; i <= ( knotIndex - degree ); i++ ) {
                i1 = i * 4;
                newControls[ i1 ] = oldControls[ i1 ];
                newControls[ i1 + 1 ] = oldControls[ i1 + 1 ];
                newControls[ i1 + 2 ] = oldControls[ i1 + 2 ];
            }

            for ( i = numControlPoints; i >= ( knotIndex - mult ); i-- ) {
                i1 = ( i + numInserts ) * 4;
                i2 = i * 4;
                newControls[ i1 ] = oldControls[ i2 ];
                newControls[ i1 + 1 ] = oldControls[ i2 + 1 ];
                newControls[ i1 + 2 ] = oldControls[ i2 + 2 ];
            }

            // calculate new controls

            for ( i = 0; i <= ( degree - mult ); i++ ) {
                i1 = i * 4;
                i2 = ( knotIndex - degree + i ) * 4;
                R[ i1 ] = oldControls[ i2 ];
                R[ i1 + 1 ] = oldControls[ i2 + 1 ];
                R[ i1 + 2 ] = oldControls[ i2 + 2 ];
            }

            for ( j = 1; j <= numInserts; j++ ) {
                L = knotIndex - degree + j;

                for ( i = 0; i <= degree - j - mult; i++ ) {
                    alpha = ( u - oldKnots[ L + i ] ) / ( oldKnots[ i + knotIndex + 1 ] - oldKnots[ L + i ] );
                    i1 = ( i + 1 ) * 4;
                    i2 = ( i ) * 4;
                    R[ i2 ] = alpha * R[ i1 ] + ( 1.0 - alpha ) * R[ i2 ];
                    R[ i2 + 1 ] = alpha * R[ i1 + 1 ] + ( 1.0 - alpha ) * R[ i2 + 1 ];
                    R[ i2 + 2 ] = alpha * R[ i1 + 2 ] + ( 1.0 - alpha ) * R[ i2 + 2 ];
                }

                i1 = L * 4;
                newControls[ i1 ] = R[ 0 ];
                newControls[ i1 + 1 ] = R[ 1 ];
                newControls[ i1 + 2 ] = R[ 2 ];
                i1 = ( knotIndex + numInserts - j - mult ) * 4;
                i2 = ( degree - j - mult ) * 4;
                newControls[ i1 ] = R[ i2 ];
                newControls[ i1 + 1 ] = R[ i2 + 1 ];
                newControls[ i1 + 2 ] = R[ i2 + 2 ];
            }

            for ( i = L + 1; i < knotIndex - mult; i++ ) {
                i1 = i * 4;
                i2 = ( i - L ) * 4;
                newControls[ i1 ] = R[ i2 ];
                newControls[ i1 + 1 ] = R[ i2 + 1 ];
                newControls[ i1 + 2 ] = R[ i2 + 2 ];
            }
        }

        /*  AYAM - insert knot u into 4D non-rational curve (np, p, UP[], P[])
            r times; k: knot span, s: already present knot multiplicity (np >= r+s!)
            nq: new length, newKnots: new knots, Q: new controls (both allocated outside!)

            @methodInsertKnotCurve3D
            @param UQ = newKnots
            @param Q = newControls
            @param UP = oldKnots;
            @param P = oldControls
            @param np = numControlPoints  (was numcontrolpoints - 1 in Ayam)
            @param p = degree ( was order -1 in Ayam)
            @param k = knotIndex (insert new knot after this index - ie which span to insert into)
            @param s = mult (pre existing multiplicity)
            @param r = numInserts (how many times to insert the knot)
            nq = is new NumControlPoints - this is no longer passed in as a parameter, as it is not needed in a non C context
        */

		public static void InsertKnotCurve4D ( int np, int order, double[] UP, List<mmVector3> controlpoints, List<double> weights,
                                               double u, int k, int s, int numInserts,
                                               ref double[] newKnots, ref double[] newControls )
        {
            np --;		//ayam expected provided arguments to be pre-decremented
            int p = order - 1;
            int i, j, L = 0, i1, i2;
            double mp = 0.0, alpha = 0.0;
            mp = np + p + 1;
//		double[] newControls = new double[ (np+numInserts+1)*4 ];
            double[] oldControls = NurbsLibCore.PointsAndWeightsToArray4D ( controlpoints, weights );
            double[] R = new double[ ( p + 1 ) * 4 ];		//used as temporary storage when working on controls

            // create new knot vector

            for ( i = 0; i <= k; i++ )
                newKnots[ i ] = UP[ i ];

            for ( i = 1; i <= numInserts; i++ )
                newKnots[ k + i ] = u;

            for ( i = k + 1; i <= mp; i++ )
                newKnots[ i + numInserts ] = UP[ i ];

            // save unaltered control points in newControls

            for ( i = 0; i <= ( k - p ); i++ ) {
                i1 = i * 4;
                newControls[ i1 ] = oldControls[ i1 ];
                newControls[ i1 + 1 ] = oldControls[ i1 + 1 ];
                newControls[ i1 + 2 ] = oldControls[ i1 + 2 ];
                newControls[ i1 + 3 ] = oldControls[ i1 + 3 ];
            }

            for ( i = np; i >= ( k - s ); i-- ) {
                i1 = ( i + numInserts ) * 4;
                i2 = i * 4;
                newControls[ i1 ] = oldControls[ i2 ];
                newControls[ i1 + 1 ] = oldControls[ i2 + 1 ];
                newControls[ i1 + 2 ] = oldControls[ i2 + 2 ];
                newControls[ i1 + 3 ] = oldControls[ i2 + 3 ];
            }

            // calculate new controls

            for ( i = 0; i <= ( p - s ); i++ ) {
                i1 = i * 4;
                i2 = ( k - p + i ) * 4;
                R[ i1 ] = oldControls[ i2 ] * oldControls[ i2 + 3 ];
                R[ i1 + 1 ] = oldControls[ i2 + 1 ] * oldControls[ i2 + 3 ];
                R[ i1 + 2 ] = oldControls[ i2 + 2 ] * oldControls[ i2 + 3 ];
                R[ i1 + 3 ] = oldControls[ i2 + 3 ];
            }

            for ( j = 1; j <= numInserts; j++ ) {
                L = k - p + j;

                for ( i = 0; i <= p - j - s; i++ ) {
                    alpha = ( u - UP[ L + i ] ) / ( UP[ i + k + 1 ] - UP[ L + i ] );
                    i1 = ( i + 1 ) * 4;
                    i2 = ( i ) * 4;
                    R[ i2 ] = alpha * R[ i1 ] + ( 1.0 - alpha ) * R[ i2 ];
                    R[ i2 + 1 ] = alpha * R[ i1 + 1 ] + ( 1.0 - alpha ) * R[ i2 + 1 ];
                    R[ i2 + 2 ] = alpha * R[ i1 + 2 ] + ( 1.0 - alpha ) * R[ i2 + 2 ];
                    R[ i2 + 3 ] = alpha * R[ i1 + 3 ] + ( 1.0 - alpha ) * R[ i2 + 3 ];
                }

                i1 = L * 4;
                newControls[ i1 ] = R[ 0 ] / R[ 3 ];
                newControls[ i1 + 1 ] = R[ 1 ] / R[ 3 ];
                newControls[ i1 + 2 ] = R[ 2 ] / R[ 3 ];
                newControls[ i1 + 3 ] = R[ 3 ];
                i1 = ( k + numInserts - j - s ) * 4;
                i2 = ( p - j - s ) * 4;
                newControls[ i1 ] = R[ i2 ] / R[ i2 + 3 ];
                newControls[ i1 + 1 ] = R[ i2 + 1 ] / R[ i2 + 3 ];
                newControls[ i1 + 2 ] = R[ i2 + 2 ] / R[ i2 + 3 ];
                newControls[ i1 + 3 ] = R[ i2 + 3 ];
            }

            for ( i = L + 1; i < k - s; i++ ) {
                i1 = i * 4;
                i2 = ( i - L ) * 4;
                newControls[ i1 ] = R[ i2 ] / R[ i2 + 3 ];
                newControls[ i1 + 1 ] = R[ i2 + 1 ] / R[ i2 + 3 ];
                newControls[ i1 + 2 ] = R[ i2 + 2 ] / R[ i2 + 3 ];
                newControls[ i1 + 3 ] = R[ i2 + 3 ];
            }
        }

        /**
            Split a curve at parmateric value u.
            This function requires two NurbsCurve objects, The first is the
            curve to split, the second is an empty NurbsCurve to place the second curve segment in
            THe first curve is altered by this operation
            @method CurveSplit
            @param src					the curve to split - altered by this operation
            @param u						the parametric value u at which to split the curve
            @param nc2					an empty NurbsCurve object, which will contain the second half of the split
        **/
        public static void CurveSplit ( NurbsCurve nc1, double u, NurbsCurve nc2 )
        {
            double[] knots;
            double[] newcontrolv = null;
            double[] newknotv = null;
            int stride = 4, k = 0, r = 0, s = 0, nc1len = 0;

            knots = nc1.knotVector;

            if ( ( u <= knots[ 0 ] ) || ( u >= knots[ nc1.numControlPoints ] ) )
                Debug.Log( "NurbsLibCurve.CurveSplit: parameter u out of range." );

            int[] fsm = NurbsLibCore.FindSpanMult ( nc1.numControlPoints - 1, nc1.order - 1, u, knots );
            k = fsm[ 0 ];
            s = fsm[ 1 ];

            if ( s == nc1.order )
                Debug.Log ( "splitDisc" );

            if ( s > nc1.order )
                Debug.Log( "NurbsLibCurve.CurveSplit: invalid number of knots" );

            r = nc1.order - 1 - s;
			int newNumControlPoints1 = nc1.numControlPoints + r;

            if ( r > 0 ) {
				newcontrolv = new double[ newNumControlPoints1 * stride ];
				newknotv = new double[ newNumControlPoints1 + nc1.order ];
            }

			InsertKnotCurve4D ( nc1.numControlPoints, nc1.order, nc1.knotVector, nc1.points, nc1.pointWeights, u, k, s, r, ref newknotv, ref newcontrolv );
            NurbsLibCore.Array4DToPointsAndWeights ( nc1, newcontrolv );
            nc1.knotVector = newknotv;

            if ( r != 0 )
                nc1len = k - ( nc1.order - 1 ) + 1 + ( nc1.order - 1 - s + r - 1 ) / 2 + 1;
			else
                nc1len = k - ( nc1.order - 1 ) + 1;

			int newNumControlPoints2 = ( newNumControlPoints1 + 1 ) - nc1len;
            newNumControlPoints1 = nc1len;
            nc2.order = nc1.order;
            newcontrolv = new double[ newNumControlPoints1 * stride ];
            newknotv = new double[ newNumControlPoints1 + nc1.order ];
			System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( nc1.points, nc1.pointWeights ), 0, newcontrolv, 0, newNumControlPoints1 * stride );
            System.Array.Copy ( nc1.knotVector, 0, newknotv, 0, newNumControlPoints1 + nc1.order );
            /* improve phantom knot 1*/
			newknotv[ newNumControlPoints1 + nc1.order - 1 ] = newknotv[ newNumControlPoints1 + nc1.order - 2 ];
			double[] tmpControlPoints = new double[ newNumControlPoints2 * stride ];
			nc2.knotVector = new double[ newNumControlPoints2 + nc2.order ];
			System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( nc1.points, nc1.pointWeights ), ( newNumControlPoints1 - 1 ) * stride, tmpControlPoints, 0, newNumControlPoints2 * stride );
			System.Array.Copy ( nc1.knotVector, newNumControlPoints1 - 1, nc2.knotVector, 0, newNumControlPoints2 + nc2.order );
            NurbsLibCore.Array4DToPointsAndWeights ( nc2, tmpControlPoints );
            nc2.knotVector[ 0 ] = nc2.knotVector[ 1 ];
            NurbsLibCore.Array4DToPointsAndWeights ( nc1, newcontrolv );
            nc1.knotVector = newknotv;
            NurbsLibCore.NormaliseKnotVector ( nc1.knotVector );
            NurbsLibCore.NormaliseKnotVector ( nc2.knotVector );
        }


        /**
            Concatenate a number of NURBS curves
            joins together the curves, can also create fillets between gaps, C1 continuity only

            @method CurveConcat
            @param curves				the curves to concatenate
            @param bool					whether to close the concatenated curves
            @param fillgaps				whether to create fillet curves for the gaps
        **/
        public static NurbsCurve CurveConcat ( NurbsCurve[] curves, bool closed = false, bool fillgaps = false )
        {
//		NurbsCurve newCurve = null;
            double[] newknotv;
            double[] newcontrolv;
            int ncv;
//	 	int numcurves = 0;
            int i, j, k, a, order = -1, length = 0;
//		int ktype;

            //number of curves
            for ( i = 0; i < curves.Length; i++ )
                length += curves[ i ].numControlPoints;

            if ( closed && !fillgaps )
                length++;

            /* take order from first curve */
            order = curves[ 0 ].order;
            /* construct new knot vector */
            //assume knot knot custom
            newknotv = new double[ length + order ];
            a = 0;
            j = 0;
            k = 0;

            for ( int c = 0; c < curves.Length; c++ ) {
                for ( i = k; i < curves[ c ].numControlPoints + curves[ c ].order; i++ ) {
                    newknotv[ a ] = curves[ c ].knotVector[ i ] + j;
                    a++;
                }

                if ( c < curves.Length - 1 )
                    k = curves[ c + 1 ].order;

                j++;
            }

            if ( closed && !fillgaps ) {
                for ( i = a; i < length + order; i++ )
                    newknotv[ i ] = newknotv[ a - 1 ] + 1;
            }

            /* construct new control point vector */
            newcontrolv = new double[ length * 4 ];
            ncv = 0;

            for ( i = 0; i < curves.Length; i++ ) {
                System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( curves[ i ].points, curves[ i ].pointWeights ), 0, newcontrolv, ncv, curves[ i ].points.Count );
                ncv += ( curves[ i ].numControlPoints * 4 );
            }

            if ( closed && !fillgaps )
                System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( curves[ 0 ].points, curves[ 0 ].pointWeights ), 0, newcontrolv, ( length - 1 ) * 4, 4 );

            return new NurbsCurve ( newknotv, NurbsLibCore.Array4DToPoints ( newcontrolv ), NurbsLibCore.Array4DToWeights ( newcontrolv ), order );
        }

        /**
            Created a fillet curve between two NURBS curves
            This creates a curve that is C1 continuous with the other two curves
            The placement of the middle control points along the tangent line is set using the tanlen parameter
            the fillet is created from the last control point if the first curve to the first control point of the second
            @method CurveFillGapC1
            @param order				the order of the curve to create
            @param tanlen				how far along line tangent to join points the middle control points will be created
            @param c1					the first curve
            @param c2					the second curve
            @return NurbsCurve			The fillet curve
        **/
        public static NurbsCurve CurveFillGapC1 ( int order, double tanlen, NurbsCurve c1, NurbsCurve c2 )
        {
            NurbsCurve newCurve = null;
            double[] p1;
            double[] p2;
            double[] p3 = new double[ 4 ];
            double[] p4 = new double[ 4 ];
            double[] n1 = new double[ 3 ];
            double[] n2 = new double[ 3 ];
            double[] l = new double[ 3 ];
            double[] U;
            double[] Pw;
            double u, d, w, len;
            double[] controlv;
            int n, p, numcontrol;
            n = c1.numControlPoints;
            p = c1.order - 1;
            U = c1.knotVector;
            Pw = NurbsLibCore.PointsAndWeightsToArray4D ( c1.points, c1.pointWeights );
            /*  get coordinates of the first and last point of the curve
                as well as the first derivative in those points */
            u = U[ n ];
            p1 = CurvePoint4D ( n, p, U, Pw, u );
            w = p1[ 3 ];
            p1[ 0 ] *= 1.0 / w;
            p1[ 1 ] *= 1.0 / w;
            p1[ 2 ] *= 1.0 / w;
            p1[ 3 ] = 1.0;
            n1 = NurbsLibCore.ComputeFirstDerivative4D ( n - 1, p, U, Pw, u );
            /* normalize n1 */
            len = Math.Sqrt ( n1[ 0 ] * n1[ 0 ] + n1[ 1 ] * n1[ 1 ] + n1[ 2 ] * n1[ 2 ] );
            n1[ 0 ] *= 1.0 / len;
            n1[ 1 ] *= 1.0 / len;
            n1[ 2 ] *= 1.0 / len;
            n = c2.numControlPoints;
            p = c2.order - 1;
            U = c2.knotVector;
            Pw = NurbsLibCore.PointsAndWeightsToArray4D ( c2.points, c2.pointWeights );
            u = U[ n ];
            p2 = CurvePoint4D ( n, p, U, Pw, u );
            w = p2[ 3 ];
            p2[ 0 ] *= 1.0 / w;
            p2[ 1 ] *= 1.0 / w;
            p2[ 2 ] *= 1.0 / w;
            p2[ 3 ] = 1.0;
            n2 = NurbsLibCore.ComputeFirstDerivative4D ( n - 1, p, U, Pw, u );
            /* normalize n2 */
            len = Math.Sqrt ( n2[ 0 ] * n2[ 0 ] + n2[ 1 ] * n2[ 1 ] + n2[ 2 ] * n2[ 2 ] );
            n2[ 0 ] *= 1.0 / len;
            n2[ 1 ] *= 1.0 / len;
            n2[ 2 ] *= 1.0 / len;

            /* first, check whether p1 and p2 are sufficiently different */
            if ( ( Math.Abs ( p1[ 0 ] - p2[ 0 ] ) < double.Epsilon ) &&
                    ( Math.Abs ( p1[ 1 ] - p2[ 1 ] ) < double.Epsilon ) &&
                    ( Math.Abs ( p1[ 2 ] - p2[ 2 ] ) < double.Epsilon ) ) {
                /* No, no fillet needs to be created, just bail out! */
                Debug.Log( "NurbsLibCurve.CurveFillGapC1: Points are too close together, no fillet needed" );
                return null;
            }

            n1[ 0 ] *= -1.0;
            n1[ 1 ] *= -1.0;
            n1[ 2 ] *= -1.0;
            p3[ 3 ] = 1.0;
            p4[ 3 ] = 1.0;
            l[ 0 ] = p2[ 0 ] - p1[ 0 ];
            l[ 1 ] = p2[ 1 ] - p1[ 1 ];
            l[ 2 ] = p2[ 2 ] - p1[ 2 ];
            d = Math.Sqrt ( l[ 0 ] * l[ 0 ] + l[ 1 ] * l[ 1 ] + l[ 2 ] * l[ 2 ] );
            n1[ 0 ] *= d * tanlen;
            n1[ 1 ] *= d * tanlen;
            n1[ 2 ] *= d * tanlen;
            n2[ 0 ] *= d * tanlen;
            n2[ 1 ] *= d * tanlen;
            n2[ 2 ] *= d * tanlen;
            p3[ 0 ] = p1[ 0 ] - n1[ 0 ];
            p3[ 1 ] = p1[ 1 ] - n1[ 1 ];
            p3[ 2 ] = p1[ 2 ] - n1[ 2 ];
            p4[ 0 ] = p2[ 0 ] + n2[ 0 ];
            p4[ 1 ] = p2[ 1 ] + n2[ 1 ];
            p4[ 2 ] = p2[ 2 ] + n2[ 2 ];

            if ( order == 2 )
                numcontrol = 2;
            else
                numcontrol = 4;

            controlv = new double[ numcontrol * 4 ];

            if ( order == 2 ) {
                System.Array.Copy ( p1, 0, controlv, 0, 4 );
                System.Array.Copy ( p2, 0, controlv, ( numcontrol - 1 ) * 4, 4 );
            }
            else {
                System.Array.Copy ( p1, 0, controlv, 0, 4 );
                System.Array.Copy ( p3, 0, controlv, 4, 4 );
                System.Array.Copy ( p4, 0, controlv, ( numcontrol - 2 ) * 4, 4 );
                System.Array.Copy ( p2, 0, controlv, ( numcontrol - 1 ) * 4, 4 );
            }

            if ( order == 3 ) {
                newCurve = new NurbsCurve ( new double[] {0, 0, 0, 1, 1, 1}, NurbsLibCore.Array4DToPoints ( controlv ), NurbsLibCore.Array4DToWeights ( controlv ), order );
            }
            else {
                newCurve = new NurbsCurve ( new double[] {0, 0, 0, 0, 1, 1, 1, 1}, NurbsLibCore.Array4DToPoints ( controlv ), NurbsLibCore.Array4DToWeights ( controlv ), order );
            }

            if ( order > 4 && order > numcontrol )
                Debug.Log( "NurbsLibCurve.CurveFillGapC1: order > 4 not currently supported (no elevation ported)" );

            return newCurve;
        }
    }
}

