using UnityEngine;
using System.Collections.Generic;
using System;

public struct Coord {
    public int x
    { get; set; }
    public int y
    { get; set; }

    public Vector3 Vector3
    {
        get
        {
            return new Vector3(x, 0, y);
        }
    }

    public Vector2 Vector2
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Coord[] GetLine(Coord from, Coord to)
    {
        var line = new List<Coord>();

        var x = from.x;
        var y = from.y;

        var dx = to.x - x;
        var dy = to.y - y;

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