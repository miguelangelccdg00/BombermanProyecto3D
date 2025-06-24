using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIDebugger : MonoBehaviour
{
    void Start()
    {
        // Espero un ratito para que todo se cargue
        Invoke("DebugAllUI", 1f);
    }
    
    public void DebugAllUI()
    {
        Debug.Log("=== DEBUG UI - Escena: " + SceneManager.GetActiveScene().name + " ===");
        
        // Busco todos los GameObjects activos
        GameObject[] allObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        
        Debug.Log("Total de objetos en escena: " + allObjects.Length);
        
        foreach (GameObject obj in allObjects)
        {
            // Busco objetos que puedan ser UI
            if (obj.name.Contains("UI") || obj.name.Contains("Player") || 
                obj.name.Contains("Lives") || obj.name.Contains("Heart") ||
                obj.name.Contains("Canvas"))
            {
                string activeStatus = obj.activeInHierarchy ? "ACTIVO" : "INACTIVO";
                string parent = obj.transform.parent != null ? obj.transform.parent.name : "ROOT";
                Debug.Log("UI Object: " + obj.name + " (" + activeStatus + ") - Padre: " + parent);
                
                // Si tiene componente Image, también lo muestro
                Image img = obj.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log("  - Tiene componente Image: " + img.enabled);
                }
                
                // Si tiene Canvas, también lo muestro
                Canvas canvas = obj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log("  - Tiene componente Canvas: " + canvas.enabled);
                }
            }
        }
        
        // Busco específicamente Canvas
        Canvas[] allCanvas = GameObject.FindObjectsOfType(typeof(Canvas)) as Canvas[];
        Debug.Log("\n=== CANVAS ENCONTRADOS ===");
        foreach (Canvas canvas in allCanvas)
        {
            string status = canvas.gameObject.activeInHierarchy ? "ACTIVO" : "INACTIVO";
            Debug.Log("Canvas: " + canvas.name + " (" + status + ")");
        }
        
        // Busco específicamente Images
        Image[] allImages = GameObject.FindObjectsOfType(typeof(Image)) as Image[];
        Debug.Log("\n=== IMAGENES ENCONTRADAS ===");
        foreach (Image img in allImages)
        {
            if (img.transform.parent != null && 
                (img.transform.parent.name.Contains("Lives") || img.transform.parent.name.Contains("Player")))
            {
                string status = img.gameObject.activeInHierarchy ? "ACTIVO" : "INACTIVO";
                string parent = img.transform.parent.name;
                Debug.Log("Image UI: " + img.name + " (" + status + ") - Padre: " + parent);
            }
        }
        
        Debug.Log("=== FIN DEBUG UI ===");
    }
}
