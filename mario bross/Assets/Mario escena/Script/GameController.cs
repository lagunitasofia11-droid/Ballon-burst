using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Jugadores")]
    public Lu player1_lu1;
    public Lu player1_lu2;
    public Bowser player2_bowser1;
    public Bowser player2_bowser2;

    [Header("Tiempo de juego")]
    public float startDelay = 4f;
    public float totalGameTime = 20f;
    private float gameTimer = 0f;
    private bool gameStarted = false;

    [Header("UI - Imágenes y Panel")]
    public GameObject imagenInicio;
    public GameObject imagenFinal;
    public GameObject panelFinal;

    [Header("Textos de Victoria")]
    public Text textoVictoriaLu;
    public Text textoVictoriaBowser;

    [Header("Audio")]
    public AudioSource audioInGame;
    public AudioSource audioVictoryLu;
    public AudioSource audioVictoryBowser;
    public AudioSource audioStart;
    public AudioSource audioFinish;

    public float fadeDuration = 1f;

    [Header("Control de volumen máximo")]
    [Range(0f, 1f)] public float maxVolumeInGame = 1f;
    [Range(0f, 1f)] public float maxVolumeVictoryLu = 1f;
    [Range(0f, 1f)] public float maxVolumeVictoryBowser = 1f;

    [Header("Cronómetro")]
    public TMP_Text cronometroTMP;   // 

    void Start()
    {
        if (imagenInicio) imagenInicio.SetActive(false);
        if (imagenFinal) imagenFinal.SetActive(false);
        if (panelFinal) panelFinal.SetActive(false);

        if (textoVictoriaLu) textoVictoriaLu.canvasRenderer.SetAlpha(0f);
        if (textoVictoriaBowser) textoVictoriaBowser.canvasRenderer.SetAlpha(0f);

        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        if (audioInGame)
            StartCoroutine(FadeIn(audioInGame, fadeDuration, maxVolumeInGame));

        StartCoroutine(ShowInitialImage());

        yield return new WaitForSeconds(startDelay);

        player1_lu1?.EnablePressing();
        player1_lu2?.EnablePressing();
        player2_bowser1?.EnablePressing();
        player2_bowser2?.EnablePressing();

        if (audioStart) audioStart.Play();

        gameStarted = true;
        gameTimer = totalGameTime;
    }

    IEnumerator ShowInitialImage()
    {
        yield return new WaitForSeconds(4f);

        if (imagenInicio)
        {
            imagenInicio.SetActive(true);
            StartCoroutine(FadeUI(imagenInicio, true));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(FadeUI(imagenInicio, false));
            yield return new WaitForSeconds(0.6f);
            imagenInicio.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameStarted) return;

        // cronometro
        gameTimer -= Time.deltaTime;
        if (gameTimer < 0) gameTimer = 0;

        int minutos = Mathf.FloorToInt(gameTimer / 60f);
        int segundos = Mathf.FloorToInt(gameTimer % 60f);
        if (cronometroTMP)
            cronometroTMP.text = $"{minutos:00}:{segundos:00}";

        // Verificar ganadores
        bool player1Won = player1_lu1.HasWon() || player1_lu2.HasWon();
        bool player2Won = player2_bowser1.HasWon() || player2_bowser2.HasWon();

        if (player1Won || player2Won || gameTimer <= 0f)
            EndGame(player1Won, player2Won);
    }

    void EndGame(bool player1Won, bool player2Won)
    {
        if (!gameStarted) return;
        gameStarted = false;  

        player1_lu1?.DisablePressing();
        player1_lu2?.DisablePressing();
        player2_bowser1?.DisablePressing();
        player2_bowser2?.DisablePressing();

        float p1 = Mathf.Max(player1_lu1.GetFillProgress(), player1_lu2.GetFillProgress());
        float p2 = Mathf.Max(player2_bowser1.GetFillProgress(), player2_bowser2.GetFillProgress());

        if (audioInGame)
            StartCoroutine(FadeOut(audioInGame, fadeDuration));

        StartCoroutine(ShowFinalImageAndPanel(player1Won, p1, p2));
        StartCoroutine(PlayFinishAndVictory(player1Won, p1, p2));
    }

    IEnumerator ShowFinalImageAndPanel(bool player1Won, float p1, float p2)
    {
        yield return new WaitForSeconds(0.3f);

        if (imagenFinal)
        {
            imagenFinal.SetActive(true);
            StartCoroutine(FadeUI(imagenFinal, true));
            yield return new WaitForSeconds(2f);
            StartCoroutine(FadeUI(imagenFinal, false));
            yield return new WaitForSeconds(0.6f);
            imagenFinal.SetActive(false);
        }

        if (panelFinal)
            panelFinal.SetActive(true);

        if (player1Won || p1 > p2)
            textoVictoriaLu?.CrossFadeAlpha(1f, 0.8f, false);
        else
            textoVictoriaBowser?.CrossFadeAlpha(1f, 0.8f, false);
    }

    IEnumerator PlayFinishAndVictory(bool player1Won, float p1, float p2)
    {
        if (audioFinish)
        {
            audioFinish.Play();
            yield return new WaitForSeconds(audioFinish.clip.length);
        }

        if (player1Won || p1 > p2)
            StartCoroutine(FadeIn(audioVictoryLu, fadeDuration, maxVolumeVictoryLu));
        else
            StartCoroutine(FadeIn(audioVictoryBowser, fadeDuration, maxVolumeVictoryBowser));
    }

    IEnumerator FadeUI(GameObject obj, bool fadeIn)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (!cg) cg = obj.AddComponent<CanvasGroup>();

        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;
        float duration = 0.6f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume)
    {
        targetVolume = Mathf.Clamp(targetVolume, 0f, 1f);
        audioSource.volume = 0f;
        audioSource.Play();
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        audioSource.Stop();
    }
}
