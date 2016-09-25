using UnityEngine;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace UnityNURBS.Types
{

    public class mmFloatList : mmType, ImmType
    {
        public List<double> list = new List<double>();

        public int Capacity
        {
            set {
                list.Capacity = value;
            }
        }

        private void Init()
        {
            isVector = false;
            isNumeric = true;
            isList = true;
        }

        public mmFloatList()
        {
            Init();
        }

        public mmFloatList ( int size )
        {
            Init();
            list.Capacity = size;
        }

        public mmFloatList ( double value, int size )
        {
            Init();
            list = new List<double> ( Enumerable.Repeat ( value, size ) );
        }

        public mmFloatList ( mmFloatList _from )
        {
            Init();
            list = new List<double> ( _from.list );
        }

        public mmFloatList ( List<double> _list )
        {
            Init();
            list = new List<double> ( _list );
        }


        // mainly for conversion from browser values
        public mmFloatList ( object _from )
        {
            Init();

            if ( _from == null ) {}
            else if ( _from is mmFloatList )
                list = new List<double> ( ( ( mmFloatList ) _from ).list ); else if ( _from is Boolean ) { // bool

                list = new List<double>() { ( bool ) _from ? ( double ) 1 : ( double ) 0 };
            }
            else if ( _from is Int32 ) {  // int
                list = new List<double>() { Convert.ToDouble ( _from ) };
            }
            else if ( _from is Double ) {  // double
                list = new List<double>() { ( double ) _from };
            }
            else if ( _from is Boolean[] ) {  // list of booleans
                list.Capacity = ( ( bool[] ) _from ).Length;

                foreach ( bool bl in ( bool[] ) _from )
                    list.Add ( bl ? ( double ) 1 : ( double ) 0 );
            }
            else if ( _from is List<Boolean> ) {  // list of booleans
                list.Capacity = ( ( List<Boolean> ) _from ).Count;

                foreach ( bool bl in ( List<Boolean> ) _from )
                    list.Add ( bl ? ( double ) 1 : ( double ) 0 );
            }
            else if ( _from is Int32[] ) {  // list of ints
                list.Capacity = ( ( Int32[] ) _from ).Length;

                foreach ( Int32 int32 in ( Int32[] ) _from )
                    list.Add ( Convert.ToDouble ( int32 ) );
            }
            else if ( _from is List<Int32> ) {  // list of ints
                list.Capacity = ( ( List<Int32> ) _from ).Count;

                foreach ( Int32 int32 in ( List<Int32> ) _from )
                    list.Add ( Convert.ToDouble ( int32 ) );
            }
            else if ( _from is Double[] )    // list of doubles
                list = new List<double> ( ( Double[] ) _from ); else if ( _from is List<Double> ) // list of doubles

                list = new List<double> ( ( List<Double> ) _from ); else if ( _from is object[] ) { // mix of ints and doubles

                list.Capacity = ( ( object[] ) _from ).Length;

                foreach ( object item in ( object[] ) _from )
                    list.Add ( TypeManager.ConvertObjectToDouble ( item, "setting FloatList from Object[]" ) );

                isList = true;
            }
            /*
            else if ( _from is mmFlexiFloat ) {
                var ff = ( mmFlexiFloat ) _from;

                if ( ff.isList )
                    list = new List<double> ( ( List<Double> ) ff );
                else
                    list = new List<double>() { ( double ) ff };
            }
            */
            else
                Debug.Log ( "conversion from " + _from.GetType().Name + " to FloatList not handled yet" );
        }


        // implicit conversions
        public static implicit operator double ( mmFloatList _from )
        {
            if ( _from.list.Count > 0 )
                return _from.list[ 0 ];
            else return 0;
        }
        public static implicit operator List<double> ( mmFloatList _from )
        {
            return new List<double> ( _from.list );
        }
        public static implicit operator List<int> ( mmFloatList _from )
        {
            return new List<int> ( TypeManager.ObjectToIntegerList ( ( object ) _from.list ) );
        }

        /*

        public static implicit operator mmFlexiFloat ( mmFloatList _from )
        {
            return new mmFlexiFloat ( _from.list );
        }

            */


        public static implicit operator mmFloatList ( List<double> _from )
        {
            return new mmFloatList ( _from );
        }
        public static implicit operator mmFloatList ( List<int> _from )
        {
            return new mmFloatList ( TypeManager.ObjectToDoubleList ( ( object ) _from ) );
        }
        public static implicit operator mmFloatList ( List<bool> _from )
        {
            return new mmFloatList ( TypeManager.ObjectToDoubleList ( ( object ) _from ) );
        }


        public static implicit operator string ( mmFloatList _from )
        {
            return _from.ToString();
        }


        public override string ToString ()
        {
            string concat = "";

            foreach ( double _value in list )
                concat += ( _value.ToString() + " " );

            return concat;
        }


        public double this[ int index ]
        {
            get {
                try {
                    return list[ Math.Min ( index, list.Count - 1 ) ];
                }
				catch ( Exception e ) {
                    Debug.Log( "FloatList index out of range (get): " + e.Message );
                    return 0;
                }
            }
            set {
                try {
                    list[ index ] = value;
                }
				catch ( Exception e ) {
                    Debug.Log( "FloatList index out of range (set): " + e.Message );
                }
            }
        }


        public int Length
        {
            get { return list.Count; }
        }

        private int _Count;
        public override int Count
        {
            get { return list.Count; }
        }




        public void Add ( double item )
        {
            list.Add ( item );
        }



        // operators


        // FIXME: inherit these from mmFlexiFloat

        public static mmFloatList operator + ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( a.list[ i ] + b.list[ i ] );

            return ff;
        }


        /*
        	public static DoubleParameter operator + (DoubleParameter a, double b) {
        		DoubleParameter bb = new DoubleParameter(b);
        		return (a+bb);
        	}
        	public static DoubleParameter operator + (double a, DoubleParameter b) {
        		DoubleParameter aa = new DoubleParameter(a);
        		return (aa+b);
        	}
        */

        public static mmFloatList operator - ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( a.list[ i ] - b.list[ i ] );

            return ff;
        }




        public static mmFloatList operator - ( mmFloatList a )
        {
            var ff = new mmFloatList();
            ff.Capacity = a.Count;

            for ( int i = 0; i < a.Count; i++ )
                ff.list.Add ( -a.list[ i ] );

            return ff;
        }




        public static mmFloatList operator * ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( a.list[ i ]*b.list[ i ] );

            return ff;
        }


        public static mmFloatList operator / ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( a.list[ i ] / b.list[ i ] );

            return ff;
        }


        public static mmBooleanList operator == ( mmFloatList a, mmFloatList b )
        {
            var bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] == b.list[ i ] );

            return bl;
        }

        public static bool Identical ( mmFloatList a, mmFloatList b )
        {
            bool equal = true;

            for ( int i = 0; i < Math.Min ( a.Count, b.Count ); i++ )
                equal = ( equal && ( a.list[ i ] == b.list[ i ] ) );

            return equal;
        }

        public static mmBooleanList operator != ( mmFloatList a, mmFloatList b )
        {
            var bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] != b.list[ i ] );

            return bl;
        }

        public static mmBooleanList operator > ( mmFloatList a, mmFloatList b )
        {
            var bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] > b.list[ i ] );

            return bl;
        }

        public static mmBooleanList operator < ( mmFloatList a, mmFloatList b )
        {
            var bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] < b.list[ i ] );

            return bl;
        }


        public static mmFloatList Min ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( Math.Min ( a.list[ i ], b.list[ i ] ) );

            return ff;
        }


        public static mmFloatList Max ( mmFloatList a, mmFloatList b )
        {
            var ff = new mmFloatList();
            int l = Math.Min ( a.Count, b.Count );
            ff.Capacity = l;

            for ( int i = 0; i < l; i++ )
                ff.list.Add ( Math.Max ( a.list[ i ], b.list[ i ] ) );

            return ff;
        }

        public double Min()
        {
            double min = list[ 0 ];

            for ( int i = 1; i < list.Count; i++ )
                min = Math.Min ( list[ i ], min );

            return min;
        }

        public double Max()
        {
            double max = list[ 0 ];

            for ( int i = 1; i < list.Count; i++ )
                max = Math.Max ( list[ i ], max );

            return max;
        }



        public static mmFloatList Sin ( mmFloatList input )
        {
            var result = new mmFloatList();
            result.Capacity = input.Count;

            for ( int i = 0; i < input.Count; i++ )
                result.list.Add ( Math.Sin ( input[ i ] ) );

            return result;
        }




        public bool ContainsZero()
        {
            foreach ( double item in list )
                if ( item == 0 )
                    return true;

            return false;
        }



    }

}