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
    public class NurbsPatch : Primitive //, IPrimitive
    {
        // parameters
        public int controlPointsU;
        public int controlPointsV;
        public int orderU;
        public int orderV;
        public double[] knotVectorU;
        public double[] knotVectorV;

        public int stride = 4;

        // for drawing mesh
        public Mesh mesh;
        public Material[] materials;
        public Material[] normalMaterials;
        public Material[] offsetMaterials;
        public Material[] transparentMaterials;
        public Material[] transparentOffsetMaterials;

        //public bool transparency
        //{
        //    get { return material.transparent; }
        //    set { material.transparent = value; }
        //}
        //public string texture
        //{
        //    get { return material.texture; }
        //    set { material.texture = value; }
        //}

        public bool showTesselation;

        // for drawing isoparms
        public int isoDensity;
		NurbsCurve[] isoparms;
		private bool dirtyWire = false;
        bool shadersOffset = false;
        private bool materialsInitialised = false;

        /**
            NurbsPatch Constructor
            Default when creating NurbsPatch primitives
            @method NurbsPatch
         **/
        public NurbsPatch ()
        {
            //InitShader ();
        }

        /**
            NurbsPatch Constructor
            Used only by NurbsLibPatch (no Init)
            @method NurbsPatch
            @param _controlPointsU		Number of control points in U direction
            @param _controlPointsV		Number of control points in V direction
            @param _orderU
            @param _orderV
            @param _knotVectorU
            @param _knotVectorV
            @param _controlPoints		array of doubles represeting xyzw coords of control points
                     **/
		public NurbsPatch ( int _controlPointsU, int _controlPointsV, int _orderU, int _orderV, double[] _knotVectorU, double[] _knotVectorV, List<mmVector3> _controlPoints, List<double> _weights )
		{
			controlPointsU = _controlPointsU;
			controlPointsV = _controlPointsV;
			orderU = _orderU;
			orderV = _orderV;
			knotVectorU = _knotVectorU;
			knotVectorV = _knotVectorV;
			points = _controlPoints;
			pointWeights = _weights;
		}

		public NurbsPatch ( int _controlPointsU, int _controlPointsV, int _orderU, int _orderV, double[] _knotVectorU, double[] _knotVectorV, mmVector3[] _controlPoints, List<double> _weights )
        {
            controlPointsU = _controlPointsU;
            controlPointsV = _controlPointsV;
            orderU = _orderU;
            orderV = _orderV;
            knotVectorU = _knotVectorU;
            knotVectorV = _knotVectorV;
            points = new List<mmVector3> ( _controlPoints );
            pointWeights = _weights;
        }

        /*

        void InitMesh ()
        {
            if ( mesh != null )
                return;

            mesh = new Mesh ();

            if ( op != null )
                mesh.name = op.name + " NurbsPatch";

            isoparms = new NurbsCurve[ 0 ];
            isoDensity = settings.isoparmDensity;
        }

        void InitShader ()
        {
            color = settings.surfaceColor;
        }

        void InitMaterials ()
        {
            if ( !materialsInitialised ) {
                normalMaterials = new Material[] {
                    new Material ( settings.patchFrontMaterial ),
                    new Material ( settings.patchBackMaterial )
                };
                offsetMaterials = new Material[] {
                    new Material ( settings.patchFrontOffsetMaterial ),
                    new Material ( settings.patchBackOffsetMaterial )
                };
                transparentMaterials = new Material[] {
                    new Material ( settings.transparentMaterial ),
                    new Material ( settings.patchBackMaterial )
                };
                transparentOffsetMaterials = new Material[] {
                    new Material ( settings.transparentOffsetMaterial ),
                    new Material ( settings.patchBackMaterial )
                };
                materialsInitialised = true;
            }

            if ( shadersOffset ) {
                if ( transparency )
                    materials = transparentOffsetMaterials;
                else
                    materials = offsetMaterials;
            }
            else {
                if ( transparency )
                    materials = transparentMaterials;
                else
                    materials = normalMaterials;
            }

            materials[ 0 ].color = color;
			materials[ 1 ].color = color;
            materials[ 0 ].SetColor ( "_EmissionColor", material.emissionColor );
            materials[ 1 ].SetColor ( "_EmissionColor", material.emissionColor );
            materials[ 0 ].SetColor ( "_SpecColor", material.specularColor );
//			materials[ 1 ].SetColor ("_SpecColor", curveMaterial.specularColor);
            materials[ 0 ].SetFloat ( "_Shininess", material.shininess );
//			materials[ 1 ].SetFloat ("_Shininess", curveMaterial.shininess);
            materials[ 0 ].SetColor ( "_ReflectColor", material.reflectionColor );
//			materials[ 1 ].SetColor ("_ReflectColor", curveMaterial.reflectionColor);
            //materials[ 0 ].SetTexture ( "_Cube", SceneManager.cubeMaps[ material.cubeMap ] );
//			materials[ 1 ].SetTexture ("_Cube", SceneManager.cubeMaps[ curveMaterial.cubeMap ]);
            materials[ 0 ].SetFloat ( "_HdrPower", material.hdrPower );
//			materials[ 1 ].SetFloat ("_HdrPower", curveMaterial.hdrPower);

            if ( texture != "" ) {
                //materials[ 0 ].SetTexture ( "_MainTex", SceneManager.textureCache[ texture ] );
                //materials[ 1 ].SetTexture ( "_MainTex", SceneManager.textureCache[ texture ] );
            }
        }

            */

        public object Clone ()
        {
            var clone = new NurbsPatch ();
            clone.isoDensity = isoDensity;
            clone.controlPointsU = controlPointsU;
            clone.controlPointsV = controlPointsV;
            clone.orderU = orderU;
            clone.orderV = orderV;
			
			foreach ( mmVector3 point in points )
				clone.points.Add ( new mmVector3 ( point ) );

            if ( knotVectorU != null )
                clone.knotVectorU = ( double[] ) knotVectorU.Clone ();

            if ( knotVectorV != null )
                clone.knotVectorV = ( double[] ) knotVectorV.Clone ();

            //CloneAttributes ( clone );
            return clone;
        }

            /*


        // this is to copy another primitive of the same type,
        // but without the parameters inherited from the Primitive parent class
        // and without the tesselation structures
        // typically to duplicate an input primitive for operator output

            /*
        public void CloneTo ( Operator targetOperator )
        {
            var clone = this.Clone () as NurbsPatch;
            clone.op = targetOperator;
            targetOperator.inputGeometry.Add ( clone );
        }
        */

        public void CopyNurbsPropertiesTo ( NurbsPatch np )
        {
            np.controlPointsU = controlPointsU;
            np.controlPointsV = controlPointsV;
            np.orderU = orderU;
            np.orderV = orderV;
            np.knotVectorU = knotVectorU;
            np.knotVectorV = knotVectorV;
            np.points = points;
            np.pointWeights = pointWeights;
            np.stride = stride;
        }

        // stuff to do when this primitive needs to be rendered

            /*
        public void Draw ()
        {
            if ( op == null )
                return;

            if ( !op.selected )
                return;

            bool rigSelected = false;

            // is primitive on same layer as a selected cam
            foreach ( Rig rig in viewManager.selectedRigs )
                if ( rig.layer == layer )
                    rigSelected = true;

            // is primitive on same layer as active rig (this takes care of standard cams which don't need to be selected)
            if ( layer == viewManager.activeRig.layer )
                rigSelected = true;

            if ( !rigSelected )
                return;

            if ( dirtyTesselation ) {
                Logging.Info ( "(re)doing tesselation of[ " + op.name + " - " + op.cid + " ] --> nurbspatch" );
                BuildMesh ( settings.patchQuality );
                dirtyWire = true;
            }

            if ( op.drawWire && !shadersOffset ) {
                if ( dirtyWire ) {
                    foreach ( NurbsCurve isoparm in isoparms )
                        isoparm.Destroy ();

                    // get the isoparm curves and attach them to the curve renderer
                    isoparms = NurbsLibPatch.PatchGetIsoparmCurvesGrid ( this, isoDensity, isoDensity, material.wireframeColor, material.wireframeEdgeColor );

                    foreach ( NurbsCurve isoparm in isoparms ) {
                        isoparm.op = op;
                        isoparm.layer = layer;
                        isoparm.dirtyTesselation = true;
                    }

                    dirtyWire = false;
                }

                // use depth offset shaders to avoid intersection with isoparms
                shadersOffset = true;
            }
            else if ( !op.drawWire && shadersOffset )
                shadersOffset = false;

            if ( op.drawWire )
                foreach ( NurbsCurve isoparm in isoparms )
                    isoparm.DrawIsoparm ();

            if ( op.drawHulls ) {
                // tell the hull renderer which patch to render the hull from
                RenderNurbsPatchHulls.patches.Add ( this );
            }

            InitMaterials ();

            Graphics.DrawMesh ( mesh, Matrix4x4.identity, materials[ 0 ], layer, null, 0, null, !transparency, true );
            if ( !transparency && material.doubleSided )
                Graphics.DrawMesh ( mesh, Matrix4x4.identity, materials[ 1 ], layer, null, 0, null, !transparency, true );

            if ( op == op.activeOperator ) {
                if ( settings.showTesselationNormals )
                    RenderVectors.meshes.Add ( mesh );
                else if ( settings.showTesselation )
                    RenderPoints.meshes.Add ( mesh );
            }
        }
        */

        // draw for primitive selection helper render

            /*
        public void DrawSelectionMask ( Color idColor, Camera cam )
        {
            if ( dirtyTesselation )
                BuildMesh ( settings.patchQuality );

            InitSelectionMaterials ();
            selectionMaterials[ 0 ].color = idColor;
            Graphics.DrawMesh ( mesh, Matrix4x4.identity, selectionMaterials[ 0 ], 20, cam, 0, null );
            //FIXME need to make the back shader work
            //selectionMaterials[ 1 ].color = idColor;
            //Graphics.DrawMesh(mesh, Matrix4x4.identity, selectionMaterials[ 1 ], 20, cam, 0, null);
        }

        // draw for thumbnail render
        public void DrawThumbnail ( Camera cam )
        {
            if ( dirtyTesselation )
                BuildMesh ( settings.patchQuality );

            InitMaterials ();
            Graphics.DrawMesh ( mesh, Matrix4x4.identity, materials[ 0 ], 21, cam, 0, null, !transparency, true );

            if ( !transparency && material.doubleSided )
                Graphics.DrawMesh ( mesh, Matrix4x4.identity, materials[ 1 ], 21, cam, 0, null, !transparency, true );
        }

            */


        // stuff to do when this primitive is not needed any more
        public void Destroy ()
        {
            if ( normalMaterials != null )
                foreach ( Material material in normalMaterials )
                    UnityEngine.Object.Destroy ( material );

            if ( offsetMaterials != null )
                foreach ( Material material in offsetMaterials )
                    UnityEngine.Object.Destroy ( material );

            if ( transparentMaterials != null )
                foreach ( Material material in transparentMaterials )
                    UnityEngine.Object.Destroy ( material );

            if ( transparentOffsetMaterials != null )
                foreach ( Material material in transparentOffsetMaterials )
                    UnityEngine.Object.Destroy ( material );

            if ( mesh != null )
                UnityEngine.Object.Destroy ( mesh );

            if ( isoparms != null )
                if ( isoparms.Length > 0 )
                    foreach ( NurbsCurve isoparm in isoparms )
                        isoparm.Destroy ();
        }

        /**
            Construct a mesh so the patch can be rendered
            quality dictates the coarseness of tesselation
            does not automatically regenerate if patch parameters are changed
            @method BuildPolyline
            @return mesh
         **/

            /*
        public void BuildMesh ( int quality )
        {
            BuildMesh ( quality, quality, false );
        }

        public void BuildMesh ( int quality, bool doubleSided )
        {
            BuildMesh ( quality, quality, doubleSided );
        }

        public void BuildMesh ( int qualityU, int qualityV )
        {
            BuildMesh ( qualityU, qualityV, false );
        }

            */

        /*

        

        public void BuildMesh ( int qualityU, int qualityV, bool doubleSided )
        {
            InitMesh ();
            Vector3[ , ] patch;
            Vector3[] vertices;
            Vector2[] uvs = new Vector2[ 0 ];
            int[] triangles;
            int numColumns, numRows;
            int trindex = 0;

            if ( orderU > 2 && orderV > 2 ) {
                patch = NurbsLibCore.SurfacePoints4D ( controlPointsU, controlPointsV,
                                                       orderU, orderV, knotVectorU, knotVectorV,
                                                       NurbsLibCore.PointsAndWeightsToArray4D ( points, pointWeights ),
                                                       qualityU, qualityV );
                numColumns = patch.GetLength ( 0 );
                numRows = patch.GetLength ( 1 );
                vertices = new Vector3[ numColumns * numRows ];

                for ( int column = 0; column < numColumns; column++ )
                    for ( int row = 0; row < numRows; row++ )
                        vertices[ column * numRows + row ] = patch[ column, row ];

                if ( settings.calculateNurbsUVs ) {
                    uvs = new Vector2[ numRows * numColumns ];

					float rowIncrement = 1.0f / (float) ( numRows - 1 );
					float columnIncrement = 1.0f / (float) ( numColumns - 1 );
					for ( int column = 0; column < numColumns; column++ ) {
						float columnUV = (float) column * columnIncrement ;
						for ( int row = 0; row < numRows; row++ ) {
							// FIXME: is not correct
							uvs[ column * numRows + row ] = new Vector2 ( columnUV, (float) row * rowIncrement );
						}
					}

                }

                triangles = new int[ ( numRows - 1 ) * ( numColumns - 1 ) * 3 * 2 ];

                for ( int column = 0; column < numColumns - 1; column++ )
                    TriStrip ( ref triangles, ref trindex, patch, column, 0, numRows - 1 );
            }
            else if ( ( orderU == 2 && orderV > 2 ) || ( orderU > 2 && orderV == 2 ) ) {
                Polyline[] poly;

                if ( orderU == 2 ) {
                    poly = new Polyline[ controlPointsU ];

                    for ( var i = 0; i < controlPointsU; i++ ) {
                        var curve = new NurbsCurve ( knotVectorV, NurbsPatch.GetColUPoints ( this, i ), NurbsPatch.GetColUWeights ( this, i ), orderV );
                        poly[ i ] = curve.BuildPolyline ( qualityV );
                    }
                }
                else {    // orderV == 2
                    poly = new Polyline[ controlPointsV ];

                    for ( int i = 0; i < controlPointsV; i++ ) {
                        var curve = new NurbsCurve ( knotVectorU, NurbsPatch.GetRowVPoints ( this, controlPointsV - 1 - i ), NurbsPatch.GetRowVWeights ( this, controlPointsV - 1 - i ), orderU ); // reverse to TEMPORARILY fix inverted normals for orderU = 2  (FIXME)
                        poly[ i ] = curve.BuildPolyline ( qualityU );
                    }
                }

                // swapping U & V for orderV == 2 case - easier
                // FIXME : don't do the swapping: it changes the layout of point numbers when U goes from order 3 to order 2
                numColumns = ( poly.Length - 2 ) * 2 + 2;
                numRows = poly[ 0 ].points.Length;
                patch = new Vector3[ numColumns, numRows ];
                int[] indexMap = new int[ numColumns ];

                for ( int u = 0; u < numColumns; u++ )
                    indexMap[ u ] = u % 2 == 0 ? u / 2 : ( u + 1 ) / 2;

                for ( int u = 0; u < numColumns; u++ )
                    for ( int v = 0; v < numRows; v++ )
                        patch[ u, v ] = poly[ indexMap[ u ] ].points[ v ];

                vertices = new Vector3[ numColumns * numRows ];

                for ( int column = 0; column < numColumns; column++ )
                    for ( int row = 0; row < numRows; row++ )
                        vertices[ column * numRows + row ] = patch[ column, row ];

                if ( settings.calculateNurbsUVs ) {
                    uvs = new Vector2[ numRows * numColumns ];
					
					float rowIncrement = 1.0f / (float) ( numRows - 1 );
					float columnIncrement = 1.0f / (float) ( poly.Length - 1 );
                    for ( int column = 0; column < numColumns; column++ ) {
						float columnUV = (float) indexMap[ column ] * columnIncrement ;
						for ( int row = 0; row < numRows; row++ ) {
							// FIXME: is not correct
							uvs[ column * numRows + row ] = new Vector2 ( columnUV, (float) row * rowIncrement );
						}
					}
                }

                triangles = new int[ ( numRows - 1 ) * ( poly.Length - 1 ) * 3 * 2 ];

                for ( int column = 0; column < numColumns - 1; column += 2 )
                    TriStrip ( ref triangles, ref trindex, patch, column, 0, numRows - 1 );
            }
            else {   // u & v = order 2
                numColumns = ( controlPointsU - 2 ) * 2 + 2;
                numRows = ( controlPointsV - 2 ) * 2 + 2;
                patch = new Vector3[ numColumns, numRows ];
                int[] indexMapU = new int[ numColumns ];
                int[] indexMapV = new int[ numRows ];

                for ( int u = 0; u < numColumns; u++ )
                    indexMapU[ u ] = u % 2 == 0 ? u / 2 : ( u + 1 ) / 2;

                for ( int v = 0; v < numRows; v++ )
                    indexMapV[ v ] = v % 2 == 0 ? v / 2 : ( v + 1 ) / 2;

                for ( int u = 0; u < numColumns; u++ )
                    for ( int v = 0; v < numRows; v++ )
                        patch[ u, v ] = points[ indexMapU[ u ] * controlPointsV + indexMapV[ v ] ];

                vertices = new Vector3[ numColumns * numRows ];

                for ( int column = 0; column < numColumns; column++ )
                    for ( int row = 0; row < numRows; row++ )
                        vertices[ column * numRows + row ] = patch[ column, row ];

                if ( settings.calculateNurbsUVs ) {
                    uvs = new Vector2[ numRows * numColumns ];
					
					float columnIncrement = 1.0f / (float) ( controlPointsU - 1 );
					float rowIncrement = 1.0f / (float) ( controlPointsV - 1 );
					for ( int column = 0; column < numColumns; column++ ) {
						float columnUV = Mathf.Ceil ( (float) column / 2.0f ) * columnIncrement ;
						for ( int row = 0; row < numRows; row++ ) {
							float rowUV = Mathf.Ceil ( (float) row / 2.0f ) * rowIncrement ;
							uvs[ column * numRows + row ] = new Vector2 ( columnUV, rowUV );
						}
					}
                }

                triangles = new int[ ( controlPointsU - 1 ) * ( controlPointsV - 1 ) * 3 * 2 ];

                for ( int column = 0; column < numColumns - 1; column += 2 )
                    for ( int row = 0; row < numRows - 1; row += 2 )
                        TriStrip ( ref triangles, ref trindex, patch, column, row, 1 );
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds ();
            mesh.RecalculateNormals ();

            if ( doubleSided ) {
                Vector3[] __normals = mesh.normals;
                Vector3[] _normals = new Vector3[ mesh.vertices.Length * 2 ];
                Vector2[] _uv = new Vector2[ mesh.vertices.Length * 2 ];
                List<Vector3> _vertices = new List<Vector3> ( vertices );
                _vertices.AddRange ( vertices );
                mesh.vertices = _vertices.ToArray ();
                List<int> _triangles = new List<int> ( triangles );
                _triangles.Capacity = triangles.Length * 2;

                for ( int i = 0; i < triangles.Length; i += 3 ) {
                    _triangles.Add ( triangles[ i + 2 ] );
                    _triangles.Add ( triangles[ i + 1 ] );
                    _triangles.Add ( triangles[ i ] );
                }

                if ( _triangles.Count > 0 )
                    mesh.triangles = _triangles.ToArray ();

                int j = 0;

                foreach ( Vector3 normal in __normals )
                    _normals[ j++ ] = normal;

                foreach ( Vector3 normal in __normals )
                    _normals[ j++ ] = -normal;

                mesh.normals = _normals;

                if ( uvs.Length > 0 ) {
                    j = 0;

                    foreach ( Vector2 uv in uvs )
                        _uv[ j++ ] = uv;

                    foreach ( Vector2 uv in uvs )
                        _uv[ j++ ] = uv;

                    mesh.uv = _uv;
                }
            }

            // calc tangents
            if ( settings.calculateMeshTangents )
                //TangentSolver.Solve ( mesh, false );

			dirtyTesselation = false;
		}

        */

        public void RefineU ()
        {
            NurbsLibPatch.PatchRefineU ( this, null, 0 );
        }

        /*  Insert knot in u direction on patch - thereby adding a row of control points
            does not change shape of patch
            @method InsertKnotU
            @param u				the u value where the knot should be inserted
            @param numInserts	the number of times the knot should be inserted
        */
        public void InsertKnotU ( double u, int numInserts )
        {
            //find where to put the knot
            //int index = Utils.FindPositionInKnotVector(u,knotVectorU);
            int k, s, r;
            int[] fsm2 = NurbsLibCore.FindSpanMult ( controlPointsU - 1, orderU - 1, u, knotVectorU );
            k = fsm2[ 0 ];
            s = fsm2[ 1 ];
            r = numInserts;
            //space for new knot and controlpoint
            double[] newKnots = new double[ controlPointsU + orderU + r ];
            double[] newControls = new double[ ( controlPointsU + r ) * controlPointsV * stride ];
            //calculate new knot vector and control points
            NurbsLibCore.InsertKnotSurfaceU ( stride, controlPointsU, controlPointsV, orderU - 1, knotVectorU, NurbsLibCore.PointsAndWeightsToArray4D ( points, pointWeights ),
                                              u, k, s, r, newKnots, newControls );
            knotVectorU = newKnots;
            NurbsLibCore.Array4DToPointsAndWeights ( this, newControls );
            controlPointsU += r;
        }

        /*  Insert knot in v direction on patch - thereby adding a column of control points
            does not change shape of patch
            @method InsertKnotV
            @param v				the v value where the knot should be inserted
            @param numInserts	the number of times the knot should be inserted
        */
        public void InsertKnotV ( double v, int numInserts )
        {
            int k, s, r;
            int[] fsm2 = NurbsLibCore.FindSpanMult ( controlPointsV - 1, orderV - 1, v, knotVectorV );
            k = fsm2[ 0 ];
            s = fsm2[ 1 ];
            r = numInserts;
            //space for new knot and controlpoint
            double[] newKnots = new double[ controlPointsU + orderU + r ];
            double[] newControls = new double[ ( controlPointsV + r ) * controlPointsU * stride ];
            //calculate new knot vector and control points
            NurbsLibCore.InsertKnotSurfaceV ( stride, controlPointsU, controlPointsV, orderV - 1, knotVectorV, NurbsLibCore.PointsAndWeightsToArray4D ( points, pointWeights ),
                                              v, k, s, r, newKnots, newControls );
            knotVectorV = newKnots;
            NurbsLibCore.Array4DToPointsAndWeights ( this, newControls );
            controlPointsV += r;
        }

        private void TriStrip ( ref int[] triangles, ref int trindex, Vector3[ , ] patch, int column, int startRow, int howManyTriPairs )
        {
            int numRows = patch.GetLength ( 1 );

            for ( int row = startRow; row < startRow + howManyTriPairs; row++ ) {
                triangles[ trindex++ ] = column * numRows + row;
                triangles[ trindex++ ] = ( column + 1 ) * numRows + row;
                triangles[ trindex++ ] = column * numRows + ( row + 1 );
                triangles[ trindex++ ] = ( column + 1 ) * numRows + row;
                triangles[ trindex++ ] = ( column + 1 ) * numRows + ( row + 1 );
                triangles[ trindex++ ] = column * numRows + ( row + 1 );
            }
        }

        public NurbsCurve ExtractCurveU ( double u )
        {
            return NurbsLibPatch.PatchExtractCurve ( this, 4, u, true );
        }

        public NurbsCurve ExtractCurveV ( double v )
        {
            return NurbsLibPatch.PatchExtractCurve ( this, 5, v, true );
        }

        public static NurbsPatch Skin ( List<NurbsCurve> curves, int orderU )
        {
            NurbsPatch patch = new NurbsPatch ();
            NurbsLibPatch.PatchSkinCurves ( curves.ToArray (), orderU ).CopyNurbsPropertiesTo ( patch );
            return patch;
        }

        public static void InterpolateU ( NurbsPatch patch, int orderU )
        {
            NurbsLibPatch.PatchInterpolateU ( patch, orderU );
        }

        public static void SplitU ( NurbsPatch source, double u, ref NurbsPatch result )
        {
            NurbsLibPatch.PatchSplitU ( source, u, ref result );
        }

        public static void SplitV ( NurbsPatch source, double v, ref NurbsPatch result )
        {
            NurbsLibPatch.PatchSplitV ( source, v, ref result );
        }

        public static void SwapUV ( ref NurbsPatch patch )
        {
            NurbsLibPatch.PatchSwapUV ( ref patch );
        }

        public static double[] GenerateKnotVector ( int order, int numControlPoints )
        {
            return NurbsLibCore.GenerateKnotVector ( order, numControlPoints );
        }

        public static NurbsPatch PatchExtrudeCurveVectorV ( NurbsCurve curveV, mmVector3 uVec, int order, int numControlPointsU )
        {
            return NurbsLibPatch.PatchExtrudeCurveVectorV ( curveV, uVec, order, numControlPointsU );
        }

        public void InvertKnotsU ()
        {
            NurbsLibCore.InvertKnots ( knotVectorU );
        }

        public void InvertKnotsV ()
        {
            NurbsLibCore.InvertKnots ( knotVectorV );
        }

        public static bool RemoveColU ( NurbsPatch np, int index )
        {
            int[] removeIndices = NurbsPatch.GetColUIndices ( np, index );
            var newControlPoints = new List<mmVector3> ( ( np.controlPointsV - 1 ) * np.controlPointsU );

            bool removePoint = false;

            for ( int i = 0; i < np.points.Count; i++ ) {
                removePoint = false;

                for ( int j = 0; j < removeIndices.Length; j++ ) {
                    if ( i == removeIndices[ j ] ) {
                        removePoint = true;
                        break;
                    }
                }

                if ( !removePoint )
                    newControlPoints.Add ( np.points[ i ] );
            }

            //np.controlPointsV--;
            np.controlPointsU--;
            np.points = newControlPoints;
            np.knotVectorU = NurbsLibCore.GenerateKnotVector ( np.orderU, np.controlPointsU );
            return true;
        }

        public static int GetColUFromIndex ( NurbsPatch np, int index )
        {
//            Debug.Log ( "Row for point is " + ( index - index % np.controlPointsU ) / np.controlPointsU );
            return ( index - index % np.controlPointsU ) / np.controlPointsU;
        }

        public static int GetRowVFromIndex ( NurbsPatch np, int index )
        {
//            Debug.Log ( "Col for point is " + ( index ) % np.controlPointsU );
            return ( index ) % np.controlPointsU;
        }

        public static int[] GetColUIndices ( NurbsPatch np, int colIndex )
        {
            if ( colIndex > np.controlPointsU )
                Debug.Log ( "NurbsLibPatch.GetColUIndices: column index is out of range" );

            int numCols = np.controlPointsV;
            int[] indices = new int[ numCols ];
            int start = colIndex * numCols;
            int indicesIdx = 0;

            for ( int i = start; i < start + numCols; i++ ) {
                indices[ indicesIdx ] = i;
                indicesIdx++;
            }

            return indices;
        }

        public static mmVector3[] GetColUPoints ( NurbsPatch np, int colIndex )
        {
            int[] indices = NurbsPatch.GetColUIndices ( np, colIndex );
            var colVec = new mmVector3[ indices.Length ];

            for ( int i = 0; i < colVec.Length; i++ )
                colVec[ i ] = np.points[ indices[ i ] ];

            return colVec;
        }

        public static List<double> GetColUWeights ( NurbsPatch np, int colIndex )
        {
            int[] indices = NurbsPatch.GetColUIndices ( np, colIndex );
			var weights = new List<double> ( indices.Length );
			bool hasWeights = ( np.pointWeights.Count == np.points.Count );

			for ( int i = 0; i < indices.Length; i++ )
                weights.Add ( hasWeights ? np.pointWeights[ indices[ i ] ] : 1 );

            return weights;
        }

        public static int[] GetRowVIndices ( NurbsPatch np, int rowIndex )
        {
            if ( rowIndex > np.controlPointsV )
                Debug.Log("NurbsLibPatch.GetRowVIndices: row index is out of range" );

            int numCols = np.controlPointsV;
            int numRows = np.controlPointsU;
            int[] indices = new int[ numRows ];
            int start = rowIndex;
            int indicesIdx = 0;

            for ( int i = start; i < numRows * numCols; i += numCols ) {
                indices[ indicesIdx ] = i;
                indicesIdx++;
            }

            return indices;
        }

        public static mmVector3[] GetRowVPoints ( NurbsPatch np, int rowIndex )
        {
            int[] indices = NurbsPatch.GetRowVIndices ( np, rowIndex );
            var rowVec = new mmVector3[ indices.Length ];

            for ( int i = 0; i < rowVec.Length; i++ )
                rowVec[ i ] = np.points[ indices[ i ] ];

            return rowVec;
        }

        public static List<double> GetRowVWeights ( NurbsPatch np, int rowIndex )
        {
            int[] indices = NurbsPatch.GetRowVIndices ( np, rowIndex );
            var weights = new List<double> ( indices.Length );
            bool hasWeights = ( np.pointWeights.Count == np.points.Count );

			for ( int i = 0; i < indices.Length; i++ )
                weights.Add ( hasWeights ? np.pointWeights[ indices[ i ] ] : 1 );

            return weights;
        }

        public string ToString ()
        {
            string str = "Nurbs Patch::: \n";
            str += "numControlPointsU : " + controlPointsU + "\n";
            str += "numControlPointsV : " + controlPointsV + "\n";
            str += "orderU : " + orderU + "\n";
            str += "orderV : " + orderV + "\n";
            str += "knotVectorU : " + NurbsLibCore.DoublesToString ( knotVectorU ) + "\n";
            str += "knotVectorU Length : " + knotVectorU.Length + "\n";
            str += "knotVectorV : " + NurbsLibCore.DoublesToString ( knotVectorV ) + "\n";
            str += "knotVectorV Length : " + knotVectorV.Length + "\n";
			str += "controlPoints : " + NurbsLibCore.DoubleVectorListToString ( points ) + "\n";
            str += "controlPoints Length: " + points.Count + "\n";
            return str;
        }
    }
}