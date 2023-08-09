//Inspired by Marnielle Lloyd Estrada: https://coffeebraingames.wordpress.com/2013/12/18/a-generic-floating-point-comparison-class/

using UnityEngine;

public static class Comparison
{
    /// <summary>
    /// Returns whether or not a == b
    /// </summary>
    public static bool TolerantEquals(float a, float b)
    {
        return Mathf.Approximately(a, b);
    }

    /// <summary>
    /// Returns whether or not a >= b.
    /// </summary>
    public static bool TolerantGreaterThanOrEquals(float a, float b)
    {
        return a > b || TolerantEquals(a, b);
    }

    /// <summary>
    /// Returns whether or not a <= b.
    /// </summary>
    public static bool TolerantLesserThanOrEquals(float a, float b)
    {
        return a < b || TolerantEquals(a, b);
    }
}