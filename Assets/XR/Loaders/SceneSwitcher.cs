using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // Check if Back was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GotoMenuScene();
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
    public void GotoProfileScene()
    {
        SceneManager.LoadScene("ProfileScene");
    }
    public void GotoErstbegehungScene()
    {
        SceneManager.LoadScene("DokumentierteErstbegehung");
    }
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}