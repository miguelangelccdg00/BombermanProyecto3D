using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Button playButton;
    public Button exitButton;
    public Button fastGameButton;
    
    private bool isMainMenuScene = false;

    void Start()
    {
        // Verifico si estamos en la escena correcta
        CheckSceneAndDestroy();
        
        if (isMainMenuScene)
        {
            SetupButtons();
            // Me apunto a cambios de escena para auto-destruirme
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
    
    void CheckSceneAndDestroy()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isMainMenuScene = (currentScene == "MainMenu");
        
        if (!isMainMenuScene)
        {
            Debug.Log("MainMenuManager destroying itself - not in MainMenu scene (current: " + currentScene + ")");
            Destroy(gameObject);
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si se carga una escena que no es MainMenu, destruyo este objeto
        if (scene.name != "MainMenu")
        {
            Debug.Log("MainMenuManager auto-destroying on scene change to: " + scene.name);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Limpio suscripciones al destruir
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SetupButtons()
    {
        // Verifico y configuro los botones
        if (playButton == null)
        {
            GameObject playObj = GameObject.Find("PlayButton");
            if (playObj != null)
                playButton = playObj.GetComponent<Button>();
        }
        if (exitButton == null)
        {
            GameObject exitObj = GameObject.Find("ExitButton");
            if (exitObj != null)
                exitButton = exitObj.GetComponent<Button>();
        }

        // Añado los listeners a los botones
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        else
            Debug.LogError("PlayButton no encontrado!");

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
        else
            Debug.LogError("ExitButton no encontrado!");

        if (fastGameButton == null)
        {
            GameObject fastGameObj = GameObject.Find("FastGame");
            if (fastGameObj != null)
                fastGameButton = fastGameObj.GetComponent<Button>();
        }

        if (fastGameButton != null)
            fastGameButton.onClick.AddListener(FastGame);
        else
            Debug.LogError("FastGameButton no encontrado!");
    }

    public void PlayGame()
    {
        Debug.Log("Iniciando modo campaña...");
        if (GlobalStateManager.Instance != null)
        {
            GlobalStateManager.Instance.ResetMatch();
            GlobalStateManager.Instance.SetCampaignMode(true);
            
            // Me aseguro de que todos sepan que estamos en modo campaña antes de cargar la escena
            Debug.Log("Modo campaña activado: " + GlobalStateManager.Instance.isCampaignMode);
        }
        else
        {
            Debug.LogError("No se encontró GlobalStateManager al iniciar modo campaña");
        }
        
        // Cargo la escena de juego
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void FastGame()
    {
        Debug.Log("Cargando selector de niveles para partida rápida...");
        if (GlobalStateManager.Instance != null)
        {
            GlobalStateManager.Instance.ResetMatch();
            GlobalStateManager.Instance.SetCampaignMode(false);
        }
        SceneManager.LoadScene("LevelSelector");
    }
}