using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

// ================= UNIEKE API MODELLEN (Hernoemd om alle fouten in Unity te voorkomen!) =================
[System.Serializable]
public class ApiUserCredentials
{
    public string userName;
    public string password;
}

[System.Serializable]
public class ApiEnvironment
{
    public string id;
    public string name;
    public int maxHeight;
    public int maxLength;
    public string userId;
}

[System.Serializable]
public class ApiObject
{
    public string id;
    public string environment2DId;
    public int prefabIndex;
    public float x;
    public float y;
}

// ================= AUTH MANAGER =================
public class AuthManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text registerErrorText;

    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginErrorText;

    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject homePanel;
    public GameObject worldPanel;

    public TMP_InputField worldNameInput;
    public TMP_Text worldErrorText;

    public Transform worldsContainer;
    public GameObject worldButtonPrefab;

    public TMP_Text worldTitleText;

    public GameObject[] availablePrefabs;

    public Transform worldObjectsContainer;

    private string currentUser;
    private string currentEnvironmentId;
    private string currentEnvironmentName;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    // De kloppende Live Render Base URL (Zonder schuine streep op het einde!)
    private readonly string baseUrl = "https://mysecurebackend.onrender.com";

    // ================= REGISTER & LOGIN =================
    public void Register()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (!IsValidPassword(password))
        {
            registerErrorText.text = "Wachtwoord voldoet niet aan de eisen (min. 10 tekens, 1H, 1K, 1C, 1 Speciaalteken).";
            return;
        }

        registerErrorText.text = "Bezig met registreren...";
        StartCoroutine(RegisterUserCoroutine(username, password));
    }

    private IEnumerator RegisterUserCoroutine(string username, string password)
    {
        string url = $"{baseUrl}/api/users/register";
        string jsonData = JsonUtility.ToJson(new ApiUserCredentials { userName = username, password = password });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                registerErrorText.text = "Registratie succesvol! Je kunt nu inloggen.";
                ShowLogin();
            }
            else
            {
                registerErrorText.text = "Fout bij registratie: " + request.error;
            }
        }
    }

    public void Login()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        loginErrorText.text = "Bezig met inloggen...";
        StartCoroutine(LoginUserCoroutine(username, password));
    }

    private IEnumerator LoginUserCoroutine(string username, string password)
    {
        string url = $"{baseUrl}/api/users/login";
        string jsonData = JsonUtility.ToJson(new ApiUserCredentials { userName = username, password = password });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                currentUser = username;

                loginErrorText.text = "";
                loginPanel.SetActive(false);
                homePanel.SetActive(true);
                LoadEnvironments();
            }
            else
            {
                loginErrorText.text = "Gebruikersnaam of wachtwoord incorrect.";
            }
        }
    }

    bool IsValidPassword(string password)
    {
        if (password.Length < 10) return false;
        return Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") &&
               Regex.IsMatch(password, "[0-9]") && Regex.IsMatch(password, "[^a-zA-Z0-9]");
    }

    // ================= WERELDEN (Environment2D) =================
    public void CreateWorld()
    {
        string envName = worldNameInput.text;
        if (string.IsNullOrEmpty(envName) || envName.Length > 25)
        {
            worldErrorText.text = "Naam moet 1-25 karakters zijn.";
            return;
        }
        worldErrorText.text = "Wereld aanmaken...";
        StartCoroutine(CreateEnvironmentCoroutine(envName));
    }

    private IEnumerator CreateEnvironmentCoroutine(string envName)
    {
        string url = $"{baseUrl}/Environment2D";

        ApiEnvironment newEnv = new ApiEnvironment
        {
            name = envName,
            maxHeight = 10,
            maxLength = 10,
            userId = currentUser
        };

        string jsonData = JsonUtility.ToJson(newEnv);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                worldNameInput.text = "";
                worldErrorText.text = "";
                LoadEnvironments();
            }
            else
            {
                worldErrorText.text = "Fout bij aanmaken wereld.";
                Debug.LogError(request.error);
            }
        }
    }

    void LoadEnvironments()
    {
        StartCoroutine(LoadEnvironmentsCoroutine());
    }

    private IEnumerator LoadEnvironmentsCoroutine()
    {
        string url = $"{baseUrl}/Environment2D";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                foreach (Transform child in worldsContainer)
                {
                    Destroy(child.gameObject);
                }

                ApiEnvironment[] environments = CustomJsonHelper.FromJson<ApiEnvironment>(request.downloadHandler.text);

                if (environments != null)
                {
                    foreach (ApiEnvironment env in environments)
                    {
                        if (env.userId != currentUser) continue;

                        GameObject button = Instantiate(worldButtonPrefab, worldsContainer);
                        button.GetComponentInChildren<TMP_Text>().text = env.name;

                        string clickEnvId = env.id;
                        string clickEnvName = env.name;
                        button.GetComponent<Button>().onClick.AddListener(() => OpenWorld(clickEnvId, clickEnvName));
                    }
                }
            }
        }
    }

    public void OpenWorld(string envId, string envName)
    {
        currentEnvironmentId = envId;
        currentEnvironmentName = envName;

        ClearWorldObjects();

        homePanel.SetActive(false);
        worldPanel.SetActive(true);
        worldTitleText.text = "Wereld: " + envName;

        LoadObjects();
    }

    public void DeleteWorld()
    {
        StartCoroutine(DeleteEnvironmentCoroutine());
    }

    private IEnumerator DeleteEnvironmentCoroutine()
    {
        string url = $"{baseUrl}/Environment2D/{currentEnvironmentId}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ClearWorldObjects();
                worldPanel.SetActive(false);
                homePanel.SetActive(true);
                LoadEnvironments();
            }
            else
            {
                Debug.LogError("Fout bij verwijderen wereld.");
            }
        }
    }

    // ================= OBJECTEN (Object2D) =================
    public void AddObject(int index)
    {
        SpawnObject(index, new Vector3(0, 0, 0));
    }

    void SpawnObject(int index, Vector3 position)
    {
        if (index < 0 || index >= availablePrefabs.Length) return;

        GameObject obj = Instantiate(availablePrefabs[index], position, Quaternion.identity);
        obj.transform.SetParent(worldObjectsContainer);

        if (obj.GetComponent<Collider2D>() == null) obj.AddComponent<BoxCollider2D>();

        spawnedObjects.Add(obj);
    }

    void LoadObjects()
    {
        StartCoroutine(LoadObjectsCoroutine());
    }

    private IEnumerator LoadObjectsCoroutine()
    {
        string url = $"{baseUrl}/Object2D";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ApiObject[] allObjects = CustomJsonHelper.FromJson<ApiObject>(request.downloadHandler.text);

                if (allObjects != null)
                {
                    foreach (ApiObject objData in allObjects)
                    {
                        if (objData.environment2DId != currentEnvironmentId) continue;

                        Vector3 pos = new Vector3(objData.x, objData.y, 0f);
                        SpawnObject(objData.prefabIndex, pos);
                    }
                }
            }
        }
    }

    public void SaveWorldObjects()
    {
        StartCoroutine(SaveObjectsCoroutine());
    }

    private IEnumerator SaveObjectsCoroutine()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            int index = -1;
            for (int i = 0; i < availablePrefabs.Length; i++)
            {
                if (obj.name.Contains(availablePrefabs[i].name))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1) continue;

            ApiObject objectData = new ApiObject
            {
                environment2DId = currentEnvironmentId,
                prefabIndex = index,
                x = obj.transform.position.x,
                y = obj.transform.position.y
            };

            string jsonData = JsonUtility.ToJson(objectData);

            using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/Object2D", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Fout bij opslaan Object2D: " + request.error);
                }
            }
        }
    }

    void ClearWorldObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    // ================= UI NAVIGATION =================
    public void Logout()
    {
        currentUser = null;
        currentEnvironmentId = null;
        ClearWorldObjects();

        homePanel.SetActive(false);
        worldPanel.SetActive(false);

        loginUsernameInput.text = "";
        loginPasswordInput.text = "";
        loginErrorText.text = "";

        loginPanel.SetActive(true);
    }

    public void ShowRegister()
    {
        registerErrorText.text = "";
        usernameInput.text = "";
        passwordInput.text = "";
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void ShowLogin()
    {
        loginErrorText.text = "";
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void BackToHome()
    {
        SaveWorldObjects();
        ClearWorldObjects();
        worldPanel.SetActive(false);
        homePanel.SetActive(true);
        LoadEnvironments();
    }
}

// ================= CUSTOM HELPER VOOR C# API ARRAYS =================
public static class CustomJsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]" || json == "null" || !json.StartsWith("["))
            return new T[0];

        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}