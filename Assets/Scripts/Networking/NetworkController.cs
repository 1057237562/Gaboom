using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
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
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.StartHost();
    }

    private void ApprovalCheck(byte[] connectionData,ulong clientId,ConnectionApprovedDelegate callback)
    {
        MemoryStream ms = new MemoryStream(connectionData);
        XmlReader reader = new XmlTextReader(ms);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        callback(false,null,true,null,null);
    }

    public void StartGame()
    {
        networkManager.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
    }

    public void StartClient()
    {
        transport.ConnectAddress = address_field.text;
        ushort port = 25565;
        ushort.TryParse(port_field.text,out port);
        transport.ConnectPort = port;

        XmlDocument doc = new XmlDocument();

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());
        networkManager.StartClient();
    }
}
