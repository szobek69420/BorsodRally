using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LobbyInfoUI : MonoBehaviour
{
    public delegate void Callback(AvailableLobby lobbyInfo);

    [SerializeField] private TMP_Text text_ownerName;
    [SerializeField] private TMP_Text text_playerCount;
    [SerializeField] private Button button_join;

    private AvailableLobby lobbyInfo = null;
    private Callback callback = null;

    private void Start()
    {
        button_join.onClick.AddListener(() => { JoinButtonFunction(); });
    }

    public void Initialize(AvailableLobby lobbyInfo, Callback callback)
    { 
        this.lobbyInfo = lobbyInfo;
        this.callback = callback;

        text_ownerName.text = this.lobbyInfo.ownerName;
        text_playerCount.text = this.lobbyInfo.playerCount + "/" + this.lobbyInfo.maxPlayerCount;
    }

    private void JoinButtonFunction()
    {
        if (callback != null)
            callback(lobbyInfo);
    }
}
