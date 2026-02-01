using TMPro;
using UnityEngine;

public class UI_PlayerScoreLeaderboardDisplay : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField]
    private TMP_Text _txtPlayerName = null;

    [SerializeField]
    private TMP_Text _txtScore = null;

    public void SetName(string playerName)
    {
        _txtPlayerName.SetText(playerName);
    }

    public void SetScore(int score)
    {
        _txtScore.SetText("{0}", score);
    }
}
