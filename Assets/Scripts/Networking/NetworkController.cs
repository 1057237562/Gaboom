using Gaboom.Scene;
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

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("RequireMap", (senderClientId,reader) => {
            using(FileStream fs = new FileStream(Application.dataPath + "/maps/" + mapname,FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                FastBufferWriter fastBufferWriter = new FastBufferWriter(buffer.Length, Allocator.Temp);
                //fastBufferWriter.WriteValueSafe(buffer.Length);
                fastBufferWriter.WriteBytes(buffer);
                networkManager.CustomMessagingManager.SendNamedMessage("MapData",senderClientId,fastBufferWriter);
            }
        });

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.StartHost();
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
    {
        MemoryStream ms = new MemoryStream(connectionData);
        XmlReader reader = new XmlTextReader(ms);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        using FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp, int.MaxValue);
        writer.WriteValueSafe(mapname);
        networkManager.CustomMessagingManager.SendNamedMessage("MapNameSync", clientId, writer, NetworkDelivery.Reliable);

        if (gameStarted)
            callback(false, null, false, null, null);
        else
            callback(false, null, true, null, null);
    }

    public void OnMapChanged()
    {
        SceneMaterial.filepath = Application.dataPath + "/maps/" + mapname +".gmap";
        using FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp, int.MaxValue);
        writer.WriteValueSafe(mapname);
        networkManager.CustomMessagingManager.SendNamedMessageToAll("MapNameSync", writer, NetworkDelivery.Reliable);
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
            reader.ReadValueSafe(out mapname);
            string mapPath = Application.dataPath + "/maps";
            SceneMaterial.filepath = mapPath + "/" + mapname + ".gmap";
            if (!File.Exists(mapPath + "/" + mapname))
            {
                networkManager.CustomMessagingManager.SendNamedMessage("RequireMap",senderClientId,new FastBufferWriter(0,Allocator.Temp),NetworkDelivery.Reliable);
            }
        });

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("MapData", (senderClientId, reader) => { 
            using (FileStream fs = new FileStream(Application.dataPath + "/maps/" + mapname, FileMode.Create))
            {
                byte[] buffer = new byte[reader.Length];
                reader.ReadBytesSafe(ref buffer,reader.Length);
                fs.Write(buffer,0,buffer.Length);
            }
        });
        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());
        networkManager.StartClient();
    }
}
