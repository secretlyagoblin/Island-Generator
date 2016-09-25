using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using Ayam.Nurbs;

namespace Ayam.Nurbs
{
    public class NurbsLibPatch
    {


        /**
            Create a patch from a number of NURBS curves
            NOTE : CURRENTLY ONLY WORKS WITH 3D CURVES
            @method PatchSkinCurves
            @param curves		the curves from which to make the skin.
            @param orderU		the order of the patch in the U direction
            @param u				the u paramater for which the point is to be found
            @return NurbsPatch
         **/
        public static NurbsPatch PatchSkinCurves ( NurbsCurve[] curves, int orderU )
        {
            //-------- assumes curves compatible -----------
            //uses AYAM custom type nurbs
            //treats curves input as V, needs to construct U
            NurbsCurve curve;
            NurbsCurve c1;
            NurbsCurve c2;
            int numcurves = curves.Length;
            int K;
            int N;
            int b;
            int degU;
            double[] controlPoints;
            double[] knotsU;
            double[] knotsV;
            double[] uk;
            double[] d;
            mmVector3 v = new mmVector3 ();
            //Get knotsV
            curve = curves[ 0 ];
            knotsV = new double[ curve.knotVector.Length ];		//knots V is the same as every curves knot vector
            //could check here for curve compatibility
            //so take the first one,
            curve.knotVector.CopyTo ( knotsV, 0 );

            //check orderU	(desired order in U direction)
            if ( orderU < 2 )
                orderU = 3;

            if ( orderU > numcurves )
                orderU = numcurves;

            K = numcurves;
            N = curve.numControlPoints;
            degU = orderU - 1;
            uk = new double[ numcurves ];
            d = new double[ curve.numControlPoints ];
            knotsU = new double[ numcurves + orderU ];

            for ( int i = 0; i < N; i++ ) {	//To curve length
                d[ i ] = 0;
                c1 = curves[ 0 ];

                for ( int k = 1; k < K; k++ ) {		//for each curve
                    c2 = curves[ k ];
                    v.x = c2.points[ i ].x - c1.points[ i ].x;
                    v.y = c2.points[ i ].y - c1.points[ i ].y;
                    v.z = c2.points[ i ].z - c1.points[ i ].z;
                    d[ i ] += mmVector3.Magnitude ( new mmVector3 ( v.x, v.y, v.z ) );
                    c1 = c2;
                }

                if ( d[ i ] < double.Epsilon )
                    Debug.Log( "NurbsLibPatch.PatchSkinCurves: precision error" );
            }

            c1 = curves[ 0 ];
            uk[ 0 ] = 0;

            for ( int k = 1; k < K; k++ ) {	//to num curves
                c2 = curves[ k ];
                uk[ k ] = 0;

                for ( int i = 0; i < N; i++ ) {
                    v.x = ( c2.points[ i ].x - c1.points[ i ].x ) / d[ i ];
                    v.y = ( c2.points[ i ].y - c1.points[ i ].y ) / d[ i ];
                    v.z = ( c2.points[ i ].z - c1.points[ i ].z ) / d[ i ];
                    uk[ k ] += mmVector3.Magnitude ( new mmVector3 ( v.x, v.y, v.z ) );
                }

                uk[ k ] /= N;
                uk[ k ] += uk[ k - 1 ];
                c1 = c2;
            }

            uk[ numcurves - 1 ] = 1.0;

            for ( int i = 1; i < K - degU; i++ ) {
                knotsU[ i + degU ] = 0.0;

                for ( int k = i; k < i + degU; k++ )
                    knotsU[ i + degU ] += uk[ k ];

                knotsU[ i + degU ] /= degU;
            }

            for ( int i = 0; i <= degU; i++ )
                knotsU[ i ] = 0.0;

            for ( int i = K; i <= K + degU; i++ )
                knotsU[ i ] = 1.0;

            /* construct patch */
            curve = curves[ 0 ];
            controlPoints = new double[ curve.numControlPoints * numcurves * 4 ];
            b = 0;
            double[] cp;
            //		int count = 0;

            foreach ( NurbsCurve c in curves ) {
                cp = NurbsLibCore.PointsAndWeightsToArray4D ( c.points, c.pointWeights );
                cp.CopyTo ( controlPoints, b );
                b += c.points.Count * 4;
            }

            return  new NurbsPatch ( numcurves, curves[ 0 ].numControlPoints, orderU, curves[ 0 ].order, knotsU, knotsV, NurbsLibCore.Array4DToPoints ( controlPoints ), NurbsLibCore.Array4DToWeights ( controlPoints ) );	 // FIXME 	Array4DToPoints + Array4DToWeights inefficient
        }

        /**
            Interpolates a patch so that it fits precisely to curves
            @method PatchInterpolateU
            @param np 	NURBS patch object to interpolate
            @param order	desired interpolation order
        */

        public static void PatchInterpolateU ( NurbsPatch np, int order )
        {
            int i, k, N, K, stride, pu, num;
            double[] Pw;
            double[] v = new double[ 3 ];
            double total, d;
            K = np.controlPointsU;
            N = np.controlPointsV;
            stride = 4;
            double[] uk = new double[ K ];
            double[] cds = new double[ K + 1 ];
            double[] U = new double[ K + np.orderU ];
            double[] Q = new double[ K * stride ];
            //Pw = new double[ np.controlPoints.Length ];
            //np.controlPoints.CopyTo(Pw,0);
            Pw = NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights );
            pu = order - 1;
            num = np.controlPointsV;
            int ind1, ind2;

            for ( i = 0; i < N; i++ ) {
                ind1 = i * stride;
                ind2 = i * stride;
                total = 0.0;

                for ( k = 1; k < K; k++ ) {
                    ind1 += N * stride;
                    v[ 0 ] = Pw[ ind1 ] - Pw[ ind2 ];
                    v[ 1 ] = Pw[ ind1 + 1 ] - Pw[ ind2 + 1 ];
                    v[ 2 ] = Pw[ ind1 + 2 ] - Pw[ ind2 + 2 ];

                    if ( Math.Abs ( v[ 0 ] ) > double.Epsilon || Math.Abs ( v[ 1 ] ) > double.Epsilon || Math.Abs ( v[ 2 ] ) > double.Epsilon ) {
                        //assumes AYAM knot type is custom
                        cds[ k ] = mmVector3.Magnitude ( new mmVector3 ( v[ 0 ], v[ 1 ], v[ 2 ] ) );
                        total += cds[ k ];
                    }

                    ind2 += N * stride;
                }

                if ( total < double.Epsilon )
                    num--; else {

                    d = 0.0;

                    for ( k = 1; k < K; k++ ) {
                        d += cds[ k ];
                        uk[ k ] += d / total;
                    }
                }
            }

            if ( num == 0 )
                Debug.Log( "NurbsLibPatch.PatchInterpolateU: can not interpolate this patch." );

            uk[ 0 ] = 0.0;

            for ( k = 1; k < K; k++ )
                uk[ k ] /= num;

            uk[ K - 1 ] = 1.0;

            for ( i = 1; i < ( K - pu ); i++ ) {
                U[ i + pu ] = 0.0;

                for ( k = i; k < ( i + pu ); k++ )
                    U[ i + pu ] += uk[ k ];

                U[ i + pu ] /= pu;
            }

            for ( i = 0; i <= pu; i++ )
                U[ i ] = 0.0;

            for ( i = K; i < ( K + pu + 1 ); i++ )
                U[ i ] = 1.0;

            int idx1;

            // interpolate
            for ( i = 0; i < N; i++ ) {		//For each col
                idx1 = i * stride;

                for ( k = 0; k < K; k++ ) {	//get each row vecs
                    Q[ k * 4 ] = Pw[ idx1 ];
                    Q[ k * 4 + 1 ] = Pw[ idx1 + 1 ];
                    Q[ k * 4 + 2 ] = Pw[ idx1 + 2 ];
                    Q[ k * 4 + 3 ] = Pw[ idx1 + 3 ];

                    if ( stride != 4 )
                        Q[ k * 4 + 3 ] = 1.0;

                    idx1 += N * stride;
                }

                NurbsLibCore.GlobalInterpolation4D ( K - 1, Q, uk, U, pu ); //*interp them
                idx1 = i * stride;

                for ( k = 0; k < K; k++ ) {	//put each vec row back
                    Pw[ idx1 ] = Q[ k * stride ];
                    Pw[ idx1 + 1 ] = Q[ k * stride + 1 ];
                    Pw[ idx1 + 2 ] = Q[ k * stride + 2 ];
                    Pw[ idx1 + 3 ] = Q[ k * stride + 3 ];
                    idx1 += N * stride;
                }
            }

            np.orderU = pu + 1;
            NurbsLibCore.Array4DToPointsAndWeights ( np, Pw );
        }


        //this enum can be used for extract options
        public enum ExtractOption {
            uStart,
            uEnd,
            vStart,
            vEnd,
            uParam,
            vParam
        }
        ;

        /*
            Extract a NURBS curve from a NURBS patch

            @method PatchExtractCurve
            @param np		the patch to extract from
            @param side		AYAM- side  specifies extraction of a boundary curve (0-3), of a curve at a
         					specific parametric value (4 - along u dimension, 5 - along v dimension),
         					the complete boundary curve (6), or the middle axis (7,8)ONLY 0 TO 5 are supported here
            @param param		parametric value at which curve is extracted; this parameter is
           				ignored for the extraction of boundary curves
            @param relative  should param be interpreted in a relative way wrt. the knot
        				    vector?; this parameter is ignored for the extraction of boundary curves
            @return NurbsCurve
        */
        public static NurbsCurve PatchExtractCurve ( NurbsPatch np, int side, double param, bool relative )
        {
            if ( side > 5 || side < 0 )
                Debug.Log ( "extraction option not currently supported" );

            NurbsCurve nc = new NurbsCurve ();
            double[] cv, Qw, UVQ;
            double uv, uvmin, uvmax;
            int stride = 4, i, a, k, s, r;
			int newNumControlPoints = 0;

            switch ( side ) {
                case 0:
                case 1:
                    nc.order = np.orderU;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsU;
                    break;

                case 2:
                case 3:
                    nc.order = np.orderV;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsV;
                    break;

                case 4:
                    nc.order = np.orderU;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsU;
                    break;

                case 5:
                    nc.order = np.orderV;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsV;
                    break;

                case 7:
                    nc.order = np.orderU;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsU;
                    break;

                case 8:
                    nc.order = np.orderV;
                    //no knot type ported from ayam at this stage
					newNumControlPoints = np.controlPointsV;
                    break;
            }

            double[] controlPoints;
            //ayam creates tranform matrix here, but transforms currently not being ported
			nc.knotVector = new double[ newNumControlPoints + nc.order ];
			controlPoints = new double[ newNumControlPoints * stride ];
            cv = controlPoints;

            switch ( side ) {
                case 0: /* u0 */
                    a = 0;

					for ( i = 0; i < newNumControlPoints * stride; i += stride ) {
                        cv[ i ] = np.points[ a ].x;
                        cv[ i + 1 ] = np.points[ a ].y;
                        cv[ i + 2 ] = np.points[ a ].z;
                        //cv[ i+3 ] = np.controlPoints[ a ];
                        cv[ i + 3 ] = 1;
                        a += np.controlPointsV;
                    }

                    System.Array.Copy ( np.knotVectorU, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 1:	 /* un */
                    a = ( np.controlPointsV - 1 ) * stride;

					for ( i = 0; i < newNumControlPoints * stride; i += stride ) {
                        cv[ i ] = np.points[ a ].x;
                        cv[ i + 1 ] = np.points[ a ].y;
                        cv[ i + 2 ] = np.points[ a ].z;
                        //cv[ i+3 ] = np.controlPoints[ a ];
                        cv[ i + 3 ] = 1;
                        a += np.controlPointsV;
                    }

                    System.Array.Copy ( np.knotVectorU, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 2: /* v0 */
                    System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights ), 0, controlPoints, 0, controlPoints.Length );
                    System.Array.Copy ( np.knotVectorV, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 3:	/* vn */
				System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights ), ( ( np.controlPointsU * np.controlPointsV ) - np.controlPointsV ) * stride, controlPoints, 0, newNumControlPoints * 4 );
                    System.Array.Copy ( np.knotVectorV, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 4: /* along u */
                    if ( relative ) {				//not surehow to test this
                        uvmin = np.knotVectorV[ np.orderV - 1 ];
                        uvmax = np.knotVectorV[ np.controlPointsV ];
                        uv = uvmin + ( ( uvmax - uvmin ) * param );
                    }
                    else
                        uv = param;

                    int[] fsm = NurbsLibCore.FindSpanMult ( np.controlPointsV - 1, np.orderV - 1, uv, np.knotVectorV );
                    k = fsm[ 0 ];
                    s = fsm[ 1 ];
                    r = np.orderV - s - 1;

                    if ( r > 0 ) {
                        Qw = new double[ ( ( np.controlPointsV + r ) * np.controlPointsU ) * stride ];
                        UVQ = new double[ np.controlPointsV + np.orderV + r ];
                        NurbsLibCore.InsertKnotSurfaceV ( stride, np.controlPointsU, np.controlPointsV, np.orderV - 1,
                                                          np.knotVectorV, NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights ), uv, k, s, r, UVQ, Qw );
                    }
                    else
                        Qw = NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights );

                    if ( r > 0 )
                        a = k - ( np.orderV - 1 ) + ( np.orderV - 1 - s + r - 1 ) / 2 + 1; else

                        a = k - ( np.orderV - 1 );

                    a *= stride;

				for ( i = 0; i < newNumControlPoints * stride; i += stride ) {
                        cv[ i ] = Qw[ a ];
                        cv[ i + 1 ] = Qw[ a + 1 ];
                        cv[ i + 2 ] = Qw[ a + 2 ];
                        cv[ i + 3 ] = Qw[ a + 3 ];

                        if ( r > 0 )
                            a += ( np.controlPointsV + r ) * stride; else

                            a += np.controlPointsV * stride;
                    }

                    System.Array.Copy ( np.knotVectorU, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 5: /* along v */
                    if ( relative ) {
                        uvmin = np.knotVectorU[ np.orderU - 1 ];
                        uvmax = np.knotVectorU[ np.controlPointsU ];
                        uv = uvmin + ( ( uvmax - uvmin ) * param );
                    }
                    else
                        uv = param;

                    int[] fsm2 = NurbsLibCore.FindSpanMult ( np.controlPointsU - 1, np.orderU - 1, uv, np.knotVectorU );
                    k = fsm2[ 0 ];
                    s = fsm2[ 1 ];
                    r = np.orderU - s - 1;

                    if ( r > 0 ) {
                        Qw = new double[ ( ( np.controlPointsU + r ) * np.controlPointsV ) * stride ];
                        UVQ = new double[ np.controlPointsU + np.orderU + r ];
                        NurbsLibCore.InsertKnotSurfaceU ( stride, np.controlPointsU, np.controlPointsV, np.orderU - 1,
                                                          np.knotVectorU, NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights ), uv, k, s, r, UVQ, Qw );
                    }
                    else
                        Qw = NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights );

                    if ( r > 0 )
                        a = k - ( np.orderU - 1 ) + ( np.orderU - 1 - s + r - 1 ) / 2 + 1; else

                        a = k - ( np.orderU - 1 );

                    a *= np.controlPointsV * stride;
                    System.Array.Copy ( Qw, a, cv, 0, np.controlPointsV * stride );
                    System.Array.Copy ( np.knotVectorV, 0, nc.knotVector, 0, nc.knotVector.Length );
                    break;

                case 7: /* middle u */
                    break;

                case 8: /* middle v */
                    break;
            }

            NurbsLibCore.Array4DToPointsAndWeights ( nc, cv );
            return nc;
        }


        /*
            Extrude a NURBS curve in direction along a vector to create a patch, curve is treated as V, vector as U

            @method PatchExtrudeCurveVectorV
            @param curveV				the curve to extrude
            @param uVec					the vector for u direction
            @param order				the order of the patch along u direction
            @param numControlPointsU  	the number of control points to create in U direction
            @return NurbsPatch
        */

        public static NurbsPatch PatchExtrudeCurveVectorV ( NurbsCurve curveV, mmVector3 uVec, int order, int numControlPointsU )
        {
            NurbsPatch np = new NurbsPatch ();
            int numControlPointsV = curveV.numControlPoints;
            int stride = 4;
            np.controlPointsU = numControlPointsU;
            np.controlPointsV = numControlPointsV;
            np.orderU = order;
            np.orderV = curveV.order;
            np.knotVectorU = NurbsLibCore.GenerateKnotVector ( order, numControlPointsU );
            np.knotVectorV = new double[ curveV.knotVector.Length ];
            System.Array.Copy ( curveV.knotVector, 0, np.knotVectorV, 0, curveV.knotVector.Length );
            double[] controlPoints = new double[ numControlPointsU * numControlPointsV * stride ];
            double uinc = 1.0f / ( double ) ( numControlPointsU - 1 );
            int idx2 = 0;
            double[] cp = NurbsLibCore.PointsAndWeightsToArray4D ( curveV.points, curveV.pointWeights );

            for ( int u = 0; u < numControlPointsU; u++ ) {
                //make a row
                System.Array.Copy ( cp, 0, controlPoints, idx2, numControlPointsV * stride );

                //extrude row (but not the first one)
                for ( int c = idx2; c < idx2 + numControlPointsV * stride; c += stride ) {
                    controlPoints[ c ] += u * uinc * uVec.x;
                    controlPoints[ c + 1 ] += u * uinc * uVec.y;
                    controlPoints[ c + 2 ] += u * uinc * uVec.z;
                }

                idx2 += numControlPointsV * stride;
            }

            NurbsLibCore.Array4DToPointsAndWeights ( np, controlPoints );
            return np;
        }

        /*
            Swap the U and V directions of the patch without changing its shape

            @method 			PatchSwapUV
            @param np		the patch to preform the swap operation on
        */

        public static void PatchSwapUV ( ref NurbsPatch np )
        {
            double[] controlPoints = NurbsLibCore.SwapArray ( NurbsLibCore.PointsAndWeightsToArray4D ( np.points, np.pointWeights ), np.stride, np.controlPointsU, np.controlPointsV );
            NurbsLibCore.Array4DToPointsAndWeights ( np, controlPoints );
            int swapTemp = np.controlPointsV;
            np.controlPointsV = np.controlPointsU;
            np.controlPointsU = swapTemp;
            swapTemp = np.orderV;
            np.orderV = np.orderU;
            np.orderU = swapTemp;
            //swap knots
            NurbsLibCore.RefSwapDoubleArrays ( ref np.knotVectorU, ref np.knotVectorV );
        }

        /*
            Get isoparm curves from the knot vectors of a patch, multiplicites will return multiple identical curves

            @method 			PatchGetIsoparmCurvesFromKnots
            @param np		the patch to get the isoparms from
            @return 			NurbsCurves[]
        */
        public static NurbsCurve[] PatchGetIsoparmCurvesFromKnots ( NurbsPatch np, Color isoparmUColor, Color isoparmVColor )
        {
            return PatchGetIsoparmCurves ( np, np.knotVectorV, np.knotVectorU, isoparmUColor, isoparmVColor );
        }

        /*
            Get isoparm curves across a regular grid on a NURBS patch

            @method 			PatchGetIsoparmCurvesGrid
            @param np		the patch to get the isoparms from
            @param segmentsU Number of segments of grid in U direction
            @param segmentsV Number of segments of grid in V direction
            @return 			NurbsCurves[]
        */
        public static NurbsCurve[] PatchGetIsoparmCurvesGrid ( NurbsPatch np, int segmentsU, int segmentsV, Color wireframeColor, Color edgeColor )
        {
            double[] us = new double[ segmentsU + 1 ];
            double[] vs = new double[ segmentsV + 1 ];
            double uinc = 1 / ( double ) segmentsU;
            double vinc = 1 / ( double ) segmentsV;

            for ( var i = 0; i <= segmentsU; i++ )
                us[ i ] = i * uinc;

            for ( var j = 0; j <= segmentsV; j++ )
                vs[ j ] = j * vinc;

			return PatchGetIsoparmCurves ( np, us, vs, wireframeColor, edgeColor );
        }

        /*
            Get isoparm curves at specifed u and v values

            @method 			PatchGetIsoparmCurves
            @param np		the patch to get the isoparms from
            @param us 		the u parameters
            @param vs		the v parameters
            @return 			NurbsCurves[]
        */
        public static NurbsCurve[] PatchGetIsoparmCurves ( NurbsPatch np, double[] us, double[] vs, Color wireframeColor, Color edgeColor )
        {
            var curves = new NurbsCurve[ vs.Length + us.Length ];
            int idx = 0;

            for ( int i = 0; i < us.Length; i++ ) {
                curves[ idx ] = NurbsLibPatch.PatchExtractCurve ( np, ( int ) ExtractOption.uParam, us[ i ], false );
				curves[ idx ].color = wireframeColor;
				if (i == 0 || i+1 == us.Length) 
					curves[ idx ].color = edgeColor;
				idx++;
            }

            for ( int i = 0; i < vs.Length; i++ ) {
				curves[ idx ] = NurbsLibPatch.PatchExtractCurve ( np, ( int ) ExtractOption.vParam, vs[ i ], false );
				curves[ idx ].color = wireframeColor;
				if (i == 0 || i+1 == us.Length) 
					curves[ idx ].color = edgeColor;
                idx++;
            }

            return curves;
        }

        public static void PatchSplitU ( NurbsPatch src, double u, ref NurbsPatch result )
        {
            NurbsPatch patch, np1, np2;
            double[] knots;
            int stride = 4, k = 0, r = 0, s = 0, np1len = 0;
            patch = src;
            knots = patch.knotVectorU;
            int[] fsm2 = NurbsLibCore.FindSpanMult ( patch.controlPointsU - 1, patch.orderU - 1, u, knots );
            k = fsm2[ 0 ];
            s = fsm2[ 1 ];
            r = patch.orderU - 1 - s;
            patch.InsertKnotU ( u, r );
            // create two new patches
            np1 = patch;
            np2 = patch.Clone () as NurbsPatch;

            if ( r != 0 )
                np1len = k - ( np1.orderU - 1 ) + 1 + ( patch.orderU - 1 - s + r - 1 ) / 2 + 1;
            else
                np1len = k - ( np1.orderU - 1 ) + 1;

            np2.controlPointsU = ( np1.controlPointsU + 1 ) - np1len;
            np1.controlPointsU = np1len;
            //PATCH 1
            double[] np1newcontrolv = new double[ np1.controlPointsU * np1.controlPointsV * stride ];
            //Create knot vector 1
            double[] np1newknotv = new double[ np1.controlPointsU + np1.orderU ];
            System.Array.Copy ( np1.knotVectorU, 0, np1newknotv, 0, np1.controlPointsU + np1.orderU );
            np1newknotv[ np1.controlPointsU + np1.orderU - 1 ] = np1newknotv[ np1.controlPointsU + np1.orderU - 2 ];
            //Create control points 1
            System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( np1.points, np1.pointWeights ), 0, np1newcontrolv, 0, np1.controlPointsU * np1.controlPointsV * stride );
            //PATCH 2
            double[] np2newcontrolv = new double[ np2.controlPointsU * np2.controlPointsV * stride ];
            //Create knot vector 2
            double[] np2newknotv = new double[ np2.controlPointsU + np2.orderU ];
            System.Array.Copy ( patch.knotVectorU, patch.controlPointsU - 1, np2newknotv, 0, np2.controlPointsU + np2.orderU );
            System.Array.Copy ( NurbsLibCore.PointsAndWeightsToArray4D ( np2.points, np2.pointWeights ), ( np1.controlPointsU - 1 ) * np2.controlPointsV * stride, np2newcontrolv, 0, np2.controlPointsU * np2.controlPointsV * stride );
            np2newknotv[ 0 ] = np2newknotv[ 1 ];
            NurbsLibCore.Array4DToPointsAndWeights ( np1, np1newcontrolv );
            np1.knotVectorU = np1newknotv;
            NurbsLibCore.Array4DToPointsAndWeights ( np2, np2newcontrolv );
            np2.knotVectorU = np2newknotv;
            NurbsLibCore.NormaliseKnotVector ( np1.knotVectorU );
            NurbsLibCore.NormaliseKnotVector ( np2.knotVectorU );
            src = np1;
            result = np2;
        }

        public static void PatchSplitV ( NurbsPatch src, double v, ref NurbsPatch result )
        {
            PatchSwapUV ( ref src );
            PatchSplitU ( src, v, ref result );
            PatchSwapUV ( ref src );
            PatchSwapUV ( ref result );
        }

        public static void PatchRefineU ( NurbsPatch patch, double[] newknotv, int newknotvlen )
        {
            double[] X, Ubar, Qw, knotv;
            int count = 0, i, j;
            knotv = patch.knotVectorU;

            if ( newknotv != null ) {
                if ( newknotvlen == 0 )
                    Debug.Log( "Refine patch: new knot V len must be > 0" );

                X = newknotv;
            }
            else
                X = null;

            if ( X == null ) {
                count = 0;

                for ( i = patch.orderU - 1; i < patch.controlPointsU; i++ ) {
                    if ( knotv[ i ] != knotv[ i + 1 ] )
                        count++;
                }

                X = new double[ count ];
                j = 0;

                for ( i = patch.orderU - 1; i < patch.controlPointsU; i++ ) {
                    if ( knotv[ i ] != knotv[ i + 1 ] ) {
                        X[ j ] = knotv[ i ] + ( ( knotv[ i + 1 ] - knotv[ i ] ) / 2.0 );
                        j++;
                    }
                }
            }
            else
                count = newknotvlen;

            Ubar = new double[ patch.controlPointsU + patch.orderU + count ];
            Qw = new double[ ( patch.controlPointsU + count ) * patch.controlPointsV * 4 ];
            RefineKnotVectorSurfaceU ( patch.controlPointsU - 1, patch.controlPointsV - 1, patch.orderU - 1, patch.knotVectorU, NurbsLibCore.PointsAndWeightsToArray4D ( patch.points, patch.pointWeights ), X, count - 1, Ubar, Qw );
            patch.knotVectorU = Ubar;
            NurbsLibCore.Array4DToPointsAndWeights ( patch, Qw );
            patch.controlPointsU += count;
        }

        public static void RefineKnotVectorSurfaceU ( int w, int h, int p, double[] U,
                double[] Pw, double[] X, int r,
                double[] Ubar, double[] Qw )
        {
            double alpha;
            int m, n, a, b, i, j, k, l, ind, col;
            int i1, i2;
            int stride = 4;
            /* convert rational coordinates from euclidean to homogeneous style */
            a = 0;

            for ( i = 0; i < ( w + 1 ) * ( h + 1 ); i++ ) {
                Pw[ a ] *= Pw[ a + 3 ];
                Pw[ a + 1 ] *= Pw[ a + 3 ];
                Pw[ a + 2 ] *= Pw[ a + 3 ];
                a += stride;  //hardcoded "stride" as 4
            }

            m = w + p + 1;
            n = w;
            h++;
            a = NurbsLibCore.FindSpan ( n, p, X[ 0 ], U );
            b = NurbsLibCore.FindSpan ( n, p, X[ r ], U );
            b++;

            for ( col = 0; col < h; col++ ) {
                for ( j = 0; j <= a - p; j++ ) {
                    /*nS.P(j,col) = P(j,col);*/
                    i1 = ( j * h + col ) * stride;
                    //memcpy(&(Qw[ i1 ]), &(Pw[ i1 ]), stride*sizeof(double));
                    Array.Copy ( Pw, i1, Qw, i1, stride );
                }

                for ( j = b - 1; j <= n; j++ ) {
                    /*nS.P(j+r+1,col) = P(j,col);*/
                    i1 = ( ( j + r + 1 ) * h + col ) * stride;
                    i2 = ( j * h + col ) * stride;
                    //memcpy(&(Qw[ i1 ]), &(Pw[ i2 ]), stride*sizeof(double));
                    Array.Copy ( Pw, i2, Qw, i1, stride );
                }
            } /* for */

            for ( j = 0; j <= a; j++ )
                Ubar[ j ] = U[ j ];

            for ( j = b + p; j <= m; j++ )
                Ubar[ j + r + 1 ] = U[ j ];

            i = b + p - 1;
            k = b + p + r;

            for ( j = r; j >= 0; j-- ) {
                while ( ( X[ j ] <= U[ i ] ) && ( i > a ) ) {
                    for ( col = 0; col < h; col++ ) {
                        /*nS.P(k-p-1,col) = P(i-p-1,col);*/
                        i1 = ( ( k - p - 1 ) * h + col ) * stride;
                        i2 = ( ( i - p - 1 ) * h + col ) * stride;
                        //memcpy(&(Qw[ i1 ]), &(Pw[ i2 ]), stride*sizeof(double));
                        Array.Copy ( Pw, i2, Qw, i1, stride );
                    }

                    Ubar[ k ] = U[ i ];
                    k--;
                    i--;
                } /* while */

                for ( col = 0; col < h; col++ ) {
                    /*nS.P(k-p-1,col) = nS.P(k-p,col);*/
                    i1 = ( ( k - p - 1 ) * h + col ) * stride;
                    i2 = ( ( k - p ) * h + col ) * stride;
                    //memcpy(&(Qw[ i1 ]), &(Qw[ i2 ]), stride*sizeof(double));
                    Array.Copy ( Qw, i2, Qw, i1, stride );
                }

                for ( l = 1; l <= p; l++ ) {
                    ind = k - p + l;
                    alpha = Ubar[ k + l ] - X[ j ];

                    if ( alpha == 0.0 ) {
                        for ( col = 0; col < h; col++ ) {
                            /*nS.P(ind-1,col) = nS.P(ind,col);*/
                            i1 = ( ( ind - 1 ) * h + col ) * stride;
                            i2 = ( ind * h + col ) * stride;
                            //memcpy(&(Qw[ i1 ]), &(Qw[ i2 ]), stride*sizeof(double));
                            Array.Copy ( Qw, i2, Qw, i1, stride );
                        }
                    }
                    else
                        alpha /= Ubar[ k + l ] - U[ i - p + l ]; /* if */

                    for ( col = 0; col < h; col++ ) {
                        /*  nS.P(ind-1,col) = alpha*nS.P(ind-1,col) +
                            (1.0-alpha)*nS.P(ind,col);*/
                        i1 = ( ( ind - 1 ) * h + col ) * stride;
                        i2 = ( ind * h + col ) * stride;
                        Qw[ i1 ] = alpha * Qw[ i1 ] + ( 1.0 - alpha ) * Qw[ i2 ];
                        Qw[ i1 + 1 ] = alpha * Qw[ i1 + 1 ] + ( 1.0 - alpha ) * Qw[ i2 + 1 ];
                        Qw[ i1 + 2 ] = alpha * Qw[ i1 + 2 ] + ( 1.0 - alpha ) * Qw[ i2 + 2 ];

                        if ( stride > 3 )
                            Qw[ i1 + 3 ] = alpha * Qw[ i1 + 3 ] + ( 1.0 - alpha ) * Qw[ i2 + 3 ];
                    } /* for */
                } /* for */

                Ubar[ k ] = X[ j ];
                k--;
            } /* for */

            /* convert rational coordinates from homogeneous to euclidean style */
            a = 0;

            for ( i = 0; i < ( w + r + 1 ) *h; i++ ) {
                Qw[ a ] /= Qw[ a + 3 ];
                Qw[ a + 1 ] /= Qw[ a + 3 ];
                Qw[ a + 2 ] /= Qw[ a + 3 ];
                a += stride;
            }
        }




    }


}