using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    public class DSErrorData
{ 
    public Color colour { get; set; }

    public DSErrorData()
    {
        GenerateRandomColour();
    }

    void GenerateRandomColour()
    {
        colour = new Color32(
            (byte)Random.Range(65, 256),
            (byte)Random.Range(50, 176),
            (byte)Random.Range(50, 176),
            255
            );
    }
}
}