#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtilities
{
    public static List<T> GetArrayElements<T>(SerializedProperty property, Func<SerializedProperty, T> asignMethod)
    {
        var list = new List<T>(property.arraySize);
        property.Next(true);
        property.Next(true);
        property.Next(false);
        for (int i = 0; i < list.Capacity; i++)
        {
            list.Add(asignMethod(property));
            property.Next(false);
        }

        return list;
    }

    public static List<SerializedProperty> GetArrayProperties(SerializedProperty property)
    {
        var list = new List<SerializedProperty>(property.arraySize);
        property.Next(true);
        property.Next(true);
        property.Next(false);
        for (int i = 0; i < list.Capacity; i++)
        {
            list.Add(property.Copy());
            property.Next(false);
        }

        return list;
    }
}
#endif