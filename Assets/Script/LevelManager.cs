using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public int expected_price = 100;
    private void Awake()
    {
        // Ensure only one LevelManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keeps manager alive between levels
        }
    }

    public void RestartLevel()
    {
        // Loads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Check if a next scene actually exists in Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Returning to main menu...");
            SceneManager.LoadScene(0); 
        }
    }

    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        
        Time.timeScale = 1f; 
    }
}