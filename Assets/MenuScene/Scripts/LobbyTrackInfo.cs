using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

public class LobbyTrackInfo
{
    public const string SEPARATOR = "||";

    public IPEndPoint ip; //the ip address of the netcode network manager
    public int length;
    public int seed;
    public float curviness;
    public int difficulty; //this value doesn't matter in multiplayer

    public LobbyTrackInfo(IPEndPoint ip, int length, int seed, float curviness, int difficulty)
    {
        this.ip = ip;
        this.length = length;
        this.seed = seed;
        this.curviness = curviness;
        this.difficulty = difficulty;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ip.Address.ToString());
        sb.Append(SEPARATOR);
        sb.Append(ip.Port.ToString());
        sb.Append(SEPARATOR);
        sb.Append(length);
        sb.Append(SEPARATOR);
        sb.Append(seed);
        sb.Append(SEPARATOR);
        sb.Append(curviness);
        sb.Append(SEPARATOR);
        sb.Append(difficulty);

        return sb.ToString();
    }

    //the inverse of ToString
    public static LobbyTrackInfo ParseString(string str)
    {
        string[] data = str.Split(SEPARATOR);

        if (data.Length != 6)
            throw new System.Exception("The input is of suboptimal form");

        return new LobbyTrackInfo(
                new IPEndPoint(IPAddress.Parse(data[0]), int.Parse(data[1])),
                int.Parse(data[2]),
                int.Parse(data[3]),
                float.Parse(data[4]),
                int.Parse(data[5])
            );
    }
}
