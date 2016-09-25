using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;

namespace UnityNURBS.Operators
{


    /*
        @class		Line

        @version 	0.1, 27 July 2012

        @desc		line creation operator
    			creates a straigth NURBS curve, of a certain nr of points and order,
    			centered around an origin, and along an indicated direction

        @url 		http://www.madeonjupiter.com

        @todo
    */


    public class Line : Operator, IOperator
    {
        /*
            private Vector center = new Vector(Vector3.zero,Vector3.zero,"center","Center point of the line in world coordinates.");
            private Vector direction = new Vector(Vector3.up,Vector3.up,"direction","Direction of the line");
            private Float length = new Float(5,5,0,5,"length","Length of the line.");
            private Int numPoints = new Int(2,2,2,10,"nr of points","Number of equally spaced points that make up the line.  Needs to be higher or equal to the order.");
            private Int order = new Int(2,2,2,10,"order","The NURBS order of the line. Order 2 is piecewise linear, ie. a polyline.");
        */
        public int mode;   // 0: centered   1: origin+direction   2: point to point
        public mmFlexiVector center;
        public mmFlexiVector origin;
        public mmFlexiVector direction;
        public mmFlexiVector end;
        public mmFlexiFloat length;
        public mmFlexiInteger numPoints;
        public mmFlexiInteger order;

//		private NurbsCurve nurbsLine;
//		private int stride = 4;


        public Line()
        {
            center = new mmFlexiVector();
            origin = new mmFlexiVector();
            direction = new mmFlexiVector();
            end = new mmFlexiVector();
            length = new mmFlexiFloat();
            numPoints = new mmFlexiInteger();
            order = new mmFlexiInteger();
        }


        public bool Cook()
        {
            // some constraints
            if ( order.Min() < 2 ) {
                errorMessage = "the order needs to be minimum 2";
                return false;
            }

            if ( ( numPoints < order ) != false ) {
                errorMessage = "the number of points needs to be equal to or higher than the order";
                return false;
            }

            int count;

            if ( mode == 0 )
                count = ConformLists ( center, direction, length, numPoints, order );
            else if ( mode == 1 )
                count = ConformLists ( origin, direction, length, numPoints, order );
            else
                count = ConformLists ( origin, end, numPoints, order );

            if ( count == 0 ) // one of the lists is empty
                return false;

            // small optimisation for case where order and numPoints doesn't vary
            bool sameTopo = ! ( order.isList || numPoints.isList );
            var knotvector = NurbsCurve.GenerateKnotVector ( order.integer, numPoints.integer );

            for ( int instance = 0; instance < count; instance++ ) {
                var _center = center[ instance ];
                var _origin = origin[ instance ];
                var _direction = direction[ instance ].normalized;
                var _end = end[ instance ];
                var _length = length[ instance ];
                var _numPoints = numPoints[ instance ];
                var _order = order[ instance ];
                var line = new NurbsCurve();
                var controlPoints = new List<mmVector3> ( _numPoints );

                // calculate point positions
                if ( mode == 0 ) {
                    var start = _center - _length / 2 * _direction;
                    var interval = _length / ( _numPoints - 1 );

                    for ( int i = 0; i < _numPoints; i++ )
                        controlPoints.Add( start + i * _direction * interval );
                }
                else if ( mode == 1 ) {
                    var interval = _length / ( _numPoints - 1 );

                    for ( int i = 0; i < _numPoints; i++ )
                        controlPoints.Add( _origin + i * _direction * interval );
                }
                else {
                    var segment = ( _end - _origin ) / ( _numPoints - 1 );

                    for ( int i = 0; i < _numPoints; i++ )
                        controlPoints.Add( _origin + i * segment );
                }

                line.points = controlPoints;
                line.order = _order;

                if ( sameTopo )
                    line.knotVector = knotvector;
                else
                    line.knotVector = NurbsCurve.GenerateKnotVector ( _order, _numPoints );

                outputGeometry.Add ( line );
            }

            return true;
        }


    }

}