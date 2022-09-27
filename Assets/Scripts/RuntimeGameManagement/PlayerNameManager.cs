using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameInput;

    public void Start()
    {
        if (PlayerPrefs.HasKey("nickname"))
        {
            nicknameInput.text = PlayerPrefs.GetString("nickname");
        }
        else
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
            OnNicknameInputValueChanged();
        }
    }

    public void OnNicknameInputValueChanged()
    {
        PhotonNetwork.NickName = nicknameInput.text;
        PlayerPrefs.SetString("nickname", nicknameInput.text);
    }
}
