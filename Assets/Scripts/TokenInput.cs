using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TokenInput : MonoBehaviour
{
    private Token token;
    private float _lastLeftClickTime = -999f;
    [SerializeField] private float _doubleClickMaxDelay = 0.3f;
    [SerializeField] private UnityEvent onLeftDoubleClick;

    void Awake()
    {
        token = GetComponent<Token>();
    }

    void OnMouseDown()
    {
        if (IsLeftDoubleClick())
        {
            HandleDoubleClick();
        }
        else
        {
            if (token.TokenManager.selectedToken != null && token.TokenManager.selectedToken != token)
            {
                token.TokenManager.SwapTokens(token.TokenManager.selectedToken, token);
            }
            else if (token.TokenManager.selectedToken != null && token.TokenManager.selectedToken == token)
            {
                token.TokenManager.SetSelectedToken(null);
            }
            else
            {
                token.TokenManager.SetSelectedToken(token);
            }

        }
    }

    public bool IsLeftDoubleClick()
    {
        float now = Time.time;
        bool isDouble = (now - _lastLeftClickTime) <= _doubleClickMaxDelay;
        _lastLeftClickTime = now;
        return isDouble;
    }

    private void HandleDoubleClick()
    {
        token.Flip();
        token.TokenManager.SetSelectedToken(null);
    }

}
