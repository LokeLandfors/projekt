

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 14f, -10f);
    public float lookHeight = 1f;

    [Header("Follow Settings")]
    public float followSpeed = 8f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 25f;

    private float currentZoom;
    private float targetZoom;

    private Vector3 velocity;

    void Start()
    {
        currentZoom = offset.magnitude;
        targetZoom = currentZoom;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleZoom();

        // Smooth zoom
        currentZoom = Mathf.Lerp(
            currentZoom,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );

        // Camera direction stays fixed
        Vector3 cameraDirection = offset.normalized;

        // Calculate camera position
        Vector3 focusPosition = target.position +
                               Vector3.up * lookHeight;

        Vector3 desiredPosition = focusPosition +
                                  cameraDirection * currentZoom;

        // Smoothly follow player
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / followSpeed
        );

        // Always look at player
        transform.LookAt(focusPosition);
    }

    void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        targetZoom -= scroll * 0.01f * zoomSpeed;

        targetZoom = Mathf.Clamp(
            targetZoom,
            minZoom,
            maxZoom
        );
    }
}
