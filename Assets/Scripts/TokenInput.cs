using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TokenInput : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pointAction;
    public InputActionReference pressAction;

    private Camera cam;
    private bool isDragging;

    private float distanceToCamera;
    private Vector3 offset;
    private float lockedY;
    private Vector3 initialPosition;
    private Token token;

    void Awake()
    {
        cam = Camera.main;
        token = GetComponent<Token>();
    }

    void OnEnable()
    {
        pointAction.action.Enable();
        pressAction.action.Enable();

        pressAction.action.started += OnPress;
        pressAction.action.canceled += OnRelease;
    }

    void OnDisable()
    {
        pressAction.action.started -= OnPress;
        pressAction.action.canceled -= OnRelease;

        pointAction.action.Disable();
        pressAction.action.Disable();
    }

    void OnPress(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = pointAction.action.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screenPos);
        initialPosition = transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                isDragging = true;
                distanceToCamera = hit.distance;
                offset = transform.position - hit.point;
                lockedY = transform.position.y;
            }
        }
    }

    void OnRelease(InputAction.CallbackContext ctx)
    {
        isDragging = false;
        transform.position = initialPosition;
    }

    void Update()
    {
        if (!isDragging || token.IsSwapping) return;

        Vector2 screenPos = pointAction.action.ReadValue<Vector2>();
        Vector3 screenPoint = new Vector3(
            screenPos.x,
            screenPos.y,
            distanceToCamera
        );

        Vector3 targetPos = cam.ScreenToWorldPoint(screenPoint) + offset;
        targetPos.y = lockedY;
        transform.position = targetPos;
    }
}
