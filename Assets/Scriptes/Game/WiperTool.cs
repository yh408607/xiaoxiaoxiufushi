using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WiperTool : MonoBehaviour
{
    [Header("²Á»Ò¿ØÖÆÆ÷")]
    [SerializeField] private DustWipeController dustWipeController;

    [Header("ÍÏ×§ÉèÖÃ")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private float draggingZ = -2f;

    private Vector3 dragOffset;
    private bool isDragging;
    private bool isEnabled;

    private void Awake()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        gameObject.SetActive(false);
    }

    public void Init(DustWipeController controller, Camera camera = null)
    {
        dustWipeController = controller;

        if (camera != null)
        {
            dragCamera = camera;
        }

        DisableWiper();
    }

    public void EnableWiper()
    {
        isEnabled = true;
        gameObject.SetActive(true);
    }

    public void DisableWiper()
    {
        isEnabled = false;
        isDragging = false;
        gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!isEnabled) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isEnabled) return;
        if (!isDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPos = mouseWorldPos + dragOffset;
        targetPos.z = draggingZ;

        transform.position = targetPos;

        if (dustWipeController != null)
        {
            dustWipeController.WipeAtWorldPosition(transform.position);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        Vector3 mouseScreenPos = Input.mousePosition;

        float zDistance = Mathf.Abs(
            dragCamera.transform.position.z - transform.position.z
        );

        mouseScreenPos.z = zDistance;

        Vector3 worldPos = dragCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = transform.position.z;

        return worldPos;
    }
}
