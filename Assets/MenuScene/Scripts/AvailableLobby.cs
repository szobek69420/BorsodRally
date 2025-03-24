using System.Net;

public class AvailableLobby
{
    public IPEndPoint ip;
    public string ownerName;
    public int playerCount;
    public int maxPlayerCount;

    public AvailableLobby(IPEndPoint ip, string ownerName, int playerCount, int maxPlayerCount)
    {
        this.ip = ip;
        this.ownerName = ownerName;
        this.playerCount = playerCount;
        this.maxPlayerCount = maxPlayerCount;
    }
}
