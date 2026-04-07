using System;

/**
 * Bijzonderheden wegens beperkingen van JsonUtility:
 * - De id is een string in plaats van een Guid omdat Unity een Guid niet in de Inspector kan tonen. Gelukkig geeft dit geen probleem omdat de backend API de string correct zal interpreteren als een Guid.
 */
[Serializable]
public class Environment2D
{
    public string Id;

    public string Name;

    public int MaxLength;

    public int MaxHeight;
}
