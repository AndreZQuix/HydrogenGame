using Photon.Pun;
using TMPro;
using UnityEngine;

public class NicknameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView playerPV;
    [SerializeField] TMP_Text text;

    private void Start()
    {
        if (playerPV.IsMine)
            gameObject.SetActive(false);
        text.text = playerPV.Owner.NickName;
    }
}
