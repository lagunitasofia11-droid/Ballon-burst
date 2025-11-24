using UnityEngine;
using System.Collections;

public class AnimacionPorClick : MonoBehaviour
{
    public Animator objeto1;      
    public Animator objeto2;     

    [Range(0.1f, 5f)]
    public float velocidadObjeto1 = 1f;
    [Range(0.1f, 5f)]
    public float velocidadObjeto2 = 1f;

    private bool objeto1EnAnimacion = false;
    private bool objeto2EnAnimacion = false;

    private float duracionObjeto1 = 0.21f;
    private float duracionObjeto2 = 1.26f;
    private float delayPrimerL = 0.1f;

    private bool enSecuenciaContinua = false;
    private float tiempoUltimaA = 0f;
    private float tiempoMaximoQuieto = 0.1f;

    
    public AudioSource sonido1; 
    public AudioSource sonido2; 
    private bool sonido1EnReproduccion = false;
    private bool sonido2EnReproduccion = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            float tiempoDesdeUltima = Time.time - tiempoUltimaA;
            bool aplicarDelay = !enSecuenciaContinua || tiempoDesdeUltima >= tiempoMaximoQuieto;

            // Animaciones
            if (!objeto1EnAnimacion)
                StartCoroutine(EjecutarBoolObjeto1(aplicarDelay));

            if (!objeto2EnAnimacion)
                StartCoroutine(EjecutarBool(objeto2, "isPumping", duracionObjeto2 / velocidadObjeto2, () => objeto2EnAnimacion = false));

            // Sonidos
            if (!sonido1EnReproduccion && sonido1 != null)
                StartCoroutine(ReproducirSonido(sonido1, () => sonido1EnReproduccion = false));

            if (!sonido2EnReproduccion && sonido2 != null)
                StartCoroutine(ReproducirSonido(sonido2, () => sonido2EnReproduccion = false));

            enSecuenciaContinua = true;
            tiempoUltimaA = Time.time;
        }

        if (Time.time - tiempoUltimaA >= tiempoMaximoQuieto)
        {
            enSecuenciaContinua = false;
        }
    }

    private IEnumerator EjecutarBoolObjeto1(bool aplicarDelay)
    {
        objeto1EnAnimacion = true;

        if (aplicarDelay)
            yield return new WaitForSeconds(delayPrimerL);

        objeto1.SetBool("an", true);
        yield return new WaitForSeconds(duracionObjeto1 / velocidadObjeto1);
        objeto1.SetBool("an", false);

        objeto1EnAnimacion = false;
    }

    private IEnumerator EjecutarBool(Animator anim, string boolName, float duracion, System.Action alTerminar)
    {
        objeto2EnAnimacion = true;
        anim.SetBool(boolName, true);
        yield return new WaitForSeconds(duracion);
        anim.SetBool(boolName, false);
        objeto2EnAnimacion = false;
        alTerminar?.Invoke();
    }

    private IEnumerator ReproducirSonido(AudioSource audioSource, System.Action alTerminar)
    {
        if (audioSource == null) yield break;

        audioSource.Play();
        if (audioSource.clip != null)
        {
            float duracion = audioSource.clip.length;
            if (audioSource == sonido1) sonido1EnReproduccion = true;
            else if (audioSource == sonido2) sonido2EnReproduccion = true;

            yield return new WaitForSeconds(duracion);
        }
        alTerminar?.Invoke();
    }
}
