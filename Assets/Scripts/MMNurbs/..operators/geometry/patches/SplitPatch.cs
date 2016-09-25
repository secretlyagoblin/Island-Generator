using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;
using UnityNURBS.Render;

namespace UnityNURBS.Operators
{

    public class SplitPatch : Operator, IOperator
    {
        public double uv;
        public int direction;
        public int discard;
        public SplitPatch() {}

        private void Split ( NurbsPatch np )
        {
            if ( uv == 0 || uv == 1 ) {
                outputGeometry.Add ( np );
                return;
            }

            var inside = np.Clone() as NurbsPatch;
            var outside = new NurbsPatch();
			
			// FIXME: support attribute transfer etc
			outside.material = new mmMaterial ( np.material );

            if ( direction == 0 )
                NurbsPatch.SplitU ( inside, uv, ref outside );
            else
                NurbsPatch.SplitV ( inside, uv, ref outside );

            if ( discard == 0 ) {
                outputGeometry.Add ( inside );
                outputGeometry.Add ( outside );
            }
            else if ( discard == 1 )
                outputGeometry.Add ( outside );
			else if ( discard == 2 )
                outputGeometry.Add ( inside );
        }


        public bool Cook()
        {
            // find all NurbsCurves in inputGeometry
            foreach ( Primitive primitive in inputGeometry )
                if ( primitive is NurbsPatch )
                    Split ( primitive as NurbsPatch );
                else
                    outputGeometry.Add ( primitive );

            return true;
        }



    }

}