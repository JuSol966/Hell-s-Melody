using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenuManager : MonoBehaviour
{
    
    public void PlayGame()
    {
       
        SceneManager.LoadScene("Sandbox");
    }

    
    public void QuitGame()
    {
     
        UnityEditor.EditorApplication.isPlaying = false;
    }
}