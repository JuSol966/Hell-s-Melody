using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using System.Collections; 

public class PauseMenuController : MonoBehaviour
{

    
    public static bool isGamePaused = false;

   
    public GameObject pauseMenuUI;
    public AudioSource gameMusic;
    public Slider volumeSlider;

  
    private float initialVolume;


    void Start()
    {
 
        pauseMenuUI.SetActive(false);
        
        Time.timeScale = 1f;
        isGamePaused = false;

        
        if (gameMusic != null && volumeSlider != null)
        {
            
            initialVolume = gameMusic.volume;
           
            volumeSlider.value = initialVolume;
            
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            
            if (gameMusic == null)
            {
                Debug.LogWarning("Audio Source da música não está atribuído no PauseMenuController.");
            }
            if (volumeSlider == null)
            {
                Debug.LogWarning("Slider de volume não está atribuído no PauseMenuController.");
            }
        }
    }

    public void TogglePause()
    {
        if (isGamePaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

   
    public void Resume()
    {
        
        pauseMenuUI.SetActive(false);
       
        Time.timeScale = 1f;
        
        isGamePaused = false;
        
        if (gameMusic != null)
        {
            gameMusic.UnPause();
        }
    }

    
    private void Pause()
    {
        
        pauseMenuUI.SetActive(true);
      
        Time.timeScale = 0f;
       
        isGamePaused = true;
       
        if (gameMusic != null)
        {
            gameMusic.Pause();
        }
    }

    public void RestartLevel()
    {

        Resume(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    public void LoadMainMenu()
    {
        Resume(); 
        SceneManager.LoadScene("Menu");
    }

    
    public void SetVolume(float volume)
    {
        
        if (gameMusic != null)
        {
            gameMusic.volume = volume;
        }
    }
}