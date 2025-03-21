using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MenuController
{
    [SerializeField] private Button button_goBack;

    [SerializeField] private Slider slider_volume;
    [SerializeField] private Slider slider_sensitivity;

    // Start is called before the first frame update
    void Start()
    {
        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Show()
    {
        if (!PlayerPrefs.HasKey("volume"))
            PlayerPrefs.SetFloat("volume", 1.0f);
        if (!PlayerPrefs.HasKey("sensitivity"))
            PlayerPrefs.SetFloat("sensitivity", 1.0f);

        float volume = PlayerPrefs.GetFloat("volume");
        float sensitivity = PlayerPrefs.GetFloat("sensitivity");

        slider_volume.value = volume;
        slider_sensitivity.value = sensitivity;

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
