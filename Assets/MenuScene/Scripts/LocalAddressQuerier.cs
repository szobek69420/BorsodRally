using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using UnityEngine;

public class LocalAddressQuerier
{
    public class NetworkInterfaceInfo
    {
        public string Name { get; private set; } = "default";
        public IPAddress Address { get; private set; } = IPAddress.None;
        public IPAddress BroadcastAddress { get; private set; } = IPAddress.None;
        public byte[] SubnetMask { get; private set; } = null;

        public NetworkInterfaceInfo(string name, IPAddress address, IPAddress broadcastAddress, byte[] subnetMask)
        {
            Name = name;
            Address = address;
            BroadcastAddress = broadcastAddress;
            SubnetMask = subnetMask;
        }

        public bool IsAddressOnSameNetwork(IPAddress address)
        {
            byte[] interfaceNetwork = Address.GetAddressBytes();
            byte[] addressNetwork = address.GetAddressBytes();

            if (interfaceNetwork.Length != addressNetwork.Length)
                return false;

            for (int i = 0; i < interfaceNetwork.Length; i++)
            {
                byte interfaceByte = (byte)(interfaceNetwork[i] & SubnetMask[i]);
                byte addressByte = (byte)(addressNetwork[i] & SubnetMask[i]);

                if(interfaceByte != addressByte) 
                    return false;
            }

            return true;
        }
    }

    //returns true if the query was gg-ful
    public static bool GetLocalAddresses(out NetworkInterfaceInfo[] networkInterfaces)
    {
        List<NetworkInterfaceInfo> infos = new List<NetworkInterfaceInfo>();

        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in interfaces)
        {
            if (ni.OperationalStatus != OperationalStatus.Up)//only active interfaces
                continue;

            IPInterfaceProperties properties = ni.GetIPProperties();

            IPAddress localAddress = null;
            IPAddress broadcastAddress = null;
            byte[] subnetMask = null;

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
                subnetMask = maskBytes;
            }

            if (localAddress == null || broadcastAddress == null)
                continue;

            infos.Add(new NetworkInterfaceInfo(ni.Name, localAddress, broadcastAddress, subnetMask.Clone() as byte[]));
        }

        if(infos.Count==0)
        {
            networkInterfaces = null;
            return false;
        }
        else
        {
            networkInterfaces = infos.ToArray();
            return true;
        }
    }
}
