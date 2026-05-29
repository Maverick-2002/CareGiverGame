using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;
    private float shakeAmount = 0f;

    private void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        float stress = GameManager.Instance.grandpaStress;
        float maxStress = GameManager.Instance.maxStress;
        float t = stress / maxStress;

        // only shake above 50% stress
        if (t > 0.5f)
        {
            shakeAmount = Mathf.Lerp(0f, 0.01f,(t - 0.5f) * 2f);
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
}