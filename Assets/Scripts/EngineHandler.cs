using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineHandler : MonoBehaviour
{
    [SerializeField] Light _Light;
    public Light Light { get { return _Light; } }

    [SerializeField] AudioSource _Audio;
    public AudioSource Audio { get { return _Audio; } }
}
