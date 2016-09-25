using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Ayam.Nurbs;
//using UnityNURBS.Operators;
//using UnityNURBS.Views;
using UnityNURBS.Types;
using UnityNURBS.Util;
//using UnityNURBS.Render;
//using UnityNURBS.Sessions;


namespace UnityNURBS.Primitives
{
    public class NurbsCurve : Primitive, IPrimitive, ICloneable
    {
        // parameters
        public double[] knotVector;

		public int numControlPoints
        {
            get { return points.Count; }

			// FIXME really shouldn't be used without being in sync with points
//			set
//			{
//				if ( points.Count < value ) {
//					Logging.Error ( "NurbsCurve.numControlPoints set to a value > points.Count" );
//					int count = points.Count;
//					for ( int i = 0; i < value - count; i++ )
//						points.Add ( mmVector3.zero );
//				} else if ( points.Count > value ) {
//					Logging.Error ( "NurbsCurve.numControlPoints set to a value < points.Count" );
//					points = points.GetRange( 0, value );
//				}
//			}
        }

        public int order;

        // for drawing curve
        public Polyline polyline = new Polyline ();

        // for drawing mesh
        public Mesh mesh;
        public Material curveMaterial;

        // for drawing hull
        public Polyline hull = new Polyline ();
        public bool showTesselation;
        private bool dirtyHulls = false;
        private bool materialsInitialised = false;

        private void Init ()
        {
            //color = settings.curveColor;
        }

        void InitMaterials ()
        {
            if ( materialsInitialised )
                return;

            //curveMaterial = new Material ( settings.nurbsCurveMaterial );
            materialsInitialised = true;
        }
        public NurbsCurve ()
        {
            Init ();
        }

		public NurbsCurve ( double[] _knotVector, List<mmVector3> _controlPoints, List<double> _weights, int _order )
		{
			Init ();
			knotVector = _knotVector;
			points = _controlPoints;
			pointWeights = _weights;
			order = _order;
		}

        public NurbsCurve ( double[] _knotVector, mmVector3[] _controlPoints, List<double> _weights, int _order )
        {
            Init ();
            knotVector = _knotVector;
			points = new List<mmVector3> ( _controlPoints );
            pointWeights = _weights;
            order = _order;
        }

        public object Clone ()
        {
            var clone = new NurbsCurve ();
            clone.order = order;
            clone.knotVector = ( double[] ) knotVector.Clone ();

			foreach ( mmVector3 point in points )
				clone.points.Add ( new mmVector3 ( point ) );

            //CloneAttributes ( clone );
            return clone;
        }

        /*

        void InitMesh ()
        {
            if ( mesh != null )
                return;

            mesh = new Mesh ();

            if ( op != null )
                mesh.name = op.name + " NurbsCurve";
        }

        public bool isIsoParm
        {
            get { return this.op == null; }
        }

            */

        /*

        public bool drawHulls
        {
            get {
                if ( this.op != null )
                    return this.op.drawHulls;

                return false;
            }
        }

            */

        // this is to copy another primitive of the same type,
        // but without the parameters inherited from the Primitive parent class
        // and without the tesselation structures
        // typically to duplicate an input primitive for operator output

            /*

        public void CloneTo ( Operator targetOperator )
        {
        
            var clone = this.Clone () as NurbsCurve;
            clone.op = targetOperator;
            targetOperator.inputGeometry.Add ( clone );
            
        }

        */

    

        // stuff to do when this primitive needs to be rendered

            
        public void Draw ()
        {

            /*
            if ( op == null )
                return;

            if ( !op.selected )
                return;

            bool rigSelected = false;

            // is primitive on same layer as a selected cam?
            foreach ( Rig rig in viewManager.selectedRigs )
                if ( rig.layer == layer )
                    rigSelected = true;

            // is primitive on same layer as active rig? (this takes care of standard cams which don't need to be selected)
            if ( layer == viewManager.activeRig.layer )
                rigSelected = true;

            if ( !rigSelected )
                return;

            InitMaterials ();

            if ( dirtyTesselation )
                BuildMesh ();

            if ( op.drawHulls ) {
                if ( dirtyHulls ) {
                    hull.segments = points.Count - 1;
                    hull.points = MiscUtil.DoubleVectorListToFloatVectorArray ( points );
                    hull.color = settings.hullColor;
                    dirtyHulls = false;
                }

                RenderNurbsCurves.curves.Add ( this );
            }

            curveMaterial.color = color;
            Graphics.DrawMesh ( mesh, Matrix4x4.identity, curveMaterial, layer, null, 0, null );

            if ( settings.showTesselation && op == op.activeOperator )
                RenderPoints.meshes.Add ( mesh );

            */
        }
        

        // draw for primitive selection helper render
        public void DrawSelectionMask ( Color idColor, Camera cam )
        {
            if ( dirtyTesselation )
                BuildMesh ();

            //InitSelectionMaterials ();
            // FIXME clean up this mess
            Material previousSelectionMaterial = selectionMaterials[ 0 ];
            //selectionMaterials[ 0 ] = new Material ( settings.fatCurveMaterial );
            UnityEngine.Object.Destroy ( previousSelectionMaterial );
            selectionMaterials[ 0 ].color = idColor;
            Graphics.DrawMesh ( mesh, Matrix4x4.identity, selectionMaterials[ 0 ], 20, cam, 0, null );
        }

        // stuff to do when this primitive needs to be rendered

            /*
        public void DrawIsoparm ()
        {
            bool rigSelected = false;

            // is primitive on same layer as a selected cam
            foreach ( Rig rig in viewManager.selectedRigs ) {
                if ( rig.layer == layer )
                    rigSelected = true;
            }

            // is primitive on same layer as active rig? (this takes care of standard cams which don't need to be selected)
            if ( layer == viewManager.activeRig.layer )
                rigSelected = true;

            if ( !rigSelected )
                return;

            InitMaterials ();

            if ( dirtyTesselation )
                BuildMesh ();

            curveMaterial.color = color;
            Graphics.DrawMesh ( mesh, Matrix4x4.identity, curveMaterial, layer, null, 0, null );

            if ( settings.showTesselation && op == op.activeOperator )
                RenderPoints.meshes.Add ( mesh );
		}

            */

        // stuff to do when this primitive is not needed any more
        public void Destroy ()
        {
            if ( curveMaterial != null )
                UnityEngine.Object.Destroy ( curveMaterial );

            if ( mesh != null )
                UnityEngine.Object.Destroy ( mesh );
        }

        /*  Insert knot at u on curve - thereby adding a control point
            u is a normalised parametric value
            does not change shape of curve
            wrapper for ported Ayam knot insertion function
            currently this wrapper does not allow inserting multiplicities
            @method InsertKnot
            @param u		the u value where the knot should be inserted
        */
        public void InsertKnot ( double u )
        {
            //find where to put the knot
            int index = NurbsLibCore.FindPositionInKnotVector ( u, knotVector );
            //space for new knot and controlpoint
            double[] newKnots = new double[ knotVector.Length + 1 ];
            double[] newControls = new double[ ( numControlPoints + 1 ) * 4 ];
            //calculate new knot vector and control points
            NurbsLibCurve.InsertKnotCurve4D ( numControlPoints, order, knotVector, points, pointWeights, u, index, 0, 1, ref newKnots, ref newControls );
            knotVector = newKnots;
            NurbsLibCore.Array4DToPointsAndWeights ( this, newControls );
        }

        public void BuildMesh ()
        {
            //InitMesh ();
            //BuildPolyline ( settings.curveQuality );
            mesh.vertices = polyline.points;
            mesh.SetIndices ( polyline.Indices (), MeshTopology.LineStrip, 0 );
            dirtyHulls = true;
        }

        /**
            Construct a polyline so the curve can be rendered
            quality dictates number of line segments
            does not automatically regenerate if curve parameters are changed
            @method BuildPolyline
            @return polyline
         **/
        public Polyline BuildPolyline ( int _quality )
        {
            //if ( op != null )
            //    Debug.Log ( "(re)doing tesselation of[ " + op.name + " - " + op.cid + " ] --> nurbscurve" );

            polyline.BuildPolylineFromCurve ( this, _quality );
            dirtyTesselation = false;
            return polyline;
        }

        public NurbsPatch ExtrudePatchV ( mmVector3 extrusion, int order, int nrOfSpans )
        {
            return 	NurbsLibPatch.PatchExtrudeCurveVectorV ( this, extrusion, order, nrOfSpans + 1 );
        }

        public mmVector3 GetPoint ( double u )
        {
            if ( pointWeights.Count != points.Count )
                return NurbsLibCurve.CurvePoint3D ( this, u );
            else
                return NurbsLibCurve.CurvePoint4D ( this, u );
        }

        public mmVector3[] GetPoints ( double[] uCoordinates )
        {
            var vertices = new mmVector3[ uCoordinates.Length ];

            if ( pointWeights.Count != points.Count )
                for ( int i = 0; i < uCoordinates.Length; i++ )
                    vertices[ i ] = NurbsLibCurve.CurvePoint3D ( this, uCoordinates[ i ] );
            else
                for ( int i = 0; i < uCoordinates.Length; i++ )
                    vertices[ i ] = NurbsLibCurve.CurvePoint4D ( this, uCoordinates[ i ] );

            return vertices;
        }

        public NurbsCurve Split ( double u, NurbsCurve newCurve )
        {
            NurbsLibCurve.CurveSplit ( this, u, newCurve );
            return newCurve;
        }

        public static NurbsCurve CurveFillGap ( int order, double tangentLength, NurbsCurve curve1, NurbsCurve curve2 )
        {
            return NurbsLibCurve.CurveFillGapC1 ( order, tangentLength, curve1, curve2 );
        }

        public static double[] GenerateKnotVector ( int order, int numControlPoints )
        {
            return NurbsLibCore.GenerateKnotVector ( order, numControlPoints );
        }

        public void InvertKnots ()
        {
            NurbsLibCore.InvertKnots ( knotVector );
        }

        public override string ToString ()
        {
            string str = "Nurbs Curve::: \n";
            str += "numControlPoints : " + numControlPoints + "\n";
            str += "order : " + order + "\n";
            str += "knotVector : " + NurbsLibCore.DoublesToString ( knotVector ) + "\n";
            str += "controlPoints : " + NurbsLibCore.DoubleVectorListToString ( points ) + "\n";
            return str;
        }


    }

}
