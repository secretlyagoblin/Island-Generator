using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;

namespace Ayam.Nurbs
{

    public struct Vec3 {
        public double x;
        public double y;
        public double z;

        public Vec3 ( double x = 0, double y = 0, double z = 0 )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3 Cross ( Vec3 other )
        {
            return new Vec3 ( this.y * other.z - this.z * other.y,
                              this.z * other.x - this.x * other.z,
                              this.x * other.y - this.y * other.x );
        }

        public Vec3 Minus ( Vec3 other )
        {
            return new Vec3 ( this.x - other.x, this.y - other.y, this.z - other.z );
        }

        public Vec3 Neg ()
        {
            this.x = - this.x;
            this.y = - this.y;
            this.z = - this.z;
            return this;
        }

        public override string ToString ()
        {
            return "Vec3(" + x + "," + y + "," + z + ")";
        }

        /*
        	// internal conversions
        	public static implicit operator Vector3(Vec3 myParam) {
        		return new Vector3((float)myParam.x,(float)myParam.y,(float)myParam.z);
        	}
        */

    }

    public class NurbsLibCore
    {
        /*  FindSpan
            n = numControlPoints
            p = degree!  (not order, I think - Tom	)
            u = param value
            U = knotVector
        * */

        public static int FindSpan ( int n, int p, double u, double[] U )
        {
            if ( u >= U[ n ] )
                return n; else if ( u <= U[ p ] )

                return p; else {

                int low = 0;
                int high = n + 1;
                int mid = ( low + high ) / 2;

                while ( u < U[ mid ] || u >= U[ mid + 1 ] ) {
                    if ( u < U[ mid ] )
                        high = mid; else

                        low = mid;

                    mid = ( low + high ) / 2;
                }

                return mid;
            }
        }



        // ayam version passed in *s for extra return value, replaced this with an int array.
        public static int[] FindSpanMult ( int n, int p, double u, double[] U )
        {
            int low, mid, high, l;

            if ( u >= U[ n ] )
                mid = n; else {

                if ( u <= U[ p ] )
                    mid = p; else {

                    low = 0;
                    high = n + 1;
                    mid = ( low + high ) / 2;

                    while ( u < U[ mid ] || u >= U[ mid + 1 ] ) {
                        if ( u < U[ mid ] )
                            high = mid;
                        else
                            low = mid;

                        mid = ( low + high ) / 2;
                    } /* while */
                } /* if */
            } /* if */

            l = mid;

            while ( l >= 0 ) {
                if ( Math.Abs ( U[ l ] - u ) > double.Epsilon )
                    break;

                l--;
            }

            return new int[] {mid, mid - l};
        }

        /*
            ay_nb_BasisFuns:
            calculate NURBS basis functions for span i, parametric value u
            order p, knot vector U[] in N[ p+1 ]
        */
        public static double[] BasisFuns ( int i, double u, int p, double[] U )
        {
            double[] left = new double[ p + 1 ];
            double[] right = new double[ p + 1 ];
            double[] N = new double[ p + 1 ];
            N[ 0 ] = 1.0;

            for ( int j = 1; j <= p; j++ ) {
                left[ j ] = u - U[ i + 1 - j ];
                right[ j ] = U[ i + j ] - u;
                double saved = 0.0;

                for ( int r = 0; r < j; r++ ) {
                    double temp = N[ r ] / ( right[ r + 1 ] + left[ j - r ] );
                    N[ r ] = saved + right[ r + 1 ] * temp;
                    saved = left[ j - r ] * temp;
                }

                N[ j ] = saved;
            }

            return N;
        }

        public static double[] DersBasisFuns ( int i, double u, int p, int n, double[] U )
        {
            double saved, temp, d;
            int j1, j2, rk, pk, s1, s2;
            double[] left = new double[ p + 1 ];
            double[] right = new double[ p + 1 ];
            double[] ndu = new double[ ( p + 1 ) * ( p + 1 ) ];
            double[] a = new double[ 2 * ( p + 1 ) ];
            double[] ders = new double[ ( p + 1 ) * ( p + 1 ) ];
            ndu[ 0 ] = 1.0;

            for ( int j = 1; j <= p; j++ ) {
                left[ j ] = u - U[ i + 1 - j ];
                right[ j ] = U[ i + j ] - u;
                saved = 0.0;

                for ( int r = 0; r < j; r++ ) {
                    ndu[ ( j ) * ( p + 1 ) + r ] = right[ r + 1 ] + left[ j - r ];
                    temp = ndu[ r * ( p + 1 ) + ( j - 1 ) ] / ndu[ j * ( p + 1 ) + r ];
                    ndu[ r * ( p + 1 ) + j ] = saved + right[ r + 1 ] * temp;
                    saved = left[ j - r ] * temp;
                }

                ndu[ j * ( p + 1 ) + j ] = saved;
            }

            for ( int j = 0; j <= p; j++ )
                ders[ j ] = ndu[ j * ( p + 1 ) + p ];

            for ( int r = 0; r <= p; r++ ) {
                s1 = 0;
                s2 = 1;
                a[ 0 ] = 1.0;

                for ( int k = 1; k <= n; k++ ) {
                    d = 0.0;
                    rk = r - k;
                    pk = p - k;

                    if ( r >= k ) {
                        a[ s2 * ( p + 1 ) ] = a[ s1 * ( p + 1 ) ] / ndu[ ( pk + 1 ) * ( p + 1 ) + rk ];
                        d = a[ s2 * ( p + 1 ) ] * ndu[ rk * ( p + 1 ) + pk ];
                    }

                    if ( rk >= -1 )
                        j1 = 1; else

                        j1 = -rk;

                    if ( r - 1 <= pk )
                        j2 = k - 1; else

                        j2 = p - r;

                    for ( int j = j1; j <= j2; j++ ) {
                        a[ s2 * ( p + 1 ) + j ] = ( a[ s1 * ( p + 1 ) + j ] - a[ s1 * ( p + 1 ) + ( j - 1 ) ] ) /
                                                  ndu[ ( pk + 1 ) * ( p + 1 ) + ( rk + j ) ];
                        d += a[ s2 * ( p + 1 ) + j ] * ndu[ ( rk + j ) * ( p + 1 ) + pk ];
                    }

                    if ( r <= pk ) {
                        a[ s2 * ( p + 1 ) + k ] = -a[ s1 * ( p + 1 ) + ( k - 1 ) ] / ndu[ ( pk + 1 ) * ( p + 1 ) + r ];
                        d += a[ s2 * ( p + 1 ) + k ] * ndu[ r * ( p + 1 ) + pk ];
                    }

                    ders[ k * ( p + 1 ) + r ] = d;
                    int tmp = s1;
                    s1 = s2;
                    s2 = tmp;
                }
            }

            int val = p;

            for ( int k = 1; k <= n; k++ ) {
                for ( int j = 0; j <= p; j++ )
                    ders[ k * ( p + 1 ) + j ] *= val;

                val *= ( p - k );
            }

            return ders;
        }

        public static double[] Bin ( int maxn, int maxk )
        {
            double[] bin = new double[ maxn * maxk ];
            /* Setup the first line */
            bin[ 0 ] = 1.0;

            for ( int k = ( maxk - 1 ); k > 0; --k )
                bin[ k ] = 0.0;

            /* Setup the other lines */
            for ( int n = 0; n < ( maxn - 1 ); n++ ) {
                int bini = ( n + 1 ) * maxk;
                bin[ bini ] = 1.0;

                for ( int k = 1; k < maxk; k++ ) {
                    if ( n + 1 < k ) {
                        bini = n * maxk + k;
                        bin[ bini ] = 0.0;
                    }
                    else {
                        bini = ( n + 1 ) * maxk + k;
                        int bini2 = n * maxk + k;
                        int bini3 = n * maxk + ( k - 1 );
                        bin[ bini ] = bin[ bini2 ] + bin[ bini3 ];
                    }
                }
            }

            return bin;
        }

        public static Vec3[] CompFirstDerSurf4D ( int n, int m, int p, int q, double[] U, double[] V, double[] Pw, double u, double v )
        {
            double[] temp = new double[ ( q + 1 ) * 4 ];
            double[] Ct = new double[ 4 * 4 ];
            Vec3[] C = new Vec3[ 4 ];
            double[] w = new double[ 3 ];
            double[] w2 = new double[ 3 ];
            double[] bin = Bin ( 2, 2 );
            int uspan = FindSpan ( n, p, u, U );
            double[] Nu = DersBasisFuns ( uspan, u, p, 1, U );
            int vspan = FindSpan ( m, q, v, V );
            double[] Nv = DersBasisFuns ( vspan, v, q, 1, V );
            Ct[ 0 ] = 0.0;
            Ct[ 1 ] = 0.0;
            Ct[ 2 ] = 0.0;
            Ct[ 3 ] = 0.0;

            for ( int k = 0; k <= 1; k++ ) {
                for ( int s = 0; s <= q; s++ ) {
                    temp[ s * 4 ] = 0.0;
                    temp[ s * 4 + 1 ] = 0.0;
                    temp[ s * 4 + 2 ] = 0.0;
                    temp[ s * 4 + 3 ] = 0.0;

                    for ( int r = 0; r <= p; r++ ) {
                        /* was: temp[ s ] = temp[ s ] + Nu[ k ][ r ]*P[ uspan-p+r ][ vspan-q+s ]; */
                        int i = ( ( ( uspan - p + r ) * ( m + 1 ) ) + ( vspan - q + s ) ) * 4;
                        temp[ s * 4 ] += Nu[ ( k * ( p + 1 ) ) + r ] * Pw[ i ] * Pw[ i + 3 ];
                        temp[ s * 4 + 1 ] += Nu[ ( k * ( p + 1 ) ) + r ] * Pw[ i + 1 ] * Pw[ i + 3 ];
                        temp[ s * 4 + 2 ] += Nu[ ( k * ( p + 1 ) ) + r ] * Pw[ i + 2 ] * Pw[ i + 3 ];
                        temp[ s * 4 + 3 ] += Nu[ ( k * ( p + 1 ) ) + r ] * Pw[ i + 3 ];
                    }
                }

                for ( int l = 0; l <= 1; l++ ) {
                    /* was: C[ k ][ l ] = 0; */
                    Ct[ ( k * 2 + l ) * 4 ] = 0.0;
                    Ct[ ( k * 2 + l ) * 4 + 1 ] = 0.0;
                    Ct[ ( k * 2 + l ) * 4 + 2 ] = 0.0;
                    Ct[ ( k * 2 + l ) * 4 + 3 ] = 0.0;

                    for ( int s = 0; s <= q; s++ ) {
                        /* was C[ k ][ l ] = C[ k ][ l ] + Nv[ l ][ s ] * temp[ s ]; */
                        int i = ( k * 2 + l ) * 4;
                        Ct[ i ] += Nv[ ( l * ( q + 1 ) ) + s ] * temp[ s * 4 ];
                        Ct[ i + 1 ] += Nv[ ( l * ( q + 1 ) ) + s ] * temp[ s * 4 + 1 ];
                        Ct[ i + 2 ] += Nv[ ( l * ( q + 1 ) ) + s ] * temp[ s * 4 + 2 ];
                        Ct[ i + 3 ] += Nv[ ( l * ( q + 1 ) ) + s ] * temp[ s * 4 + 3 ];
                    }
                }
            }

            /***/
            for ( int k = 0; k <= 1; k++ ) {
                for ( int l = 0; l <= 1 - k; l++ ) {
                    /* was: w = Ct[ k ][ l ]; */
                    int i = ( k * 2 + l ) * 4;
                    w[ 0 ] = Ct[ i ];
                    w[ 1 ] = Ct[ i + 1 ];
                    w[ 2 ] = Ct[ i + 2 ];

                    for ( int j = 1; j <= l; j++ ) {
                        /* was: w -= bin[ l ][ j ]*wders[ 0 ][ j ]*C[ k ][ l-j ]; */
                        i = ( k * 2 + ( l - j ) );
                        w[ 0 ] -= bin[ l * 2 + j ] * Ct[ ( j * 4 ) + 3 ] * C[ i ].x;
                        w[ 1 ] -= bin[ l * 2 + j ] * Ct[ ( j * 4 ) + 3 ] * C[ i ].y;
                        w[ 2 ] -= bin[ l * 2 + j ] * Ct[ ( j * 4 ) + 3 ] * C[ i ].z;
                    }

                    for ( int h = 1; h <= k; h++ ) {
                        /* was: w -= bin[ k ][ h ]*wders[ h ][ 0 ]*C[ k-h ][ l ]; */
                        i = ( ( k - h ) * 2 + l );
                        w[ 0 ] -= bin[ k * 2 + h ] * Ct[ ( h * 2 * 4 ) + 3 ] * C[ i ].x;
                        w[ 1 ] -= bin[ k * 2 + h ] * Ct[ ( h * 2 * 4 ) + 3 ] * C[ i ].y;
                        w[ 2 ] -= bin[ k * 2 + h ] * Ct[ ( h * 2 * 4 ) + 3 ] * C[ i ].z;
                        w2[ 0 ] = 0.0;
                        w2[ 1 ] = 0.0;
                        w2[ 2 ] = 0.0;

                        for ( int j = 1; j <= l; j++ ) {
                            /* was: w2 += bin[ l ][ j ]*wders[ h ][ j ]*C[ k-h ][ l-j ]; */
                            i = ( ( k - h ) * 2 + ( l - j ) );
                            w2[ 0 ] += bin[ l * 2 + j ] * Ct[ ( ( h * 2 + j ) * 4 ) + 3 ] * C[ i ].x;
                            w2[ 1 ] += bin[ l * 2 + j ] * Ct[ ( ( h * 2 + j ) * 4 ) + 3 ] * C[ i ].y;
                            w2[ 2 ] += bin[ l * 2 + j ] * Ct[ ( ( h * 2 + j ) * 4 ) + 3 ] * C[ i ].z;
                        }

                        /* was: w -= bin[ k ][ h ]*w2; */
                        w[ 0 ] -= bin[ k * 2 + h ] * w2[ 0 ];
                        w[ 1 ] -= bin[ k * 2 + h ] * w2[ 1 ];
                        w[ 2 ] -= bin[ k * 2 + h ] * w2[ 2 ];
                    }

                    i = ( k * 2 + l );
                    C[ i ].x = w[ 0 ] / Ct[ 3 ];
                    C[ i ].y = w[ 1 ] / Ct[ 3 ];
                    C[ i ].z = w[ 2 ] / Ct[ 3 ];
                }
            }

            return C;
        } /* ay_nb_CompFirstDerSurf4D */



        // p and q are ORDER here I suppose, not DEGREE, as above - Tom
        // that's why they are being decremented

        public static Vec3 SurfacePoint4D ( int n, int m, int p, int q, double[] U, double[] V, double[] Pw, double u, double v )
        {
            p--;
            q--;
            int spanu = 0, spanv = 0, indu = 0, indv = 0, l = 0, k = 0, i = 0, j = 0;
            double[] Nu, Nv, temp;
            double[] Cw = new double[ 4 ];
            Vec3 pt = new Vec3 ();
            Nu = new double[ p + 1 ];
            Nv = new double[ q + 1 ];
            temp = new double[ ( q + 1 ) * 4 ];
            spanu = NurbsLibCore.FindSpan ( n, p, u, U );
            Nu = NurbsLibCore.BasisFuns ( spanu, u, p, U );
            spanv = NurbsLibCore.FindSpan ( m, q, v, V );
            Nv = NurbsLibCore.BasisFuns ( spanv, v, q, V );
            indu = spanu - p;

            for ( l = 0; l <= q; l++ ) {
                indv = spanv - q + l;

                for ( k = 0; k <= p; k++ ) {
                    i = ( ( ( indu + k ) * ( m + 1 ) ) + indv ) * 4;
                    temp[ j + 0 ] += Nu[ k ] * Pw[ i ] * Pw[ i + 3 ];
                    temp[ j + 1 ] += Nu[ k ] * Pw[ i + 1 ] * Pw[ i + 3 ];
                    temp[ j + 2 ] += Nu[ k ] * Pw[ i + 2 ] * Pw[ i + 3 ];
                    temp[ j + 3 ] += Nu[ k ] * Pw[ i + 3 ];
                }

                j += 4;
            }

            j = 0;

            for ( l = 0; l <= q; l++ ) {
                Cw[ 0 ] += Nv[ l ] * temp[ j + 0 ];
                Cw[ 1 ] += Nv[ l ] * temp[ j + 1 ];
                Cw[ 2 ] += Nv[ l ] * temp[ j + 2 ];
                Cw[ 3 ] += Nv[ l ] * temp[ j + 3 ];
                j += 4;
            }

            pt.x = Cw[ 0 ] / Cw[ 3 ];
            pt.y = Cw[ 1 ] / Cw[ 3 ];
            pt.z = Cw[ 2 ] / Cw[ 3 ];
            return pt;
        }


        /*  SurfacePoints4D
            m = controlPointsU
            n = controlPointsV
            p = orderU
            q = orderV
            U = knotVectorU
            V = knotVectorV
            Pw = controlPoints
            qf = quality of tesselated patch
        */

        // p and q are ORDER here I suppose, not DEGREE, as above - Tom
        // that's why they are being decremented

        public static Vec3[ , , ] SurfacePointsAndNormals4D ( int n, int m, int p, int q, double[] U, double[] V, double[] Pw, int qfu, int qfv )
        {
            p--;		//order vars are no longer predecremented when passed in
            q--;		//this is consistent with other functions in this class
            double[] temp = new double[ ( q + 1 ) * 4 ];
            int Cn = ( 4 + n ) * qfu;
            double ud = ( U[ n ] - U[ p ] ) / ( Cn - 1 );
            int Cm = ( 4 + m ) * qfv;
            double vd = ( V[ m ] - V[ q ] ) / ( Cm - 1 );
            int[] spanus = new int[ Cn ];
            int[] spanvs = new int[ Cm ];
            Vec3[ , , ] Ct = new Vec3[ Cn, Cm, 2 ];
            /*  employ linear variants of FindSpan() as they are much faster
                than a binary search; especially, since we calculate
                spans for all parameters in order */
            {
                double u = U[ p ];
                int spanu = p;
                int a = 0;

                for ( ; a < Cn - 1; a++ ) {
                    if ( u > U[ p + 1 ] ) {
                        while ( u > U[ spanu + 1 ] )
                            spanu++;
                    }

                    spanus[ a ] = spanu;
                    u += ud;
                }

                spanus[ a ] = spanus[ a - 1 ];
            }
            {
                double v = V[ q ];
                int spanv = q;
                int a = 0;

                for ( ; a < Cm - 1; a++ ) {
                    if ( v > V[ q + 1 ] ) {
                        while ( v > V[ spanv + 1 ] )
                            spanv++;
                    }

                    spanvs[ a ] = spanv;
                    v += vd;
                }

                spanvs[ a ] = spanvs[ a - 1 ];
            }
            {
                int j = 0;
                double u = U[ p ];

                for ( int a = 0; a < Cn; a++ ) {
                    int spanu = spanus[ a ];
                    double[] Nu = BasisFuns ( spanu, u, p, U );
                    int indu = spanu - p;
                    double v = V[ q ];

                    for ( int b = 0; b < Cm; b++ ) {
                        int spanv = spanvs[ b ];
                        double[] Nv = BasisFuns ( spanv, v, q, V );
                        {
                            int ti = 0;

                            for ( int l = 0; l <= q; l++ ) {
                                System.Array.Clear ( temp, l * 4, 4 );
                                int indv = spanv - q + l;

                                for ( int k = 0; k <= p; k++ ) {
                                    int i = ( ( ( indu + k ) * m ) + indv ) * 4;
                                    temp[ ti + 0 ] += Nu[ k ] * Pw[ i ] * Pw[ i + 3 ];
                                    temp[ ti + 1 ] += Nu[ k ] * Pw[ i + 1 ] * Pw[ i + 3 ];
                                    temp[ ti + 2 ] += Nu[ k ] * Pw[ i + 2 ] * Pw[ i + 3 ];
                                    temp[ ti + 3 ] += Nu[ k ] * Pw[ i + 3 ];
                                }

                                ti += 4;
                            }
                        }
                        {
                            double[] Cw = new double[ 4 ];
                            int ti = 0;

                            for ( int l = 0; l <= q; l++ ) {
                                /* was: Cw = Cw + Nv[ l ]*temp */
                                Cw[ 0 ] += Nv[ l ] * temp[ ti + 0 ];
                                Cw[ 1 ] += Nv[ l ] * temp[ ti + 1 ];
                                Cw[ 2 ] += Nv[ l ] * temp[ ti + 2 ];
                                Cw[ 3 ] += Nv[ l ] * temp[ ti + 3 ];
                                ti += 4;
                            }

                            /*j = (a*(*Cn)+b)*3;*/
                            Ct[ a, b, 0 ].x = Cw[ 0 ] / Cw[ 3 ];
                            Ct[ a, b, 0 ].y = Cw[ 1 ] / Cw[ 3 ];
                            Ct[ a, b, 0 ].z = Cw[ 2 ] / Cw[ 3 ];
                        }
                        /* calculate normal */
                        Vec3[] fder = CompFirstDerSurf4D ( n - 1, m - 1, p, q, U, V, Pw, u, v );
                        Ct[ a, b, 1 ] = fder[ 2 ].Cross ( fder[ 1 ] );
                        j += 6;
                        v += vd;
                    }

                    u += ud;
                }
            }
            /* return result */
            return Ct;
        }

        public static Vector3[ , ] SurfacePoints4D ( int n, int m, int p, int q, double[] U, double[] V, double[] Pw, int qfu, int qfv )
        {
            p--;		//order vars are no longer predecremented when passed in
            q--;		//this is consistent with other functions in this class
            double[] temp = new double[ ( q + 1 ) * 4 ];
            int Cn = ( 4 + n ) * qfu;
            double ud = ( U[ n ] - U[ p ] ) / ( Cn - 1 );
            int Cm = ( 4 + m ) * qfv;
            double vd = ( V[ m ] - V[ q ] ) / ( Cm - 1 );
            int[] spanus = new int[ Cn ];
            int[] spanvs = new int[ Cm ];
            Vector3[ , ] Ct = new Vector3[ Cn, Cm ];
            /*  employ linear variants of FindSpan() as they are much faster
                than a binary search; especially, since we calculate
                spans for all parameters in order */
            {
                double u = U[ p ];
                int spanu = p;
                int a = 0;

                //now calculates entire spanus within the loop (no longer copies the last value from the second to last, this was causing
                //tesselation issues with small sample sizes.
                for ( ; a < Cn; a++ ) {
                    if ( u > U[ p + 1 ] ) {
                        while ( u > U[ spanu + 1 ] )
                            spanu++;
                    }

                    spanus[ a ] = spanu;
                    u += ud;

                    //added - if u overreaches 1 then use smallest possible value less than one
                    //		- takes care of small precision problems
                    if ( u >= 1 )
                        u = 1 - System.Double.Epsilon;
                }

                //removed - last value is now calculated inside the loop, but from value kept less than one
                //spanus[ a ] = spanus[ a-1 ];
            }
            {
                double v = V[ q ];
                int spanv = q;
                int a = 0;

                for ( ; a < Cm; a++ ) {
                    if ( v > V[ q + 1 ] ) {
                        while ( v > V[ spanv + 1 ] )
                            spanv++;
                    }

                    spanvs[ a ] = spanv;
                    v += vd;

                    //added - if v overreaches 1 then use smallest possible value less than one
                    if ( v >= 1 )
                        v = 1 - System.Double.Epsilon;
                }

                //spanvs[ a ] = spanvs[ a-1 ];
            }
            {
                int j = 0;
                double u = U[ p ];

                for ( int a = 0; a < Cn; a++ ) {
                    int spanu = spanus[ a ];
                    double[] Nu = BasisFuns ( spanu, u, p, U );
                    int indu = spanu - p;
                    double v = V[ q ];

                    for ( int b = 0; b < Cm; b++ ) {
                        int spanv = spanvs[ b ];
                        double[] Nv = BasisFuns ( spanv, v, q, V );
                        {
                            int ti = 0;

                            for ( int l = 0; l <= q; l++ ) {
                                System.Array.Clear ( temp, l * 4, 4 );
                                int indv = spanv - q + l;

                                for ( int k = 0; k <= p; k++ ) {
                                    int i = ( ( ( indu + k ) * m ) + indv ) * 4;
                                    temp[ ti + 0 ] += Nu[ k ] * Pw[ i ] * Pw[ i + 3 ];
                                    temp[ ti + 1 ] += Nu[ k ] * Pw[ i + 1 ] * Pw[ i + 3 ];
                                    temp[ ti + 2 ] += Nu[ k ] * Pw[ i + 2 ] * Pw[ i + 3 ];
                                    temp[ ti + 3 ] += Nu[ k ] * Pw[ i + 3 ];
                                }

                                ti += 4;
                            }
                        }
                        {
                            double[] Cw = new double[ 4 ];
                            int ti = 0;

                            for ( int l = 0; l <= q; l++ ) {
                                /* was: Cw = Cw + Nv[ l ]*temp */
                                Cw[ 0 ] += Nv[ l ] * temp[ ti + 0 ];
                                Cw[ 1 ] += Nv[ l ] * temp[ ti + 1 ];
                                Cw[ 2 ] += Nv[ l ] * temp[ ti + 2 ];
                                Cw[ 3 ] += Nv[ l ] * temp[ ti + 3 ];
                                ti += 4;
                            }

                            /*j = (a*(*Cn)+b)*3;*/
                            Ct[ a, b ].x = ( float ) ( Cw[ 0 ] / Cw[ 3 ] );
                            Ct[ a, b ].y = ( float ) ( Cw[ 1 ] / Cw[ 3 ] );
                            Ct[ a, b ].z = ( float ) ( Cw[ 2 ] / Cw[ 3 ] );
                        }
                        j += 6;
                        v += vd;
                    }

                    u += ud;
                }
            }
            /* return result */
            return Ct;
        }


        /*  Generate and return a knot vector compatible with a curve specified as with "order", having "numControlPoints" control points.
            If "normalize" is true then vector range is from 0 to 1
            knot vector is clamped with "order" multiplicity at each end

            egs..............
            order = 3, numControlPoints = 6, normalize = false
          		-> 0,0,0,1,2,3,3,3
            order = 3, numControlPoints = 6, normalize = true
          		-> 0,0,0,0.333333333333333,0.666666666666667,1,1,1

        */

        public static double[] GenerateKnotVector ( int order, int numPoints )
        {
            double[] knotVector = new double[ numPoints + order ];

            for ( int i = 0; i < order; i++ )
                knotVector[ i ] = 0f;

            for ( int i = 0; i < numPoints - order; i++ )
                knotVector[ i + order ] = ( double ) ( i + 1 ) / ( double ) ( numPoints - order + 1 );

            for ( int i = 0; i < order; i++ )
                knotVector[ i + numPoints ] = 1f;

            return knotVector;
        }


        /*  ValidateKnotVector
            Checks "knotVector" is consistent with a curve "order" order and "numControlPoints" control points
            Check for :
            -right number of knots
            -no overlarge multiplicities
            -no decreasing knots
        */

        public static void ValidateKnotVector ( double[] knotVector, int order, int numControlPoints )
        {
            int knotCount = knotVector.Length;
            int mult_count = 1;

            if ( knotCount < ( numControlPoints + order ) )
                Debug.Log ( "NurbsLibCore.ValidateKnotVector: knot sequence has too few knots!" );

            if ( knotCount > ( numControlPoints + order ) )
                Debug.Log( "NurbsLibCore.ValidateKnotVector: knot sequence has too many knots!" );

            for ( int i = 0; i < ( knotCount - 1 ); i++ ) {
                if ( knotVector[ i ] == knotVector[ i + 1 ] ) {
                    mult_count++;

                    if ( mult_count > order )
                        Debug.Log( "NurbsLibCore.ValidateKnotVector: knot multiplicity higher than order!" );
                }
                else {
                    mult_count = 1;

                    if ( knotVector[ i ] > knotVector[ i + 1 ] )
                        Debug.Log( "NurbsLibCore.ValidateKnotVector: knot sequence has decreasing knots!" );
                }
            }
        }



        /*  Find the index of the next least value to u in  knotVector
            used to find out where to insert a knot
            if there is no value less than u then -1 is returned
        */

        public static int FindPositionInKnotVector ( double u, double[] knotVector )
        {
            int pos = 0;

            for ( int i = 0; i < knotVector.Length; i++ ) {
                if ( knotVector[ i ] < u )
                    pos++; else

                    break;
            }

            return pos - 1;
        }

        public static void NormaliseKnotVector ( double[] knotVector )
        {
            double min = knotVector[ 0 ];
            double range = knotVector[ knotVector.Length - 1 ] - min;

            for ( int i = 0; i < knotVector.Length; i++ )
                knotVector[ i ] = ( knotVector[ i ] - min ) / range;
        }

        public static void LUDecompose ( int n, double[] A, int[] pivot )
        {
            int errval;
            int i, j, k, l, kp1, nm1, sign;
            double t, q;
            double den = 0.0, ten = 0.0;
            double[] elem = new double[ n * n ];
            A.CopyTo ( elem, 0 );
            errval = 0;
            nm1 = n - 1;

            for ( k = 0; k < n; k++ )
                pivot[ k ] = k;

            sign = 1;

            if ( nm1 >= 1 ) {	/* AYAM[ non-trivial problem ]*/
                for ( k = 0; k < nm1; k++ ) {
                    kp1 = k + 1;
                    /*AYAM[ partial pivoting ROW exchanges */
                    /* -- search over column        ]*/
                    ten = Math.Abs ( A[ k * n + k ] );
                    l = k;

                    for ( i = kp1; i < n; i++ ) {
                        den = Math.Abs ( A[ i * n + k ] );

                        if ( den > ten ) {
                            ten = den;
                            l = i;
                        }
                    }

                    pivot[ k ] = l;

                    if ( elem[ l * n + k ] != 0.0 ) {
                        /*AYAM[ nonsingular pivot found */
                        /* interchange needed ]*/
                        if ( l != k ) {
                            for ( i = k; i < n; i++ ) {
                                t = elem[ l * n + i ];
                                elem[ l * n + i ] = elem[ k * n + i ];
                                elem[ k * n + i ] = t;
                            }

                            sign = -sign;
                        }

                        q = elem[ k * n + k ];	/* scale row */

                        for ( i = kp1; i < n; i++ ) {
                            t = - elem[ i * n + k ] / q;
                            elem[ i * n + k ] = t;

                            for ( j = kp1; j < n; j++ )
                                elem[ i * n + j ] += ( t * elem[ k * n + j ] );
                        }
                    }
                    else 		/* pivot singular */
                        errval = k;
                }
            }

            pivot[ nm1 ] = nm1;

            if ( elem[ nm1 * n + nm1 ] == 0.0 )
                errval = nm1;

            elem.CopyTo ( A, 0 );
            errval++;
            errval--;
//	Debug.Log(errval);
        }

        public static void LUInvert ( int n, double[] inv, int[] pivot )
        {
            double ten = 0;
            double[] work = new double[ n ];
            int i, j, k, l, kb, kp1, nm1;
            nm1 = n - 1;

            for ( k = 0; k < n; k++ ) {
                ten = 1.0 / inv[ k * n + k ];
                inv[ k * n + k ] = ten;
                ten = -ten;

                for ( i = 0; i < k; i++ )
                    inv[ i * n + k ] *= ten;

                kp1 = k + 1;

                if ( nm1 >= kp1 ) {
                    for ( j = kp1; j < n; j++ ) {
                        ten = inv[ k * n + j ];
                        inv[ k * n + j ] = 0.0;

                        for ( i = 0; i < kp1; i++ )
                            inv[ i * n + j ] += ( ten * inv[ i * n + k ] );
                    }
                }
            }

            if ( nm1 >= 1 ) {
                for ( kb = 0; kb < nm1; kb++ ) {
                    k = nm1 - kb - 1;
                    kp1 = k + 1;

                    for ( i = kp1; i < n; i++ ) {
                        work[ i ] = inv[ i * n + k ];
                        inv[ i * n + k ] = 0.0;
                    }

                    for ( j = kp1; j < n; j++ ) {
                        ten = work[ j ];

                        for ( i = 0; i < n; i++ )
                            inv[ i * n + k ] += ( ten * inv[ i * n + j ] );
                    }

                    l = pivot[ k ];

                    if ( l != k ) {
                        for ( i = 0; i < n; i++ ) {
                            ten = inv[ i * n + k ];
                            inv[ i * n + k ] = inv[ i * n + l ];
                            inv[ i * n + l ] = ten;
                        }
                    }
                }
            } /* if(nm >= 1) */
        }


        /*
            ay_nb_GlobalInterpolation4D: (NURBS++)
            interpolate the n+1 4D points in Q[] with
            n+1 precalculated parametric values in ub[]
            and n+d+1 knots in Uc[] with desired degree d (d <= n!)
        */
        public static void GlobalInterpolation4D ( int n, double[] Q, double[] ub, double[] Uc, int d )
        {
            int i, j, k, span, ind;
            int[] pivot = new int[ n + 1 ];
            double t;
            double[] A = new double[ ( n + 1 ) * ( n + 1 ) ];
            double[] U = Uc;
            double[] N;
            double[] qq = new double[ ( n + 1 ) * 4 ];
            double[] xx = new double[ ( n + 1 ) * 4 ];

            // AYAM[ Fill A ]
            for ( i = 1; i < n; i++ ) {
                span = FindSpan ( n, d, ub[ i ], U );
                N = BasisFuns ( span, ub[ i ], d, U );

                for ( j = 0; j <= d; j++ ) {
                    ind = ( i * ( n + 1 ) ) + ( span - d + j );
                    A[ ind ] = N[ j ];
                }
            }

            //Debug.Log(Util.DoublesToString(A));
            A[ 0 ] = 1.0;
            A[ n * ( n + 1 ) + n ] = 1.0;
            LUDecompose ( n + 1, A, pivot );
            LUInvert ( n + 1, A, pivot );
            //AYAM[ Init matrix for LSE ]
            //memcpy(qq,Q,(n+1)*4*sizeof(double));
            Q.CopyTo ( qq, 0 );

            for ( i = 0; i < ( n + 1 ); i++ ) {
                for ( j = 0; j < 4; j++ ) {
                    t = 0.0;

                    for ( k = 0; k < ( n + 1 ); k++ )
                        t += ( A[ i * ( n + 1 ) + k ] * qq[ k * 4 + j ] );

                    xx[ i * 4 + j ] = t;
                }
            }

            //AYAM[ Store the results ]
            xx.CopyTo ( Q, 0 );
        }


        /*
            ay_nb_InsertKnotSurfV:
            insert knot v into surface stride, w, h, q, VP, Pw
            r times
            VQ: new knots, Qw: new controls (both allocated outside!)
        */

        public static void InsertKnotSurfaceV ( int stride, int w, int h, int q, double[] VP, double[] Pw,
                                                double v, int k, int s, int r, double[] VQ, double[] Qw )
        {
            w--;		//in ayam, these were expected to be pre decremented
            h--;
            int i, j, L, col, vl, nh, h1 = h + 1;
            int ai, i1, i2;
            double[] alpha = new double[ ( r + 1 ) * ( q - s + 1 ) ];
            double[] Rw = new double[ ( q - s + 1 ) * stride ];
            nh = h + r + 1;
            /* Load new knot vector. */
            vl = h + q + 1;

            for ( i = 0; i <= k; i++ )
                VQ[ i ] = VP[ i ];

            for ( i = 1; i <= r; i++ )
                VQ[ k + i ] = v;

            for ( i = k + 1; i <= vl; i++ )
                VQ[ i + r ] = VP[ i ];

            /* Save the alphas. */
            for ( j = 1; j <= r; j++ ) {
                L = k - q + j;

                for ( i = 0; i <= q - j - s; i++ ) {
                    ai = j * ( q - s ) + i;
                    alpha[ ai ] = ( v - VP[ L + i ] ) / ( VP[ i + k + 1 ] - VP[ L + i ] );
                } /* for */
            } /* for */

            /* For each column... */
            for ( col = 0; col <= w; col++ ) {
                /* Save unaltered control points. */
                for ( i = 0; i <= k - q; i++ ) {
                    i1 = ( col * nh + i ) * stride;
                    i2 = ( col * h1 + i ) * stride;
                    System.Array.Copy ( Pw, i2, Qw, i1, stride );
                }

                for ( i = k - s; i <= h; i++ ) {
                    i1 = ( col * nh + i + r ) * stride;
                    i2 = ( col * h1 + i ) * stride;
                    System.Array.Copy ( Pw, i2, Qw, i1, stride );
                }

                /* Load auxiliary control points. */
                for ( i = 0; i <= q - s; i++ ) {
                    i1 = i * stride;
                    i2 = ( col * h1 + ( k - q + i ) ) * stride;
                    System.Array.Copy ( Pw, i2, Rw, i1, stride );
                    /* convert euclidean rational to homogeneous */
                    Rw[ i1 ] *= Rw[ i1 + 3 ];
                    Rw[ i1 + 1 ] *= Rw[ i1 + 3 ];
                    Rw[ i1 + 2 ] *= Rw[ i1 + 3 ];
                }

                /* Insert the knot r times. */
                for ( j = 1; j <= r; j++ ) {
                    L = k - q + j;

                    for ( i = 0; i <= q - j - s; i++ ) {
                        ai = j * ( q - s ) + i;
                        i1 = i * stride;
                        i2 = ( i + 1 ) * stride;
                        Rw[ i1 ] = alpha[ ai ] * Rw[ i2 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 ];
                        Rw[ i1 + 1 ] = alpha[ ai ] * Rw[ i2 + 1 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 1 ];
                        Rw[ i1 + 2 ] = alpha[ ai ] * Rw[ i2 + 2 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 2 ];
                        //Debug.Log("Rw[ i1+2 ]b" + Rw[ i1+2 ]);
                        //Debug.Log("alpha[ ai ]" + alpha[ ai ]);
                        //Debug.Log("1.0-alpha[ ai ]" + (1.0-alpha[ ai ]));

                        //Debug.Log(
                        if ( stride > 3 )
                            Rw[ i1 + 3 ] = alpha[ ai ] * Rw[ i2 + 3 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 3 ];
                    } /* for */

                    i1 = ( col * nh + L ) * stride;
                    System.Array.Copy ( Rw, 0, Qw, i1, stride );
                    /* convert homogeneous rational to euclidean */
                    Qw[ i1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];
//Debug.Log(Util.DoublesToString(Qw));
                    i1 = ( col * nh + ( k + r - j - s ) ) * stride;
                    i2 = ( q - j - s ) * stride;
                    System.Array.Copy ( Rw, i2, Qw, i1, stride );
                    /* convert homogeneous rational to euclidean */
                    Qw[ i1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];

                    /* Load the remaining control points. */
                    for ( i = L + 1; i < k - s; i++ ) {
                        /*Qw[ col ][ i ] = Rw[ i-L ];*/
                        i1 = ( col * nh + i ) * stride;
                        i2 = ( i - L ) * stride;
                        System.Array.Copy ( Rw, i2, Qw, i1, stride );
                        /* convert homogeneous rational to euclidean */
                        Qw[ i1 ] /= Qw[ i1 + 3 ];
                        Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                        Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];
                    } /* for */
                } /* for */
            } /* for */
        }


        /*
            ay_nb_InsertKnotSurfU:
            insert knot u into surface stride, w, h, p, UP, Pw
            r times
            UQ: new knots, Qw: new controls (both allocated outside!)
        */

        public static void InsertKnotSurfaceU ( int stride, int w, int h, int p, double[] UP, double[] Pw,
                                                double u, int k, int s, int r, double[] UQ, double[] Qw )
        {
            w--;
            h--;
            int i, j, L, row, ul, h1 = h + 1;
            int ai, i1, i2;
            double[] alpha = new double[ ( r + 1 ) * ( p - s + 1 ) ];
            double[] Rw = new double[ ( p - s + 1 ) * stride ];
            /* Load new knot vector. */
            ul = w + p + 1;

            for ( i = 0; i <= k; i++ )
                UQ[ i ] = UP[ i ];

            for ( i = 1; i <= r; i++ )
                UQ[ k + i ] = u;

            for ( i = k + 1; i <= ul; i++ )
                UQ[ i + r ] = UP[ i ];

            /* Save the alphas. */
            for ( j = 1; j <= r; j++ ) {
                L = k - p + j;

                for ( i = 0; i <= p - j - s; i++ ) {
                    ai = j * ( p - s ) + i;
                    alpha[ ai ] = ( u - UP[ L + i ] ) / ( UP[ i + k + 1 ] - UP[ L + i ] );
                } /* for */
            } /* for */

            /* For each row... */
            for ( row = 0; row <= h; row++ ) {
                /* Save unaltered control points. */
                for ( i = 0; i <= k - p; i++ ) {
                    i1 = ( i * h1 + row ) * stride;
                    i2 = ( i * h1 + row ) * stride;
                    System.Array.Copy ( Pw, i2, Qw, i1, stride );
                }

                for ( i = k - s; i <= w; i++ ) {
                    i1 = ( ( i + r ) * h1 + row ) * stride;
                    i2 = ( i * h1 + row ) * stride;
                    System.Array.Copy ( Pw, i2, Qw, i1, stride );
                }

                for ( i = 0; i <= p - s; i++ ) {
                    i1 = i * stride;
                    i2 = ( ( k - p + i ) * h1 + row ) * stride;
                    System.Array.Copy ( Pw, i2, Rw, i1, stride );
                    /* convert euclidean rational to homogeneous */
                    Rw[ i1 ] *= Rw[ i1 + 3 ];
                    Rw[ i1 + 1 ] *= Rw[ i1 + 3 ];
                    Rw[ i1 + 2 ] *= Rw[ i1 + 3 ];
                }

                /* Insert the knot r times. */
                for ( j = 1; j <= r; j++ ) {
                    L = k - p + j;

                    for ( i = 0; i <= p - j - s; i++ ) {
                        ai = j * ( p - s ) + i;
                        i1 = i * stride;
                        i2 = ( i + 1 ) * stride;
                        Rw[ i1 ] = alpha[ ai ] * Rw[ i2 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 ];
                        Rw[ i1 + 1 ] = alpha[ ai ] * Rw[ i2 + 1 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 1 ];
                        Rw[ i1 + 2 ] = alpha[ ai ] * Rw[ i2 + 2 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 2 ];

                        if ( stride > 3 )
                            Rw[ i1 + 3 ] = alpha[ ai ] * Rw[ i2 + 3 ] + ( 1.0 - alpha[ ai ] ) * Rw[ i1 + 3 ];
                    } /* for */

                    i1 = ( L * h1 + row ) * stride;
                    System.Array.Copy ( Rw, 0, Qw, i1, stride );
                    /* convert homogeneous rational to euclidean */
                    Qw[ i1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];
                    i1 = ( ( k + r - j - s ) * h1 + row ) * stride;
                    i2 = ( p - j - s ) * stride;
                    System.Array.Copy ( Rw, i2, Qw, i1, stride );
                    /* convert homogeneous rational to euclidean */
                    Qw[ i1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                    Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];

                    /* Load the remaining control points. */
                    for ( i = L + 1; i < k - s; i++ ) {
                        i1 = ( i * h1 + row ) * stride;
                        i2 = ( i - L ) * stride;
                        System.Array.Copy ( Rw, i2, Qw, i1, stride );
                        /* convert homogeneous rational to euclidean */
                        Qw[ i1 ] /= Qw[ i1 + 3 ];
                        Qw[ i1 + 1 ] /= Qw[ i1 + 3 ];
                        Qw[ i1 + 2 ] /= Qw[ i1 + 3 ];
                    } /* for */
                } /* for */
            } /* for */
        }

        public static double[] SwapArray ( double[] controlPoints, int stride, int controlPointsU, int controlPointsV )
        {
            int i1, i2, i, j;
            double[] newControlPoints = new double[ controlPointsU * controlPointsV * stride ];
            i1 = 0;

            for ( i = 0; i < controlPointsU; i++ ) {
                i2 = i * stride;

                for ( j = 0; j < controlPointsV; j++ ) {
                    System.Array.Copy ( controlPoints, i1, newControlPoints, i2, stride );
                    i1 += stride;
                    i2 += ( controlPointsU * stride );
                }
            }

            return newControlPoints;
        }

        public static double GetCurvature ( NurbsCurve nc, double u )
        {
            return GetCurvature ( nc.numControlPoints, nc.order - 1, nc.knotVector, NurbsLibCore.PointsToArray3D ( nc.points ), u );
        }

        public static double GetCurvature ( int n, int p, double[] U, double[] Pw, double u )
        {
            mmVector3 vel, acc;
            double velsqrlen, numer, denom;
            vel = ComputeFirstDerivative3D ( n, p, U, Pw, u );
            velsqrlen = ( vel[ 0 ] * vel[ 0 ] ) + ( vel[ 1 ] * vel[ 1 ] ) + ( vel[ 2 ] * vel[ 2 ] );

            if ( velsqrlen > double.Epsilon ) {
                acc = new mmVector3 ( ComputeSecondDerivative3D ( n, p, U, Pw, u ) );
                numer = mmVector3.Cross ( vel, acc ).magnitude;
                denom = Math.Pow ( velsqrlen, 1.5 );
                return ( numer / denom );
            }
            else
                return 0;
        }

        public static mmVector3 GetCurvatureVector ( NurbsCurve nc, double u )
        {
            return GetCurvatureVector ( nc.numControlPoints, nc.order - 1, nc.knotVector, NurbsLibCore.PointsToArray3D ( nc.points ), u );
        }

        public static mmVector3 GetCurvatureVector ( int n, int p, double[] U, double[] Pw, double u )
        {
            //p--; decrementing necessary unless p=degree
            mmVector3 vel, acc;
            double velsqrlen;
            vel = ComputeFirstDerivative3D ( n, p, U, Pw, u );
            velsqrlen = ( vel[ 0 ] * vel[ 0 ] ) + ( vel[ 1 ] * vel[ 1 ] ) + ( vel[ 2 ] * vel[ 2 ] );

            if ( velsqrlen > double.Epsilon ) {
                acc = new mmVector3 ( ComputeSecondDerivative3D ( n, p, U, Pw, u ) );
                return mmVector3.Cross ( vel, acc );
                //return vel;
            }
            else
                return mmVector3.zero;
        }

        public static double[] ComputeFirstDerivative3D ( int n, int p, double[] U, double[] P, double u )
        {
            //FIXME this is a patch - u=1 doesn't work
            u = Math.Min ( u, .999999999999 );
            int span = 0, j, r;
            double[] nders;// = new double((p+1) * (p+1));
            double[] C1 = new double[ 3 ];
            span = FindSpan ( n, p, u, U );
            nders = DersBasisFuns ( span, u, p, 1, U );
            C1[ 0 ] = 0.0;
            C1[ 1 ] = 0.0;
            C1[ 2 ] = 0.0;

            for ( j = 0; j <= p; j++ ) {
                r = ( span - p + j ) * 3;
                C1[ 0 ] = C1[ 0 ] + nders[ ( p + 1 ) + j ] * P[ r ];
                C1[ 1 ] = C1[ 1 ] + nders[ ( p + 1 ) + j ] * P[ r + 1 ];
                C1[ 2 ] = C1[ 2 ] + nders[ ( p + 1 ) + j ] * P[ r + 2 ];
            }

            return C1;
        }

        public static double[]  ComputeSecondDerivative3D ( int n, int p, double[] U, double[] P, double u )
        {
            //FIXME this is a patch - u=1 doesn't work
            u = Math.Min ( u, .999999999999 );
            int span = 0, j, r;
            double[] nders;// = new double((p+1) * (p+1));
            double[] C2 = new double[ 3 ];
            span = FindSpan ( n, p, u, U );
            nders = DersBasisFuns ( span, u, p, 2, U );
            C2[ 0 ] = 0.0;
            C2[ 1 ] = 0.0;
            C2[ 2 ] = 0.0;

            for ( j = 0; j <= p; j++ ) {
                r = ( span - p + j ) * 3;
                C2[ 0 ] = C2[ 0 ] + nders[ ( ( p + 1 ) * 2 ) + j ] * P[ r ];
                C2[ 1 ] = C2[ 1 ] + nders[ ( ( p + 1 ) * 2 ) + j ] * P[ r + 1 ];
                C2[ 2 ] = C2[ 2 ] + nders[ ( ( p + 1 ) * 2 ) + j ] * P[ r + 2 ];
            }

            return C2;
        }

        public static double[] ComputeFirstDerivative4D ( int n, int p, double[] U, double[] Pw, double u )
        {
            //FIXME this is a patch - u=1 doesn't work
            u = Math.Min ( u, .999999999999 );
            int span = 0, j, k;
            double[] nders;// = new double((p+1) * (p+1));
            double[] C0 = new double[ 3 ];
            double[] C1 = new double[ 3 ];
            double wder0 = 0.0, wder1 = 0.0;
            span = FindSpan ( n, p, u, U );
            nders = DersBasisFuns ( span, u, p, 1, U );

            for ( j = 0; j <= p; j++ ) {
                k = ( span - p + j ) * 4;
                C0[ 0 ] = C0[ 0 ] + nders[ j ] * ( Pw[ k ] * Pw[ k + 3 ] );
                C0[ 1 ] = C0[ 1 ] + nders[ j ] * ( Pw[ k + 1 ] * Pw[ k + 3 ] );
                C0[ 2 ] = C0[ 2 ] + nders[ j ] * ( Pw[ k + 2 ] * Pw[ k + 3 ] );
                wder0 = wder0 + nders[ j ] * ( Pw[ k + 3 ] );
                C1[ 0 ] = C1[ 0 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k ] * Pw[ k + 3 ] );
                C1[ 1 ] = C1[ 1 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k + 1 ] * Pw[ k + 3 ] );
                C1[ 2 ] = C1[ 2 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k + 2 ] * Pw[ k + 3 ] );
                wder1 = wder1 + nders[ ( p + 1 ) + j ] * ( Pw[ k + 3 ] );
            }

            C0[ 0 ] /= wder0;
            C0[ 1 ] /= wder0;
            C0[ 2 ] /= wder0;
            C1[ 0 ] = C1[ 0 ] - wder1 * C0[ 0 ];
            C1[ 1 ] = C1[ 1 ] - wder1 * C0[ 1 ];
            C1[ 2 ] = C1[ 2 ] - wder1 * C0[ 2 ];
            C1[ 0 ] /= wder0;
            C1[ 1 ] /= wder0;
            C1[ 2 ] /= wder0;
            return C1;
        }

        public static double[] ComputeSecondDerivative4D ( int n, int p, double[] U, double[] Pw, double u )
        {
            //FIXME this is a patch - u=1 doesn't work
            u = Math.Min ( u, .999999999999 );
            int span, j, k;
            double[] nders;
            double wder0 = 0.0, wder1 = 0.0, wder2 = 0.0;
            double[] C0 = new double[ 3 ];
            double[] C1 = new double[ 3 ];
            double[] C2 = new double[ 3 ];
            span = FindSpan ( n, p, u, U );
            nders = DersBasisFuns ( span, u, p, 2, U );

            for ( j = 0; j <= p; j++ ) {
                k = ( span - p + j ) * 4;
                C0[ 0 ] = C0[ 0 ] + nders[ j ] * ( Pw[ k ] * Pw[ k + 3 ] );
                C0[ 1 ] = C0[ 1 ] + nders[ j ] * ( Pw[ k + 1 ] * Pw[ k + 3 ] );
                C0[ 2 ] = C0[ 2 ] + nders[ j ] * ( Pw[ k + 2 ] * Pw[ k + 3 ] );
                wder0 = wder0 + nders[ j ] * ( Pw[ k + 3 ] );
                C1[ 0 ] = C1[ 0 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k ] * Pw[ k + 3 ] );
                C1[ 1 ] = C1[ 1 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k + 1 ] * Pw[ k + 3 ] );
                C1[ 2 ] = C1[ 2 ] + nders[ ( p + 1 ) + j ] * ( Pw[ k + 2 ] * Pw[ k + 3 ] );
                wder1 = wder1 + nders[ ( p + 1 ) + j ] * ( Pw[ k + 3 ] );
                C2[ 0 ] = C2[ 0 ] + nders[ ( p + 1 ) * 2 + j ] * ( Pw[ k ] * Pw[ k + 3 ] );
                C2[ 1 ] = C2[ 1 ] + nders[ ( p + 1 ) * 2 + j ] * ( Pw[ k + 1 ] * Pw[ k + 3 ] );
                C2[ 2 ] = C2[ 2 ] + nders[ ( p + 1 ) * 2 + j ] * ( Pw[ k + 2 ] * Pw[ k + 3 ] );
                wder2 = wder2 + nders[ ( p + 1 ) * 2 + j ] * ( Pw[ k + 3 ] );
            } /* for */

            C0[ 0 ] /= wder0;
            C0[ 1 ] /= wder0;
            C0[ 2 ] /= wder0;
            /* k == 1 */
            C1[ 0 ] = C1[ 0 ] - wder1 * C0[ 0 ];
            C1[ 1 ] = C1[ 1 ] - wder1 * C0[ 1 ];
            C1[ 2 ] = C1[ 2 ] - wder1 * C0[ 2 ];
            C1[ 0 ] /= wder0;
            C1[ 1 ] /= wder0;
            C1[ 2 ] /= wder0;
            /* k == 2 */
            C2[ 0 ] = C2[ 0 ] - 2.0 * wder1 * C1[ 0 ];
            C2[ 1 ] = C2[ 1 ] - 2.0 * wder1 * C1[ 1 ];
            C2[ 2 ] = C2[ 2 ] - 2.0 * wder1 * C1[ 2 ];
            C2[ 0 ] = C2[ 0 ] - wder2 * C0[ 0 ];
            C2[ 1 ] = C2[ 1 ] - wder2 * C0[ 1 ];
            C2[ 2 ] = C2[ 2 ] - wder2 * C0[ 2 ];
            C2[ 0 ] /= wder0;
            C2[ 1 ] /= wder0;
            C2[ 2 ] /= wder0;
            return C2;
        }

        public static double[] InvertKnots ( double[] knots )
        {
            for ( var i = 0; i < knots.Length; i++ )
                knots[ i ] = 1 - knots[ i ];

            return knots;
        }

        public static double[] PointsToArray3D ( List<mmVector3> points )
        {
            var d3 = new double[ points.Count * 3 ];
            int j = 0;

			for ( int i = 0; i < points.Count; i++ ) {
                d3[ j++ ] = points[ i ].x;
                d3[ j++ ] = points[ i ].y;
                d3[ j++ ] = points[ i ].z;
            }

            return d3;
        }

        public static double[] PointsAndWeightsToArray4D ( List<mmVector3> points, List<double> weights )
        {
            var d4 = new double[ points.Count * 4 ];
            int j = 0;

			if ( weights.Count != points.Count ) { // no weights, or something wrong with them -> set weight to 1
				for ( int i = 0; i < points.Count; i++ ) {
                    d4[ j++ ] = points[ i ].x;
                    d4[ j++ ] = points[ i ].y;
                    d4[ j++ ] = points[ i ].z;
                    d4[ j++ ] = 1;
                }
            }
            else {
				for ( int i = 0; i < points.Count; i++ ) {
                    d4[ j++ ] = points[ i ].x;
                    d4[ j++ ] = points[ i ].y;
                    d4[ j++ ] = points[ i ].z;
                    d4[ j++ ] = weights[ i ];
                }
            }

            return d4;
        }

        public static mmVector3[] Array4DToPoints ( double[] points4D )
        {
            int length = points4D.Length / 4;
            var controlpoints = new mmVector3[ length ];
            int j = 0;

            for ( int i = 0; i < length; i++ ) {
                controlpoints[ i ] = new mmVector3 ( points4D[ j++ ], points4D[ j++ ], points4D[ j++ ] );
                j++;
            }

            return controlpoints;
        }

        public static void Array4DToPointsAndWeights ( NurbsCurve curve, double[] points4D )
        {
            int length = points4D.Length / 4;
			var controlpoints = new List<mmVector3> ( length );
            var weights = new List<double> ( length );
            int j = 0;

            for ( int i = 0; i < length; i++ ) {
                controlpoints.Add ( new mmVector3 ( points4D[ j++ ], points4D[ j++ ], points4D[ j++ ] ) );
                weights.Add ( points4D[ j++ ] );
            }

            curve.points = controlpoints;
            curve.pointWeights = weights;
        }

        public static void Array4DToPointsAndWeights ( NurbsPatch patch, double[] points4D )
        {
            int length = points4D.Length / 4;
            var controlpoints = new List<mmVector3> ( length );
            var weights = new List<double> ( length );
            int j = 0;

            for ( int i = 0; i < length; i++ ) {
                controlpoints.Add ( new mmVector3 ( points4D[ j++ ], points4D[ j++ ], points4D[ j++ ] ) );
                weights.Add ( points4D[ j++ ] );
            }

            patch.points = controlpoints;
            patch.pointWeights = weights;
        }


        public static List<double> Array4DToWeights ( double[] points4D )
        {
            int length = points4D.Length / 4;
            var weights = new List<double> ( length );

            for ( int i = 0; i < length; i++ )
                weights.Add ( points4D[ i * 4 + 3 ] );

            return weights;
        }

        public static string DoublesToString ( double[] D )
        {
            if ( D != null ) {
                string s = "";
                s += ":";

                foreach ( double d in D ) {
                    s += d;
                    s += ",";
                }

                return s;
            }
            else {
                Debug.Log ( "Array of doubles in null" );
                return null;
            }
        }

        public static string DoubleVectorArrayToString ( Vector3[] V )
        {
            string s = "";
            s += ":";

            if ( V != null )
                foreach ( Vector3 v in V )
                    s += ":::" + v.x + "," + v.y + "," + v.z;

            return s;
        }
		
		public static string DoubleVectorArrayToString ( mmVector3[] V )
		{
			string s = "";
			s += ":";
			
			if ( V != null )
				foreach ( mmVector3 v in V )
					s += ":::" + v.x + "," + v.y + "," + v.z;
			
			return s;
		}


		public static string DoubleVectorListToString ( List<mmVector3> V )
		{
			string s = "";
			s += ":";
			
			if ( V != null )
				foreach ( mmVector3 v in V )
					s += ":::" + v.x + "," + v.y + "," + v.z;
			
			return s;
		}

        public static void RefSwapDoubleArrays ( ref double[] a1, ref double[] a2 )
        {
            double[] temp = a1;
            a1 = a2;
            a2 = temp;
        }

    }




}