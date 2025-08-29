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

    // Vari�vel para armazenar o valor inicial do volume
    private float initialVolume;

    // A fun��o Start � chamada na primeira vez que o script � ativado
    void Start()
    {
        // Garante que o menu est� desativado no in�cio do jogo
        pauseMenuUI.SetActive(false);
        // Garante que o jogo n�o est� pausado quando a cena inicia
        Time.timeScale = 1f;
        isGamePaused = false;

        // Verifica se o objeto de m�sica e o slider est�o definidos
        if (gameMusic != null && volumeSlider != null)
        {
            // Salva o volume inicial da m�sica
            initialVolume = gameMusic.volume;
            // Define o valor do slider com o volume atual da m�sica
            volumeSlider.value = initialVolume;
            // Adiciona um listener para a mudan�a de valor no slider
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            // Mensagens de aviso se as refer�ncias n�o estiverem configuradas
            if (gameMusic == null)
            {
                Debug.LogWarning("Audio Source da m�sica n�o est� atribu�do no PauseMenuController.");
            }
            if (volumeSlider == null)
            {
                Debug.LogWarning("Slider de volume n�o est� atribu�do no PauseMenuController.");
            }
        }
    }

    // A fun��o Update() n�o � mais usada para detectar a pausa via tecla
    // A ativa��o e desativa��o do menu agora � feita pelos bot�es da UI

    // Fun��o p�blica para ser chamada por um bot�o para pausar ou continuar o jogo
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

    // Fun��o p�blica para o bot�o de "Voltar ao Jogo"
    public void Resume()
    {
        // Desativa a interface do menu de pausa
        pauseMenuUI.SetActive(false);
        // Volta a velocidade normal do jogo
        Time.timeScale = 1f;
        // Atualiza o estado do jogo
        isGamePaused = false;
        // Retoma a m�sica
        if (gameMusic != null)
        {
            gameMusic.UnPause();
        }
    }

    // Fun��o privada para ser chamada internamente
    private void Pause()
    {
        // Ativa a interface do menu de pausa
        pauseMenuUI.SetActive(true);
        // Congela o tempo do jogo
        Time.timeScale = 0f;
        // Atualiza o estado do jogo
        isGamePaused = true;
        // Pausa a m�sica
        if (gameMusic != null)
        {
            gameMusic.Pause();
        }
    }

    // Fun��o p�blica para o bot�o de "Reiniciar Fase"
    public void RestartLevel()
    {
        Resume(); // Garante que o jogo n�o est� pausado antes de recarregar a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fun��o p�blica para o bot�o de "Menu Inicial"
    public void LoadMainMenu()
    {
        Resume(); // Garante que o jogo n�o est� pausado antes de carregar a cena
        // Substitua "MainMenu" pelo nome da sua cena de menu inicial
        SceneManager.LoadScene("Menu");
    }

    // Fun��o p�blica para o slider de volume
    public void SetVolume(float volume)
    {
        // Define o volume da m�sica com base no valor do slider
        if (gameMusic != null)
        {
            gameMusic.volume = volume;
        }
    }
}