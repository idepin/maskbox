using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TokenInput : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pointAction;
    public InputActionReference pressAction;
    public InputActionReference doublePressAction;

    private Camera cam;
    private bool isDragging;

    private float distanceToCamera;
    private Vector3 offset;
    private float lockedY;
    private Vector3 initialPosition;
    private Token token;
    private Collider[] tokenColliders;

    void Awake()
    {
        cam = Camera.main;
        token = GetComponent<Token>();
        tokenColliders = GetComponentsInChildren<Collider>();
    }

    void OnEnable()
    {
        pointAction.action.Enable();
        pressAction.action.Enable();
        doublePressAction.action.Enable();

        pressAction.action.started += OnPress;
        pressAction.action.canceled += OnRelease;
        doublePressAction.action.performed += OnDoublePress;

    }

    void OnDisable()
    {
        pressAction.action.started -= OnPress;
        pressAction.action.canceled -= OnRelease;
        doublePressAction.action.performed -= OnDoublePress;

        pointAction.action.Disable();
        pressAction.action.Disable();
        doublePressAction.action.Disable();
    }

    void OnDoublePress(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = pointAction.action.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                token.Flip();
            }
        }

    }

    private void SetActiveColliders(bool enabled)
    {
        foreach (var c in tokenColliders)
        {
            c.enabled = enabled;
        }
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
                SetActiveColliders(false);
                distanceToCamera = hit.distance;
                offset = transform.position - hit.point;
                lockedY = transform.position.y;
            }
        }
    }

    void OnRelease(InputAction.CallbackContext ctx)
    {
        isDragging = false;
        SetActiveColliders(true);
        Sequence transformBack = DOTween.Sequence();
        transformBack.AppendInterval(0.1f);
        transformBack.AppendCallback(() =>
        {
            SetActiveColliders(false);
        });
        transformBack.Append(transform.DOMove(initialPosition, 0.2f));
        transformBack.AppendCallback(() =>
        {
            SetActiveColliders(true);
        });
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
