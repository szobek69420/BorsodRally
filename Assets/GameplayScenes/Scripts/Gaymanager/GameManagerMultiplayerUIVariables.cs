using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//a helper class to make easier for the (by NetworkManager) instantiated GameManagers to find the ui elements
//it should be attached to the NetworkManager game object
public class GameManagerMultiplayerUIVariables : MonoBehaviour
{
    //lobby things
    public Canvas canvas_lobby;
    public RectTransform lobbyElement_origin;
    public TMP_Text text_waitingForHost;
    public TMP_Text text_hostAddress;
    public Button button_startGame;

    //countdown things
    public Canvas canvas_countdown;
    public TMP_Text text_countdown;

    //ingame things
    public Canvas canvas_ingame;
    public SpeedometerHandler speedo;
    public TMP_Text text_progress;

    private float greatestProgress = 0;

    //end things
    public Canvas canvas_end;
    public TMP_Text text_position;
    public Button button_returnToMenu;
}
