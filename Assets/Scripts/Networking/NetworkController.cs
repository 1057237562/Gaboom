using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UNetTransport))]
public class NetworkController : MonoBehaviour
{
    public Text address_field;
    public Text port_field;

    NetworkManager networkManager;
    UNetTransport transport;

    public string mapname;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        transport = GetComponent<UNetTransport>();
    }

    public void StartHost()
    {
        transport.ConnectAddress = address_field.text;
        ushort port = 25565;
        ushort.TryParse(port_field.text, out port);
        transport.ConnectPort = port;
        networkManager.StartHost();
    }

    public void StartClient()
    {
        transport.ConnectAddress = address_field.text;
        ushort port = 25565;
        ushort.TryParse(port_field.text,out port);
        transport.ConnectPort = port;
        networkManager.StartClient();
    }
}
