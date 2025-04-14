using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageMenuController : MenuController
{

    public GameObject[] carPrefabs;

    [SerializeField] private int currentIndex = 0;
    [SerializeField] private GameObject currentCar;

    [SerializeField] private Button button_left;
    [SerializeField] private Button button_right;

    [SerializeField] private Button button_goBack;

    // Start is called before the first frame update
    void Start()
    {
        currentCar = GameObject.Find("car_menu");

        button_left.onClick.AddListener(() => { LeftButtonFunction(); PlayClickSound(); });
        button_right.onClick.AddListener(() => { RightButtonFunction(); PlayClickSound(); });

        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); PlayClickSound(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeftButtonFunction()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = carPrefabs.Length - 1;

        ShowCar(currentIndex);
    }

    public void RightButtonFunction()
    {
        currentIndex++;
        if (currentIndex >= carPrefabs.Length)
            currentIndex = 0;

        ShowCar(currentIndex);
    }

    void ShowCar(int index)
    {
        if (currentCar != null)
            Destroy(currentCar);

        currentCar = Instantiate(carPrefabs[index], 
            GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().TargetCarPos, 
            GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().TargetCarRot);

        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Car = currentCar.transform;
    }

    public void GoBackButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().MainMenu();

        MenuController mc = GameObject.Find("MainMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }
}
