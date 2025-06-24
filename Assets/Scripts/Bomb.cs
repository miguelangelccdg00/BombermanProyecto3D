using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

	public GameObject explosionPrefab;
	public LayerMask levelMask;
	private bool exploded = false;



	// Inicialización
	void Start () {
		Invoke("Explode", 3f);
	}
	
	// Se llama cada frame
	void Update () {
		
	}

	void Explode() 
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity); // Explosión central

		StartCoroutine(CreateExplosions(Vector3.forward));
		StartCoroutine(CreateExplosions(Vector3.right));
		StartCoroutine(CreateExplosions(Vector3.back));
		StartCoroutine(CreateExplosions(Vector3.left)); 

		GetComponent<MeshRenderer>().enabled = false;
		exploded = true;
		transform.Find("Collider").gameObject.SetActive(false); // Desactivo el collider
		Destroy(gameObject, .3f); // Destruyo la bomba tras un ratito

		 
	}

	private IEnumerator CreateExplosions(Vector3 direction) 
	{
  		// Creo explosiones en la dirección dada
for (int i = 1; i < 3; i++) 
  { 
  // Compruebo si hay algo que bloquee
  RaycastHit hit; 
  // Lanzo un raycast para detectar obstáculos
  Physics.Raycast(transform.position + new Vector3(0,.5f,0), direction, out hit, 
    i, levelMask); 

  // Si no hay obstáculo, pongo la explosión
  if (!hit.collider) 
  { 
    Instantiate(explosionPrefab, transform.position + (i * direction),
    // Uso la rotación del prefab
      explosionPrefab.transform.rotation); 
    // Continúo con la siguiente posición
  } 
  else 
  { // Si hay obstáculo, paro aquí
    break; 
  }

  // Espero un poquito antes de la siguiente explosión
  yield return new WaitForSeconds(.05f); 
}

	}

	public void OnTriggerEnter(Collider other) 
{
	if (!exploded && other.CompareTag("Explosion"))
	{ // Si no he explotado ya y me toca una explosión
  CancelInvoke("Explode"); // Cancelo la explosión programada
  Explode(); // Exploto inmediatamente por reacción en cadena
	}  
}
  
  

}
