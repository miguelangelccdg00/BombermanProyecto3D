using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    [Range(1, 2)]
    public int playerNumber = 1;

    public float moveSpeed = 5f;
    public bool canDropBombs = true;
    public bool canMove = true;
    public bool dead = false;

    public int lives = 3;

    private Rigidbody rigidBody;
    private Transform myTransform;
    private Animator animator;
    public GameObject bombPrefab;

    // Array de posiciones de respawn para evitar reaparecer siempre en el mismo sitio
    public Transform[] spawnPoints;

    // Referencia al UI Manager para actualizar vidas en pantalla
    public LivesUIManager livesUIManager;
   
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();

        // Inicializo UI después de un ratito para asegurar que LivesUIManager esté listo
        StartCoroutine(InitializeUI());
    }
    
    private IEnumerator InitializeUI()
    {
        // Espero un poco más cuando estamos en modo campaña para darle tiempo a todo a inicializarse
        if (GlobalStateManager.Instance != null)
        {
            Debug.Log("Player " + playerNumber + ": GlobalStateManager encontrado, modo campaña: " + GlobalStateManager.Instance.isCampaignMode);
            if (GlobalStateManager.Instance.isCampaignMode)
            {
                Debug.Log("Player " + playerNumber + ": Esperando delay adicional para modo campaña");
                yield return new WaitForSeconds(0.3f); // Delay más largo para modo campaña
            }
        }
        else
        {
            Debug.LogWarning("Player " + playerNumber + ": GlobalStateManager es nulo, no se puede verificar modo de juego");
            // Espero un poco de todos modos por si acaso
            yield return new WaitForSeconds(0.1f);
        }
        
        // Espero un frame para que todos los objetos se inicialicen
        yield return new WaitForEndOfFrame();
        
        // Busco LivesUIManager si no tengo referencia
        if (livesUIManager == null)
        {
            // Intento obtener la instancia singleton primero
            livesUIManager = LivesUIManager.Instance;
            
            // Si aún no existe, busco por nombre específico según la imagen de la interfaz
            if (livesUIManager == null)
            {
                GameObject livesUIObj = GameObject.Find("Lives UI Manager");
                if (livesUIObj != null)
                {
                    Debug.Log("Player " + playerNumber + ": Encontrado objeto 'Lives UI Manager' directamente");
                    livesUIManager = livesUIObj.GetComponent<LivesUIManager>();
                }
            }
            
            // Si aún no existe, busco en la escena de forma genérica
            if (livesUIManager == null)
            {
                livesUIManager = FindObjectOfType<LivesUIManager>();
                
                // Si aún no se encuentra, creo uno nuevo 
                if (livesUIManager == null)
                {
                    Debug.Log("Player " + playerNumber + ": Creando un nuevo LivesUIManager");
                    GameObject livesUIObj = new GameObject("LivesUIManager");
                    livesUIManager = livesUIObj.AddComponent<LivesUIManager>();
                    
                    // Busco las imágenes de corazones en la escena
                    GameObject player1UI = GameObject.Find("Player1UI");
                    GameObject player2UI = GameObject.Find("Player2UI");
                    
                    if (player1UI != null)
                    {
                        // Activo el UI
                        player1UI.SetActive(true);
                        // Intento con Player1Lives primero
                        Transform heartsParent = player1UI.transform.Find("Player1Lives");
                        
                        // Si no existe, intento con Hearts (nombre alternativo)
                        if (heartsParent == null) {
                            heartsParent = player1UI.transform.Find("Hearts");
                            if (heartsParent != null) {
                                Debug.Log("Usando fallback 'Hearts' para Player 1");
                            }
                        }
                        
                        if (heartsParent != null)
                        {
                            Image[] heartsPlayer1 = heartsParent.GetComponentsInChildren<Image>(true);
                            livesUIManager.player1Lives = heartsPlayer1;
                            Debug.Log("Encontrados " + heartsPlayer1.Length + " corazones para Player 1");
                        }
                        else
                        {
                            Debug.LogWarning("No se encontró Player1Lives ni Hearts dentro de Player1UI");
                            // Listo los hijos para depuración
                            for (int i = 0; i < player1UI.transform.childCount; i++) {
                                Debug.Log("Player1UI child " + i + ": " + player1UI.transform.GetChild(i).name);
                            }
                        }
                    }
                    
                    if (player2UI != null)
                    {
                        // Activar el UI
                        player2UI.SetActive(true);
                        // Intentar con Player2Lives primero
                        Transform heartsParent = player2UI.transform.Find("Player2Lives");
                        
                        // Si no existe, intentar con Hearts (nombre alternativo)
                        if (heartsParent == null) {
                            heartsParent = player2UI.transform.Find("Hearts");
                            if (heartsParent != null) {
                                Debug.Log("Usando fallback 'Hearts' para Player 2");
                            }
                        }
                        
                        if (heartsParent != null)
                        {
                            Image[] heartsPlayer2 = heartsParent.GetComponentsInChildren<Image>(true);
                            livesUIManager.player2Lives = heartsPlayer2;
                            Debug.Log("Encontrados " + heartsPlayer2.Length + " corazones para Player 2");
                        }
                        else
                        {
                            Debug.LogWarning("No se encontró Player2Lives ni Hearts dentro de Player2UI");
                            // Listar los hijos para depuración
                            for (int i = 0; i < player2UI.transform.childCount; i++) {
                                Debug.Log("Player2UI child " + i + ": " + player2UI.transform.GetChild(i).name);
                            }
                        }
                    }
                }
            }
            
            if (livesUIManager == null)
            {
                Debug.LogError("Player " + playerNumber + ": No se pudo crear/encontrar LivesUIManager en la escena");
                yield break;
            }
        }

        // Me aseguro de que los GameObjects de UI estén activos antes de actualizar
        string uiName = "Player" + playerNumber + "UI";
        GameObject playerUI = GameObject.Find(uiName);
        if (playerUI != null && !playerUI.activeSelf)
        {
            Debug.Log("Activando " + uiName + " que estaba desactivado");
            playerUI.SetActive(true);
        }

        // Me aseguro de que tengo las imágenes antes de actualizar la UI
        if ((playerNumber == 1 && (livesUIManager.player1Lives == null || livesUIManager.player1Lives.Length == 0)) ||
            (playerNumber == 2 && (livesUIManager.player2Lives == null || livesUIManager.player2Lives.Length == 0)))
        {
            Debug.LogWarning("Player " + playerNumber + ": Forzando busqueda de imágenes antes de actualizar UI");
            // Doy una última oportunidad para encontrar las imágenes
            GameObject livesUIObj = GameObject.Find("Lives UI Manager");
            if (livesUIObj != null)
            {
                Debug.Log("Player " + playerNumber + ": Encontrado Lives UI Manager en la escena");
                // Si ya existe en la escena, uso esa instancia
                LivesUIManager existingManager = livesUIObj.GetComponent<LivesUIManager>();
                if (existingManager != null && existingManager != livesUIManager)
                {
                    Debug.Log("Player " + playerNumber + ": Reemplazando referencia al LivesUIManager existente");
                    livesUIManager = existingManager;
                }
            }
        }

        // Actualizo UI con las vidas completas
        livesUIManager.UpdateLivesUI(playerNumber, lives);
        Debug.Log("Player " + playerNumber + ": UI inicializada con " + lives + " vidas");
    }

    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        animator.SetBool("Walking", false);

        if (!canMove || dead)
            return;

        if (playerNumber == 1)
            UpdatePlayer1Movement();
        else
            UpdatePlayer2Movement();
    }

    private void UpdatePlayer1Movement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, moveSpeed);
            myTransform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, 0);
            myTransform.rotation = Quaternion.Euler(0, 270, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, -moveSpeed);
            myTransform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, 0);
            myTransform.rotation = Quaternion.Euler(0, 90, 0);
            animator.SetBool("Walking", true);
        }
        else
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
        }

        if (canDropBombs && Input.GetKeyDown(KeyCode.Space))
            DropBomb();
    }

    private void UpdatePlayer2Movement()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, moveSpeed);
            myTransform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, 0);
            myTransform.rotation = Quaternion.Euler(0, 270, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, -moveSpeed);
            myTransform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetBool("Walking", true);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, 0);
            myTransform.rotation = Quaternion.Euler(0, 90, 0);
            animator.SetBool("Walking", true);
        }
        else
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
        }

        if (canDropBombs && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
            DropBomb();
    }

    private void DropBomb()
    {
        if (bombPrefab)
        {
            Instantiate(
                bombPrefab,
                new Vector3(Mathf.RoundToInt(myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
                bombPrefab.transform.rotation
            );
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion") && !dead)
        {
            Debug.Log("P" + playerNumber + " hit by explosion!");
            LoseLife();
        }
    }

    private void LoseLife()
    {
        if (dead) return;

        lives--;
        Debug.Log("P" + playerNumber + " lost a life. Remaining lives: " + lives);

        // Actualizo la UI cada vez que pierde vida
        if (livesUIManager != null)
        {
            livesUIManager.UpdateLivesUI(playerNumber, lives);
        }
        else
        {
            // Si por alguna razón no tengo referencia, intento obtenerla de nuevo
            livesUIManager = LivesUIManager.Instance;
            if (livesUIManager == null)
            {
                // Último intento: busco en la escena
                livesUIManager = FindObjectOfType<LivesUIManager>();
            }
            
            if (livesUIManager != null)
            {
                livesUIManager.UpdateLivesUI(playerNumber, lives);
                Debug.Log("P" + playerNumber + ": Recuperada referencia a LivesUIManager");
            }
            else
            {
                Debug.LogError("P" + playerNumber + ": No se pudo actualizar UI de vidas, LivesUIManager no encontrado");
            }
        }

        if (lives <= 0)
        {
            dead = true;
            canMove = false;
            rigidBody.velocity = Vector3.zero;
            Debug.Log("P" + playerNumber + " has no lives left!");

            // Mejor no desactivo GameObject aquí
            if (GlobalStateManager.Instance != null)
                GlobalStateManager.Instance.PlayerDied(playerNumber);
        }
        else
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    private IEnumerator RespawnPlayer()
    {
        canMove = false;
        animator.SetBool("Walking", false);
        rigidBody.velocity = Vector3.zero;

        SetPlayerVisible(false);

        yield return new WaitForSeconds(1.5f);

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[index].position;
        }
        else
        {
            transform.position = new Vector3(0, transform.position.y, 0);
        }

        dead = false;  // Lo pongo aquí, justo antes de reaparecer
        canMove = true;
        SetPlayerVisible(true);
    }

    private void SetPlayerVisible(bool visible)
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.enabled = visible;

        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = visible;
    }
}