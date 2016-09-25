using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//using Ayam.Nurbs;
using UnityNURBS.Primitives;
using UnityNURBS.Util;

namespace UnityNURBS{

public class Polyline
{
    public Vector3[] points;
    public int segments;
    public Color color;

    public Polyline () {}

    public Polyline ( int _segments )
    {
        segments = _segments;
        points = new Vector3[ segments + 1 ];
    }

    public void CloneFrom ( Polyline polyline )
    {
        segments = polyline.segments;
        color = polyline.color;

        if ( polyline.points != null )
            points = ( Vector3[] ) polyline.points.Clone ();
    }

    public int[] Indices ()
    {
        return Enumerable.Range ( 0, segments + 1 ).ToArray ();
    }

    public void BuildPolylineFromCurve ( NurbsCurve nc, int quality )
    {
        int numPoints;

        if ( nc.order == 2 )
            numPoints = nc.numControlPoints;
        else
//			numPoints = quality*(nc.numControlPoints-1) + 1;
            numPoints = ( nc.numControlPoints + 4 ) * quality; // mimics NurbsLibCore.SurfacePoints4D

        segments = numPoints - 1;
        points = new Vector3[ numPoints ];
        double uinc = 1f / ( double ) ( numPoints - 1 );
        var uCoordinates = new double[ numPoints ];

        for ( int i = 0; i < numPoints - 1; i++ )
            uCoordinates[ i ] = i * uinc;

        uCoordinates[ numPoints - 1 ] = 1;
        points = MiscUtil.DoubleVectorArrayToFloatVectorArray ( nc.GetPoints ( uCoordinates ) );
    }

}
}
