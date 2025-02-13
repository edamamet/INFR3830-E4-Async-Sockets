using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour {
    [SerializeField] UIDocument document;

    VisualElement root;
    Button clientButton, serverButton;

    void Start() {
        Debug.Log("hi");
    }

    void OnEnable() {
        root = document.rootVisualElement;
        clientButton = root.Q<Button>("Client");
        serverButton = root.Q<Button>("Server");
        
        clientButton.clicked += LaunchClient;
        serverButton.clicked += LaunchServer;
    }

    void OnDisable() {
        clientButton.clicked -= LaunchClient;
        serverButton.clicked -= LaunchServer;
    }
    
    void LaunchClient() {
        _ = LoadClientScene();
    }
    
    void LaunchServer() {
        _ = LoadServerScene();
    }

    async Task LoadServerScene() {
        await SceneManager.LoadSceneAsync("Server");
    }

    async Task LoadClientScene() {
        await SceneManager.LoadSceneAsync("Client");
    }
}