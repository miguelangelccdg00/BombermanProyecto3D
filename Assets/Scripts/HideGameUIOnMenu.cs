using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HideGameUIOnMenu : MonoBehaviour
{    
    // Nombres de UI que busco - varias opciones posibles
    private string[] uiNamesToFind = { "Player1UI", "Player2UI", "Player 1 Lives", "Player 2 Lives", "Lives" };
    private string[] menuScenes = { "MainMenu", "LevelSelector", "WinnerScene" };
    
    // Esto hace que el script persista entre escenas y siempre gestione la UI
    private static HideGameUIOnMenu instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {        
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMenuScene = System.Array.Exists(menuScenes, scene => scene == currentScene);
        
        Debug.Log("HideGameUIOnMenu Start: Scene=" + currentScene + ", IsMenu=" + isMenuScene);

        if (isMenuScene)
        {
            // En escenas de menú, oculto inmediatamente las UI de juego
            HideGameUI();
        }
        else
        {            
            // En escenas de juego, muestro inmediatamente las UI
            ShowGameUI();
        }
    }
    
    private void HideGameUI()
    {
        Debug.Log("HideGameUI: Ocultando UI de juego...");

        GameObject gsm = GameObject.Find("Global State Manager");
        if (gsm != null)
        {
            Transform canvas = gsm.transform.Find("Canvas");
            if (canvas != null)
            {
                Transform p1 = canvas.Find("Player1UI");
                if (p1 != null) p1.gameObject.SetActive(false);
                Transform p2 = canvas.Find("Player2UI");
                if (p2 != null) p2.gameObject.SetActive(false);
                Debug.Log("HideGameUIOnMenu: Ocultando Player1UI y Player2UI del Canvas de GSM");
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;
        bool isMenuScene = System.Array.Exists(menuScenes, s => s == currentScene);
        Debug.Log("HideGameUIOnMenu OnSceneLoaded: Scene=" + currentScene + ", IsMenu=" + isMenuScene);
        
        if (isMenuScene)
        {
            // Un poquito de delay para asegurar que los objetos estén cargados
            StartCoroutine(HideGameUIDelayed());
        }
        else
        {
            // En escenas de juego, uso varios intentos para mostrar la UI
            StartCoroutine(ShowGameUIWithRetry());
        }
    }
    
    private IEnumerator HideGameUIDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HideGameUI();
    }

    private IEnumerator ShowGameUIWithRetry()
    {
        // Primer intento inmediato
        ShowGameUI();
        
        // Segundo intento después de medio segundo
        yield return new WaitForSeconds(0.5f);
        Debug.Log("ShowGameUIWithRetry: Segundo intento después de 0.5s");
        ShowGameUI();
        
        // Tercer intento después de 1 segundo total
        yield return new WaitForSeconds(0.5f);
        Debug.Log("ShowGameUIWithRetry: Tercer intento después de 1s");
        ShowGameUI();
    }

    private void ShowGameUI()
    {
        Debug.Log("ShowGameUI: Mostrando UI de juego...");

        GameObject gsm = GameObject.Find("Global State Manager");
        if (gsm != null)
        {
            Transform canvas = gsm.transform.Find("Canvas");
            if (canvas != null)
            {
                Transform p1 = canvas.Find("Player1UI");
                if (p1 != null) p1.gameObject.SetActive(true);
                Transform p2 = canvas.Find("Player2UI");
                if (p2 != null) p2.gameObject.SetActive(true);
                Debug.Log("ShowGameUI: Mostrando Player1UI y Player2UI del Canvas de GSM");
            }
        }
    }
}