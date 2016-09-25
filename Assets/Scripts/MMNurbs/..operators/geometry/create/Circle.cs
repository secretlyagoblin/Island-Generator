using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;

namespace UnityNURBS.Operators
{

    public class Circle : Operator, IOperator
    {
		public int mode;
		public mmFlexiVector center;
		public mmFlexiInteger axis;  // XZ YZ XY
        public mmFlexiFloat radius;
        public mmFlexiInteger segments;
        public mmFlexiFloat startAngle;
        public mmFlexiFloat endAngle;

        private double degToRad;

        public Circle()
        {
			center = new mmFlexiVector();
			axis = new mmFlexiInteger();
			radius = new mmFlexiFloat();
            segments = new mmFlexiInteger();
            startAngle = new mmFlexiFloat();
            endAngle = new mmFlexiFloat();
            degToRad = Math.PI / 180.0f;
        }


        public bool Cook()
        {
            // some constraints
            if ( segments.Min() < 1 ) {
                errorMessage = "minimum segments is 1";
                return false;
            }

            int count = ConformLists ( center, axis, radius, segments, startAngle, endAngle );

            if ( count == 0 ) // one of the lists is empty
                return false;
			
			for ( int instance = 0; instance < count; instance++ ) {
				
				var _radius = radius[ instance ];
				var _axis = axis[ instance ];
				var _center = center[ instance ];
				var _startAngle = startAngle[ instance ];
				var _endAngle = endAngle[ instance ];
				var _segments = segments[ instance ];

				if ( mode == 0 ) {  // NURBS circle
					
					// TODO arbirary arcs : http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/spline/NURBS/RB-circles.html
					// TODO order 3 : http://www.gamedev.net/page/resources/_/technical/math-and-physics/practical-guide-to-bezier-curves-r3166
					
					
					var circle = new NurbsCurve();
					circle.order = 3;

					double cornerWeight = Math.Sqrt( 2 ) / 2;
					circle.pointWeights = new List<double>( new double[] { 1, cornerWeight, 1, cornerWeight, 1, cornerWeight, 1, cornerWeight, 1 } );
					circle.knotVector = new double[] { 0, 0, 0, .25, .25, .5, .5, .75, .75, 1, 1, 1 };
					
					var controlPoints = new List<mmVector3>();
					if ( _axis == 0 )  // XZ
						controlPoints.AddRange ( new mmVector3[] { 
							_center + new mmVector3 ( radius, 0, 0 ),
							_center + new mmVector3 ( radius, 0, radius ), 
							_center + new mmVector3 ( 0, 0, radius ), 
							_center + new mmVector3 ( -radius, 0, radius ), 
							_center + new mmVector3 ( -radius, 0, 0 ), 
							_center + new mmVector3 ( -radius, 0, -radius ), 
							_center + new mmVector3 ( 0 , 0, -radius ), 
							_center + new mmVector3 ( radius, 0, -radius ), 
							_center + new mmVector3 ( radius, 0, 0 ) } );
					else if ( _axis == 1 )  // YZ
						controlPoints.AddRange ( new mmVector3[] { 
							_center + new mmVector3 ( 0, radius, 0 ),
							_center + new mmVector3 ( 0, radius, radius ), 
							_center + new mmVector3 ( 0, 0, radius ), 
							_center + new mmVector3 ( 0, -radius, radius ), 
							_center + new mmVector3 ( 0, -radius, 0 ), 
							_center + new mmVector3 ( 0, -radius, -radius ), 
							_center + new mmVector3 ( 0, 0 , -radius ), 
							_center + new mmVector3 ( 0, radius, -radius ), 
							_center + new mmVector3 ( 0, radius, 0 ) } );
					else  // XY
						controlPoints.AddRange ( new mmVector3[] { 
							_center + new mmVector3 ( radius, 0, 0 ),
							_center + new mmVector3 ( radius, radius, 0 ), 
							_center + new mmVector3 ( 0, radius, 0 ), 
							_center + new mmVector3 ( -radius, radius, 0 ), 
							_center + new mmVector3 ( -radius, 0, 0 ), 
							_center + new mmVector3 ( -radius, -radius, 0 ), 
							_center + new mmVector3 ( 0 , -radius, 0 ), 
							_center + new mmVector3 ( radius, -radius, 0 ), 
							_center + new mmVector3 ( radius, 0, 0 ) } );

					circle.points = controlPoints;
					
					outputGeometry.Add ( circle );
					
					
				} else if ( mode > 0 ) {  // polyline or polygon

					bool polygons = ( mode == 2 );

	                int numEdgePoints = _segments + 1;

	                // calculate point positions
	                var vertices = new List<mmVector3>();

	                if ( polygons )
	                    vertices.Add ( _center );

					double start, end, angle, angleDelta;
					
					start = _startAngle * degToRad;
					end = _endAngle * degToRad;
					angleDelta = ( end - start ) / _segments;

					for ( int i = 0; i < numEdgePoints; i++ ) {
						angle = start + i * angleDelta;
						if ( _axis == 0 )  // XZ
							vertices.Add ( new mmVector3 ( _center.x + _radius * Math.Cos ( angle ), _center.y, _center.z + _radius * Math.Sin ( angle ) ) );
						else if ( _axis == 1 )  // YZ
							vertices.Add ( new mmVector3 ( _center.x, _center.y + _radius * Math.Cos ( angle ), _center.z + _radius * Math.Sin ( angle ) ) );
						else  // XY
	                        vertices.Add ( new mmVector3 ( _center.x + _radius * Math.Cos ( angle ), _center.y + _radius * Math.Sin ( angle ), _center.z ) );
	                }

					// make sure endpoints are identical when it's a closed circle
					if ( Math.Abs ( _endAngle - _startAngle ) % 360 < Double.Epsilon ) {
						if ( polygons )
							vertices [ numEdgePoints ] = new mmVector3 ( vertices [ 1 ] );
						else
							vertices [ numEdgePoints - 1 ] = new mmVector3 ( vertices [ 0 ] );
					}

	                if ( polygons ) {
	                    var disc = new PolyMesh();
	                    disc.points = vertices;
	                    // create triangles
	                    var triangles = new List<Triangle>();

	                    for ( int i = 0; i < _segments; i++ )
	                        triangles.Add ( new Triangle ( 0, i + 1, i + 2, disc ) );

	                    disc.triangles = triangles;
	                    outputGeometry.Add ( disc );
	                }
	                else {
	                    var circle = new NurbsCurve();
	                    circle.knotVector = NurbsCurve.GenerateKnotVector ( 2, vertices.Count );
	                    circle.points = vertices;
	                    circle.order = 2;
	                    outputGeometry.Add ( circle );
	                }

				}
			}

            return true;
        }




    }

}