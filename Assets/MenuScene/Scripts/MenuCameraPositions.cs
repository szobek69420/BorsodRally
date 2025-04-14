using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraPositions : MonoBehaviour
{
    [SerializeField] private Transform cum;
    [SerializeField] private Transform car;

    [SerializeField] private Vector3 cumPos_mainMenu;
    [SerializeField] private Vector3 cumRot_mainMenu;
    [SerializeField] private Vector3 carPos_mainMenu;
    [SerializeField] private Vector3 carRot_mainMenu;

    [SerializeField] private Vector3 cumPos_garage;
    [SerializeField] private Vector3 cumRot_garage;
    [SerializeField] private Vector3 carPos_garage;
    [SerializeField] private Vector3 carRot_garage;
    
    [SerializeField] private Vector3 cumPos_settings;
    [SerializeField] private Vector3 cumRot_settings;
    [SerializeField] private Vector3 carPos_settings;
    [SerializeField] private Vector3 carRot_settings;

    [SerializeField] private Vector3 cumPos_gamemode;
    [SerializeField] private Vector3 cumRot_gamemode;
    [SerializeField] private Vector3 carPos_gamemode;
    [SerializeField] private Vector3 carRot_gamemode;

    [SerializeField] private Vector3 cumPos_singleplayer;
    [SerializeField] private Vector3 cumRot_singleplayer;
    [SerializeField] private Vector3 carPos_singleplayer;
    [SerializeField] private Vector3 carRot_singleplayer;

    [SerializeField] private Vector3 cumPos_multiplayer;
    [SerializeField] private Vector3 cumRot_multiplayer;
    [SerializeField] private Vector3 carPos_multiplayer;
    [SerializeField] private Vector3 carRot_multiplayer;


    private Vector3 targetCumPos;
    private Quaternion targetCumRot;

    private Vector3 targetCarPos;
    private Quaternion targetCarRot;

    public Vector3 TargetCarPos
    {
        get; private set;
    }

    public Quaternion TargetCarRot
    {
        get; private set;
    }

    public Transform Car
    {
        get { return car; } set { car = value; }
    }

    private void Start()
    {
        targetCumPos = cumPos_mainMenu;
        targetCumRot = Quaternion.Euler(cumRot_mainMenu);

        targetCarPos = carPos_mainMenu;
        targetCarRot = Quaternion.Euler(carRot_mainMenu);

        TargetCarPos = carPos_mainMenu;
        TargetCarRot = Quaternion.Euler(carRot_mainMenu);
    }

    private void FixedUpdate()
    {
        cum.position = Vector3.Lerp(cum.position, targetCumPos, 0.1f);
        cum.rotation = Quaternion.Lerp(cum.rotation, targetCumRot, 0.1f);

        car.position = Vector3.Lerp(car.position, targetCarPos, 0.1f);
        car.rotation = Quaternion.Lerp(car.rotation, targetCarRot, 0.1f);
    }

    public void MainMenu()
    {
        targetCumPos = cumPos_mainMenu;
        targetCumRot = Quaternion.Euler(cumRot_mainMenu);

        targetCarPos = carPos_mainMenu;
        targetCarRot = Quaternion.Euler(carRot_mainMenu);
    }

    public void Garage()
    {
        targetCumPos = cumPos_garage;
        targetCumRot = Quaternion.Euler(cumRot_garage);

        targetCarPos = carPos_settings;
        targetCarRot = Quaternion.Euler(carRot_garage);
    }

    public void Settings()
    {
        targetCumPos = cumPos_settings;
        targetCumRot = Quaternion.Euler(cumRot_settings);

        targetCarPos = carPos_settings;
        targetCarRot = Quaternion.Euler(carRot_settings);
    }

    public void Gamemode()
    {
        targetCumPos = cumPos_gamemode;
        targetCumRot = Quaternion.Euler(cumRot_gamemode);

        targetCarPos = carPos_gamemode;
        targetCarRot = Quaternion.Euler(carRot_gamemode);
    }

    public void Singleplayer()
    {
        targetCumPos = cumPos_singleplayer;
        targetCumRot = Quaternion.Euler(cumRot_singleplayer);

        targetCarPos = carPos_singleplayer;
        targetCarRot = Quaternion.Euler(carRot_singleplayer);
    }

    public void Multiplayer()
    {
        targetCumPos = cumPos_multiplayer;
        targetCumRot = Quaternion.Euler(cumRot_multiplayer);

        targetCarPos = carPos_multiplayer;
        targetCarRot = Quaternion.Euler(carRot_multiplayer);
    }
}
