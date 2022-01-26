using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Netcode.NetworkManager;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UNetTransport))]
public class NetworkController : MonoBehaviour
{
    public Text address_hfield;
    public Text port_hfield;
    public Text password_hfield;
    public Text address_cfield;
    public Text port_cfield;
    public Text password_cfield;

    NetworkManager networkManager;
    UNetTransport transport;

    public string mapname;
    public static bool gameStarted = false;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        transport = GetComponent<UNetTransport>();
    }

    public void StartHost()
    {
        transport.ConnectAddress = address_hfield.text;
        ushort port = 25565;
        ushort.TryParse(port_hfield.text, out port);
        transport.ConnectPort = port;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.StartHost();
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
    {
        MemoryStream ms = new MemoryStream(connectionData);
        XmlReader reader = new XmlTextReader(ms);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        using FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp);
        writer.WriteValueSafe(mapname);
        networkManager.CustomMessagingManager.SendNamedMessage("MapNameSync", clientId, writer, NetworkDelivery.Reliable);

        if (gameStarted)
            callback(false, null, false, null, null);
        else
            callback(false, null, true, null, null);
    }

    public void StartGame()
    {
        networkManager.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        gameStarted = true;
        Destroy(this);
    }

    public void StartClient()
    {
        transport.ConnectAddress = address_cfield.text;
        ushort port = 25565;
        ushort.TryParse(port_cfield.text,out port);
        transport.ConnectPort = port;

        XmlDocument doc = new XmlDocument();

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("MapNameSync", (senderClientId, reader) =>
        {
            reader.ReadValueSafe(out string mapname); //Example
        });

        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());
        networkManager.StartClient();
    }
}
