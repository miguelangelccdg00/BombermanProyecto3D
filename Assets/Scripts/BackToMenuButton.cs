using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    [Tooltip("Nombre de la escena del menú principal")]
    public string menuSceneName = "MainMenu";

    void Start()
    {
        // Compruebo que la escena esté en los build settings
        Debug.Log("BackToMenuButton: Inicializado. Escena del menú: " + menuSceneName);
        
        // Me aseguro de que el botón funciona bien y registro el evento
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            Debug.Log("BackToMenuButton: Botón encontrado. Está activo: " + button.interactable);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(GoToMenu);
            Debug.Log("BackToMenuButton: onClick configurado correctamente");
            
            // Miro a ver si el Canvas tiene GraphicRaycaster (hace falta para la UI)
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                UnityEngine.UI.GraphicRaycaster raycaster = parentCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("BackToMenuButton: Canvas: " + parentCanvas.name + ", tiene raycaster: " + (raycaster != null));
            }
            
            // Compruebo que haya EventSystem (sin esto los botones no van)
            UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            Debug.Log("BackToMenuButton: EventSystem existe: " + (eventSystem != null));
        }
        else
        {
            Debug.LogError("BackToMenuButton: ¡No tiene el componente Button!");
        }
    }

    public void GoToMenu()
    {
        Debug.Log("BackToMenuButton: ¡Clic! Volviendo al menú: " + menuSceneName);
        
        // Reseteo el tiempo por si acaso estaba pausado
        Time.timeScale = 1f;
        
        // Cargo la escena del menú
        SceneManager.LoadScene(menuSceneName);
        
        Debug.Log("BackToMenuButton: LoadScene ejecutado");
    }
    
    // Para hacer pruebas rápidas con el teclado
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("BackToMenuButton: Prueba con la M");
            GoToMenu();
        }
    }
}
