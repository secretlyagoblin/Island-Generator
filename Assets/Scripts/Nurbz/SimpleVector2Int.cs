using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SimpleVector2Int : IEquatable<SimpleVector2Int> {

    public int x;
    public int y;

    public static SimpleVector2Int operator +(SimpleVector2Int a, SimpleVector2Int b)
    {
        return new SimpleVector2Int(a.x + b.x, a.y + b.y);
    }

    public static SimpleVector2Int operator -(SimpleVector2Int a, SimpleVector2Int b)
    {
        return new SimpleVector2Int(a.x - b.x, a.y - b.y);
    }

    public static SimpleVector2Int operator *(SimpleVector2Int a, SimpleVector2Int b)
    {
        return new SimpleVector2Int(a.x * b.x, a.y * b.y);
    }

    public static SimpleVector2Int operator *(SimpleVector2Int a, int b)
    {
        return new SimpleVector2Int(a.x * b, a.y * b);
    }

    public static SimpleVector2Int operator /(SimpleVector2Int a, SimpleVector2Int b)
    {
        return new SimpleVector2Int(a.x / b.x, a.y / b.y);
    }

    public static bool operator ==(SimpleVector2Int a, SimpleVector2Int b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(SimpleVector2Int a, SimpleVector2Int b)
    {
        return !a.Equals(b);
    }

    bool IEquatable<SimpleVector2Int>.Equals(SimpleVector2Int other)
    {
        return other.x == x && other.y == y;
    }

    public bool Equals(SimpleVector2Int other)
    {
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {        
        return x.GetHashCode()* y.GetHashCode();;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override string ToString()
    {
        return "(" + x +", " + y + ")";
    }

    public SimpleVector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

}
