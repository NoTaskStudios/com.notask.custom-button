using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
    /// <summary>
    /// The higher the value, the closest to white.
    /// </summary>
    /// <param name="shade"></param>
    /// <returns></returns>
    public static Color GrayShade(float shade) => new(shade, shade, shade, 1);

    public static Color TransparentBlack(float transparency) => new(0, 0, 0, transparency);
    
    public static Color Transparent(this Color color, float transparency) => new(color.r, color.g, color.b, transparency);

    public static Color InvertColor(Color color)
    {
        Color invertedColor = color;
        invertedColor.r = 1 - invertedColor.r;
        invertedColor.g = 1 - invertedColor.g;
        invertedColor.b = 1 - invertedColor.b;

        return invertedColor;
    }
}
