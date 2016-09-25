
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
//using UnityNURBS.Views;
//using UnityNURBS.Sessions;

namespace UnityNURBS.Render
{

    public class RenderNurbsCurves : MonoBehaviour
    {


        public static List<NurbsCurve> curves = new List<NurbsCurve>();

        //used for checking if curve is on same layer as the rig this is attached to
        public Rig rig;

        private Scene scene
        {
            get { return rig.scene; }
        }

        public int numCurves;  // to see in Unity

        private static Material lineMaterial;

        void Start ()
        {
            lineMaterial = scene.settings.glLineMaterial; //set line rendering meterial - could be parameterised
        }

        public void OnPostRender()
        {
            if ( curves.Count == 0 ) return;

            numCurves = curves.Count;

            foreach ( NurbsCurve nurbsCurve in curves )
                if ( nurbsCurve.layer == rig.layer )
                    if ( !nurbsCurve.isIsoParm )
                        if ( nurbsCurve.drawHulls )
                            DrawPolyline ( nurbsCurve.hull );
        }


        public static void DrawPolyline ( Polyline polyLine )
        {
            lineMaterial.SetPass ( 0 );
            GL.Color ( polyLine.color );
            GL.Begin ( GL.LINES );

            for ( int i = 0; i < polyLine.segments; i++ ) {
                GL.Vertex ( polyLine.points[ i ] );
                GL.Vertex ( polyLine.points[ i + 1 ] );
            }

            GL.End();
        }
    }
}