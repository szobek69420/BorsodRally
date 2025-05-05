using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using UnityEngine;

public class LocalAddressQuerier
{
    //returns true if the query was gg-ful
    public static bool GetLocalAddress(out IPAddress localAddress, out IPAddress broadcastAddress)
    {
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in interfaces)
        {
            if (ni.OperationalStatus != OperationalStatus.Up)//only active interfaces
                continue;

            IPInterfaceProperties properties = ni.GetIPProperties();
            foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
            {
                if (unicastAddress.IPv4Mask == null)//only ipv4 addresses
                    continue;

                byte[] ipBytes = unicastAddress.Address.GetAddressBytes();
                byte[] maskBytes = unicastAddress.IPv4Mask.GetAddressBytes();
                byte[] broadcastBytes = new byte[ipBytes.Length];

                if (ipBytes.Length != maskBytes.Length)//invalid mask
                    continue;

                for (int i = 0; i < ipBytes.Length; i++)
                    broadcastBytes[i] = (byte)(ipBytes[i] | (~maskBytes[i] & 0xFF));//0xFF is necessary as the result of ~maskBytes[i] is an int, not a byte

                localAddress = new IPAddress(ipBytes);
                broadcastAddress = new IPAddress(broadcastBytes);
                return true;
            }
        }

        localAddress = null;
        broadcastAddress = null;
        return false;
    }
}
