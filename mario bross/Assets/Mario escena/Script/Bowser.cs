using System.Collections;
using UnityEngine;

public class Bowser : MonoBehaviour
{
    // Referencias
    private Animator animator;
    public Transform balloon;      
    public Animator extraAnimator;  

    // Configuración del inflado
    public int pressesToWin = 20;
    public float deflateRate = 0.25f;
    public float extraAnimPulse = 0.18f;

    // Suavizado de animación
    public float smoothSpeed = 5f;

    // Estado interno
    private float fillProgress = 0f;
    private bool canPress = false;
    private bool hasWon = false;

    // Valores del Animator
    private float STATE_IDLE = 0f;
    private float STATE_PUMP = 0.3f;
    private float STATE_SAD = 0.6f;
    private float STATE_VICTORY = 1f;
    private float currentStateValue = 0f;
    private float targetStateValue = 0f;

    private Vector3 initialScale;
    private bool extraHasIsPumping = false;

    void Start()
    {
        
        animator = GetComponent<Animator>();
        if (extraAnimator != null && extraAnimator.runtimeAnimatorController != null)
            extraHasIsPumping = HasBoolParameter(extraAnimator, "isPumping2");

        if (extraAnimator != null && extraHasIsPumping)
            extraAnimator.SetBool("isPumping2", false);

        // Estado inicial
        currentStateValue = STATE_IDLE;
        targetStateValue = STATE_IDLE;
        animator.SetFloat("state", currentStateValue);
        initialScale = balloon != null ? balloon.localScale : Vector3.one;
    }

    void Update()
    {
        
        if (!canPress || hasWon)
        {
            SmoothState();
            return;
        }

        
        if (Input.GetKeyDown(KeyCode.L))
        {
            PumpOnce();
        }

       
        if (!Input.GetKey(KeyCode.L) && fillProgress > 0f && !hasWon)
        {
            fillProgress -= deflateRate * Time.deltaTime;
            fillProgress = Mathf.Clamp01(fillProgress);
            UpdateBalloonScaleFromProgress();
        }

        SmoothState();
    }

    private void PumpOnce()
    {
        // Inflar y animación principal
        targetStateValue = STATE_PUMP;
        animator.CrossFade("Inflar", 0.1f);

        fillProgress += 1f / Mathf.Max(1, pressesToWin);
        fillProgress = Mathf.Clamp01(fillProgress);
        UpdateBalloonScaleFromProgress();

        if (extraAnimator != null && extraHasIsPumping)
        {
            extraAnimator.speed = 2f;
            extraAnimator.SetBool("isPumping2", true);
            StopAllCoroutines();
            StartCoroutine(DisableExtraPumpAfterDelay(extraAnimPulse));
        }

      
        StopCoroutine(nameof(ReturnToIdle));
        StartCoroutine(nameof(ReturnToIdle));

        
        if (fillProgress >= 1f && !hasWon)
        {
            SetVictoryState(true);
        }
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

    private IEnumerator DisableExtraPumpAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (extraAnimator != null && extraHasIsPumping)
            extraAnimator.SetBool("isPumping2", false);
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

    // Métodos públicos para el GameController
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

        if (exploded && balloon != null)
            balloon.gameObject.SetActive(false);
    }
}
