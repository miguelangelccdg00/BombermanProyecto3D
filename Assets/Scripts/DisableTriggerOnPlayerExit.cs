using UnityEngine;
using System.Collections;

/// <summary>
/// Este script se asegura de que se pueda poner una bomba a los pies del jugador sin que cause movimiento raro cuando el jugador se aleje.
/// Desactiva el trigger del collider, haciendo que el objeto sea sólido.
/// </summary>
public class DisableTriggerOnPlayerExit : MonoBehaviour
{

    public void OnTriggerExit (Collider other)
    {
        if (other.gameObject.CompareTag ("Player"))
        { // Cuando el jugador sale del área del trigger
            GetComponent<Collider> ().isTrigger = false; // Desactivo el trigger
        }
    }
}
