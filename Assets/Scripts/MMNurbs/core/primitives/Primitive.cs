using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using UnityNURBS.Operators;
using UnityNURBS.Types;
//using UnityNURBS.Render;
//using UnityNURBS.Views;
//using UnityNURBS.Sessions;
//
//using Schema.JsonMsg;

namespace UnityNURBS.Primitives
{

// all primitive classes need to implement these methods
    interface IPrimitive
    {
        void Draw ();

        void Destroy ();

        //void CloneTo ( Operator op );

        object Clone ();

        void DrawSelectionMask ( Color idColor, Camera cam );

        //void ToSocket ( NodeUpdate nodeUpdate );
    }


// primitive parent class
    public class Primitive
    {
        // to keep record of all gameObjects created in association with geometry primitives
        public List<GameObject> gameObjects
        {
            get
            {
                return null;
            }//Scene.activeScene.gameObjects; }
        }

        public Settings settings
        {
            get { return null; }//Scene.activeScene.settings; }
        }

        // the operator with which this primitive is associated
        //public virtual Operator op { get; set; }

        // this primitive's points list
        public virtual List<mmVector3> points { get; set; }

        // this primitive's attributes
        public Dictionary<string, object> attributes { get; set; }
        // this primitive's per-point attributes
        public Dictionary<string, List<object>> perpointAttributes { get; set; }

        public virtual bool dirtyTesselation { get; set; }

        public virtual int layer { get; set; }

        public virtual Material material { get; set; }

        public virtual Color color {
            get { return material.color; }
            set { material.color = value; }
        }

        public virtual List<Color32> pointColors { get; set; }
        public virtual List<double> pointWeights { get; set; }

        public virtual Material[] selectionMaterials { get; set; }

        // GUI widget stuff
        public int border;
        public int rowHeight;
        public int nameColumnWidth;
        public float left = 0;
        public float top = 0;
        public GameObject widgetGameObject;

        public Primitive ()
        {
            points = new List<mmVector3> ();
            dirtyTesselation = true;
            layer = 0;
            //material = new Material();
            pointColors = new List<Color32> ();
            pointWeights = new List<double> ();

            attributes = new Dictionary<string, object> ();
            perpointAttributes = new Dictionary<string, List<object>> ();

           // border = settings.inViewControlBorder;
            //rowHeight = settings.inViewControlRowHeight;
            //nameColumnWidth = settings.inViewControlNameColumnWidth;
        }

        // expand PrimitiveGroup primitives to their children, recursively

            /*
        public static List<Primitive> Flatten ( List<Primitive> primitives )
        {
            var flattenedPrimitives = new List<Primitive> ();

            foreach ( Primitive primitive in primitives ) {
                if ( primitive is PrimitiveGroup ) {
                    var primitiveGroup = primitive as PrimitiveGroup;
                    flattenedPrimitives.AddRange ( Flatten ( primitiveGroup.primitives ) );
                }
                else
                    flattenedPrimitives.Add ( primitive );
            }

            return flattenedPrimitives;
        }

    */

        // expand PrimitiveGroup primitive to its children

            /*
        public static List<Primitive> Flatten ( Primitive primitive )
        {
            var flattenedPrimitives = new List<Primitive> ();

            if ( primitive is PrimitiveGroup ) {
                var primitiveGroup = primitive as PrimitiveGroup;
                flattenedPrimitives.AddRange ( Flatten ( primitiveGroup.primitives ) );
            }
            else
                flattenedPrimitives.Add ( primitive );

            return flattenedPrimitives;
        }

    */
    /*
        // expand top level PrimitiveGroup primitives to their children
        public static List<Primitive> UnGroup ( List<Primitive> primitives )
        {
            var flattenedPrimitives = new List<Primitive> ();

            foreach ( Primitive primitive in primitives ) {
                if ( primitive is PrimitiveGroup ) {
                    var primitiveGroup = primitive as PrimitiveGroup;
                    flattenedPrimitives.AddRange ( primitiveGroup.primitives );
                }
                else
                    flattenedPrimitives.Add ( primitive );
            }

            return flattenedPrimitives;
        }

    */

        public static List<Primitive> ClonePrimitives ( List<Primitive> primitives )
        {
            var clonedPrimitives = new List<Primitive> ();

            foreach ( Primitive primitive in primitives )
                clonedPrimitives.Add ( ( Primitive ) ( ( IPrimitive ) primitive ).Clone() );

            return clonedPrimitives;
        }

        /*

        public virtual void InitSelectionMaterials ()
        {
            if ( selectionMaterials == null )
                selectionMaterials = new Material[] {
                    new Material ( settings.selectionFrontMaterial ),
                    new Material ( settings.selectionBackMaterial )
                };
        }

    */

            /*

        public virtual void DestroyPrimitive ()
        {
            if ( selectionMaterials != null )
                foreach ( Material material in selectionMaterials )
                    UnityEngine.Object.Destroy ( material );

            ( ( IPrimitive ) this ).Destroy ();
        }

        public virtual void ToSocket( NodeUpdate nodeUpdate ) {}

    */
    }

    
}