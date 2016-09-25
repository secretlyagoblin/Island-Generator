using System;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace UnityNURBS.Types {
	
public struct mmVector4
{
	
	public double x;
	public double y;
	public double z;
	public double w;
	
	public mmVector4 (double x, double y, double z, double w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
	
	public static double Dot (mmVector4 a, mmVector4 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}		
	
	public static double SqrMagnitude (mmVector4 a)
	{
		return mmVector4.Dot (a, a);
	}
	
	public static bool operator == (mmVector4 lhs, mmVector4 rhs)
	{
		return mmVector4.SqrMagnitude (lhs - rhs) < 9.99999944E-11f;
	}
	public static bool operator != (mmVector4 lhs, mmVector4 rhs)
	{
		return mmVector4.SqrMagnitude (lhs - rhs) >= 9.99999944E-11f;
	}
	
	public static mmVector4 operator + (mmVector4 a, mmVector4 b)
	{
		return new mmVector4 (a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}
	public static mmVector4 operator - (mmVector4 a, mmVector4 b)
	{
		return new mmVector4 (a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}
	public static mmVector4 operator - (mmVector4 a)
	{
		return new mmVector4 (-a.x, -a.y, -a.z, -a.w);
	}
	public static mmVector4 operator * (mmVector4 a, double d)
	{
		return new mmVector4 (a.x * d, a.y * d, a.z * d, a.w * d);
	}
	public static mmVector4 operator * (double d, mmVector4 a)
	{
		return new mmVector4 (a.x * d, a.y * d, a.z * d, a.w * d);
	}
	public static mmVector4 operator / (mmVector4 a, double d)
	{
		return new mmVector4 (a.x / d, a.y / d, a.z / d, a.w / d);
	}

}

public struct mmMatrix4
{
	public double m00; //11
	public double m10; //21
	public double m20; //31
	public double m30; //41
	public double m01; //12
	public double m11; //22
	public double m21; //32
	public double m31; //42
	public double m02; //13
	public double m12; //23
	public double m22; //33
	public double m32; //43
	public double m03; //14
	public double m13; //24
	public double m23; //34
	public double m33; //44
	public double this[int row, int column]
	{
		get
		{
			return this[row + column * 4];
		}
		set
		{
			this[row + column * 4] = value;
		}
	}
	public double this[int index]
	{
		get
		{
			switch (index)
			{
			case 0:

				{
					return this.m00;
				}
			case 1:

				{
					return this.m10;
				}
			case 2:

				{
					return this.m20;
				}
			case 3:

				{
					return this.m30;
				}
			case 4:

				{
					return this.m01;
				}
			case 5:

				{
					return this.m11;
				}
			case 6:

				{
					return this.m21;
				}
			case 7:

				{
					return this.m31;
				}
			case 8:

				{
					return this.m02;
				}
			case 9:

				{
					return this.m12;
				}
			case 10:

				{
					return this.m22;
				}
			case 11:

				{
					return this.m32;
				}
			case 12:

				{
					return this.m03;
				}
			case 13:

				{
					return this.m13;
				}
			case 14:

				{
					return this.m23;
				}
			case 15:

				{
					return this.m33;
				}
			default:

				{
					throw new IndexOutOfRangeException ("Invalid matrix index!");
				}
			}
		}
		set
		{
			switch (index)
			{
			case 0:

				{
					this.m00 = value;
					break;
				}
			case 1:

				{
					this.m10 = value;
					break;
				}
			case 2:

				{
					this.m20 = value;
					break;
				}
			case 3:

				{
					this.m30 = value;
					break;
				}
			case 4:

				{
					this.m01 = value;
					break;
				}
			case 5:

				{
					this.m11 = value;
					break;
				}
			case 6:

				{
					this.m21 = value;
					break;
				}
			case 7:

				{
					this.m31 = value;
					break;
				}
			case 8:

				{
					this.m02 = value;
					break;
				}
			case 9:

				{
					this.m12 = value;
					break;
				}
			case 10:

				{
					this.m22 = value;
					break;
				}
			case 11:

				{
					this.m32 = value;
					break;
				}
			case 12:

				{
					this.m03 = value;
					break;
				}
			case 13:

				{
					this.m13 = value;
					break;
				}
			case 14:

				{
					this.m23 = value;
					break;
				}
			case 15:

				{
					this.m33 = value;
					break;
				}
			default:

				{
					throw new IndexOutOfRangeException ("Invalid matrix index!");
				}
			}
		}
	}
	
/*
	 * public static Vector RotateX (Vector v) {
	
		return new Vector();
		
		
	}
*/	
	
	
/*	
	public Matrix4 inverse
	{
		get
		{
			return Matrix4.Inverse (this);
		}
	}
	public Matrix4 transpose
	{
		get
		{
			return Matrix4.Transpose (this);
		}
	}
	
	*/
	/*
	public extern bool isIdentity
	{
		[WrapperlessIcall ]
		[MethodImpl (MethodImplOptions.InternalCall)]
		get;
	}*/
	public static mmMatrix4 zero
	{
		get
		{
			mmMatrix4 result;
			result.m00 = 0;
			result.m01 = 0;
			result.m02 = 0;
			result.m03 = 0;
			result.m10 = 0;
			result.m11 = 0;
			result.m12 = 0;
			result.m13 = 0;
			result.m20 = 0;
			result.m21 = 0;
			result.m22 = 0;
			result.m23 = 0;
			result.m30 = 0;
			result.m31 = 0;
			result.m32 = 0;
			result.m33 = 0;
			return result;
		}
	}
	public static mmMatrix4 identity
	{
		get
		{
			mmMatrix4 result;
			result.m00 = 1;
			result.m01 = 0;
			result.m02 = 0;
			result.m03 = 0;
			result.m10 = 0;
			result.m11 = 1;
			result.m12 = 0;
			result.m13 = 0;
			result.m20 = 0;
			result.m21 = 0;
			result.m22 = 1;
			result.m23 = 0;
			result.m30 = 0;
			result.m31 = 0;
			result.m32 = 0;
			result.m33 = 1;
			return result;
		}
	}
	
	public mmMatrix4 SetAll(double v00,double v01,double v02,double v03,
						double v10,	double v11,double v12,double v13,
						double v20,double v21,double v22,double v23,
						double v30,double v31,double v32,double v33) {
		m00 = v00;	m10 = v10; m20 = v20; m30 = v30;
		m01 = v01;	m11 = v11; m21 = v21; m31 = v31;
		m02 = v02;	m12 = v12; m22 = v22; m32 = v32;
		m03 = v03;	m13 = v13; m23 = v23; m33 = v33;
		return this;
	}

	public mmMatrix4 SetScale(double x, double y, double z) {
		SetAll(
				x, 0, 0, 0,
				0, y, 0, 0,
				0, 0, z, 0,
				0, 0, 0, 1
			);
		return this;	
	}
	
	public mmMatrix4 Scale(double x, double y, double z) {
		
		var scalemat = new mmMatrix4();
		
		scalemat.SetAll(
				x, 0, 0, 0,
				0, y, 0, 0,
				0, 0, z, 0,
				0, 0, 0, 1
			);
	
		this = this * scalemat;
		
		return this;	
	}
	
	public mmMatrix4 SetTranslation(double x, double y, double z) {
		
		SetAll(
				1, 0, 0, x,
				0, 1, 0, y,
				0, 0, 1, z,
				0, 0, 0, 1
			);
	
		return this;
	}
	
	public mmMatrix4 Translate(double x, double y, double z) {
		
		var transmat = new mmMatrix4();
		
		transmat.SetAll(
				1, 0, 0, x,
				0, 1, 0, y,
				0, 0, 1, z,
				0, 0, 0, 1
			);
	
		this = this * transmat;
		
		return this;
	}
	
	
	
	
	public const string X_AXIS = "X_AXIS";
	public const string Y_AXIS = "Y_AXIS";
	public const string Z_AXIS = "Z_AXIS";
	
	
	public void Rotate(string axis,double angle) {
		mmMatrix4 rotmat = new mmMatrix4();
		double c = Math.Cos(angle);
		double s = Math.Sin(angle);
		switch (axis) {
			case X_AXIS:
				rotmat.SetAll(
				1, 0,  0, 0,
				0, c, -s, 0,
				0, s,  c, 0,
				0, 0,  0, 1
				);
			break;
			case Y_AXIS:
				rotmat.SetAll(
				 c, 0, s, 0,
				 0, 1, 0, 0,
				-s, 0, c, 0,
				 0, 0, 0, 1
				);
			break;
			default :
			case Z_AXIS:
				rotmat.SetAll(
				c, -s, 0, 0,
				s, c,  0, 0,
				0, 0,  1, 0,
				0, 0,  0, 1
				);
			break;
		
		
		}
		this = this * rotmat;
	}
	
	public void SetRotation(string axis,double angle) {
		double c = Math.Cos(angle);
		double s = Math.Sin(angle);
		
		switch (axis) {
			case X_AXIS:
				SetAll(
				1, 0,  0, 0,
				0, c, -s, 0,
				0, s,  c, 0,
				0, 0,  0, 1
				);
			break;
			case Y_AXIS:
				SetAll(
				 c, 0, s, 0,
				 0, 1, 0, 0,
				-s, 0, c, 0,
				 0, 0, 0, 1
				);
			break;
			default :
			case Z_AXIS:
				SetAll(
				c, -s, 0, 0,
				s, c,  0, 0,
				0, 0,  1, 0,
				0, 0,  0, 1
				);
			break;
			
			
			
		}
	}
	
	public override int GetHashCode ()
	{
		return this.GetColumn (0).GetHashCode () ^ this.GetColumn (1).GetHashCode () << 2 ^ this.GetColumn (2).GetHashCode () >> 2 ^ this.GetColumn (3).GetHashCode () >> 1;
	}
	public override bool Equals (object other)
	{
		if (!(other is mmMatrix4))
		{
			return false;
		}
		mmMatrix4 matrix4x = (mmMatrix4)other;
		return this.GetColumn (0).Equals (matrix4x.GetColumn (0)) && this.GetColumn (1).Equals (matrix4x.GetColumn (1)) && this.GetColumn (2).Equals (matrix4x.GetColumn (2)) && this.GetColumn (3).Equals (matrix4x.GetColumn (3));
	}
/*	
	public static Matrix4 Inverse (Matrix4 m)
	{
		//return Matrix4x4.INTERNAL_CALL_Inverse (ref m);
		return new Matrix4();
	}
	
	//private static extern Matrix4x4 INTERNAL_CALL_Inverse (ref Matrix4x4 m);
	
	public static Matrix4 Transpose (Matrix4 m)
	{
		//return Matrix4x4.INTERNAL_CALL_Transpose (ref m);
		return new Matrix4();
	}
	
	//private static extern Matrix4x4 INTERNAL_CALL_Transpose (ref Matrix4x4 m);
	
	//internal static bool Invert (Matrix4 inMatrix, out Matrix4 dest)
//	{
		//return Matrix4x4.INTERNAL_CALL_Invert (ref inMatrix, out dest);
//		return false;
//	}
	
	//private static extern bool INTERNAL_CALL_Invert (ref Matrix4x4 inMatrix, out Matrix4x4 dest);
*/	
	public mmVector4 GetColumn (int i)
	{
		return new mmVector4 (this[0, i], this[1, i], this[2, i], this[3, i]);
	}
	
	public mmVector4 GetRow (int i)
	{
		return new mmVector4 (this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
	}
	
	public void SetColumn (int i, mmVector4 v)
	{
		this[0, i] = v.x;
		this[1, i] = v.y;
		this[2, i] = v.z;
		this[3, i] = v.w;
	}
	public void SetRow (int i, mmVector4 v)
	{
		this[i, 0] = v.x;
		this[i, 1] = v.y;
		this[i, 2] = v.z;
		this[i, 3] = v.w;
	}
	
	public mmVector3 MultiplyPoint (mmVector3 v)
	{
		mmVector3 result = new mmVector3();
		result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
		result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
		result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
		double num = this.m30 * v.x + this.m31 * v.y + this.m32 * v.z + this.m33;
		num = 1f / num;
		result.x *= num;
		result.y *= num;
		result.z *= num;
		return result;
	}
	
	public mmVector3 MultiplyPoint3x4 (mmVector3 v)
	{
		mmVector3 result = new mmVector3();
		result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
		result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
		result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
		return result;
	}
	
	public mmVector3 MultiplyVector (mmVector3 v)
	{
		mmVector3 result = new mmVector3();
		result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z;
		result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z;
		result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z;
		return result;
	}
	
	public static mmMatrix4 Scale (mmVector3 v)
	{
		mmMatrix4 result;
		result.m00 = v.x;
		result.m01 = 0;
		result.m02 = 0;
		result.m03 = 0;
		result.m10 = 0;
		result.m11 = v.y;
		result.m12 = 0;
		result.m13 = 0;
		result.m20 = 0;
		result.m21 = 0;
		result.m22 = v.z;
		result.m23 = 0;
		result.m30 = 0;
		result.m31 = 0;
		result.m32 = 0;
		result.m33 = 1;
		return result;
	}
	
	public override string ToString ()
	{
		return string.Format ("{0:F5}\t{1:F5}\t{2:F5}\t{3:F5}\n{4:F5}\t{5:F5}\t{6:F5}\t{7:F5}\n{8:F5}\t{9:F5}\t{10:F5}\t{11:F5}\n{12:F5}\t{13:F5}\t{14:F5}\t{15:F5}\n", new object[]
		{
			this.m00,
			this.m01,
			this.m02,
			this.m03,
			this.m10,
			this.m11,
			this.m12,
			this.m13,
			this.m20,
			this.m21,
			this.m22,
			this.m23,
			this.m30,
			this.m31,
			this.m32,
			this.m33
		});
	}
	public string ToString (string format)
	{
		return string.Format ("{0}\t{1}\t{2}\t{3}\n{4}\t{5}\t{6}\t{7}\n{8}\t{9}\t{10}\t{11}\n{12}\t{13}\t{14}\t{15}\n", new object[]
		{
			this.m00.ToString (format),
			this.m01.ToString (format),
			this.m02.ToString (format),
			this.m03.ToString (format),
			this.m10.ToString (format),
			this.m11.ToString (format),
			this.m12.ToString (format),
			this.m13.ToString (format),
			this.m20.ToString (format),
			this.m21.ToString (format),
			this.m22.ToString (format),
			this.m23.ToString (format),
			this.m30.ToString (format),
			this.m31.ToString (format),
			this.m32.ToString (format),
			this.m33.ToString (format)
		});
	}
	
	//public static extern Matrix4x4 Ortho (float left, float right, float bottom, float top, float zNear, float zFar);
	
	//public static extern Matrix4x4 Perspective (float fov, float aspect, float zNear, float zFar);
	public static mmMatrix4 operator * (mmMatrix4 lhs, mmMatrix4 rhs)
	{
		mmMatrix4 result;
		result.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
		result.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
		result.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
		result.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;
		result.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
		result.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
		result.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
		result.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;
		result.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
		result.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
		result.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
		result.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;
		result.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
		result.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
		result.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
		result.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
		return result;
	}
	
	public static mmVector4 operator * (mmMatrix4 lhs, mmVector4 v)
	{
		mmVector4 result;
		result.x = lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z + lhs.m03 * v.w;
		result.y = lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z + lhs.m13 * v.w;
		result.z = lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z + lhs.m23 * v.w;
		result.w = lhs.m30 * v.x + lhs.m31 * v.y + lhs.m32 * v.z + lhs.m33 * v.w;
		return result;
	}
	public static bool operator == (mmMatrix4 lhs, mmMatrix4 rhs)
	{
		return lhs.GetColumn (0) == rhs.GetColumn (0) && lhs.GetColumn (1) == rhs.GetColumn (1) && lhs.GetColumn (2) == rhs.GetColumn (2) && lhs.GetColumn (3) == rhs.GetColumn (3);
	}
	public static bool operator != (mmMatrix4 lhs, mmMatrix4 rhs)
	{
		return !(lhs == rhs);
	}
	
	

	public Matrix4x4 ToMatrix4x4() {
		
		Matrix4x4 matrix = new Matrix4x4();
		
		matrix.m00 = (float)m00;
		matrix.m01 = (float)m01;
		matrix.m02 = (float)m02;
		matrix.m03 = (float)m03;
		
		matrix.m10 = (float)m10;
		matrix.m11 = (float)m11;
		matrix.m12 = (float)m12;
		matrix.m13 = (float)m13;
		
		matrix.m20 = (float)m20;
		matrix.m21 = (float)m21;
		matrix.m22 = (float)m22;
		matrix.m23 = (float)m23;
		
		matrix.m30 = (float)m30;
		matrix.m31 = (float)m31;
		matrix.m32 = (float)m32;
		matrix.m33 = (float)m33;
		
		return matrix;
		
	}
	
	
	public static implicit operator Matrix4x4(mmMatrix4 matrix) {
		return matrix.ToMatrix4x4();
		
	}
	
	public static implicit operator mmMatrix4(Matrix4x4 matrix) {
		return new mmMatrix4(matrix);
	}
	
	
	public mmMatrix4(Matrix4x4 matrix) {
		
		this.m00 = matrix.m00;
		this.m01 = matrix.m01;
		this.m02 = matrix.m02;
		this.m03 = matrix.m03;
		
		this.m10 = matrix.m10;
		this.m11 = matrix.m11;
		this.m12 = matrix.m12;
		this.m13 = matrix.m13;
		
		this.m20 = matrix.m20;
		this.m21 = matrix.m21;
		this.m22 = matrix.m22;
		this.m23 = matrix.m23;
		
		this.m30 = matrix.m30;
		this.m31 = matrix.m31;
		this.m32 = matrix.m32;
		this.m33 = matrix.m33;
			
	}
	
}
}