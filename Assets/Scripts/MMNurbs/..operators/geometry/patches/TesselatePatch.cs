using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using UnityNURBS.Render;
using Ayam.Nurbs;

namespace UnityNURBS.Operators
{

    public class TesselatePatch : Operator, IOperator
    {
        public int mode;
        public int quality;
        public int qualityU;
        public int qualityV;

        public TesselatePatch() {}

        public bool Cook()
        {
            foreach ( Primitive primitive in inputGeometry ) {
                if ( primitive is NurbsPatch )
                    outputGeometry.Add ( Tesselate ( ( NurbsPatch ) primitive ) );
                else
                    outputGeometry.Add ( primitive );
            }

            return true;
        }
		
		private PolyMesh Tesselate ( NurbsPatch patch )
		{
			if ( mode == 0 )
				patch.BuildMesh ( quality );
			else if ( mode == 1 )
				patch.BuildMesh ( qualityU, qualityV );
			
			var polyMesh = new PolyMesh ( patch.mesh );
			
			// FIXME: support attribute transfer etc
			polyMesh.material = new mmMaterial ( patch.material );

			patch.DestroyPrimitive();
			
			return polyMesh;
		}
    }
}