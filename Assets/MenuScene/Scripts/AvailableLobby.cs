using System.Net;
using System.Text;

public class AvailableLobby
{
    public const string SEPARATOR = "||";

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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ip.Address.ToString());
        sb.Append(SEPARATOR);
        sb.Append(ip.Port.ToString());
        sb.Append(SEPARATOR);
        sb.Append(ownerName);
        sb.Append(SEPARATOR);
        sb.Append(playerCount.ToString());
        sb.Append(SEPARATOR);
        sb.Append(maxPlayerCount.ToString());

        return sb.ToString();
    }

    //the inverse of ToString
    public static AvailableLobby ParseString(string str)
    {
        string[] data = str.Split(SEPARATOR);

        if (data.Length != 5)
            throw new System.Exception("The input is of suboptimal form");

        return new AvailableLobby(
                new IPEndPoint(IPAddress.Parse(data[0]), int.Parse(data[1])),
                data[2],
                int.Parse(data[3]),
                int.Parse(data[4])
            );
    }
}
