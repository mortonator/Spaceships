using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int spawnLocationIndex;
    WorldManager world;
    int currentScene;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadScene(int sceneIndex, int spawnIndex)
    {
        spawnLocationIndex = spawnIndex;
        SceneManager.LoadScene(sceneIndex);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == currentScene)
            return;

        currentScene = scene.buildIndex;
        world = FindObjectOfType<WorldManager>();
        world.SpawnAtLocation(spawnLocationIndex);
    }
}
