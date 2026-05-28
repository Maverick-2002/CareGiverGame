using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public float xClamp = 80f;

    public static bool isUIOpen = false;

    private float xRotation = 0f;
    private Transform playerBody;

    void Start()
    {
        // camera's parent is the player
        playerBody = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isUIOpen) return;

        float mouseX = Input.GetAxis("Mouse X")
            * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")
            * mouseSensitivity * Time.deltaTime;

        // vertical — camera only
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);
        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        // horizontal — rotate whole player body
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public static void OpenUI()
    {
        isUIOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void CloseUI()
    {
        isUIOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}