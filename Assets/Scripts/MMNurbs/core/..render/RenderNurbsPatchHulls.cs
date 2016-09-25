using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
//using UnityNURBS.Operators;
//using UnityNURBS.Views;
//using UnityNURBS.Sessions;

namespace UnityNURBS.Render
{

    public class RenderNurbsPatchHulls : MonoBehaviour
    {
        public Rig rig;

        private Scene scene
        {
            get { return rig.scene; }
        }

        public static List<NurbsPatch> patches = new List<NurbsPatch>();
        private static Material lineMaterial;
        public int numPatches;


        void Start ()
        {
            lineMaterial = scene.settings.glLineMaterial;
        }


        public void OnPostRender()
        {
            if ( patches.Count == 0 ) return;

            numPatches = patches.Count;

            foreach ( NurbsPatch patch in patches )
                DrawHull ( patch );
        }


        private void DrawHull ( NurbsPatch patch )
        {
            if ( patch.layer != rig.layer ) return;

            lineMaterial.SetPass ( 0 );
            GL.Begin ( GL.LINES );
            GL.Color ( scene.settings.hullColorV );
            int uoffset = 0;

            for ( int u = 0; u < patch.controlPointsU; u++ ) {
                for ( int v = 0; v < patch.controlPointsV - 1; v++ ) {
                    GL.Vertex ( new Vector3 (	( float ) patch.points[ v + uoffset ].x,
                                                ( float ) patch.points[ v + uoffset ].y,
                                                ( float ) patch.points[ v + uoffset ].z )
                              );
                    GL.Vertex ( new Vector3 (	( float ) patch.points[ ( v + 1 ) + uoffset ].x,
                                                ( float ) patch.points[ ( v + 1 ) + uoffset ].y,
                                                ( float ) patch.points[ ( v + 1 ) + uoffset ].z )
                              );
                }

                uoffset += patch.controlPointsV;
            }

            GL.Color ( scene.settings.hullColorU );
            int voffset = 0;

            for ( int v = 0; v < patch.controlPointsV; v++ ) {
                for ( int u = 0; u < patch.controlPointsU - 1; u++ ) {
                    GL.Vertex ( new Vector3 (	( float ) patch.points[ u * patch.controlPointsV + voffset ].x,
                                                ( float ) patch.points[ u * patch.controlPointsV + voffset ].y,
                                                ( float ) patch.points[ u * patch.controlPointsV + voffset ].z )
                              );
                    GL.Vertex ( new Vector3 (	( float ) patch.points[ ( u + 1 ) * patch.controlPointsV + voffset ].x,
                                                ( float ) patch.points[ ( u + 1 ) * patch.controlPointsV + voffset ].y,
                                                ( float ) patch.points[ ( u + 1 ) * patch.controlPointsV + voffset ].z )
                              );
                }

                voffset ++;
            }

            GL.End();
        }
    }
}