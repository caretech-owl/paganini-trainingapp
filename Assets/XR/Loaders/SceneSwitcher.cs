using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // Check if Back was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        
    }
    public void GotoARGPSScene()
    {
        SceneManager.LoadScene("ARGPS Example");
    }

    public void GotoMenuScene()
    {
        SceneManager.LoadScene("DevUI");
    }
}