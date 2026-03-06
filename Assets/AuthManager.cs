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

    // ⭐ NIEUW: container waar alle wereld objecten in komen
    public Transform worldObjectsContainer;

    private Dictionary<string, string> users = new Dictionary<string, string>();
    private string currentUser;
    private string currentWorld;

    // ⭐ lijst met objecten in de wereld
    private List<GameObject> spawnedObjects = new List<GameObject>();

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
        registerErrorText.text = "Registratie succesvol!";
        ShowLogin();
    }

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

    public void OpenWorld(string worldName)
    {
        currentWorld = worldName;

        homePanel.SetActive(false);
        worldPanel.SetActive(true);

        worldTitleText.text = "Wereld: " + worldName;

        LoadObjects();
    }

    public void AddObject(int index)
    {
        string key = currentUser + "_" + currentWorld + "_objects";
        string saved = PlayerPrefs.GetString(key, "");

        List<string> objects = new List<string>();
        if (saved != "")
            objects = new List<string>(saved.Split(','));

        objects.Add(index.ToString());

        PlayerPrefs.SetString(key, string.Join(",", objects));
        PlayerPrefs.Save();

        SpawnObject(index);
    }

    void LoadObjects()
    {
        string key = currentUser + "_" + currentWorld + "_objects";
        string saved = PlayerPrefs.GetString(key, "");

        if (saved == "") return;

        string[] objects = saved.Split(',');

        foreach (string obj in objects)
        {
            int index = int.Parse(obj);
            SpawnObject(index);
        }
    }

    void SpawnObject(int index)
    {
        GameObject obj = Instantiate(
            availablePrefabs[index],
            new Vector3(0, 0, 0),
            Quaternion.identity
        );

        // ⭐ object onder world container plaatsen
        obj.transform.SetParent(worldObjectsContainer);

        if (obj.GetComponent<Collider2D>() == null)
            obj.AddComponent<BoxCollider2D>();

        if (obj.GetComponent<Draggable>() == null)
            obj.AddComponent<Draggable>();

        spawnedObjects.Add(obj);
    }

    // ⭐ verwijder alle objecten uit de wereld
    void ClearWorldObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }

        spawnedObjects.Clear();
    }

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
        // ⭐ objecten verwijderen bij verlaten wereld
        ClearWorldObjects();

        worldPanel.SetActive(false);
        homePanel.SetActive(true);
    }
}