using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;

namespace UnityNURBS.Operators
{

    public class FilletCurves : Operator, IOperator
    {
//		public int order;	// order 4 only
        public double tangentLength;
        public int skip;
        public bool keepInputCurves;

        public FilletCurves() {}

        public bool Cook()
        {
            if ( skip < 1 ) {
                errorMessage = "smallest meaningful skip is 1";
                return false;
            }

//			if (order < 2) {
//				errorMessage = "order needs to be > 2";
//				return false;
//			}
            List<NurbsCurve> curves = new List<NurbsCurve>();

            // find all NurbsCurves in inputGeometry
            foreach ( Primitive primitive in inputGeometry )
                if ( primitive.GetType() == typeof ( NurbsCurve ) )
                    curves.Add ( ( NurbsCurve ) primitive );
                else
                    outputGeometry.Add ( primitive );

            /*
            			for(int i=0;i<curves.Count;i+=skip) {
            				for(int j=0;j<skip-1;j++) {
            					if (!(i+j+1 > curves.Count-1)) {
            						NurbsCurve nc = NurbsLibCurve.CurveFillGapC1(4,tangentLength,curves[ i+j ],curves[ i+j+1 ]);
            						if (nc != null)
            							outputGeometry.Add(nc);
            					}
            				}
            			}
            */

            for ( int i = 0; i < curves.Count - skip; i++ ) {
                NurbsCurve curve = NurbsCurve.CurveFillGap ( 4, tangentLength, curves[ i ], curves[ i + skip ] );

                if ( keepInputCurves )
                    outputGeometry.Add ( curves[ i ] );

                if ( curve != null )
                    outputGeometry.Add ( curve );
            }

            if ( keepInputCurves )
                for ( int i = curves.Count - skip; i < curves.Count; i++ )
                    outputGeometry.Add ( curves[ i ] );

            return true;
        }



    }

}