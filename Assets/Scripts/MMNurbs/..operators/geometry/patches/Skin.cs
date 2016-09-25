using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;

namespace UnityNURBS.Operators
{

    public class Skin : Operator, IOperator
    {
        /*
            private Bool interpolate = new Bool(true,true,"interpolate","Make skin go through curves (true), or use the curves' control points as the skin's control points (false).");
            private Int orderU = new Int(4,4,2,10,"order in U","The NURBS order of the resulting surface along U (ie, not along the input curves' direction).");
            private Bool keepInputCurves = new Bool(false,false,"keep inputcurves","If false, delete the inputcurves.");
        */
        public bool interpolate;
        public int orderU;
        public int skip;
        public bool batch;
        public int per;
        public Skin() {}

        /*
        		public Skin(int _orderU,bool _interpolate, bool _keepInputCurves) {
            			orderU.val = _orderU;
        			interpolate.val = _interpolate;
        			keepInputCurves.val = _keepInputCurves;

        			Init();

        		}



        		// legacy
        		public Skin(NurbsCurve[] _curves,int _orderU,bool _interpolate,bool _keepInputCurves) {
            			curves = _curves;
        			orderU.val = _orderU;
        			interpolate.val = _interpolate;
        			keepInputCurves.val = _keepInputCurves;

        			Init();

        		}

        		// legacy
        		public Skin(NurbsCurve[] _curves,int _orderU,double[] _uknotv,bool _interpolate) {
            			curves = _curves;
        			orderU.val = _orderU;
        	//		uknotv = _uknotv;
        			interpolate.val = _interpolate;

        			Init();

        		}


        		// stuff to do when this operator is being created
        		public void Init() {


        			type = "skin";
        			name = "skin";

        		}
        */


        public bool Cook()
        {
            var curves = new List<NurbsCurve>();

            // find all NurbsCurves in inputGeometry
            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsCurve ) {
                    var nc = ( NurbsCurve ) primitive;
                    curves.Add ( nc );
                }
                else
                    outputGeometry.Add ( primitive );
            }

            // constraints
            if ( orderU < 2 ) {
                errorMessage = ( "skin orderU needs to be minimum 2 (is currently " + orderU + ")" );
                return false;
            }

            if ( skip < 1 ) {
                errorMessage = ( "smallest meaningful skip is 1 (is currently " + skip + ")" );
                return false;
            }

            if ( batch && per < 2 ) {
                errorMessage = ( "batch per 2 minimum (per is currently " + per + ")" );
                return false;
            }

            if ( curves.Count < orderU ) {
                errorMessage = ( "need at least " + orderU + " curves to create a skin of order " + orderU + " (currently only " + curves.Count + " curves)" );
                return false;
            }

            int _per = ( batch ? per : curves.Count );

            for ( int h = 0; h < skip; h++ ) {
                var curveBundle = new List<NurbsCurve>();

                for ( int i = h; i < curves.Count; i += skip )
                    curveBundle.Add ( curves[ i ] );

                var curveBatch = new List<NurbsCurve>();
                int j = 0;

                while ( j < curveBundle.Count ) {
                    curveBatch.Add ( curveBundle[ j++ ] );

                    if ( ( j % _per ) == 0 || j == curveBundle.Count ) {
                        int numControlPoints = -1;
                        int order = -1;

                        // make sure all curves have the same number of control points and are of the same order
                        foreach ( var curve in curveBatch ) {
                            if ( numControlPoints == -1 ) {
                                numControlPoints = curve.numControlPoints;
                                order = curve.order;
                            }
                            else if ( numControlPoints != curve.numControlPoints ) {
                                errorMessage = ( "input curves need to have same number of points" );
                                return false;
                            }
                            else if ( order != curve.order ) {
                                errorMessage = ( "input curves need to be of same order" );
                                return false;
                            }
                        }
						
						// FIXME: support attribute transfer etc
                        var patch = NurbsPatch.Skin ( curveBatch, orderU );

                        if ( interpolate )
                            NurbsPatch.InterpolateU ( patch, orderU );

                        outputGeometry.Add ( patch );
                        curveBatch = new List<NurbsCurve>();
                    }
                }
            }

            // cleanup
            foreach ( NurbsCurve curve in curves )
                curve.Destroy();

            return true;
        }



    }

}