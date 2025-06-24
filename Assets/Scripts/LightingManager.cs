using UnityEngine;
using UnityEngine.SceneManagement;

public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance;

    // Configuración de iluminación para mantener consistencia entre escenas
    [Header("Configuración de Iluminación")]
    public Color ambientLightColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Color gris claro por defecto
    public float ambientIntensity = 1.0f;
    public float directionalLightIntensity = 1.0f;
    
    [Tooltip("Tiempo de espera en segundos antes de aplicar la iluminación")]
    public float lightingApplyDelay = 0.5f;

    private void Awake()
    {
        // Singleton para acceso global
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantengo entre escenas
        }
        else
        {
            Destroy(gameObject);
        }

        // Me apunto al evento para cuando cambie la escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Aplico configuración inicial
        ApplyLightingSettings();
    }

    private void OnDestroy()
    {
        // Quito el registro del evento al destruir
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Se llama cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Aplico la misma configuración de iluminación en cada escena
        ApplyLightingSettings();
        Debug.Log("Configuración de iluminación aplicada a la escena: " + scene.name);
    }

    // Permite establecer los valores de iluminación ANTES de aplicar la configuración en la nueva escena
    public void SetLightingValues(float ambientValue, float directionalValue)
    {
        ambientIntensity = ambientValue;
        directionalLightIntensity = directionalValue;
    }
    // Aplica la configuración de iluminación a la escena actual
    public void ApplyLightingSettings()
    {
        // Configuro iluminación ambiental
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientIntensity;

        // Espero un ratito para asegurar que todas las luces estén cargadas
        StopAllCoroutines(); // Paro cualquier corrutina anterior para evitar conflictos
        StartCoroutine(ConfigureLightsAfterDelay());
    }

    // Corrutina para configurar las luces después de un poquito de delay
    private System.Collections.IEnumerator ConfigureLightsAfterDelay()
    {
        // Espero un tiempo específico para asegurar que todos los objetos estén inicializados
        yield return new WaitForSeconds(lightingApplyDelay);
        
        // Busco y configuro la luz direccional principal si existe
        Light[] lights = FindObjectsOfType<Light>();
        bool foundDirectionalLight = false;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = directionalLightIntensity;
                foundDirectionalLight = true;
                Debug.Log("Luz direccional configurada con intensidad: " + directionalLightIntensity + " en objeto: " + light.gameObject.name);
            }
        }
        
        if (!foundDirectionalLight)
        {
            Debug.LogWarning("No se encontró ninguna luz direccional en la escena actual. Creando una luz direccional temporal.");
            CreateTemporaryDirectionalLight();
        }
        
        // Fuerzo una segunda actualización después de un poquito más de retraso
        yield return new WaitForSeconds(0.2f);
        
        // Verifico nuevamente las luces direccionales (por si alguna se creó después)
        lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = directionalLightIntensity;
                Debug.Log("Luz direccional actualizada en segunda pasada con intensidad: " + directionalLightIntensity);
            }
        }
    }

    // Creo una luz direccional temporal si no encuentro ninguna en la escena
    private void CreateTemporaryDirectionalLight()
    {
        GameObject lightObj = new GameObject("Temporary Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = directionalLightIntensity;
        light.color = Color.white;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f); // Rotación estándar para luz direccional
        Debug.Log("Luz direccional temporal creada con intensidad: " + directionalLightIntensity);
    }

    // Método público para ajustar la intensidad de la iluminación en tiempo de ejecución
    public void SetLightingIntensity(float ambientValue, float directionalValue)
    {
        ambientIntensity = ambientValue;
        directionalLightIntensity = directionalValue;
        ApplyLightingSettings();
    }
}