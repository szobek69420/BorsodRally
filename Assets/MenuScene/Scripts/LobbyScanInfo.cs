using System;

public class LobbyScanInfo
{
    private const string SEPARATOR = "@@";

    public AvailableLobby lobbyInfo;
    public int lastScanCount;  //the last scan to which the server responed

    public LobbyScanInfo(AvailableLobby lobbyInfo, int lastScanCount)
    {
        this.lobbyInfo = lobbyInfo;
        this.lastScanCount = lastScanCount;
    }

    public override string ToString()
    {
        return lastScanCount.ToString() + SEPARATOR + lobbyInfo.ToString();
    }

    public static LobbyScanInfo ParseString(string data)
    {
        int separatorIndex = -1;
        for (int i = 0; i < data.Length - SEPARATOR.Length + 1; i++)
        {
            for (int j = 0; j < SEPARATOR.Length; j++)
            {
                if (data[i + j] != SEPARATOR[j])
                    goto SEPARATOR_NOT_FOUND;
            }
            separatorIndex = i;
            break;

        SEPARATOR_NOT_FOUND:
            continue;
        }

        if (separatorIndex == -1)
            throw new Exception("Invalid LobbyScanInfo string");

        string lastScanCountString = data.Substring(0, separatorIndex);
        string lobbyInfoString = data.Substring(separatorIndex + SEPARATOR.Length);

        return new LobbyScanInfo(AvailableLobby.ParseString(lobbyInfoString), Convert.ToInt32(lastScanCountString));
    }
}
