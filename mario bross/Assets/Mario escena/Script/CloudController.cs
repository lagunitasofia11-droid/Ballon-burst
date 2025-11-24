using UnityEngine;

public class CloudController : MonoBehaviour
{
    [Header("Clouds")]
    public Transform[] clouds = new Transform[5]; 

    [Header("Horizontal Limits")]
    public float leftLimit = -50f;
    public float rightLimit = 30f;

    [Header("Visibility Safety")]
    public float safeMargin = 2f; 

    [Header("Movement Settings")]
    public float speed = 3f;
    public float amplitude = 0.3f;
    public float frequency = 1f;

    private float[] baseY;
    private bool[] movingRight;

    void Start()
    {
        baseY = new float[clouds.Length];
        movingRight = new bool[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            baseY[i] = clouds[i].position.y;

           
            movingRight[i] = i % 2 == 0;
        }
    }

    void Update()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            MoveCloud(i);
            OscillateCloud(i);
        }
    }

    void MoveCloud(int i)
    {
        Transform cloud = clouds[i];
        Vector3 pos = cloud.position;

        if (movingRight[i])
        {
            pos.x += speed * Time.deltaTime;

            if (pos.x >= rightLimit - safeMargin)
            {
                pos.x = rightLimit - safeMargin;
                movingRight[i] = false;
            }
        }
        else
        {
            pos.x -= speed * Time.deltaTime;

            if (pos.x <= leftLimit + safeMargin)
            {
                pos.x = leftLimit + safeMargin;
                movingRight[i] = true;
            }
        }

        cloud.position = pos;
    }

    void OscillateCloud(int i)
    {
        Transform cloud = clouds[i];
        float newY = baseY[i] + Mathf.Sin(Time.time * frequency + i * 0.5f) * amplitude;

        cloud.position = new Vector3(cloud.position.x, newY, cloud.position.z);
    }
}
