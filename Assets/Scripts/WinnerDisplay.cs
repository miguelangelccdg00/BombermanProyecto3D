using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class WinnerDisplay : MonoBehaviour
{
    public Text winnerText;
    public Text scoreText; // Para mostrar el marcador

    void Start()
    {
        // Intento encontrar el componente Text si no está asignado
        if (winnerText == null)
        {
            winnerText = GetComponent<Text>();
            if (winnerText == null)
            {
                Debug.LogError("WinnerDisplay: No Text component found! Please assign a UI Text component in the inspector.");
                return;
            }
        }

        // Intento encontrar el texto del marcador
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<Text>();
            }
        }

        // Solo muestro el mensaje si estamos en la escena WinnerScene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "WinnerScene")
        {
            // Uso siempre DisplayWinner para forzar el texto correcto
            int winner = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.GetWinner() : 0;
            int p1 = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.player1Wins : 0;
            int p2 = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.player2Wins : 0;
            string winnerStr = winner == 1 ? "Player 1" : (winner == 2 ? "Player 2" : "");
            DisplayWinner(winnerStr, p1, p2);
        }
        else
        {
            if (winnerText != null) winnerText.enabled = false;
            if (scoreText != null) scoreText.enabled = false;
        }
    }

    private void DisplayInitialMessage()
    {
        int winner = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.GetWinner() : 0;
        int p1 = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.player1Wins : 0;
        int p2 = GlobalStateManager.Instance != null ? GlobalStateManager.Instance.player2Wins : 0;

        if (winnerText != null)
        {
            if (winner == 1)
            {
                winnerText.text = "Player 1 Wins!";
            }
            else if (winner == 2)
            {
                winnerText.text = "Player 2 Wins!";
            }
            else
            {
                winnerText.text = "It's a Tie!";
            }
        }
        if (scoreText != null)
        {
            scoreText.text = "Victorias: Player 1 (" + p1 + ") - Player 2 (" + p2 + ")";
            scoreText.enabled = true;
        }
    }

    public void DisplayWinner(string winner, int player1Wins, int player2Wins)
    {
        if (winnerText != null)
        {
            if (string.IsNullOrEmpty(winner))
            {
                winnerText.text = "It's a Tie!";
            }
            else 
            {
                winnerText.text = winner + " Wins!";
            }
            winnerText.enabled = true;
        }

        if (scoreText != null)
        {
            scoreText.text = "Victorias: Player 1 (" + player1Wins + ") - Player 2 (" + player2Wins + ")";
            scoreText.enabled = true;
            Debug.Log("Marcador actualizado: " + scoreText.text);
        }
    }
}
