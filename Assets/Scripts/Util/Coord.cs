using UnityEngine;
using System.Collections;

public struct Coord {
    public int TileX
    { get; private set; }
    public int TileY
    { get; private set; }

    public Coord(int x, int y)
    {
        TileX = x;
        TileY = y;
    }
}