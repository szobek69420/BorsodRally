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
    [SerializeField] private Canvas canvas_end;
    [SerializeField] private TMP_Text text_position;
    [SerializeField] private Button button_returnToMenu;
}
