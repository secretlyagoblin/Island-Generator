using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityNURBS.Types
{

    public class mmVector3 : ICloneable
    {
        public const double kEpsilon = double.Epsilon;
        public double x;
        public double y;
        public double z;

        public mmVector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public mmVector3 ( mmVector3 _value )
        {
            this.x = _value.x;
            this.y = _value.y;
            this.z = _value.z;
        }

        public mmVector3 ( Vector3 _value )
        {
            this.x = _value.x;
            this.y = _value.y;
            this.z = _value.z;
        }
		
		public mmVector3 ( double x, double y, double z )
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public mmVector3 ( Color color )
		{
			this.x = color.r;
			this.y = color.g;
			this.z = color.b;
		}

        // for conversion of json vectors
        public mmVector3 ( Int32[] xyz )
        {
            this.x = System.Convert.ToDouble ( xyz[ 0 ] );
            this.y = System.Convert.ToDouble ( xyz[ 1 ] );
            this.z = System.Convert.ToDouble ( xyz[ 2 ] );
        }

        public object Clone()
        {
            return new mmVector3 ( x, y, z );
        }


        public void Set ( double _x, double _y, double _z )
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public void Set ( mmVector3 vector )
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public double this[ int index ]
        {
            get {
                switch ( index ) {
                    case 0: {
                        return this.x;
                    }

                    case 1: {
                        return this.y;
                    }

                    case 2: {
                        return this.z;
                    }

                    default: {
                        throw new IndexOutOfRangeException ( "out of range Vector3 index" );
                    }
                }
            }
            set {
                switch ( index ) {
                    case 0: {
                        this.x = value;
                        break;
                    }

                    case 1: {
                        this.y = value;
                        break;
                    }

                    case 2: {
                        this.z = value;
                        break;
                    }

                    default: {
                        throw new IndexOutOfRangeException ( "out of range Vector3 index" );
                    }
                }
            }
        }


        public void MoveTo ( mmVector3 to )
        {
            this.x = to.x;
            this.y = to.y;
            this.z = to.z;
        }

        public float r
        {
            get { return ( float ) this.x; }
            set { x = ( double ) value; }
        }
        public float g
        {
            get { return ( float ) this.y; }
            set { y = ( double ) value; }
        }
        public float b
        {
            get { return ( float ) this.z; }
            set { z = ( double ) value; }
        }

        public mmVector3 normalized
        {
            get {
                return mmVector3.Normalize ( this );
            }
        }
        public double magnitude
        {
            get {
                return Math.Sqrt ( this.x * this.x + this.y * this.y + this.z * this.z );
            }
        }

        public double sqrMagnitude
        {
            get {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public static mmVector3 zero
        {
            get {
                return new mmVector3 ( 0, 0, 0 );
            }
        }

        public static mmVector3 one
        {
            get {
                return new mmVector3 ( 1, 1, 1 );
            }
        }

        public static mmVector3 forward
        {
            get {
                return new mmVector3 ( 0, 0, 1 );
            }
        }
        public static mmVector3 back
        {
            get {
                return new mmVector3 ( 0, 0, -1 );
            }
        }
        public static mmVector3 up
        {
            get {
                return new mmVector3 ( 0, 1, 0 );
            }
        }
        public static mmVector3 down
        {
            get {
                return new mmVector3 ( 0, -1, 0 );
            }
        }
        public static mmVector3 left
        {
            get {
                return new mmVector3 ( -1, 0, 0 );
            }
        }
        public static mmVector3 right
        {
            get {
                return new mmVector3 ( 1, 0, 0 );
            }
        }

        public static mmVector3 Scale ( mmVector3 a, mmVector3 b )
        {
            return new mmVector3 ( a.x * b.x, a.y * b.y, a.z * b.z );
        }

        public mmVector3 Lerp ( mmVector3 a, double t )
        {
            return this + t * ( a - this );
        }

        public void Scale ( mmVector3 vector )
        {
            this.x *= vector.x;
            this.y *= vector.y;
            this.z *= vector.z;
        }

        public void Add ( mmVector3 vector )
        {
            this.x += vector.x;
            this.y += vector.y;
            this.z += vector.z;
        }

        public static mmVector3 Cross ( mmVector3 lhs, mmVector3 rhs )
        {
            return new mmVector3 ( lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x );
        }

        public mmVector3 Cross ( mmVector3 other )
        {
            return new mmVector3 ( this.y * other.z - this.z * other.y, this.z * other.x - this.x * other.z, this.x * other.y - this.y * other.x );
        }

        public override bool Equals ( object other )
        {
            if ( ! ( other is mmVector3 ) )
                return false;

            mmVector3 vector = ( mmVector3 ) other;
            return this.x.Equals ( vector.x ) && this.y.Equals ( vector.y ) && this.z.Equals ( vector.z );
        }


        public static mmVector3 Reflect ( mmVector3 inDirection, mmVector3 inNormal )
        {
            return -2f * mmVector3.Dot ( inNormal, inDirection ) * inNormal + inDirection;
        }

        public static mmVector3 Normalize ( mmVector3 value )
        {
            double num = mmVector3.Magnitude ( value );

            if ( num > kEpsilon )
                return value / num;

            return mmVector3.zero;
        }

        public void Normalize ()
        {
            double num = mmVector3.Magnitude ( this );

            if ( num > kEpsilon ) {
                mmVector3.Normalize ( this );
                //this /= num;
            }
            else {
                x = 0;
                y = 0;
                z = 0;
            }
        }

        public double[] ToArray ()
        {
            return new double[] {this.x, this.y, this.z};
        }
		
		public override string ToString ()
		{
			return string.Format ( "({0:F1}, {1:F1}, {2:F1})", this.x, this.y, this.z );
		}

		public string ToString ( int precision )
		{
			return string.Format ( "({0:F" + precision + "}, {1:F" + precision + "}, {2:F" + precision + "})", this.x, this.y, this.z );
		}

        public string ToString ( string format )
        {
            return string.Format ( "({0}, {1}, {2})", this.x.ToString ( format ), this.y.ToString ( format ), this.z.ToString ( format ) );
        }


        public static double Dot ( mmVector3 lhs, mmVector3 rhs )
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }
        public double Dot ( mmVector3 other )
        {
            return this.x * other.x + this.y * other.y + this.z * other.z;
        }

        public static mmVector3 Project ( mmVector3 vector, mmVector3 onNormal )
        {
            double num = mmVector3.Dot ( onNormal, onNormal );

            if ( num < kEpsilon )
                return mmVector3.zero;

            return onNormal * mmVector3.Dot ( vector, onNormal ) / num;
        }

        public static mmVector3 Exclude ( mmVector3 excludeThis, mmVector3 fromThat )
        {
            return fromThat - mmVector3.Project ( fromThat, excludeThis );
        }


        public static double Distance ( mmVector3 a, mmVector3 b )
        {
            mmVector3 vector = new mmVector3 ( a.x - b.x, a.y - b.y, a.z - b.z );
            return Math.Sqrt ( vector.x * vector.x + vector.y * vector.y + vector.z * vector.z );
        }

        public static mmVector3 ClampMagnitude ( mmVector3 vector, float maxLength )
        {
            if ( vector.sqrMagnitude > maxLength * maxLength )
                return vector.normalized * maxLength;

            return vector;
        }


        public static double Magnitude ( mmVector3 a )
        {
            return Math.Sqrt ( a.x * a.x + a.y * a.y + a.z * a.z );
        }

        public static double SqrMagnitude ( mmVector3 a )
        {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }


        public static mmVector3 Min ( mmVector3 lhs, mmVector3 rhs )
        {
            return new mmVector3 ( Math.Min ( lhs.x, rhs.x ), Math.Min ( lhs.y, rhs.y ), Math.Min ( lhs.z, rhs.z ) );
        }
        public static mmVector3 Max ( mmVector3 lhs, mmVector3 rhs )
        {
            return new mmVector3 ( Math.Max ( lhs.x, rhs.x ), Math.Max ( lhs.y, rhs.y ), Math.Max ( lhs.z, rhs.z ) );
        }

        public static mmVector3 operator + ( mmVector3 a, mmVector3 b )
        {
            return new mmVector3 ( a.x + b.x, a.y + b.y, a.z + b.z );
        }

        public mmVector3 Plus ( mmVector3 other )
        {
            return new mmVector3 ( this.x + other.x, this.y + other.y, this.z + other.z );
        }

        public static mmVector3 operator - ( mmVector3 a, mmVector3 b )
        {
            return new mmVector3 ( a.x - b.x, a.y - b.y, a.z - b.z );
        }

        public mmVector3 Minus ( mmVector3 other )
        {
            return new mmVector3 ( this.x - other.x, this.y - other.y, this.z - other.z );
        }

        public static mmVector3 operator - ( mmVector3 a )
        {
            return new mmVector3 ( -a.x, -a.y, -a.z );
        }

        public mmVector3 Negated ()
        {
            return new mmVector3 ( -this.x, -this.y, -this.z );
        }

        public static mmVector3 operator * ( mmVector3 a, double d )
        {
            return new mmVector3 ( a.x * d, a.y * d, a.z * d );
        }

        public static mmVector3 operator * ( double d, mmVector3 a )
        {
            return new mmVector3 ( a.x * d, a.y * d, a.z * d );
        }

        public mmVector3 Times ( double other )
        {
            return new mmVector3 ( this.x * other, this.y * other, this.z * other );
        }

        public static mmVector3 operator * ( mmVector3 a, mmVector3 b )
        {
            return new mmVector3 ( a.x * b.x, a.y * b.y, a.z * b.z );
        }

        public static mmVector3 operator / ( mmVector3 a, double d )
        {
            return new mmVector3 ( a.x / d, a.y / d, a.z / d );
        }
        public static mmVector3 operator / ( mmVector3 a, mmVector3 b )
        {
            return new mmVector3 ( a.x / b.x, a.y / b.y, a.z / b.z );
        }

        public mmVector3 DividedBy ( double other )
        {
            return new mmVector3 ( this.x / other, this.y / other, this.z / other );
        }

        public static bool operator == ( mmVector3 lhs, mmVector3 rhs )
        {
            if ( Math.Abs ( lhs.x - rhs.x ) > kEpsilon ) return false;

            if ( Math.Abs ( lhs.y - rhs.y ) > kEpsilon ) return false;

            if ( Math.Abs ( lhs.z - rhs.z ) > kEpsilon ) return false;

            return true;
//		return Vector.SqrMagnitude (lhs - rhs) < 9.99999944E-11f;
        }

        public static bool operator != ( mmVector3 lhs, mmVector3 rhs )
        {
            return ! ( lhs == rhs );
//		return Vector.SqrMagnitude (lhs - rhs) >= 9.99999944E-11f;
        }





        /*
        	public Vector3 ToVector3() {
        		return new Vector3((float)x,(float)y,(float)z);
        	}
        */

        // internal conversions
        public static implicit operator Vector3 ( mmVector3 myParam )
        {
            return new Vector3 ( ( float ) myParam.x, ( float ) myParam.y, ( float ) myParam.z );
        }
        public static implicit operator Color ( mmVector3 colorAsVector )
        {
            return new Color ( ( float ) colorAsVector.x, ( float ) colorAsVector.y, ( float ) colorAsVector.z );
		}
		public static implicit operator Color32 ( mmVector3 colorAsVector )
		{
			return new Color32 ( ( byte ) ( colorAsVector.x * 255 ), ( byte ) ( colorAsVector.y * 255 ), ( byte ) ( colorAsVector.z * 255 ), 255 );
		}
		public static implicit operator Single[] ( mmVector3 vector )
		{
			return new Single[] { (Single)vector.x, (Single)vector.y, (Single)vector.z };
		}


        public static implicit operator mmVector3 ( double[] myParam )
        {
            return new mmVector3 ( myParam[ 0 ], myParam[ 1 ], myParam[ 2 ] );
        }
		public static implicit operator mmVector3 ( Vector3 vec3 )
		{
			return new mmVector3 ( vec3 );
		}
		public static implicit operator mmVector3 ( Color color )
		{
			return new mmVector3 ( color );
		}


    }

}