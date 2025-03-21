using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MenuController
{
    [SerializeField] private Button button_start;
    [SerializeField] private Button button_settings;
    [SerializeField] private Button button_exit;

    // Start is called before the first frame update
    void Start()
    {
        button_start.onClick.AddListener(() => { StartButtonFunction(); });
        button_settings.onClick.AddListener(() => { SettingsButtonFunction(); });
        button_exit.onClick.AddListener(() => { ExitButtonFunction(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void StartButtonFunction()
    {

    }

    public void SettingsButtonFunction()
    {
        GameObject.Find("PositionManager").GetComponent<MenuCameraPositions>().Settings();

        MenuController mc = GameObject.Find("SettingsMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void ExitButtonFunction()
    {
        Application.Quit();
    }
}
