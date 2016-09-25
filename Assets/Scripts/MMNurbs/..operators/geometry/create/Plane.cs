using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using Ayam.Nurbs;


namespace UnityNURBS.Operators
{

    public class Plane : Operator, IOperator
    {
        public int mode;  // 0: NurbsPatch, 1: points only, TODO 2: boundary, 3: line grid, 4: PolyMesh
        public mmFlexiInteger axis;  // XZ YZ XY
        public mmFlexiVector center;
        public mmFlexiFloat length;
        public mmFlexiFloat width;
        public mmFlexiInteger columns;
        public mmFlexiInteger rows;
        public mmFlexiInteger orderU;
        public mmFlexiInteger orderV;


        public Plane()
        {
            axis = new mmFlexiInteger();
            center = new mmFlexiVector();
            length = new mmFlexiFloat();
            width = new mmFlexiFloat();
            columns = new mmFlexiInteger();
            rows = new mmFlexiInteger();
            orderU = new mmFlexiInteger();
            orderV = new mmFlexiInteger();
        }


        public bool Cook()
        {
            // some constraints
            if ( orderU.Min() < 2 || orderV.Min() < 2 ) {
                errorMessage = "order needs to be minimum 2";
                return false;
            }

            if ( mode == 0 && ( columns < orderU ) != false ) {
                errorMessage = "the number of columns needs to be equal or higher to the U order";
                return false;
            }

			if ( mode == 0 && ( rows < orderV ) != false ) {
                errorMessage = "the number of rows needs to be equal or higher to the V order";
                return false;
            }

            int count = ConformLists ( axis, center, length, width, columns, rows, orderU, orderV );

            if ( count == 0 ) // one of the lists is empty
                return false;

            for ( int instance = 0; instance < count; instance++ ) {
                var _axis = axis[ instance ];
                var _center = center[ instance ];
                var _length = length[ instance ];
                var _width = width[ instance ];
                var _columns = columns[ instance ];
                var _rows = rows[ instance ];
                var _orderU = orderU[ instance ];
                var _orderV = orderV[ instance ];
                int numPoints = _columns;

                // calculate point positions
                var controlPoints = new List<mmVector3> ( _columns );
                double increment = _width / ( _columns - 1 );

                for ( int i = 0; i < _columns; i++ ) {
                    if ( _axis == 0 )
                        controlPoints.Add ( new mmVector3 ( _center.x - _width / 2 + i * increment, _center.y, _center.z - _length / 2 ) );
                    else if ( _axis == 1 )
                        controlPoints.Add ( new mmVector3 ( _center.x, _center.y - _length / 2, _center.z - _width / 2 + i * increment ) );
                    else if ( _axis == 2 )
						controlPoints.Add ( new mmVector3 ( _center.x - _length / 2, _center.y - _width / 2 + i * increment, _center.z ) );
                }

                var helperCurve = new NurbsCurve();
                helperCurve.order = _orderU;
                helperCurve.points = controlPoints;
                helperCurve.knotVector = NurbsLibCore.GenerateKnotVector ( _orderU, controlPoints.Count );

                //extrude line along vector
                var direction = mmVector3.zero;

                if ( _axis == 0 )
                    direction = new mmVector3 ( 0, 0, _length );
                else if ( _axis == 1 )
                    direction = new mmVector3 ( 0, _length, 0 );
                else if ( _axis == 2 )
                    direction = new mmVector3 ( _length, 0, 0 );

                var np = NurbsPatch.PatchExtrudeCurveVectorV ( helperCurve, direction, _orderV, _rows );

                // FIXME: this is a temp hack
                if ( mode == 0 )
                    outputGeometry.Add ( np );
                else {
                    drawPoints = true;
                    outputGeometry.Add ( new PointsPrimitive ( np.points ) );
                }
            }

            return true;
        }
    }
}