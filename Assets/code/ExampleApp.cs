using UnityEngine;

public class ExampleApp : MonoBehaviour
{
    private UserApiClient userApiClient;
    private Environment2DApiClient environment2DApiClient;
    private Object2DApiClient object2DApiClient;
    private WebClient webClient;

    private void Awake()
    {
        // Haal alle componenten op
        webClient = GetComponent<WebClient>();
        userApiClient = GetComponent<UserApiClient>();
        environment2DApiClient = GetComponent<Environment2DApiClient>();
        object2DApiClient = GetComponent<Object2DApiClient>();

        // Check of alles gevonden is
        if (webClient == null)
        {
            Debug.LogError("❌ WebClient component niet gevonden!");
            return;
        }

        // Stel Base URL in
        if (string.IsNullOrEmpty(webClient.baseUrl))
        {
            webClient.baseUrl = "http://localhost:5000";
            Debug.Log("✅ Base URL set to: http://localhost:5000");
        }

        // Koppel WebClient aan alle API clients
        if (userApiClient != null)
        {
            userApiClient.webClient = webClient;
            Debug.Log("✅ UserApiClient linked to WebClient");
        }

        if (environment2DApiClient != null)
        {
            environment2DApiClient.webClient = webClient;
            Debug.Log("✅ Environment2DApiClient linked to WebClient");
        }

        if (object2DApiClient != null)
        {
            object2DApiClient.webClient = webClient;
            Debug.Log("✅ Object2DApiClient linked to WebClient");
        }

        Debug.Log("🎉 All API clients configured!");
    }

    [ContextMenu("User/Register")]
    public async void Register()
    {
        Debug.Log("📤 Sending register request...");
        User user = new User { Email = "test@example.com", Password = "Test1234!" };
        IWebRequestReponse response = await userApiClient.Register(user);
        Debug.Log("📥 Register response: " + response);
    }

    [ContextMenu("User/Login")]
    public async void Login()
    {
        Debug.Log("📤 Sending login request...");
        User user = new User { Email = "test@example.com", Password = "Test1234!" };
        IWebRequestReponse response = await userApiClient.Login(user);
        Debug.Log("📥 Login response: " + response);
    }

    [ContextMenu("Environment2D/Create")]
    public async void CreateEnvironment()
    {
        Debug.Log("📤 Creating environment...");
        Environment2D env = new Environment2D
        {
            Name = "TestWorld",
            MaxHeight = 100,
            MaxLength = 200
        };
        IWebRequestReponse response = await environment2DApiClient.CreateEnvironment(env);
        Debug.Log("📥 Create environment response: " + response);
    }

    [ContextMenu("Environment2D/Read")]
    public async void ReadEnvironments()
    {
        Debug.Log("📤 Reading environments...");
        IWebRequestReponse response = await environment2DApiClient.ReadEnvironment2Ds();
        Debug.Log("📥 Read environments response: " + response);
    }

    [ContextMenu("Object2D/Create")]
    public async void CreateObject2D()
    {
        Debug.Log("📤 Creating Object2D...");
        Object2D obj = new Object2D
        {
            PrefabId = "character_001",
            PositionX = 10.5f,
            PositionY = 20.3f,
            ScaleX = 1.0f,
            ScaleY = 1.0f,
            RotationZ = 0.0f,
            SortingLayer = 1
        };
        IWebRequestReponse response = await object2DApiClient.CreateObject2D(obj);
        Debug.Log("📥 Create Object2D response: " + response);
    }

    [ContextMenu("Object2D/Read")]
    public async void ReadObject2Ds()
    {
        Debug.Log("📤 Reading Object2Ds...");
        IWebRequestReponse response = await object2DApiClient.ReadObject2Ds(); // ✅ GEEN argument!
        Debug.Log("📥 Read Object2Ds response: " + response);
    }

    [ContextMenu("Object2D/Update")]
    public async void UpdateObject2D()
    {
        Debug.Log("📤 Updating Object2D...");
        Object2D obj = new Object2D
        {
            Id = "test-guid-hier", // Vervang met echte GUID
            PrefabId = "character_001",
            PositionX = 15.0f,
            PositionY = 25.0f,
            ScaleX = 1.5f,
            ScaleY = 1.5f,
            RotationZ = 45.0f,
            SortingLayer = 2
        };
        IWebRequestReponse response = await object2DApiClient.UpdateObject2D(obj);
        Debug.Log("📥 Update Object2D response: " + response);
    }
}
