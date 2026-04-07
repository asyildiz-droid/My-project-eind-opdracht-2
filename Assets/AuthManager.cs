using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

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

    private Dictionary<string, string> users = new Dictionary<string, string>();
    private string currentUser;
    private string currentWorld;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    // ================= START =================
    void Start()
    {
        // Laad alle opgeslagen gebruikers in zodra de game start
        LoadUsers();
    }

    // ================= DATA OPSLAAN & INLADEN (USERS) =================
    void LoadUsers()
    {
        string savedUsers = PlayerPrefs.GetString("AllUsers", "");

        if (string.IsNullOrEmpty(savedUsers)) return;

        // Formaat is: User1:Pass1,User2:Pass2
        string[] userPairs = savedUsers.Split(',');

        foreach (string pair in userPairs)
        {
            if (string.IsNullOrEmpty(pair)) continue;

            string[] userData = pair.Split(':');
            if (userData.Length == 2)
            {
                // Als de user nog niet is toegevoegd, zet deze erin
                if (!users.ContainsKey(userData[0]))
                {
                    users.Add(userData[0], userData[1]);
                }
            }
        }
    }

    void SaveUsers()
    {
        List<string> userPairs = new List<string>();

        foreach (KeyValuePair<string, string> user in users)
        {
            userPairs.Add(user.Key + ":" + user.Value);
        }

        PlayerPrefs.SetString("AllUsers", string.Join(",", userPairs));
        PlayerPrefs.Save();
    }

    // ================= REGISTER =================
    public void Register()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (users.ContainsKey(username))
        {
            registerErrorText.text = "Gebruikersnaam bestaat al.";
            return;
        }

        if (!IsValidPassword(password))
        {
            registerErrorText.text = "Wachtwoord voldoet niet aan eisen.";
            return;
        }

        users.Add(username, password);

        // Sla de actuele gebruikerslijst op!
        SaveUsers();

        registerErrorText.text = "Registratie succesvol!";
        ShowLogin();
    }

    // ================= LOGIN =================
    public void Login()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (!users.ContainsKey(username) || users[username] != password)
        {
            loginErrorText.text = "Gebruikersnaam of wachtwoord incorrect.";
            return;
        }

        currentUser = username;

        loginPanel.SetActive(false);
        homePanel.SetActive(true);

        LoadWorlds();
    }

    bool IsValidPassword(string password)
    {
        if (password.Length < 10) return false;

        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasDigit = Regex.IsMatch(password, "[0-9]");
        bool hasSpecial = Regex.IsMatch(password, "[^a-zA-Z0-9]");

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }

    // ================= CREATE WORLD =================
    public void CreateWorld()
    {
        string worldName = worldNameInput.text;

        if (string.IsNullOrEmpty(worldName) || worldName.Length > 25)
        {
            worldErrorText.text = "Naam moet 1-25 karakters zijn.";
            return;
        }

        string key = currentUser + "_worlds";
        string saved = PlayerPrefs.GetString(key, "");

        List<string> worlds = new List<string>();
        if (saved != "")
            worlds = new List<string>(saved.Split(','));

        if (worlds.Contains(worldName))
        {
            worldErrorText.text = "Deze wereld bestaat al.";
            return;
        }

        if (worlds.Count >= 5)
        {
            worldErrorText.text = "Maximaal 5 werelden toegestaan.";
            return;
        }

        worlds.Add(worldName);
        PlayerPrefs.SetString(key, string.Join(",", worlds));
        PlayerPrefs.Save();

        worldNameInput.text = "";
        LoadWorlds();

        OpenWorld(worldName);
    }

    // ================= LOAD WORLDS =================
    void LoadWorlds()
    {
        foreach (Transform child in worldsContainer)
            Destroy(child.gameObject);

        string key = currentUser + "_worlds";
        string saved = PlayerPrefs.GetString(key, "");

        if (saved == "") return;

        string[] worlds = saved.Split(',');

        foreach (string world in worlds)
        {
            GameObject button = Instantiate(worldButtonPrefab, worldsContainer);
            button.GetComponentInChildren<TMP_Text>().text = world;
            button.GetComponent<Button>().onClick.AddListener(() => OpenWorld(world));
        }
    }

    // ================= OPEN WORLD =================
    public void OpenWorld(string worldName)
    {
        currentWorld = worldName;

        ClearWorldObjects(); // ⭐ BELANGRIJK FIX

        homePanel.SetActive(false);
        worldPanel.SetActive(true);

        worldTitleText.text = "Wereld: " + worldName;

        LoadObjects();
    }

    // ================= ADD OBJECT =================
    public void AddObject(int index)
    {
        Vector3 pos = new Vector3(0, 0, 0);

        string data = index + "|" +
                      pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
                      pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string key = currentUser + "_" + currentWorld + "_objects";
        string saved = PlayerPrefs.GetString(key, "");

        List<string> objects = new List<string>();
        if (saved != "")
            objects = new List<string>(saved.Split(','));

        objects.Add(data);

        PlayerPrefs.SetString(key, string.Join(",", objects));
        PlayerPrefs.Save();

        SpawnObject(index, pos);
    }

    // ================= LOAD OBJECTS (GEFIXT) =================
    void LoadObjects()
    {
        string key = currentUser + "_" + currentWorld + "_objects";
        string saved = PlayerPrefs.GetString(key, "");

        if (saved == "") return;

        string[] objects = saved.Split(',');

        foreach (string obj in objects)
        {
            if (string.IsNullOrEmpty(obj)) continue;

            string[] data = obj.Split('|');

            if (data.Length < 3)
            {
                Debug.LogWarning("Foute data overgeslagen: " + obj);
                continue;
            }

            int index;
            if (!int.TryParse(data[0], out index)) continue;

            if (index < 0 || index >= availablePrefabs.Length)
            {
                Debug.LogWarning("Index fout: " + index);
                continue;
            }

            float x, y;
            if (!float.TryParse(data[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x) ||
                !float.TryParse(data[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y))
                continue;

            Vector3 pos = new Vector3(x, y, 0);

            SpawnObject(index, pos);
        }
    }

    // ================= SPAWN =================
    void SpawnObject(int index, Vector3 position)
    {
        GameObject obj = Instantiate(
            availablePrefabs[index],
            position,
            Quaternion.identity
        );

        obj.transform.SetParent(worldObjectsContainer);

        if (obj.GetComponent<Collider2D>() == null)
            obj.AddComponent<BoxCollider2D>();

        if (obj.GetComponent<Draggable>() == null)
            obj.AddComponent<Draggable>();

        spawnedObjects.Add(obj);
    }

    // ================= SAVE OBJECT POSITIONS =================
    public void SaveWorldObjects()
    {
        string key = currentUser + "_" + currentWorld + "_objects";

        List<string> objects = new List<string>();

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

            Vector3 pos = obj.transform.position;

            string data = index + "|" +
                          pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
                          pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture);

            objects.Add(data);
        }

        PlayerPrefs.SetString(key, string.Join(",", objects));
        PlayerPrefs.Save();
    }

    // ================= CLEAR =================
    void ClearWorldObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }

        spawnedObjects.Clear();
    }

    // ================= DELETE =================
    public void DeleteWorld()
    {
        string key = currentUser + "_worlds";
        string saved = PlayerPrefs.GetString(key, "");

        List<string> worlds = new List<string>(saved.Split(','));
        worlds.Remove(currentWorld);

        PlayerPrefs.SetString(key, string.Join(",", worlds));
        PlayerPrefs.DeleteKey(currentUser + "_" + currentWorld + "_objects");
        PlayerPrefs.Save();

        ClearWorldObjects();

        worldPanel.SetActive(false);
        homePanel.SetActive(true);

        LoadWorlds();
    }

    public void Logout()
    {
        ClearWorldObjects();

        homePanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void ShowLogin()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void BackToHome()
    {
        SaveWorldObjects(); // ⭐ BELANGRIJK

        ClearWorldObjects();

        worldPanel.SetActive(false);
        homePanel.SetActive(true);
    }
}