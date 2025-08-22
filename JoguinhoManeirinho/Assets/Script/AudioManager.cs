using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
   
    public Slider volumeSlider;

   
    void Awake()
    {
        
        if (GameObject.FindObjectOfType<AudioManager>() != this)
        {
            Destroy(gameObject);
            return;
        }

        
        DontDestroyOnLoad(gameObject);
    }

    
    void Start()
    {
        
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
           
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

   
    public void SetVolume(float volume)
    {
       
        AudioListener.volume = volume;
    }
}
