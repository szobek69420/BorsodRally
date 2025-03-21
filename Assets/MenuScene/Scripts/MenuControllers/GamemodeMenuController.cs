using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeMenuController : MenuController
{
    [SerializeField] private Button button_goBack;

    [SerializeField] private Button button_singleplayer;
    [SerializeField] private Button button_multiplayer;

    // Start is called before the first frame update
    void Start()
    {
        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); });
    }


    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void GoBackButtonFunction()
    {
        GameObject.Find("PositionManager").GetComponent<MenuCameraPositions>().MainMenu();

        MenuController mc = GameObject.Find("MainMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }
}
