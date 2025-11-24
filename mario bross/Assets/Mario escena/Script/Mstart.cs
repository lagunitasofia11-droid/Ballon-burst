using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Mstart : MonoBehaviour
{
    
    [Header("Cancion del menu")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volumen = 1f;

   
    [Header("Boton de sonido")]
    public Button muteButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    
    [Header("Sonidos de botones")]
    public AudioClip clickClip;
    public AudioClip hoverClip;

  
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private bool isMuted = false;

  
    void Start()
    {
        
        musicSource = gameObject.GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = volumen;
        musicSource.Play();

       
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

       
        muteButton.image.sprite = soundOnSprite;
        muteButton.onClick.AddListener(TToggleMute);

        // agregar eventos de hover y click a todos los botones
        Button[] botones = FindObjectsOfType<Button>();
        foreach (Button btn in botones)
        {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryHover = new EventTrigger.Entry();
            entryHover.eventID = EventTriggerType.PointerEnter;
            entryHover.callback.AddListener((data) => { PlayHover(); });
            trigger.triggers.Add(entryHover);

            btn.onClick.AddListener(() => PlayClick());
        }
    }

    
    void Update()
    {
        if (!isMuted)
            musicSource.volume = volumen;
    }

    
    public void TToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            musicSource.volume = 0f;
            muteButton.image.sprite = soundOffSprite;
        }
        else
        {
            musicSource.volume = volumen;
            muteButton.image.sprite = soundOnSprite;
        }
    }

    
    public void jugar()
    {
        PlayClick();
        SceneManager.LoadScene(1);
    }

    
    public void salir()
    {
        PlayClick();
        Debug.Log("salir...");
        Time.timeScale = 1f;
    }

   
    public void PlayClick()
    {
        if (clickClip != null && sfxSource != null)
            sfxSource.PlayOneShot(clickClip);
    }

    
    public void PlayHover()
    {
        if (hoverClip != null && sfxSource != null)
            sfxSource.PlayOneShot(hoverClip);
    }
}
