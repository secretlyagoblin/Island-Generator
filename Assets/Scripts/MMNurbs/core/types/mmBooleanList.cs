using UnityEngine;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace UnityNURBS.Types
{

    public class mmBooleanList : mmType, ImmType
    {
        public List<bool> list = new List<bool>();

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


        public mmBooleanList()
        {
            Init();
        }

        public mmBooleanList ( int size )
        {
            Init();
            list.Capacity = size;
        }

        public mmBooleanList ( bool value, int size )
        {
            Init();
            list = new List<bool> ( Enumerable.Repeat ( value, size ) );
        }

        public mmBooleanList ( mmBooleanList _from )
        {
            Init();
            list = new List<bool> ( _from.list );
        }

        public mmBooleanList ( List<bool> _list )
        {
            Init();
            list = new List<bool> ( _list );
        }


        // mainly for conversion from browser values
        public mmBooleanList ( object _from )
        {
            Init();

            try {
                if ( _from == null ) {}
                else if ( _from is mmBooleanList )
                    list = new List<bool> ( ( ( mmBooleanList ) _from ).list ); else if ( _from is Boolean ) { // bool

                    list = new List<bool>() { ( bool ) _from };
                }
                else if ( _from is Int32 ) {  // int
                    list =  new List<bool>() { ( int ) _from == 0 ? false : true };
                }
                else if ( _from is Double ) {  // double
                    list =  new List<bool>() { ( double ) _from == 0 ? false : true };
                }
                else if ( _from is Boolean[] ) {  // array of bools
                    list.Capacity = ( ( Boolean[] ) _from ).Length;

                    foreach ( bool boolean in ( Boolean[] ) _from )
                        list.Add ( boolean );
                }
                else if ( _from is List<Boolean> )    // list of bools
                    list = new List<bool> ( ( List<bool> ) _from ); else if ( _from is Int32[] ) { // list of ints

                    list.Capacity = ( ( Int32[] ) _from ).Length;

                    foreach ( Int32 int32 in ( Int32[] ) _from )
                        list.Add ( int32 == 0 ? false : true );
                }
                else if ( _from is List<Int32> ) {  // list of ints
                    list.Capacity = ( ( List<Int32> ) _from ).Count;

                    foreach ( Int32 int32 in ( List<Int32> ) _from )
                        list.Add ( int32 == 0 ? false : true );
                }
                else if ( _from is Double[] ) {  // list of doubles
                    list.Capacity = ( ( Double[] ) _from ).Length;

                    foreach ( double db in ( Double[] ) _from )
                        list.Add ( db == 0 ? false : true );
                }
                else if ( _from is List<Double> ) {  // list of doubles
                    list.Capacity = ( ( List<Double> ) _from ).Count;

                    foreach ( double db in ( List<Double> ) _from )
                        list.Add ( db == 0 ? false : true );
                }
                else if ( _from is mmFloatList ) {
                    mmFloatList pf = ( mmFloatList ) _from;
                    list.Capacity = pf.Count;

                    foreach ( double db in pf.list )
                        list.Add ( db == 0 ? false : true );
                }/*
                else if ( _from is mmFlexiBoolean ) {
                    mmFlexiBoolean fb = ( mmFlexiBoolean ) _from;

                    if ( fb.isList )
                        list = new List<bool> ( ( List<bool> ) fb );
                    else
                        list = new List<bool>() { ( bool ) fb };
                }
                */
                else if ( _from is object[] ) {
                    list.Capacity = ( ( object[] ) _from ).Length;

                    foreach ( object obj in ( object[] ) _from )
                        list.Add ( TypeManager.ConvertObjectToDouble ( obj, "setting scBooleanList from Object[]" ) == 0 ? false : true );
                }
                else
                    throw new mmTypeException();
            }
            catch ( mmTypeException e ) {
                Debug.Log( "conversion from " + _from.GetType().Name + "[ " + _from.ToString() + " ] to scBooleanList not handled yet" );
            }
        }


        // implicit conversions
        public static implicit operator bool ( mmBooleanList _from )
        {
            if ( _from.list.Count > 0 )
                return _from.list[ 0 ];
            else return false;
        }
        public static implicit operator List<bool> ( mmBooleanList _from )
        {
            return new List<bool> ( _from.list );
        }

        /*

        public static implicit operator mmFlexiBoolean ( mmBooleanList _from )
        {
            return new mmFlexiBoolean ( _from.list );
        }

    */


        public static implicit operator mmBooleanList ( bool[] _from )
        {
            return new mmBooleanList ( _from );
        }

        public static implicit operator mmBooleanList ( List<bool> _from )
        {
            return new mmBooleanList ( _from );
        }
        public static implicit operator mmBooleanList ( List<int> _from )
        {
            return new mmBooleanList ( ( object ) _from );
        }
        public static implicit operator mmBooleanList ( List<double> _from )
        {
            return new mmBooleanList ( ( object ) _from );
        }


        public static implicit operator string ( mmBooleanList _from )
        {
            return _from.list.ToString();
        }


        public override string ToString ()
        {
            string concat = "";

            foreach ( bool _value in list )
                concat += ( _value.ToString() + " " );

            return concat;
        }



        public bool this[ int index ]
        {
            get {
                try {
                    return list[ Math.Min ( index, list.Count - 1 ) ];
                }
                catch ( Exception e ) {
                    Debug.Log( "BooleanList index out of range (get): " + e.Message );
                    return false;
                }
            }
            set {
                try {
                    list[ index ] = value;
                }
				catch ( Exception e ) {
                    Debug.Log( "BooleanList index out of range (set): " + e.Message );
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



        public void Add ( bool item )
        {
            list.Add ( item );
        }



        // operators
        public static mmBooleanList operator & ( mmBooleanList a, mmBooleanList b )
        {
            mmBooleanList bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.list.Add ( a.list[ i ] && b.list[ i ] );

            return bl;
        }


        public static mmBooleanList operator | ( mmBooleanList a, mmBooleanList b )
        {
            mmBooleanList bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.list.Add ( a.list[ i ] || b.list[ i ] );

            return bl;
        }




        public static mmBooleanList operator ! ( mmBooleanList a )
        {
            mmBooleanList bl = new mmBooleanList();
            bl.Capacity = a.Count;

            for ( int i = 0; i < a.Count; i++ )
                bl.list.Add ( !a.list[ i ] );

            return bl;
        }


        public static mmBooleanList operator == ( mmBooleanList a, mmBooleanList b )
        {
            mmBooleanList bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] == b.list[ i ] );

            return bl;
        }

        public static bool Identical ( mmBooleanList a, mmBooleanList b )
        {
            bool equal = true;

            for ( int i = 0; i < Math.Min ( a.list.Count, b.list.Count ); i++ )
                equal = ( equal && ( a.list[ i ] == b.list[ i ] ) );

            return equal;
        }


        public static mmBooleanList operator != ( mmBooleanList a, mmBooleanList b )
        {
            mmBooleanList bl = new mmBooleanList();
            int l = Math.Min ( a.Count, b.Count );
            bl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                bl.Add ( a.list[ i ] != b.list[ i ] );

            return bl;
        }



    }
}