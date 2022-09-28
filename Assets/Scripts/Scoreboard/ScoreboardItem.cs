using TMPro;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;

    private Player player;

    public void Initialize(Player player)
    {
        this.player = player;
        nicknameText.text = player.NickName;
    }

    public void UpdateStats()
    {
        if(player.CustomProperties.TryGetValue("killsCount", out object killsCount))
            killsText.text = killsCount.ToString();

        if (player.CustomProperties.TryGetValue("deathsCount", out object deathsCount))
            deathsText.text = deathsCount.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(targetPlayer == player)
        {
            if(changedProps.ContainsKey("killsCount") || changedProps.ContainsKey("deathsCount"))
            {
                UpdateStats();
            }
        }
    }
}
