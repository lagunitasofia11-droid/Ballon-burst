using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lu : MonoBehaviour
{
   

    [Header("Animación por Click (Integrada)")]
    [Range(0.1f, 5f)] public float velocidadAn = 1f;          // Objeto 1 = an
    [Range(0.1f, 5f)] public float velocidadPumping = 1f;      // Objeto 2 = isPumping

    public AudioSource sonidoAn;
    public AudioSource sonidoPumping;

    private bool anEnAnimacion = false;
    private bool pumpingEnAnimacion = false;

    private float duracionAn = 0.21f;
    private float duracionPumping = 1.26f;
    private float delayPrimerA = 0.1f;

    private bool enSecuenciaContinua = false;
    private float tiempoUltimaA = 0f;
    private float tiempoMaximoQuieto = 0.1f;

    private bool sonidoAnEnReproduccion = false;
    private bool sonidoPumpEnReproduccion = false;


    

    private Animator animator;
    public Transform balloon;

    [Header("Animaciones extras de objetos")]
    public Animator isPumpingAnimator;
    public Animator anAnimator;

    [Header("Efectos al Completar")]
    public Light victoryLight;
    public AudioSource victoryAudio;
    public GameObject explosionFx;

    private bool effectsTriggered = false;

    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

    private List<Renderer> balloonRenderers = new List<Renderer>();
    private List<Color[]> originalColors = new List<Color[]>();
    private Color blinkColor = new Color(1f, 0.435f, 0f);

    private Quaternion initialRotation;
    private float targetTilt = 0f;
    private float tiltSpeed = 2f;

    public int pressesToWin = 20;
    public float deflateRate = 0.25f;
    public float smoothSpeed = 5f;

    private float fillProgress = 0f;
    private bool canPress = false;
    private bool hasWon = false;

    private float STATE_IDLE = 0f;
    private float STATE_PUMP = 0.3f;
    private float STATE_SAD = 0.6f;
    private float STATE_VICTORY = 1f;
    private float currentStateValue = 0f;
    private float targetStateValue = 0f;

    private Vector3 initialScale;
    private bool isPumpingHasParameter = false;
    private bool anHasParameter = false;



    

    void Start()
    {
        animator = GetComponent<Animator>();

        if (isPumpingAnimator != null)
            isPumpingHasParameter = HasBoolParameter(isPumpingAnimator, "isPumping");

        if (anAnimator != null)
            anHasParameter = HasBoolParameter(anAnimator, "an");

        currentStateValue = STATE_IDLE;
        targetStateValue = STATE_IDLE;
        animator.SetFloat("state", currentStateValue);

        initialScale = balloon != null ? balloon.localScale : Vector3.one;

        if (balloon != null)
            initialRotation = balloon.localRotation;

        if (victoryLight != null)
            victoryLight.enabled = false;

        if (explosionFx != null)
            explosionFx.SetActive(false);

        if (victoryAudio != null)
            victoryAudio.playOnAwake = false;

        if (balloon != null)
        {
            balloonRenderers.AddRange(balloon.GetComponentsInChildren<Renderer>());

            foreach (var rend in balloonRenderers)
            {
                Color[] mats = new Color[rend.materials.Length];
                for (int i = 0; i < rend.materials.Length; i++)
                    mats[i] = rend.materials[i].color;
                originalColors.Add(mats);
            }
        }
    }


   

    void Update()
    {
        if (PauseMenu.isPaused) return;

        if (!canPress || hasWon)
        {
            SmoothState();
            UpdateBalloonTilt();
            return;
        }

      
        if (Input.GetKeyDown(KeyCode.A))
        {
            EjecutarAnimacionClick();
            PumpOnce();
        }

        
        if (!Input.GetKey(KeyCode.A) && fillProgress > 0f)
        {
            fillProgress -= deflateRate * Time.deltaTime;
            fillProgress = Mathf.Clamp01(fillProgress);
            UpdateBalloonScaleFromProgress();
        }

        HandleBlinkLogic();
        HandleTiltLogic();
        UpdateBalloonTilt();
        SmoothState();
    }



    

    private void EjecutarAnimacionClick()
    {
        float tiempoDesdeUltima = Time.time - tiempoUltimaA;
        bool aplicarDelay = !enSecuenciaContinua || tiempoDesdeUltima >= tiempoMaximoQuieto;

        
        if (!anEnAnimacion && anAnimator != null)
            StartCoroutine(EjecutarBoolAn(aplicarDelay));

       
        if (!pumpingEnAnimacion && isPumpingAnimator != null)
            StartCoroutine(EjecutarBoolPumping());

        // Sonidos
        if (!sonidoAnEnReproduccion && sonidoAn != null)
            StartCoroutine(ReproducirSonido(sonidoAn, () => sonidoAnEnReproduccion = false));

        if (!sonidoPumpEnReproduccion && sonidoPumping != null)
            StartCoroutine(ReproducirSonido(sonidoPumping, () => sonidoPumpEnReproduccion = false));

        enSecuenciaContinua = true;
        tiempoUltimaA = Time.time;

        if (Time.time - tiempoUltimaA >= tiempoMaximoQuieto)
            enSecuenciaContinua = false;
    }


    private IEnumerator EjecutarBoolAn(bool aplicarDelay)
    {
        anEnAnimacion = true;
        if (aplicarDelay)
            yield return new WaitForSeconds(delayPrimerA);

        anAnimator.SetBool("an", true);
        yield return new WaitForSeconds(duracionAn / velocidadAn);
        anAnimator.SetBool("an", false);

        anEnAnimacion = false;
    }


    private IEnumerator EjecutarBoolPumping()
    {
        pumpingEnAnimacion = true;

        isPumpingAnimator.SetBool("isPumping", true);
        yield return new WaitForSeconds(duracionPumping / velocidadPumping);
        isPumpingAnimator.SetBool("isPumping", false);

        pumpingEnAnimacion = false;
    }


    private IEnumerator ReproducirSonido(AudioSource audioSource, System.Action alTerminar)
    {
        if (audioSource == null) yield break;

        audioSource.Play();

        if (audioSource.clip != null)
        {
            float duracion = audioSource.clip.length;

            if (audioSource == sonidoAn) sonidoAnEnReproduccion = true;
            else if (audioSource == sonidoPumping) sonidoPumpEnReproduccion = true;

            yield return new WaitForSeconds(duracion);
        }

        alTerminar?.Invoke();
    }




    private void PumpOnce()
    {
        targetStateValue = STATE_PUMP;
        animator.CrossFade("Inflar", 0.1f);

        fillProgress += 1f / Mathf.Max(1, pressesToWin);
        fillProgress = Mathf.Clamp01(fillProgress);

        UpdateBalloonScaleFromProgress();

        if (!isBlinking && fillProgress >= 0.75f && !hasWon)
        {
            isBlinking = true;
            blinkCoroutine = StartCoroutine(BalloonBlinkRoutine());
        }

        StopCoroutine(nameof(ReturnToIdle));
        StartCoroutine(nameof(ReturnToIdle));

        if (fillProgress >= 1f && !hasWon)
            SetVictoryState(true);
    }


    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(0.5f);

        if (!hasWon)
        {
            targetStateValue = STATE_IDLE;
            animator.CrossFade("Idle", 0.1f);
        }
    }


    private void UpdateBalloonScaleFromProgress()
    {
        if (balloon == null) return;

        float maxScale = 2f;
        float scaleFactor = Mathf.Lerp(1f, maxScale, fillProgress);
        balloon.localScale = initialScale * scaleFactor;
    }


    private void HandleBlinkLogic()
    {
        if (isBlinking && fillProgress < 0.75f)
        {
            StopCoroutine(blinkCoroutine);

            for (int r = 0; r < balloonRenderers.Count; r++)
            {
                for (int m = 0; m < balloonRenderers[r].materials.Length; m++)
                    balloonRenderers[r].materials[m].color = originalColors[r][m];
            }

            isBlinking = false;
        }
    }


    private void HandleTiltLogic()
    {
        if (balloon == null) return;

        if (fillProgress >= 0.70f)
        {
            float t = Mathf.InverseLerp(0.70f, 1f, fillProgress);
            targetTilt = Mathf.Lerp(0f, 30f, t);
        }
        else
        {
            targetTilt = 0f;
        }
    }


    private void UpdateBalloonTilt()
    {
        if (balloon == null) return;

        Quaternion targetRot = initialRotation * Quaternion.Euler(0f, targetTilt, 0f);

        balloon.localRotation =
            Quaternion.Lerp(balloon.localRotation, targetRot, Time.deltaTime * tiltSpeed);
    }


    private void SmoothState()
    {
        currentStateValue = Mathf.Lerp(currentStateValue, targetStateValue, Time.deltaTime * smoothSpeed);
        animator.SetFloat("state", currentStateValue);
    }


    private bool HasBoolParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;

        foreach (var p in anim.parameters)
            if (p.type == AnimatorControllerParameterType.Bool && p.name == paramName)
                return true;

        return false;
    }



   
    //  feliz triste
   

    public void DisablePressing() => canPress = false;
    public void EnablePressing() => canPress = true;
    public bool HasWon() => hasWon;
    public float GetFillProgress() => fillProgress;

    public void SetSadState()
    {
        targetStateValue = STATE_SAD;
        currentStateValue = STATE_SAD;
        animator.CrossFade("Sad", 0.1f);

        canPress = false;
        hasWon = true;
    }


    public void SetVictoryState(bool exploded)
    {
        targetStateValue = STATE_VICTORY;
        currentStateValue = STATE_VICTORY;
        animator.CrossFade("Victory", 0.1f);

        canPress = false;
        hasWon = true;

        if (isBlinking && blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);

            for (int r = 0; r < balloonRenderers.Count; r++)
            {
                for (int m = 0; m < balloonRenderers[r].materials.Length; m++)
                    balloonRenderers[r].materials[m].color = originalColors[r][m];
            }

            isBlinking = false;
        }


        if (exploded && balloon != null)
        {
            foreach (Renderer rend in balloonRenderers)
                if (rend != null)
                    rend.enabled = false;

            StartCoroutine(DisableBalloonNextFrame());
        }

        if (!effectsTriggered)
        {
            effectsTriggered = true;

            if (victoryLight != null)
                StartCoroutine(VictoryLightRoutine());

            if (victoryAudio != null)
                victoryAudio.Play();

            if (explosionFx != null)
                explosionFx.SetActive(true);
        }
    }


    private IEnumerator DisableBalloonNextFrame()
    {
        yield return null;

        if (balloon != null)
        {
            foreach (Renderer rend in balloonRenderers)
                if (rend != null)
                    rend.enabled = false;

            balloon.gameObject.SetActive(false);
        }
    }


    private IEnumerator VictoryLightRoutine()
    {
        victoryLight.enabled = true;
        yield return new WaitForSeconds(0.5f);
        victoryLight.enabled = false;
    }


    private IEnumerator BalloonBlinkRoutine()
    {
        Color emissiveOn = new Color(1f, 0.435f, 0f) * 0.6f;
        Color emissiveOff = Color.black;

        while (true)
        {
            foreach (Renderer rend in balloonRenderers)
                foreach (var mat in rend.materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissiveOn);
                }

            yield return new WaitForSeconds(0.2f);

            foreach (Renderer rend in balloonRenderers)
                foreach (var mat in rend.materials)
                    mat.SetColor("_EmissionColor", emissiveOff);

            yield return new WaitForSeconds(0.15f);
        }
    }
}
