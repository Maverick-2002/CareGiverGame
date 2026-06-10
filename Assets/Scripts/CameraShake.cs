using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;
    private float shakeAmount = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        originalPos = transform.localPosition;
    }
    public void Start()
    {
        GameManager.Instance.OnStressChanged.AddListener(UpdateShake);
    }
    public void Update()
    {
        if(shakeAmount > 0f)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
    private void OnDisable()
    {
            GameManager.Instance.OnStressChanged.RemoveListener(UpdateShake);
    }
    private void UpdateShake(float stress)
    {
        float maxStress = GameManager.Instance.maxStress;
        float t = stress / maxStress;

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