using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace UnityNURBS.Types
{

    public interface ImmType {}

    public class mmType
    {
        public bool isNumeric;
        public bool isVector;
        public bool isList = false;

        public virtual int Count { get; set; }

    }

    public class mmTypeException : System.Exception {}

}