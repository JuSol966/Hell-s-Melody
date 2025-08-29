using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using System.Collections; // Usado para a rotina de delay

public class PauseMenuController : MonoBehaviour
{

    
    public static bool isGamePaused = false;

   
    public GameObject pauseMenuUI;
    public AudioSource gameMusic;
    public Slider volumeSlider;

    // Variável para armazenar o valor inicial do volume
    private float initialVolume;

    // A função Start é chamada na primeira vez que o script é ativado
    void Start()
    {
        // Garante que o menu está desativado no início do jogo
        pauseMenuUI.SetActive(false);
        // Garante que o jogo não está pausado quando a cena inicia
        Time.timeScale = 1f;
        isGamePaused = false;

        // Verifica se o objeto de música e o slider estão definidos
        if (gameMusic != null && volumeSlider != null)
        {
            // Salva o volume inicial da música
            initialVolume = gameMusic.volume;
            // Define o valor do slider com o volume atual da música
            volumeSlider.value = initialVolume;
            // Adiciona um listener para a mudança de valor no slider
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            // Mensagens de aviso se as referências não estiverem configuradas
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

    // A função Update() não é mais usada para detectar a pausa via tecla
    // A ativação e desativação do menu agora é feita pelos botões da UI

    // Função pública para ser chamada por um botão para pausar ou continuar o jogo
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

    // Função pública para o botão de "Voltar ao Jogo"
    public void Resume()
    {
        // Desativa a interface do menu de pausa
        pauseMenuUI.SetActive(false);
        // Volta a velocidade normal do jogo
        Time.timeScale = 1f;
        // Atualiza o estado do jogo
        isGamePaused = false;
        // Retoma a música
        if (gameMusic != null)
        {
            gameMusic.UnPause();
        }
    }

    // Função privada para ser chamada internamente
    private void Pause()
    {
        // Ativa a interface do menu de pausa
        pauseMenuUI.SetActive(true);
        // Congela o tempo do jogo
        Time.timeScale = 0f;
        // Atualiza o estado do jogo
        isGamePaused = true;
        // Pausa a música
        if (gameMusic != null)
        {
            gameMusic.Pause();
        }
    }

    // Função pública para o botão de "Reiniciar Fase"
    public void RestartLevel()
    {
        Resume(); // Garante que o jogo não está pausado antes de recarregar a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Função pública para o botão de "Menu Inicial"
    public void LoadMainMenu()
    {
        Resume(); // Garante que o jogo não está pausado antes de carregar a cena
        // Substitua "MainMenu" pelo nome da sua cena de menu inicial
        SceneManager.LoadScene("Menu");
    }

    // Função pública para o slider de volume
    public void SetVolume(float volume)
    {
        // Define o volume da música com base no valor do slider
        if (gameMusic != null)
        {
            gameMusic.volume = volume;
        }
    }
}