using UnityEngine;
using System.Collections;

/// <summary>
/// Script sencillo para destruir un objeto después de un rato
/// </summary>
public class DestroySelf : MonoBehaviour
{
    public float Delay = 3f;
    // Tiempo en segundos antes de destruir el objeto

    void Start ()
    {
        Destroy (gameObject, Delay);
    }
}
