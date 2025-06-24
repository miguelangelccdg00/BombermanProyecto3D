using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalStateManager : MonoBehaviour
{
    public static GlobalStateManager Instance;
    private WinnerDisplay winnerDisplay;

    public int player1Wins = 0;
    public int player2Wins = 0;
    public System.Collections.Generic.List<int> roundWinners = new System.Collections.Generic.List<int>();
    private bool roundOver = false;
    public static bool nextCampaignMode = false;
    [HideInInspector]
    public bool isCampaignMode = false; // true para modo campaña, false para modo individual
    public bool player1Won = false; // Para seguir si el jugador 1 ha ganado

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            isCampaignMode = nextCampaignMode;
        }
        else if (Instance != this)  // Solo destruyo si no soy la instancia actual
        {
            Debug.Log("Destruyendo instancia duplicada de GlobalStateManager");
            Destroy(gameObject);
            return;
        }
        
        // Busco el WinnerDisplay en la escena al inicio
        winnerDisplay = FindObjectOfType<WinnerDisplay>();
    }
    
    // Para actualizar la referencia al WinnerDisplay cuando cambia la escena
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Actualizo la referencia al WinnerDisplay en la nueva escena
        winnerDisplay = FindObjectOfType<WinnerDisplay>();
        
        // Reseteo el estado roundOver cuando se carga una nueva escena de juego en modo campaña
        if (isCampaignMode && (scene.name == "Game" || scene.name == "Game 1" || scene.name == "Game 2"))
        {
            Debug.Log("Nueva escena de campaña cargada: " + scene.name + ", reseteando roundOver");
            roundOver = false;
        }
        
        // Si estamos en la escena de ganador, muestro el resultado
        if (scene.name == "WinnerScene" && winnerDisplay != null)
        {
            int winner = GetWinner();
            string winnerStr = winner == 1 ? "Player 1" : (winner == 2 ? "Player 2" : "");
            
            Debug.Log("WinnerScene cargada - Mostrando ganador: " + winnerStr + 
                     " (P1: " + player1Wins + ", P2: " + player2Wins + 
                     ", Último ganador en roundWinners: " + (roundWinners.Count > 0 ? roundWinners[roundWinners.Count - 1].ToString() : "ninguno") + ")");
            
            winnerDisplay.DisplayWinner(winnerStr, player1Wins, player2Wins);
        }
    }

    public void PlayerDied(int playerNumber)
    {
        if (roundOver) return;
        roundOver = true;
    
        int winner = (playerNumber == 1) ? 2 : 1;
        player1Won = (winner == 1);
    
        // Apunto el ganador de esta ronda
        roundWinners.Add(winner);
    
        if (winner == 1)
        {
            player1Wins++;
            Debug.Log("Jugador 1 gana esta ronda. Victorias: " + player1Wins);
        }
        else
        {
            player2Wins++;
            Debug.Log("Jugador 2 gana esta ronda. Victorias: " + player2Wins);
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (isCampaignMode)
        {
            Debug.Log("Modo Campaña - " + (currentScene == "Game" ? "Nivel 1" : 
                                         currentScene == "Game 1" ? "Nivel 2" : 
                                         currentScene == "Game 2" ? "Nivel Final" : "Nivel Desconocido"));
        }
    
        StartCoroutine(LoadNextRound());
    }

    private IEnumerator LoadNextRound()
    {
        yield return new WaitForSeconds(2f);

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[DEBUG] LoadNextRound() - Escena actual: '" + currentScene + "', isCampaignMode: " + isCampaignMode);
        
        if (isCampaignMode)
        {
            if (currentScene == "Game") 
            {
                Debug.Log("[DEBUG] Avanzando a Game 1...");
                SceneManager.LoadScene("Game 1");
            }
            else if (currentScene == "Game 1")
            {
                Debug.Log("[DEBUG] Avanzando a Game 2...");
                SceneManager.LoadScene("Game 2");
            }
            else if (currentScene == "Game 2")
            {
                Debug.Log("[DEBUG] Último nivel completado, mostrando resultados finales...");
                SceneManager.LoadScene("WinnerScene");
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(ReturnToMenuWithDelay());
                yield break;
            }
            else
            {
                Debug.LogWarning("[DEBUG] Escena inesperada en campaña: '" + currentScene + "'");
            }
            // Reseteo el estado para el siguiente nivel
            yield return new WaitForSeconds(0.1f);
            roundOver = false;
            yield break;
        }
        else
        {
            Debug.Log("[DEBUG] FastGame - Estado antes de WinnerScene - P1: " + player1Wins + ", P2: " + player2Wins + ", Último ganador: " + (roundWinners.Count > 0 ? roundWinners[roundWinners.Count - 1].ToString() : "ninguno"));
            SceneManager.LoadScene("WinnerScene");
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ReturnToMenuWithDelay());
        }

        if (LightingManager.Instance != null)
        {
            StartCoroutine(EnsureLightingAfterSceneLoad(LightingManager.Instance.ambientIntensity, 
                                                  LightingManager.Instance.directionalLightIntensity));
        }
    }

    // Corrutina para asegurar que la iluminación se aplique bien después de cargar la escena
    private IEnumerator EnsureLightingAfterSceneLoad(float ambientIntensity, float directionalIntensity)
    {
        // Espero a que la escena esté completamente cargada
        yield return new WaitForSeconds(0.5f); // Aumentado de 0.2f a 0.5f para dar más tiempo a la carga
        
        // Aplico la configuración de iluminación guardada
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.SetLightingIntensity(ambientIntensity, directionalIntensity);
            Debug.Log("Iluminación restaurada después del cambio de escena");
            
            // Fuerzo una actualización adicional después de un ratito para asegurar que se aplique bien
            StartCoroutine(ForceAdditionalLightingUpdate(ambientIntensity, directionalIntensity));
        }
    }
    
    // Corrutina adicional para forzar una segunda actualización de iluminación
    private IEnumerator ForceAdditionalLightingUpdate(float ambientIntensity, float directionalIntensity)
    {
        yield return new WaitForSeconds(0.3f);
        
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.SetLightingIntensity(ambientIntensity, directionalIntensity);
            Debug.Log("Actualización adicional de iluminación aplicada");
        }
    }

    private void ShowFinalResults()
    {
        // Este método ya no se usa directamente, la lógica se ha movido a LoadNextRound
        Debug.Log("ShowFinalResults está obsoleto, usar LoadNextRound");
    }

    private IEnumerator ReturnToMenuWithDelay()
    {
        // Espero lo suficiente para que el jugador vea el resultado
        yield return new WaitForSeconds(3f);
        
        // Guardo el estado final antes de resetear
        int finalWinner = GetWinner();
        int finalP1Wins = player1Wins;
        int finalP2Wins = player2Wins;
        Debug.Log("Estado final antes de volver al menú - Ganador: " + finalWinner + ", P1: " + finalP1Wins + ", P2: " + finalP2Wins);
        
        // Primero cargo el menú
        SceneManager.LoadScene("MainMenu");
        
        // Espero a que la escena se cargue completamente
        yield return new WaitForSeconds(0.1f);
        
        // Ahora sí, reseteo el estado
        ResetGameState();
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainMenu");
        ResetGameState();
    }

    public void ResetGameState()
    {
        player1Wins = 0;
        player2Wins = 0;
        roundOver = false;
        player1Won = false;
        roundWinners.Clear();
    }

    public void ResetMatch()
    {
        roundOver = false;
        player1Won = false;
    }

    public int GetWinner()
    {
        // Para el modo FastGame, donde solo hay una ronda, uso el último ganador
        if (!isCampaignMode && roundWinners.Count > 0)
        {
            return roundWinners[roundWinners.Count - 1];
        }

        // Para el modo campaña y otros casos, comparo las victorias totales
        if (player1Wins > player2Wins)
            return 1;
        else if (player2Wins > player1Wins)
            return 2;
        else
            return 0; // Empate
    }

    public void SetCampaignMode(bool isCampaign)
    {
        isCampaignMode = isCampaign;
        nextCampaignMode = isCampaign;
    }
}
