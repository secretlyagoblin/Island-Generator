using UnityEngine;
using System.Collections.Generic;
using System;

public struct Coord {
    public int TileX
    { get; set; }
    public int TileY
    { get; set; }

    public Vector3 Vector3
    {
        get
        {
            return new Vector3(TileX, 0, TileY);
        }
    }

    public Vector2 Vector2
    {
        get
        {
            return new Vector2(TileX, TileY);
        }
    }

    public Coord(int x, int y)
    {
        TileX = x;
        TileY = y;
    }

    public static Coord[] GetLine(Coord from, Coord to)
    {
        var line = new List<Coord>();

        var x = from.TileX;
        var y = from.TileY;

        var dx = to.TileX - x;
        var dy = to.TileY - y;

        var step = Math.Sign(dx);
        var gradientStep = Math.Sign(dy);

        var longest = Mathf.Abs(dx);
        var shortest = Mathf.Abs(dy);

        var inverted = false;

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        var gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line.ToArray();
    }
}