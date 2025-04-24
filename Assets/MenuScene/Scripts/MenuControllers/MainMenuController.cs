using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuController : MenuController
{
    [SerializeField] private Button button_start;
    [SerializeField] private Button button_garage;
    [SerializeField] private Button button_settings;
    [SerializeField] private Button button_exit;

    [SerializeField] private TMP_InputField inputField_name;

    // Start is called before the first frame update
    void Start()
    {
        button_start.onClick.AddListener(() => { StartButtonFunction(); PlayClickSound(); });

        button_garage.onClick.AddListener(() => { GarageButtonFunction(); PlayClickSound(); });
        button_settings.onClick.AddListener(() => { SettingsButtonFunction(); PlayClickSound(); });
        button_exit.onClick.AddListener(() => { ExitButtonFunction(); PlayClickSound(); });

        inputField_name.onSubmit.AddListener((string sugus) => { NameInputFieldOnSubmit(); });

        Show();
    }

    public override void Show()
    {
        base.Show();

        if (!PlayerPrefs.HasKey("name"))
            PlayerPrefs.SetString("name", "ebic gaymer");

        inputField_name.text = PlayerPrefs.GetString("name");
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void StartButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Gamemode();

        MenuController mc = GameObject.Find("GamemodeMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void GarageButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Garage();

        MenuController mc = GameObject.Find("GarageMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void SettingsButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Settings();

        MenuController mc = GameObject.Find("SettingsMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void ExitButtonFunction()
    {
        Application.Quit();
    }

    private void NameInputFieldOnSubmit()
    {
        PlayerPrefs.SetString("name", inputField_name.text);
    }
}
