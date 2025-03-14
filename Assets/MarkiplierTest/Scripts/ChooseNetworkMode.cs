using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChooseNetworkMode : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;

    private bool alreadyPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        hostButton.onClick.AddListener(() => {
            if (alreadyPressed)
                return;
            alreadyPressed = true;
            NetworkManager.Singleton.StartHost(); 
        });
        serverButton.onClick.AddListener(() => {
            if (alreadyPressed)
                return;
            alreadyPressed = true;
            NetworkManager.Singleton.StartServer();
        });
        clientButton.onClick.AddListener(() => {
            if (alreadyPressed)
                return;
            alreadyPressed = true;
            NetworkManager.Singleton.StartClient();
        });
    }
}
