using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct JoinedPlayerInfo : INetworkSerializable
{
    private static PlayerInfo PLACEHOLDER = new PlayerInfo("Water Weight", 69420);

    public int playerCount;
    public PlayerInfo player1;
    public PlayerInfo player2;
    public PlayerInfo player3;
    public PlayerInfo player4;

    public JoinedPlayerInfo(int canBeAnything)
    {
        playerCount = 0;
        this.player1 = PLACEHOLDER;
        this.player2 = PLACEHOLDER;
        this.player3 = PLACEHOLDER;
        this.player4 = PLACEHOLDER;
    }
    public JoinedPlayerInfo(PlayerInfo player1)
    {
        playerCount = 1;
        this.player1 = player1;
        this.player2 = PLACEHOLDER;
        this.player3 = PLACEHOLDER;
        this.player4 = PLACEHOLDER;
    }
    public JoinedPlayerInfo(PlayerInfo player1, PlayerInfo player2)
    {
        playerCount = 2;
        this.player1 = player1;
        this.player2 = player2;
        this.player3 = PLACEHOLDER;
        this.player4 = PLACEHOLDER;
    }
    public JoinedPlayerInfo(PlayerInfo player1, PlayerInfo player2, PlayerInfo player3)
    {
        playerCount = 3;
        this.player1 = player1;
        this.player2 = player2;
        this.player3 = player3;
        this.player4 = PLACEHOLDER;
    }
    public JoinedPlayerInfo(PlayerInfo player1, PlayerInfo player2, PlayerInfo player3, PlayerInfo player4)
    {
        playerCount = 4;
        this.player1 = player1;
        this.player2 = player2;
        this.player3 = player3;
        this.player4 = player4;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerCount);

        serializer.SerializeValue(ref player1);
        serializer.SerializeValue(ref player2);
        serializer.SerializeValue(ref player3);
        serializer.SerializeValue(ref player4);
    }
}
