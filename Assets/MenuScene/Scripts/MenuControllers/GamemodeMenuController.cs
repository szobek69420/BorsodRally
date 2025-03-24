using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamemodeMenuController : MenuController
{
    [SerializeField] private Button button_goBack;
    [SerializeField] private Button button_goBackFromSingle;

    [SerializeField] private Button button_singleplayer;
    [SerializeField] private Button button_multiplayer;

    [SerializeField] private Button button_start;

    [SerializeField] private Slider slider_length;
    [SerializeField] private Slider slider_curviness;
    [SerializeField] private Slider slider_difficulty;

    [SerializeField] private TMP_InputField inputField_seed;

    [SerializeField] private enum ActiveCanvas { GAMEMODE, SINGLE, MULTI};

    // Start is called before the first frame update
    void Start()
    {
        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); PlayClickSound(); });
        button_goBackFromSingle.onClick.AddListener(() => { GoBackFromSingleButtonFunction(); PlayClickSound(); });

        button_singleplayer.onClick.AddListener(() => { SingleplayerButtonFunction(); PlayClickSound(); });
        button_multiplayer.onClick.AddListener(() => { PlayClickSound(); });

        button_start.onClick.AddListener(() => { PlayClickSound(); StartButtonFunction(); });
    }


    public override void Show()
    {
        if (!PlayerPrefs.HasKey("length"))
            PlayerPrefs.SetInt("length", 30);
        if (!PlayerPrefs.HasKey("curviness"))
            PlayerPrefs.SetFloat("curviness", 3f);
        if (!PlayerPrefs.HasKey("difficulty"))
            PlayerPrefs.SetInt("difficulty", 1);
        if (!PlayerPrefs.HasKey("seed"))
            PlayerPrefs.SetInt("seed", 69420);

        int length = PlayerPrefs.GetInt("length");
        float curviness = PlayerPrefs.GetFloat("curviness");
        int diffficulty = PlayerPrefs.GetInt("difficulty");

        slider_length.minValue = 10;
        slider_length.maxValue = 100;
        slider_length.wholeNumbers = true;
        slider_length.value = length;

        slider_curviness.minValue = 0f;
        slider_curviness.maxValue = 20f;
        slider_curviness.value = curviness;

        slider_difficulty.minValue = 1f;
        slider_difficulty.maxValue = 5f;
        slider_difficulty.wholeNumbers = true;
        slider_difficulty.value = diffficulty;

        inputField_seed.ActivateInputField();

        base.Show();

        switchCanvas(ActiveCanvas.GAMEMODE);
    }

    private void switchCanvas(ActiveCanvas canvas)
    {
        Hide();

        switch (canvas)
        {
            case ActiveCanvas.GAMEMODE:
                canvases[0].enabled = true;
                break;
            case ActiveCanvas.SINGLE:
                canvases[1].enabled = true;
                break;
            case ActiveCanvas.MULTI:
                canvases[2].enabled = true;
                break;
        }
    }

    public void GoBackButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().MainMenu();

        MenuController mc = GameObject.Find("MainMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void SingleplayerButtonFunction()
    {
        switchCanvas(ActiveCanvas.SINGLE);
        
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Singleplayer();
    }

    public void GoBackFromSingleButtonFunction()
    {
        switchCanvas(ActiveCanvas.GAMEMODE);

        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Gamemode();
    }

    public void StartButtonFunction()
    {
        PlayerPrefs.SetInt("length", (int)slider_length.value);

        int seed;
        if (int.TryParse(inputField_seed.text, out seed)){
            PlayerPrefs.SetInt("seed", seed);   
        }

        PlayerPrefs.SetFloat("curviness", slider_curviness.value);
        PlayerPrefs.SetInt("difficulty", (int)slider_difficulty.value);
        
        SceneManager.LoadSceneAsync("TrackGen");
    }
}
