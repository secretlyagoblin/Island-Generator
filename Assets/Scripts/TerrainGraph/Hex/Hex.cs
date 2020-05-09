using UnityEngine;

namespace WanderingRoad.Procgen.RecursiveHex
{
    public struct Hex
    {
        public HexIndex Index;
        public HexPayload Payload;
        public string DebugData;

        public bool IsBorder;

        private bool _notNull;


        /// <summary>
        /// Create a new hex just from the XY. Will need to be expanded later.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Hex(HexIndex index, HexPayload payload, bool isBorder, string debugData = "")
        {
            Index = index;
            Payload = payload;
            DebugData = debugData;
            IsBorder = isBorder;

            _notNull = true;
        }

        public Hex(Hex hex, bool isBorder)
        {
            Index = hex.Index;
            Payload = hex.Payload;
            DebugData = hex.DebugData;
            IsBorder = isBorder;

            _notNull = true;
        }

        public static Hex InvalidHex(HexIndex index)
        {            
                return new Hex()
                {
                    IsBorder = true,
                    _notNull = false,
                    Index = index
                };            
        }

        public static bool IsInvalid(Hex hex)
        {
            return !hex._notNull;
        }

    }


}