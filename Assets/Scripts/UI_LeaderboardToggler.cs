using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UI_LeaderboardToggler : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField]
    private RectTransform _leaderboardRoot = null;

    [SerializeField]
    private CanvasGroup _leaderboardView = null;

    [Header("Properties")]
    [SerializeField]
    private float _onActiveHeight = 256f;

    [SerializeField]
    private bool _toggleActiveOnStart = false;

    // Runtime variable data.
    private RectTransform _viewRect = null;
    private Button _toggleBtn = null;
    private bool _isActive = false;

    private void Awake()
    {
        TryGetComponent(out _toggleBtn);
        if (_toggleActiveOnStart) _isActive = true;
        _viewRect = _leaderboardView.transform as RectTransform;
        OnToggleView(_isActive);

        // Subscribe events.
        _toggleBtn.onClick.AddListener(ListenOnToggleClicked);
    }

    private void OnDestroy()
    {
        // Unsubscribe events.
        _toggleBtn.onClick.RemoveListener(ListenOnToggleClicked);
    }

    private void ListenOnToggleClicked() => OnToggleView(!_isActive);

    private void OnToggleView(bool active)
    {
        if (active) OnToggledActive();
        else OnToggledInactive();
    }

    private void OnToggledActive()
    {
        Vector2 currentSize = _leaderboardRoot.sizeDelta;
        currentSize.y = _onActiveHeight;
        _leaderboardView.alpha = 1f;
        _leaderboardRoot.sizeDelta = currentSize;
        _isActive = true;
    }

    private void OnToggledInactive()
    {
        Vector2 currentSize = _leaderboardRoot.sizeDelta;
        currentSize.y = 0f;
        _leaderboardView.alpha = 0f;
        _leaderboardRoot.sizeDelta = currentSize;
        _isActive = false;
    }
}
