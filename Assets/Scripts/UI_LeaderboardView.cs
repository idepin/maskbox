using UnityEngine;

public class UI_LeaderboardView : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField]
    private RectTransform _contents = null;

    public RectTransform PlayerScoreContainer => _contents;
}
