using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace UnityNURBS.Types
{

    public class TypeManager
    {
        public static bool IsList ( object obj )
        {
            if ( obj == null ) return false;

            if ( obj is List<bool> ) return true;

            if ( obj is List<int> ) return true;

            if ( obj is List<double> ) return true;

            if ( obj is List<mmVector3> ) return true;

            if ( obj is List<string> ) return true;

            if ( obj is mmBooleanList ) return true;

            if ( obj is mmFloatList ) return true;

            if ( obj is mmVectorList ) return true;

            /*

            if ( obj is mmStringList ) return true;

            if ( obj is mmFlexiBoolean ) {
                mmFlexiBoolean tmp = ( mmFlexiBoolean ) obj;

                if ( tmp.isList )
                    return true;
            }

            if ( obj is mmFlexiFloat ) {
                mmFlexiFloat tmp = ( mmFlexiFloat ) obj;

                if ( tmp.isList )
                    return true;
            }

            if ( obj is mmFlexiVector ) {
                mmFlexiVector tmp = ( mmFlexiVector ) obj;

                if ( tmp.isList )
                    return true;
            }

            if ( obj is mmFlexiString ) {
                mmFlexiString tmp = ( mmFlexiString ) obj;

                if ( tmp.isList )
                    return true;
            }

            */

            return false;
        }


        public static bool Is1D ( object obj )
        {
            if ( obj == null ) return false;

            if ( obj is mmVector3 ) return false;

            if ( obj is List<mmVector3> ) return false;

            if ( obj is mmVectorList ) return false;

            //if ( obj is mmFlexiVector ) return false;

            return true;
        }


        public static bool IsNumerical ( object obj )
        {
            if ( obj == null ) return false;

            if ( obj is bool ) return true;

            if ( obj is List<bool> ) return true;

            if ( obj is int ) return true;

            if ( obj is List<int> ) return true;

            if ( obj is double ) return true;

            if ( obj is List<double> ) return true;

            if ( obj is mmVector3 ) return true;

            if ( obj is List<mmVector3> ) return true;

            if ( obj is mmBooleanList ) return true;

            //if ( obj is mmFlexiBoolean ) return true;

            if ( obj is mmFloatList ) return true;

            if ( obj is mmFlexiFloat ) return true;

            if ( obj is mmVectorList ) return true;

           // if ( obj is mmFlexiVector ) return true;

            return false;
        }


        /*
        	public static List<object> ObjectToObjectList(object obj) {
            		List<object> list = new List<object>();

        		if (obj is List<bool>))
        			foreach(bool item in (List<bool>)obj)
        				list.Add((object)item);
        		if (obj is List<int>))
        			foreach(int item in (List<int>)obj)
        				list.Add((object)item);
        		if (obj is List<double>))
        			foreach(double item in (List<double>)obj)
        				list.Add((object)item);
        		if (obj is List<Vector>))
        			foreach(Vector item in (List<Vector>)obj)
        				list.Add((object)item);
        		if (obj is List<string>))
        			foreach(string item in (List<string>)obj)
        				list.Add((object)item);

        		return list;
        	}
        */

        public static int CountFromObjectList ( object obj )
        {
            if ( obj is List<bool> ) {
                List<bool> tmp = ( List<bool> ) obj;
                return tmp.Count;
            }

            if ( obj is List<int> ) {
                List<int> tmp = ( List<int> ) obj;
                return tmp.Count;
            }

            if ( obj is List<double> ) {
                List<double> tmp = ( List<double> ) obj;
                return tmp.Count;
            }

            if ( obj is List<mmVector3> ) {
                List<mmVector3> tmp = ( List<mmVector3> ) obj;
                return tmp.Count;
            }

            if ( obj is List<string> ) {
                List<string> tmp = ( List<string> ) obj;
                return tmp.Count;
            }

            if ( obj is mmBooleanList ) {
                mmBooleanList tmp = ( mmBooleanList ) obj;
                return tmp.Count;
            }

            if ( obj is mmFloatList ) {
                mmFloatList tmp = ( mmFloatList ) obj;
                return tmp.Count;
            }

            if ( obj is mmVectorList ) {
                mmVectorList tmp = ( mmVectorList ) obj;
                return tmp.Count;
            }

            /*

            if ( obj is mmStringList ) {
                mmStringList tmp = ( mmStringList ) obj;
                return tmp.Count;
            }

            if ( obj is mmFlexiBoolean ) {
                mmFlexiBoolean tmp = ( mmFlexiBoolean ) obj;
                return tmp.Count;
            }

            if ( obj is mmFlexiFloat ) {
                mmFlexiFloat tmp = ( mmFlexiFloat ) obj;
                return tmp.Count;
            }

            if ( obj is mmFlexiVector ) {
                mmFlexiVector tmp = ( mmFlexiVector ) obj;
                return tmp.Count;
            }

            if ( obj is mmFlexiString ) {
                mmFlexiString tmp = ( mmFlexiString ) obj;
                return tmp.Count;
            }

    */

            return 0;
        }


        public static List<double> ObjectToDoubleList ( object obj )
        {
            var list = new List<double>();

            if ( obj is List<bool> ) {
                list.Capacity = ( ( List<bool> ) obj ).Count;

                foreach ( bool item in ( List<bool> ) obj )
                    list.Add ( item ? 1 : 0 );
            }

            else if ( obj is List<int> ) {
                list.Capacity = ( ( List<int> ) obj ).Count;

                foreach ( int item in ( List<int> ) obj )
                    list.Add ( Convert.ToDouble ( item ) );
            }

            else if ( obj is List<double> )
                list = ( List<double> ) obj;

            return list;
        }

        public static List<int> ObjectToIntegerList ( object obj )
        {
            var list = new List<int>();

            if ( obj is List<bool> ) {
                list.Capacity = ( ( List<bool> ) obj ).Count;

                foreach ( bool item in ( List<bool> ) obj )
                    list.Add ( item ? 1 : 0 );
            }

            else if ( obj is List<int> )
                list = ( List<int> ) obj;

            else if ( obj is List<double> ) {
                list.Capacity = ( ( List<double> ) obj ).Count;

                foreach ( int item in ( List<double> ) obj )
                    list.Add ( Convert.ToInt32 ( item ) );
            }

            return list;
        }

        public static List<int> ObjectArrayToIntegerList ( object[] obj )
        {
            var list = new List<int> ( obj.Length );

            foreach ( var item in obj )
                list.Add ( ( int ) item );

            return list;
        }


        public static List<double> ObjectArrayToDoubleList ( object[] from )
        {
            var to = new List<double> ( from.Length );

            foreach ( var item in from )
                to.Add ( Convert.ToDouble ( item ) );

            return to;
        }
        public static List<double> IntArrayToDoubleList ( int[] from )
        {
            var to = new List<double> ( from.Length );

            foreach ( var item in from )
                to.Add ( ( double ) item );

            return to;
        }


        public static double ObjectToDouble ( object obj )
        {
            double value = 0;

            if ( obj is bool )
                value = ( ( bool ) obj ? 1 : 0 );

            else if ( obj is int )
                value = Convert.ToDouble ( ( int ) obj );

            else if ( obj is double )
                value = ( double ) obj;

            return value;
        }


        public static double ObjectToInt ( object obj )
        {
            int value = 0;

            if ( obj is bool )
                value = ( ( bool ) obj ? 1 : 0 );

            else if ( obj is int )
                value = Convert.ToInt32 ( obj );

            else if ( obj is double )
                value = Convert.ToInt32 ( ( int ) obj );

            return value;
        }



        // FIXME: these should ideally be methods of mm-type parameters, like mmFlexiFloat.ConvertToJavascript()
        // detects property type and does according conversion to arrays for return to browser
        public static object FormatForBrowser ( object property )
        {
            return FormatForBrowser ( property, false );
        }

        // FIXME: these should ideally be methods of mm-type parameters, like mmFlexiFloat.ConvertToJavascript()
        // detects property type and does according conversion to arrays for return to browser
        public static object FormatForBrowser ( object property, bool noList )
        {
            object propertyValue = new object();

            if ( property is bool || property is int || property is double || property is string )
                propertyValue = property;
			else if ( property is mmVector3 ) {
                propertyValue = ( ( mmVector3 ) property ).ToArray();
            }
            else if ( property is List<bool> ) {
                if ( ( ( List<bool> ) property ).Count > 0 )
                    propertyValue = ( ( List<bool> ) property ).ToArray();
                else propertyValue = new bool[ 0 ];
            }
            else if ( property is List<int> ) {
                if ( ( ( List<int> ) property ).Count > 0 )
                    propertyValue = ( ( List<int> ) property ).ToArray();
                else propertyValue = new int[ 0 ];
            }
            else if ( property is List<double> ) {
                if ( ( ( List<double> ) property ).Count > 0 )
                    propertyValue = ( ( List<double> ) property ).ToArray();
                else propertyValue = new double[ 0 ];
            }
            else if ( property is List<mmVector3> ) {
                var vl = ( List<mmVector3> ) property;
                int count = vl.Count;

                if ( count > 0 ) {
                    double[][] vectorsAsArrays = new double[ count ][];

                    for ( int c = 0; c < count; c++ ) {
                        vectorsAsArrays[ c ] = new double[ 3 ];
                        vectorsAsArrays[ c ][ 0 ] = vl[ c ].x;
                        vectorsAsArrays[ c ][ 1 ] = vl[ c ].y;
                        vectorsAsArrays[ c ][ 2 ] = vl[ c ].z;
                    }

                    propertyValue = vectorsAsArrays;
                }
                else propertyValue = new double[ 0 ][];
            }
            else if ( property is List<string> ) {
                if ( ( ( List<string> ) property ).Count > 0 )
                    propertyValue = ( ( List<string> ) property ).ToArray();
                else propertyValue = new string[ 0 ];
            }
            else if ( property is mmBooleanList ) {
                var bl = ( mmBooleanList ) property;

                if ( bl.Count > 0 )
                    propertyValue = ( bl.list ).ToArray();
                else propertyValue = new bool[ 0 ];
            }
            /*
            else if ( property is mmFlexiBoolean ) {
                var fb = ( mmFlexiBoolean ) property;

                if ( fb.isList && fb.Count > 0 )
                    if ( noList )
                        propertyValue = fb.list[ 0 ];
                    else
                        propertyValue = ( fb.list ).ToArray();
                else if ( !fb.isList )
                    propertyValue = fb.state;
                else propertyValue = new bool[ 0 ];
            }
            else if ( property is mmIntegerList ) {
                var il = ( mmIntegerList ) property;

                if ( il.Count > 0 )
                    propertyValue = ( il.list ).ToArray();
                else propertyValue = new int[ 0 ];
            }
            else if ( property is mmFlexiInteger ) {
                var fi = ( mmFlexiInteger ) property;

                if ( fi.isList && fi.Count > 0 )
                    if ( noList )
                        propertyValue = fi.list[ 0 ];
                    else
                        propertyValue = ( fi.list ).ToArray();
                else if ( !fi.isList )
                    propertyValue = fi.integer;
                else propertyValue = new int[ 0 ];
            }
            */
            else if ( property is mmFloatList ) {
                var fl = ( mmFloatList ) property;

                if ( fl.Count > 0 )
                    propertyValue = ( fl.list ).ToArray();
                else propertyValue = new double[ 0 ];
            }
            /*
            else if ( property is mmFlexiFloat ) {
                var ff = ( mmFlexiFloat ) property;

                if ( ff.isList && ff.Count > 0 )
                    if ( noList )
                        propertyValue = ff.list[ 0 ];
                    else
                        propertyValue = ( ff.list ).ToArray();
                else if ( !ff.isList )
                    propertyValue = ff.scalar;
                else propertyValue = new double[ 0 ];
            }
            */
            else if ( property is mmVectorList ) {
                var vl = ( mmVectorList ) property;
                int count = vl.Count;

                if ( count > 0 ) {
                    double[][] vectorsAsArrays = new double[ count ][];

                    for ( int c = 0; c < count; c++ ) {
                        vectorsAsArrays[ c ] = new double[ 3 ];
                        vectorsAsArrays[ c ][ 0 ] = vl[ c ].x;
                        vectorsAsArrays[ c ][ 1 ] = vl[ c ].y;
                        vectorsAsArrays[ c ][ 2 ] = vl[ c ].z;
                    }

                    propertyValue = vectorsAsArrays;
                }
                else propertyValue = new double[ 0 ][];
            }

            /*
            
            else if ( property is mmFlexiVector ) {
                var fv = ( mmFlexiVector ) property;
                int count = fv.Count;

                if ( fv.isList && count > 0 ) {
                    if ( noList )
                        propertyValue = fv.list[ 0 ].ToArray();
                    else {
                        var vectorsAsArrays = new double[ count ][];

                        for ( int c = 0; c < count; c++ ) {
                            vectorsAsArrays[ c ] = new double[ 3 ];
                            vectorsAsArrays[ c ][ 0 ] = fv[ c ].x;
                            vectorsAsArrays[ c ][ 1 ] = fv[ c ].y;
                            vectorsAsArrays[ c ][ 2 ] = fv[ c ].z;
                        }

                        propertyValue = vectorsAsArrays;
                    }
                }
                
                else if ( !fv.isList )
                    propertyValue = fv.vector.ToArray(); else propertyValue = new double[ 0 ][];
            }
            else if ( property is mmStringList ) {
                var sl = ( mmStringList ) property;

                if ( sl.Count > 0 )
                    propertyValue = ( sl.list ).ToArray();
                else propertyValue = new string[ 0 ];
            }
            else if ( property is mmFlexiString ) {
                var fs = ( mmFlexiString ) property;

                if ( fs.isList && fs.Count > 0 )
                    propertyValue = ( fs.list ).ToArray();
                else if ( !fs.isList )
                    propertyValue = fs.text;
                else propertyValue = new string[ 0 ];
            }
            */
            else {
                propertyValue = null;
                Debug.Log ( "Conversion from type 'object' to " + property.GetType().Name + " not handled yet (TypeManager.FormatForBrowser) - returning NULL." );
            }

            return propertyValue;
        }



        public static double ConvertObjectToDouble ( object obj )
        {
            return ConvertObjectToDouble ( obj, "" );
        }

        public static double ConvertObjectToDouble ( object obj, string what )
        {
            if ( obj == null ) {
                Debug.Log(( what == "" ? "" : what + ": " ) + "Trying to convert NULL value to a Double. Returning 0." );
                return 0;
            }
			else if ( obj is Double[] ) {
				double tmp = ((Double[])obj)[0];
                Debug.Log( ( what == "" ? "" : what + ": " ) + "Converted Double[] [ " + obj + " ] to a Double[ " + tmp + " ]." );
				return tmp;
			}

            try {
                double tmp = Convert.ToDouble ( obj );
                Debug.Log( ( what == "" ? "" : what + ": " ) + "Converted object[ " + obj + " ] to a Double[ " + tmp + " ]." );
                return tmp;
            }
            catch ( FormatException ) {
                Debug.Log( ( what == "" ? "" : what + ": " ) + "The " + obj.GetType().Name + " value '" + obj + "' is not recognised as a valid Double value. Returning 0." );
                return 0;
            }
            catch ( InvalidCastException ) {
                Debug.Log( ( what == "" ? "" : what + ": " ) + "Conversion of the " + obj.GetType().Name + " value '" + obj + "' to a Double is not supported. Returning 0." );
                return 0;
            }
        }

        /*

        public static FieldInfo FindField ( Operators.Operator op, string fieldName )
        {
            foreach ( FieldInfo fieldInfo in op.GetType().GetFields ( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) )
                if ( fieldInfo.Name == fieldName )
                    return fieldInfo;

            return null;
        }

    */


    }

}