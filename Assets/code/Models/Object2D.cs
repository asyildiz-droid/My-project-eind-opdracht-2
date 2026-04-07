using System;
using UnityEngine;

/**
 * Bijzonderheden wegens beperkingen van JsonUtility:
 * - De id is een string in plaats van een Guid omdat Unity een Guid niet in de Inspector kan tonen. Gelukkig geeft dit geen probleem omdat de backend API de string correct zal interpreteren als een Guid.
*/
[Serializable]
public class Object2D
{
    public string Id;

    public string EnvironmentId;

    public string PrefabId;

    public float PositionX;

    public float PositionY;

    public float ScaleX;

    public float ScaleY;

    public float RotationZ;

    public int SortingLayer;
}