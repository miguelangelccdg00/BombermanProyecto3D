using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectorManager : MonoBehaviour
{
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button backButton;

    void Start()
    {
        // Busco y configuro los botones si no están asignados
        if (level1Button == null)
        {
            GameObject lvl1Obj = GameObject.Find("NIVEL 1");
            if (lvl1Obj != null)
                level1Button = lvl1Obj.GetComponent<Button>();
        }

        if (level2Button == null)
        {
            GameObject lvl2Obj = GameObject.Find("NIVEL 2");
            if (lvl2Obj != null)
                level2Button = lvl2Obj.GetComponent<Button>();
        }

        if (level3Button == null)
        {
            GameObject lvl3Obj = GameObject.Find("NIVEL 3");
            if (lvl3Obj != null)
                level3Button = lvl3Obj.GetComponent<Button>();
        }

        if (backButton == null)
        {
            GameObject backObj = GameObject.Find("VOLVER");
            if (backObj != null)
                backButton = backObj.GetComponent<Button>();
        }

        // Añado los listeners a los botones
        if (level1Button != null)
            level1Button.onClick.AddListener(LoadLevel1);
        else
            Debug.LogError("Botón de Nivel 1 no encontrado!");

        if (level2Button != null)
            level2Button.onClick.AddListener(LoadLevel2);
        else
            Debug.LogError("Botón de Nivel 2 no encontrado!");

        if (level3Button != null)
            level3Button.onClick.AddListener(LoadLevel3);
        else
            Debug.LogError("Botón de Nivel 3 no encontrado!");

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);
        else
            Debug.LogError("Botón de Volver no encontrado!");
    }
    public void LoadLevel1()
    {
        if (GlobalStateManager.Instance != null)
        {
            // No cambio el modo de juego - mantengo el que se estableció desde MainMenu
            // El FastGame ya configuró isCampaignMode = false
            GlobalStateManager.Instance.ResetMatch();
        }
        SceneManager.LoadScene("Game");
    }

    public void LoadLevel2()
    {
        if (GlobalStateManager.Instance != null)
        {
            // No cambio el modo de juego - mantengo el que se estableció desde MainMenu
            GlobalStateManager.Instance.ResetMatch();
        }
        SceneManager.LoadScene("Game 1");
    }

    public void LoadLevel3()
    {
        if (GlobalStateManager.Instance != null)
        {
            // No cambio el modo de juego - mantengo el que se estableció desde MainMenu
            GlobalStateManager.Instance.ResetMatch();
        }
        SceneManager.LoadScene("Game 2");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
