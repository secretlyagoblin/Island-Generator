using UnityEngine;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;


namespace UnityNURBS.Types
{

    public class mmVectorList : mmType, ImmType
    {
        public List<mmVector3> list = new List<mmVector3>();

        public int Capacity
        {
            set {
                list.Capacity = value;
            }
        }

        private void Init()
        {
            isVector = true;
            isNumeric = true;
            isList = true;
        }

        public mmVectorList ()
        {
            Init();
        }

        public mmVectorList ( int size )
        {
            Init();
            list.Capacity = size;
        }

        public mmVectorList ( mmVector3 value, int size )
        {
            Init();
            list.Capacity = size;

            for ( int i = 0; i < size; i++ )
                list.Add ( new mmVector3 ( value ) );
        }

        public mmVectorList ( mmVectorList _from )
        {
            Init();
            list = new List<mmVector3> ( _from.Count );

            foreach ( mmVector3 _vector in _from.list )
                list.Add ( new mmVector3 ( _vector ) );
        }

        public mmVectorList ( List<mmVector3> _from )
        {
            Init();
            list = new List<mmVector3> ( _from.Count );

            foreach ( mmVector3 _vector in _from )
                list.Add ( new mmVector3 ( _vector ) );
        }


        // mainly for conversion from browser values
        public mmVectorList ( object _from )
        {
            Init();

            //		Logging.Warn("converting from "+_from.GetType().Name);

            if ( _from == null ) {}
            else if ( _from is mmVectorList ) {
                var vl = ( mmVectorList ) _from;
                list = new List<mmVector3> ( vl.Count );

                foreach ( mmVector3 _vector in vl.list )
                    list.Add ( new mmVector3 ( _vector ) );
            }
            else if ( _from is List<mmVector3> ) {  // vector list
                list = new List<mmVector3> ( ( ( List<mmVector3> ) _from ).Count );

                foreach ( mmVector3 _vector in ( List<mmVector3> ) _from )
                    list.Add ( new mmVector3 ( _vector ) );
            }
            else if ( _from is object[] ) {  // Vector or Vector list
                
                var vectorList = new List<mmVector3>();
                object[] tmpArray = _from as object[];
                
                if ( tmpArray.Length > 0 ) {
                    
                    if ( tmpArray[ 0 ] is Double || tmpArray[ 0 ] is Int32 ) {
                        
                        vectorList = new List<mmVector3> ( tmpArray.Length/3 );
                        
                        for ( int i = 0; i < tmpArray.Length; i+=3 ) {
                            var _vector = mmVector3.zero;
                            _vector.x = TypeManager.ConvertObjectToDouble ( tmpArray[ i ],   "VectorList from object[]" );
                            _vector.y = TypeManager.ConvertObjectToDouble ( tmpArray[ i+1 ], "VectorList from object[]" );
                            _vector.z = TypeManager.ConvertObjectToDouble ( tmpArray[ i+2 ], "VectorList from object[]" );
                            
//                            Debug.Log ( "vector " + i/3 + " " + _vector );
                            vectorList.Add ( _vector );
                        }
                        
                    } else {
                        
                        // probably a list of vectors
                        
                        vectorList = new List<mmVector3> ( tmpArray.Length );
                        
                        for ( int j = 0; j < tmpArray.Length; j++ ) {
                            if ( tmpArray[ j ] is object[] ) { // vector (1,1,.5) for example
                                object[] tmpVector = tmpArray[ j ] as object[];
                                mmVector3 _vector = mmVector3.zero;
                                
                                for ( int i = 0; i < 3; i++ )
                                    _vector[ i ] = TypeManager.ConvertObjectToDouble ( tmpVector[ i ], "VectorList from Object[]" );
                                
                                vectorList.Add ( _vector );
                            }
                            else if ( tmpArray[ j ] is Int32[] ) {  // vector (1,1,1) for example
                                Int32[] tmpVector = tmpArray[ j ] as Int32[];
                                mmVector3 _vector = mmVector3.zero;
                                
                                for ( int i = 0; i < 3; i++ )
                                    _vector[ i ] = Convert.ToDouble ( tmpVector[ i ] );
                                
                                vectorList.Add ( _vector );
                            }
                            else if ( tmpArray[ j ] is Double[] ) {  // vector (.1,.1,.5) for example
                                Double[] tmpVector = tmpArray[ j ] as Double[];
                                mmVector3 _vector = mmVector3.zero;
                                
                                for ( int i = 0; i < 3; i++ )
                                    _vector[ i ] = ( double ) tmpVector[ i ];
                                
                                vectorList.Add ( _vector );
                            }
                        }
                    }
                }
                
                list = vectorList;
            }
            else if ( _from is Double[] ) {  // Vector or Vector list

				var vectorList = new List<mmVector3>();
                Double[] tmpArray = _from as Double[];

                if ( tmpArray.Length > 0 ) {
                    
                    vectorList = new List<mmVector3> ( tmpArray.Length/3 );
                    
                    for ( int i = 0; i < tmpArray.Length; i+=3 ) {
                        var _vector = mmVector3.zero;
                        _vector.x = TypeManager.ConvertObjectToDouble ( tmpArray[ i ],   "VectorList from Double[]" );
                        _vector.y = TypeManager.ConvertObjectToDouble ( tmpArray[ i+1 ], "VectorList from Double[]" );
                        _vector.z = TypeManager.ConvertObjectToDouble ( tmpArray[ i+2 ], "VectorList from Double[]" );
                        
//                        Debug.Log ( "vector " + i/3 + " " + _vector );
                        vectorList.Add ( _vector );
                    }
                        
                }

                list = vectorList;
            }
            else if ( _from is double[][] ) {  // vector array
                double[][] tmpVectorArray = _from as double[][];
                var vectorList = new List<mmVector3> ( tmpVectorArray.Length );

                for ( int j = 0; j < tmpVectorArray.Length; j++ ) {
                    double[] tmpVector = tmpVectorArray[ j ] as double[];
                    mmVector3 _vector = mmVector3.zero;

                    for ( int i = 0; i < 3; i++ )
                        _vector[ i ] = tmpVector[ i ];

                    vectorList.Add ( _vector );
                }

                list = vectorList;
            }
            else if ( _from is object[][] ) {  // vector array
                object[][] tmpVectorArray = _from as object[][];
                var vectorList = new List<mmVector3> ( tmpVectorArray.Length );

                for ( int j = 0; j < tmpVectorArray.Length; j++ ) {
                    object[] tmpVector = tmpVectorArray[ j ] as object[];
                    mmVector3 _vector = mmVector3.zero;

                    for ( int i = 0; i < 3; i++ )
                        _vector[ i ] = TypeManager.ConvertObjectToDouble ( tmpVector[ i ], "VectorList from Object[][]" );

                    vectorList.Add ( _vector );
                }

                list = vectorList;
            }
            else if ( _from is Int32[][] ) {  // vector array
                Int32[][] tmpVectorArray = _from as Int32[][];
                var vectorList = new List<mmVector3> ( tmpVectorArray.Length );

                for ( int j = 0; j < tmpVectorArray.Length; j++ ) {
                    Int32[] tmpVector = tmpVectorArray[ j ] as Int32[];
                    mmVector3 _vector = mmVector3.zero;

                    for ( int i = 0; i < 3; i++ )
                        _vector[ i ] = Convert.ToDouble ( tmpVector[ i ] );

                    vectorList.Add ( _vector );
                }

                list = vectorList;
            }
            /*
            else if ( _from is mmFlexiVector ) {
                var fv = ( mmFlexiVector ) _from;

                if ( fv.isList ) {
                    list = new List<mmVector3> ( fv.Count );

                    for ( int j = 0; j < fv.Count; j++ )
                        list.Add ( new mmVector3 ( fv[ j ] ) );
                }
                else
                    list = new List<mmVector3>() { new mmVector3 ( ( mmVector3 ) fv ) };
            }
            */
            else
                Debug.Log( "conversion from " + _from.GetType().Name + " to VectorList not handled yet" );
        }


        // implicit conversions
        public static implicit operator mmVector3 ( mmVectorList _from )
        {
            if ( _from.list.Count > 0 )
                return new mmVector3 ( _from.list[ 0 ] );
            else return mmVector3.zero;
        }
        public static implicit operator List<mmVector3> ( mmVectorList _from )
        {
            var _list = new List<mmVector3> ( _from.Count );

            foreach ( mmVector3 _vector in _from.list )
                _list.Add ( new mmVector3 ( _vector ) );

            return _list;
        }

        public static implicit operator mmVectorList ( List<mmVector3> _from )
        {
            return new mmVectorList ( _from );
        }
        public static implicit operator mmVectorList ( Vector3[] _from )
        {
            var _list = new mmVectorList();
            _list.Capacity = _from.Length;

            foreach ( mmVector3 _vector in _from )
                _list.Add ( new mmVector3 ( _vector ) );

            return _list;
        }

        /*
        public static implicit operator mmVectorList ( mmFlexiVector _from )
        {
            return new mmVectorList ( _from.list );
        }
        */


        public static implicit operator string ( mmVectorList _from )
        {
            return _from.ToString();
        }


        public override string ToString ()
        {
            string concat = "";

            foreach ( mmVector3 _vector in list )
                concat += ( _vector.ToString() + " " );

            return concat;
        }



        public mmVector3 this[ int index ]
        {
            get {
                try {
                    return list[ Math.Min ( index, list.Count - 1 ) ];
                }
				catch ( Exception e ) {
                    Debug.Log( "VectorList index out of range (get): " + e.Message );
                    return mmVector3.zero;
                }
            }
            set {
                try {
                    list[ index ] = value;
                }
				catch ( Exception e ) {
                    Debug.Log ( "VectorList index out of range (set): " + e.Message );
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



        public void Add ( mmVector3 item )
        {
            list.Add ( item );
        }



        // operators

        // FIXME: inherit these from mmFlexiVector (see magnitude methods below)

        public static mmVectorList operator + ( mmVectorList a, mmVectorList b )
        {
            var vl = new mmVectorList();
            int l = Math.Min ( a.Count, b.Count );
            vl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                vl.Add ( new mmVector3 ( a.list[ i ] + b.list[ i ] ) );

            return vl;
        }



        public static mmVectorList operator - ( mmVectorList a, mmVectorList b )
        {
            var vl = new mmVectorList();
            int l = Math.Min ( a.Count, b.Count );
            vl.Capacity = l;

            for ( int i = 0; i < l; i++ )
                vl.Add ( new mmVector3 ( a.list[ i ] - b.list[ i ] ) );

            return vl;
        }


        public static mmVectorList operator - ( mmVectorList a )
        {
            var vl = new mmVectorList();
            vl.Capacity = a.Count;

            for ( int i = 0; i < a.Count; i++ )
                vl.Add ( new mmVector3 ( -a.list[ i ] ) );

            return vl;
        }



        public static mmVectorList operator * ( mmVectorList a, mmFlexiFloat b )
        {
            var vl = new mmVectorList();
            int l = a.Count;

            /*

            if ( b.isList )
                l = Math.Min ( a.Count, b.Count );

            vl.Capacity = l;

            if ( b.isList )
                for ( int i = 0; i < l; i++ )
                    vl.Add ( new mmVector3 ( a.list[ i ]*b.list[ i ] ) );
            else
                for ( int i = 0; i < l; i++ )
                    vl.Add ( new mmVector3 ( a.list[ i ]*b.scalar ) );

    */

            return vl;
        }


        public static mmVectorList operator * ( mmFlexiFloat a, mmVectorList b )
        {
            return ( b * a );
        }


        public static mmVectorList operator / ( mmVectorList a, mmFlexiFloat b )
        {
            var vl = new mmVectorList();
            int l = a.Count;

            /*

            if ( b.isList )
                l = Math.Min ( a.Count, b.Count );

            vl.Capacity = l;

            if ( b.isList )
                for ( int i = 0; i < l; i++ )
                    vl.Add ( new mmVector3 ( a.list[ i ] / b.list[ i ] ) );
            else
                for ( int i = 0; i < l; i++ )
                    vl.Add ( new mmVector3 ( a.list[ i ] / b.scalar ) );

            */

            return vl;
            
        }

        /*

        public static mmFlexiBoolean operator == ( mmVectorList a, mmVectorList b )
        {
            var fb = new mmFlexiBoolean();
            int l = Math.Min ( a.Count, b.Count );
            fb.Capacity = l;
            fb.state = true;

            for ( int i = 0; i < l; i++ ) {
                fb.Add ( a.list[ i ] == b.list[ i ] );
                fb.state = ( fb.state && fb[ i ] );
            }

            return fb;
        }

        public static mmFlexiBoolean operator != ( mmVectorList a, mmVectorList b )
        {
            var fb = new mmFlexiBoolean();
            int l = Math.Min ( a.Count, b.Count );
            fb.Capacity = l;
            fb.state = true;

            for ( int i = 0; i < l; i++ ) {
                fb.Add ( a.list[ i ] != b.list[ i ] );
                fb.state = ( fb.state && fb[ i ] );
            }

            return fb;
        }

    */


        public mmFloatList x
        {
            get {
                var fl = new mmFloatList();
                fl.Capacity = this.Count;

                for ( int i = 0; i < this.Count; i++ )
                    fl.Add ( this[ i ].x );

                return fl;
            }
        }

        public mmFloatList y
        {
            get {
                var fl = new mmFloatList();
                fl.Capacity = this.Count;

                for ( int i = 0; i < this.Count; i++ )
                    fl.Add ( this[ i ].y );

                return fl;
            }
        }

        public mmFloatList z
        {
            get {
                var fl = new mmFloatList();
                fl.Capacity = this.Count;

                for ( int i = 0; i < this.Count; i++ )
                    fl.Add ( this[ i ].z );

                return fl;
            }
        }



        public static mmVectorList Normalize ( mmVectorList input )
        {
            var result = new mmVectorList ( input );

            for ( int i = 0; i < result.Count; i++ )
                result[ i ] = result[ i ].normalized;

            return result;
        }


        public void Normalize ()
        {
            for ( int i = 0; i < list.Count; i++ )
                list[ i ] = list[ i ].normalized;
        }


        public mmVectorList normalized
        {
            get { return mmVectorList.Normalize ( this ); }
        }

        /*

        public mmFlexiFloat magnitude
        {
            get {
                var fv = new mmFlexiVector ( this );
                return fv.magnitude;
            }
        }

        public mmFlexiFloat sqrMagnitude
        {
            get {
                mmFlexiVector fv = new mmFlexiVector ( this );
                return fv.sqrMagnitude;
            }
        }

    */


    }

}