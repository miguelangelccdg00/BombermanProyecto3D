using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LivesUIManager : MonoBehaviour
{
    public static LivesUIManager Instance;

    [Tooltip("Corazones que representan las vidas del jugador 1")]
    public Image[] player1Lives;

    [Tooltip("Corazones que representan las vidas del jugador 2")]
    public Image[] player2Lives;

    private void Awake()
    {
        // Singleton para acceso global
        if (Instance == null)
        {
            Instance = this;
            
            // Evito que sea destruido al cambiar de escena
            DontDestroyOnLoad(gameObject);
            
            // Verifico si somos el LivesUIManager de la interfaz de Unity
            CheckIfUILivesManager();
            
            // Busco los componentes de UI inmediatamente
            EnsureUIComponentsExist();
        }
        else if (Instance != this)
        {
            Debug.Log("Ya existe un LivesUIManager. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }
        
        // Me apunto para eventos de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log("LivesUIManager inicializado como singleton");
    }
    
    // Verifico si este componente es el LivesUIManager de la interfaz de Unity
    private void CheckIfUILivesManager()
    {
        // Compruebo si este objeto tiene el nombre esperado en la interfaz
        if (gameObject.name == "Lives UI Manager")
        {
            Debug.Log("Este LivesUIManager es el componente de UI del editor");
            
            // Busco si ya tiene asignadas las imágenes de corazones en el inspector
            if (player1Lives == null || player1Lives.Length == 0)
            {
                Transform player1LivesTransform = transform.Find("Player 1 Lives");
                if (player1LivesTransform != null)
                {
                    Image[] hearts = player1LivesTransform.GetComponentsInChildren<Image>(true);
                    if (hearts != null && hearts.Length > 0)
                    {
                        player1Lives = hearts;
                        Debug.Log("Encontradas " + hearts.Length + " imágenes para Player 1 Lives en el inspector");
                    }
                }
            }
            
            if (player2Lives == null || player2Lives.Length == 0)
            {
                Transform player2LivesTransform = transform.Find("Player 2 Lives");
                if (player2LivesTransform != null)
                {
                    Image[] hearts = player2LivesTransform.GetComponentsInChildren<Image>(true);
                    if (hearts != null && hearts.Length > 0)
                    {
                        player2Lives = hearts;
                        Debug.Log("Encontradas " + hearts.Length + " imágenes para Player 2 Lives en el inspector");
                    }
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Limpio suscripción al destruirse
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si cambiamos a una escena de juego, me aseguro que la UI se inicialice
        if (scene.name.StartsWith("Game") || scene.name == "Game")
        {
            Debug.Log("LivesUIManager: Escena de juego detectada, inicializando UI");
            StartCoroutine(InitializeUIAfterDelay(0.1f));
            
            // Si estamos en modo campaña, me aseguro de que la UI se inicialice correctamente
            if (GlobalStateManager.Instance != null && GlobalStateManager.Instance.isCampaignMode)
            {
                Debug.Log("LivesUIManager: Modo campaña detectado, inicialización adicional");
                StartCoroutine(InitializeUIAfterDelay(0.3f));
            }
        }
    }
    
    private void Start()
    {
        // Me aseguro de que la UI se inicialice correctamente al cargar la escena
        StartCoroutine(InitializeUIAfterDelay());
    }
    
    private IEnumerator InitializeUIAfterDelay(float additionalDelay = 0f)
    {
        // Espero el delay adicional si se especifica
        if (additionalDelay > 0)
        {
            Debug.Log("LivesUIManager: Esperando delay adicional de " + additionalDelay + " segundos");
            yield return new WaitForSeconds(additionalDelay);
        }
        
        // Si estamos en modo campaña, me aseguro de esperar suficiente
        if (GlobalStateManager.Instance != null && GlobalStateManager.Instance.isCampaignMode)
        {
            Debug.Log("LivesUIManager: En modo campaña, esperando frame adicional");
            yield return new WaitForEndOfFrame(); // Frame adicional para modo campaña
        }
        
        // Espero un frame para que los Players se inicialicen
        yield return new WaitForEndOfFrame();
        
        // Me aseguro de que tengo referencias a los arrays de corazones
        EnsureUIComponentsExist();
        
        // Busco players en la escena e inicializo UI
        Player[] players = FindObjectsOfType<Player>();
        
        if (players.Length == 0)
        {
            Debug.LogWarning("LivesUIManager: No se encontraron jugadores en la escena");
            yield break;
        }
        
        foreach (Player player in players)
        {
            // Me aseguro de que los GameObjects de UI estén activos
            string uiName = "Player" + player.playerNumber + "UI";
            GameObject playerUI = GameObject.Find(uiName);
            if (playerUI != null)
            {
                if (!playerUI.activeSelf)
                {
                    Debug.Log("LivesUIManager: Activando " + uiName + " que estaba desactivado");
                    playerUI.SetActive(true);
                }
                
                // Si no tengo los corazones del jugador actual, los busco
                if ((player.playerNumber == 1 && (player1Lives == null || player1Lives.Length == 0)) || 
                    (player.playerNumber == 2 && (player2Lives == null || player2Lives.Length == 0)))
                {
                    // Intento con la estructura Player1Lives/Player2Lives primero
                Transform heartsParent = playerUI.transform.Find("Player" + player.playerNumber + "Lives");
                
                // Si no existe, intento con Hearts (nombre alternativo)
                if (heartsParent == null) {
                    heartsParent = playerUI.transform.Find("Hearts");
                    if (heartsParent != null) {
                        Debug.Log("LivesUIManager: Usando fallback 'Hearts' para Player " + player.playerNumber);
                    }
                }
                
                if (heartsParent != null)
                {
                    Image[] hearts = heartsParent.GetComponentsInChildren<Image>(true);
                    if (player.playerNumber == 1)
                        player1Lives = hearts;
                    else
                        player2Lives = hearts;
                }
                }
            }
            else
            {
                Debug.LogError("LivesUIManager: No se encontró " + uiName + " en la escena");
            }
            
            UpdateLivesUI(player.playerNumber, player.lives);
            Debug.Log("LivesUIManager: UI inicializada para Player " + player.playerNumber + " con " + player.lives + " vidas");
        }
    }

    /// <summary>
    /// Actualiza la interfaz gráfica de las vidas visibles para un jugador.
    /// </summary>
    /// <param name="playerNumber">1 o 2</param>
    /// <param name="livesRemaining">Número actual de vidas</param>
    public void UpdateLivesUI(int playerNumber, int livesRemaining)
    {
        // Intento obtener las referencias si no las tengo
        if ((playerNumber == 1 && (player1Lives == null || player1Lives.Length == 0)) ||
            (playerNumber == 2 && (player2Lives == null || player2Lives.Length == 0)))
        {
            Debug.Log("UpdateLivesUI: Intentando conseguir componentes UI para Player " + playerNumber);
            EnsureUIComponentsExist();
        }
        
        Image[] targetArray = playerNumber == 1 ? player1Lives : player2Lives;

        // Me aseguro de que el array no sea nulo
        if (targetArray == null || targetArray.Length == 0)
        {
            Debug.LogError("Hearts array is null or empty for player " + playerNumber);
            
            // Último intento de obtener las imágenes
            string uiName = "Player " + playerNumber + " Lives";
            GameObject playerLivesObj = GameObject.Find(uiName);
            
            if (playerLivesObj != null)
            {
                Debug.Log("UpdateLivesUI: Último intento - Encontrado objeto " + uiName);
                Image[] foundImages = playerLivesObj.GetComponentsInChildren<Image>(true);
                
                if (foundImages != null && foundImages.Length > 0)
                {
                    if (playerNumber == 1)
                        player1Lives = foundImages;
                    else
                        player2Lives = foundImages;
                    
                    targetArray = foundImages;
                    Debug.Log("UpdateLivesUI: Recuperadas " + foundImages.Length + " imágenes para Player " + playerNumber);
                }
                else
                {
                    Debug.LogError("UpdateLivesUI: No se encontraron imágenes en " + uiName);
                    return;
                }
            }
            else
            {
                Debug.LogError("UpdateLivesUI: Último intento fallido - No se encuentra " + uiName);
                return;
            }
        }

        // Imprimo información de depuración
        Debug.Log("Updating UI for Player " + playerNumber + " with " + livesRemaining + " lives remaining");
        
        for (int i = 0; i < targetArray.Length; i++)
        {
            // Activo o desactivo el corazón según si la vida está activa
            if (targetArray[i] != null)
            {
                // Me aseguro de que el GameObject esté activo
                if (!targetArray[i].gameObject.activeInHierarchy)
                {
                    targetArray[i].gameObject.SetActive(true);
                }
                
                // En lugar de desactivar el componente, cambio la transparencia
                Color imageColor = targetArray[i].color;
                imageColor.a = i < livesRemaining ? 1f : 0f; // Alpha 1 (visible) o 0 (invisible)
                targetArray[i].color = imageColor;
                
                // Mantengo el componente activo para no afectar el layout
                targetArray[i].enabled = true;
                
                Debug.Log("Player " + playerNumber + " heart " + i + " alpha set to " + (i < livesRemaining ? "1" : "0"));
            }
            else
            {
                Debug.LogWarning("Heart image at index " + i + " is null for player " + playerNumber);
            }
        }
    }
    
    private void EnsureUIComponentsExist()
    {
        // Busco el Global State Manager y su Canvas
        GameObject gsm = GameObject.Find("Global State Manager");
        if (gsm != null)
        {
            Transform canvas = gsm.transform.Find("Canvas");
            if (canvas != null)
            {
                // --- PLAYER 1 ---
                if (player1Lives == null || player1Lives.Length == 0)
                {
                    Transform player1UI = canvas.Find("Player1UI");
                    if (player1UI != null)
                    {
                        player1UI.gameObject.SetActive(true);
                        Transform lives = player1UI.Find("Player1Lives");
                        if (lives != null)
                        {
                            player1Lives = lives.GetComponentsInChildren<Image>(true);
                            Debug.Log("LivesUIManager: Asignados corazones de Player1 desde Global State Manager");
                        }
                        else
                        {
                            Debug.LogError("LivesUIManager: No se encontró Player1Lives bajo Player1UI en Global State Manager");
                        }
                    }
                    else
                    {
                        Debug.LogError("LivesUIManager: No se encontró Player1UI bajo Canvas en Global State Manager");
                    }
                }
                // --- PLAYER 2 ---
                if (player2Lives == null || player2Lives.Length == 0)
                {
                    Transform player2UI = canvas.Find("Player2UI");
                    if (player2UI != null)
                    {
                        player2UI.gameObject.SetActive(true);
                        Transform lives = player2UI.Find("Player2Lives");
                        if (lives != null)
                        {
                            player2Lives = lives.GetComponentsInChildren<Image>(true);
                            Debug.Log("LivesUIManager: Asignados corazones de Player2 desde Global State Manager");
                        }
                        else
                        {
                            Debug.LogError("LivesUIManager: No se encontró Player2Lives bajo Player2UI en Global State Manager");
                        }
                    }
                    else
                    {
                        Debug.LogError("LivesUIManager: No se encontró Player2UI bajo Canvas en Global State Manager");
                    }
                }
                return; // Si encuentro el Canvas en GSM, no sigo buscando en otros lados
            }
        }
        
        // Si no tengo referencias a los corazones, los busco en la escena
        if (player1Lives == null || player1Lives.Length == 0)
        {
            GameObject player1UI = FindInAllScenes("Player1UI");
            if (player1UI != null)
            {
                player1UI.SetActive(true);
                
                // Intento primero obtener las imágenes directamente
                player1Lives = GetHeartImages(1);
                
                // Si no se encuentran por el método directo, busco por jerarquía
                if (player1Lives == null || player1Lives.Length == 0)
                {
                    // Intento con Player1Lives primero
                    Transform heartsParent = player1UI.transform.Find("Player1Lives");
                    
                    // Si no existe, intento con Hearts (nombre alternativo)
                    if (heartsParent == null) {
                        heartsParent = player1UI.transform.Find("Hearts");
                        if (heartsParent != null) {
                            Debug.Log("LivesUIManager: Usando fallback 'Hearts' para Player 1");
                        }
                    }
                    
                    if (heartsParent != null)
                    {
                        player1Lives = heartsParent.GetComponentsInChildren<Image>(true);
                        Debug.Log("LivesUIManager: Encontrados " + player1Lives.Length + " corazones para Player 1");
                    }
                    else
                    {
                        Debug.LogError("LivesUIManager: No se encontró el objeto Player1Lives ni Hearts dentro de Player1UI");
                        // Intento listar los hijos para depuración
                        for (int i = 0; i < player1UI.transform.childCount; i++) {
                            Debug.Log("Player1UI child " + i + ": " + player1UI.transform.GetChild(i).name);
                        }
                        
                        // Como último recurso, creo imágenes de corazón dinámicamente
                        if (player1Lives == null || player1Lives.Length == 0) {
                            CreateHeartImages(player1UI.transform, 1);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("LivesUIManager: No se encontró Player1UI en la escena");
            }
        }
        
        if (player2Lives == null || player2Lives.Length == 0)
        {
            GameObject player2UI = FindInAllScenes("Player2UI");
            if (player2UI != null)
            {
                player2UI.SetActive(true);
                
                // Intentar primero obtener las imágenes directamente
                player2Lives = GetHeartImages(2);
                
                // Si no se encuentran por el método directo, buscar por jerarquía
                if (player2Lives == null || player2Lives.Length == 0)
                {
                    // Intento con Player2Lives primero
                    Transform heartsParent = player2UI.transform.Find("Player2Lives");
                    
                    // Si no existe, intento con Hearts (nombre alternativo)
                    if (heartsParent == null) {
                        heartsParent = player2UI.transform.Find("Hearts");
                        if (heartsParent != null) {
                            Debug.Log("LivesUIManager: Usando fallback 'Hearts' para Player 2");
                        }
                    }
                    
                    if (heartsParent != null)
                    {
                        player2Lives = heartsParent.GetComponentsInChildren<Image>(true);
                        Debug.Log("LivesUIManager: Encontrados " + player2Lives.Length + " corazones para Player 2");
                    }
                    else
                    {
                        Debug.LogError("LivesUIManager: No se encontró el objeto Player2Lives ni Hearts dentro de Player2UI");
                        // Intento listar los hijos para depuración
                        for (int i = 0; i < player2UI.transform.childCount; i++) {
                            Debug.Log("Player2UI child " + i + ": " + player2UI.transform.GetChild(i).name);
                        }
                        
                        // Como último recurso, creo imágenes de corazón dinámicamente
                        if (player2Lives == null || player2Lives.Length == 0) {
                            CreateHeartImages(player2UI.transform, 2);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("LivesUIManager: No se encontró Player2UI en la escena");
            }
        }
    }

    // Para obtener imágenes de corazón de un jugador específico
    private Image[] GetHeartImages(int playerNumber)
    {
        string livesObjName = "Player " + playerNumber + " Lives";
        GameObject livesObj = GameObject.Find(livesObjName);
        
        if (livesObj != null)
        {
            Image[] hearts = livesObj.GetComponentsInChildren<Image>(true);
            if (hearts != null && hearts.Length > 0)
            {
                Debug.Log("LivesUIManager: Encontradas " + hearts.Length + " imágenes para " + livesObjName);
                return hearts;
            }
        }
        
        Debug.Log("LivesUIManager: No se encontraron imágenes para " + livesObjName + " por búsqueda directa");
        return null;
    }
    
    // Para crear imágenes de corazón dinámicamente si no se encuentran en la escena
    private void CreateHeartImages(Transform parentTransform, int playerNumber)
    {
        Debug.Log("LivesUIManager: Creando imágenes de corazón para Player " + playerNumber);
        
        // Creo un objeto padre para las imágenes de corazón
        GameObject livesHolder = new GameObject("Player" + playerNumber + "Lives");
        livesHolder.transform.SetParent(parentTransform, false);
        
        // Creo tres imágenes de corazón (esto requiere que tengas un sprite de corazón en tu proyecto)
        Image[] heartImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject heartObj = new GameObject("Heart" + (i+1));
            heartObj.transform.SetParent(livesHolder.transform, false);
            
            // Añado componente de imagen
            Image heartImage = heartObj.AddComponent<Image>();
            
            // Intento cargar un sprite de corazón (esto depende de la estructura de tu proyecto)
            // Si tienes una imagen de corazón en tu proyecto, reemplaza la ruta
            Sprite heartSprite = Resources.Load<Sprite>("Heart");
            if (heartSprite != null)
            {
                heartImage.sprite = heartSprite;
            }
            else
            {
                Debug.LogWarning("LivesUIManager: No se encontró un sprite de corazón, usando un color sólido");
                heartImage.color = Color.red;
            }
            
            // Configuro el RectTransform para que tenga un tamaño adecuado
            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(30, 30);
            rectTransform.anchoredPosition = new Vector2(i * 35, 0);
            
            heartImages[i] = heartImage;
        }
        
        // Asigno las imágenes al array correspondiente
        if (playerNumber == 1)
            player1Lives = heartImages;
        else
            player2Lives = heartImages;
        
        Debug.Log("LivesUIManager: Creadas " + heartImages.Length + " imágenes de corazón para Player " + playerNumber);
    }
    
    // Método auxiliar para buscar un objeto por nombre en toda la jerarquía, incluyendo DontDestroyOnLoad
    private GameObject FindInAllScenes(string name)
    {
        Debug.Log("LivesUIManager: Buscando objeto UI '" + name + "' en todas las escenas...");
        
        // Lista de nombres alternativos que podrían corresponder al objeto
        List<string> alternativeNames = new List<string>();
        if (name == "Player1UI") 
        {
            alternativeNames.Add("Player 1 UI");
            alternativeNames.Add("UI Player 1");
            alternativeNames.Add("PlayerUI_1");
            alternativeNames.Add("P1UI");
        }
        else if (name == "Player2UI") 
        {
            alternativeNames.Add("Player 2 UI");
            alternativeNames.Add("UI Player 2");
            alternativeNames.Add("PlayerUI_2");
            alternativeNames.Add("P2UI");
        }
        
        // 1. Intento búsqueda directa primero
        GameObject obj = GameObject.Find(name);
        if (obj != null) {
            Debug.Log("LivesUIManager: Encontrado " + name + " mediante búsqueda directa");
            return obj;
        }
        
        // 1.5 Intento con nombres alternativos
        foreach (string altName in alternativeNames) {
            GameObject altObj = GameObject.Find(altName);
            if (altObj != null) {
                Debug.Log("LivesUIManager: Encontrado objeto alternativo '" + altName + "' para '" + name + "'");
                return altObj;
            }
        }
        
        // 2. Busco en la escena activa
        Debug.Log("LivesUIManager: Buscando " + name + " en los objetos root de la escena activa");
        foreach(GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            // Busco por nombre exacto
            if (rootObj.name == name) return rootObj;
            
            // Busco en los hijos profundos
            Transform found = rootObj.transform.FindDeepChild(name);
            if (found != null) {
                Debug.Log("LivesUIManager: Encontrado " + name + " como hijo profundo de " + rootObj.name);
                return found.gameObject;
            }
            
            // Busco por nombres alternativos en hijos
            foreach (string altName in alternativeNames) {
                Transform altFound = rootObj.transform.FindDeepChild(altName);
                if (altFound != null) {
                    Debug.Log("LivesUIManager: Encontrado objeto alternativo '" + altName + "' para '" + name + "'");
                    return altFound.gameObject;
                }
            }
        }
        
        // 3. Busco en objetos DontDestroyOnLoad
        Debug.Log("LivesUIManager: Buscando " + name + " en objetos DontDestroyOnLoad");
        GameObject tempObj = new GameObject("__TempFinderObject");
        DontDestroyOnLoad(tempObj);
        Scene dontDestroyScene = tempObj.scene;
        DestroyImmediate(tempObj);
        
        foreach(GameObject rootObj in dontDestroyScene.GetRootGameObjects())
        {
            if (rootObj.name == name) {
                Debug.Log("LivesUIManager: Encontrado " + name + " en DontDestroyOnLoad");
                return rootObj;
            }
            
            // Busco en hijos profundos
            Transform found = rootObj.transform.FindDeepChild(name);
            if (found != null) {
                Debug.Log("LivesUIManager: Encontrado " + name + " como hijo en DontDestroyOnLoad");
                return found.gameObject;
            }
            
            // Busco por nombres alternativos
            foreach (string altName in alternativeNames) {
                if (rootObj.name == altName) {
                    Debug.Log("LivesUIManager: Encontrado objeto DontDestroyOnLoad alternativo '" + altName + "' para '" + name + "'");
                    return rootObj;
                }
                
                Transform altFound = rootObj.transform.FindDeepChild(altName);
                if (altFound != null) {
                    Debug.Log("LivesUIManager: Encontrado objeto hijo alternativo '" + altName + "' en DontDestroyOnLoad");
                    return altFound.gameObject;
                }
            }
        }

        // 4. Busco específicamente en Canvas y objetos UI
        Debug.Log("LivesUIManager: Buscando " + name + " en Canvas y objetos UI");
        Canvas[] allCanvases = GameObject.FindObjectsOfType(typeof(Canvas)) as Canvas[];
        foreach(Canvas canvas in allCanvases)
        {
            Debug.Log("LivesUIManager: Examinando canvas: " + canvas.name);
            
            // Compruebo si el Canvas mismo es lo que busco
            if (canvas.name == name) {
                Debug.Log("LivesUIManager: El canvas mismo es " + name);
                return canvas.gameObject;
            }
            
            // Busco en los hijos del Canvas
            Transform found = canvas.transform.FindDeepChild(name);
            if (found != null)
            {
                Debug.Log("LivesUIManager: Encontrado " + name + " bajo el canvas: " + canvas.name);
                return found.gameObject;
            }
            
            // Busco por nombres alternativos
            foreach (string altName in alternativeNames) {
                if (canvas.name == altName) {
                    Debug.Log("LivesUIManager: Canvas con nombre alternativo '" + altName + "' para '" + name + "'");
                    return canvas.gameObject;
                }
                
                Transform altFound = canvas.transform.FindDeepChild(altName);
                if (altFound != null) {
                    Debug.Log("LivesUIManager: Encontrado objeto alternativo '" + altName + "' bajo canvas " + canvas.name);
                    return altFound.gameObject;
                }
            }
            
            // Listo todos los hijos para depuración
            Debug.Log("LivesUIManager: Listando todos los hijos directos del canvas " + canvas.name + ":");
            for (int i = 0; i < canvas.transform.childCount; i++) {
                Debug.Log("  - " + canvas.transform.GetChild(i).name);
            }
        }
        
        // 5. Busco objetos con componentes UI que podrían ser lo que busco
        Debug.Log("LivesUIManager: Buscando en todos los componentes UI");
        GameObject[] allObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject go in allObjects) {
            // Si el objeto tiene componentes UI y su nombre contiene indicadores
            if ((go.GetComponent<RectTransform>() != null || 
                go.GetComponent<Canvas>() != null ||
                go.GetComponent<CanvasGroup>() != null) && 
                (go.name.ToLower().Contains("player") || go.name.ToLower().Contains("ui"))) {
                
                Debug.Log("LivesUIManager: Posible candidato para " + name + ": " + go.name);
                
                // Si busco Player1UI y el nombre contiene Player y 1, probablemente es lo que busco
                if (name == "Player1UI" && go.name.ToLower().Contains("player") && go.name.Contains("1")) {
                    Debug.Log("LivesUIManager: Encontrado probable Player1UI: " + go.name);
                    return go;
                }
                
                // Si busco Player2UI y el nombre contiene Player y 2, probablemente es lo que busco
                if (name == "Player2UI" && go.name.ToLower().Contains("player") && go.name.Contains("2")) {
                    Debug.Log("LivesUIManager: Encontrado probable Player2UI: " + go.name);
                    return go;
                }
            }
        }

        // 6. Busco en el Global State Manager
        Debug.Log("LivesUIManager: Buscando " + name + " en el Global State Manager");
        GameObject gsm = GameObject.Find("Global State Manager");
        if (gsm != null)
        {
            Transform found = gsm.transform.FindDeepChild(name);
            if (found != null) {
                Debug.Log("LivesUIManager: Encontrado " + name + " en Global State Manager");
                return found.gameObject;
            }
            
            // Busco por nombres alternativos
            foreach (string altName in alternativeNames) {
                Transform altFound = gsm.transform.FindDeepChild(altName);
                if (altFound != null) {
                    Debug.Log("LivesUIManager: Encontrado objeto alternativo '" + altName + "' en Global State Manager");
                    return altFound.gameObject;
                }
            }
        }

        Debug.LogWarning("[LivesUIManager] No se pudo encontrar " + name + " en ninguna parte de la jerarquía");
        return null;
    }
}

// Extensión para búsqueda profunda en jerarquía
public static class TransformDeepChildExtension
{
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        foreach(Transform child in aParent)
        {
            if(child.name == aName )
                return child;
            var result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
}
