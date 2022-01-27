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

    public List<ulong> players;

    NetworkManager networkManager;
    UNetTransport transport;

    public string mapname;
    public static bool gameStarted = false;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        transport = GetComponent<UNetTransport>();
    }

    private void OnDestroy()
    {
        networkManager.Shutdown();
    }

    public void StartHost()
    {
        transport.ConnectAddress = address_hfield.text;
        ushort port = 25565;
        ushort.TryParse(port_hfield.text, out port);
        transport.ConnectPort = port;

        XmlDocument doc = new XmlDocument();

        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        XmlElement root = doc.CreateElement("Root");
        doc.AppendChild(root);

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.StartHost();

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("RequireMap", (senderClientId, reader) => {
            using (FileStream fs = new FileStream(Application.dataPath + "/maps/" + mapname + ".gmap", FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                FastBufferWriter fastBufferWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(buffer)*2, Allocator.Temp);
                Debug.Log(FastBufferWriter.GetWriteSize(buffer) + ":" + buffer.Length);
                Debug.Log(fastBufferWriter.TryBeginWrite(FastBufferWriter.GetWriteSize(buffer)));
                fastBufferWriter.WriteBytes(buffer);
                Debug.Log(fastBufferWriter.Length);
                networkManager.CustomMessagingManager.SendNamedMessage("MapData", senderClientId, fastBufferWriter);
            }
        });
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
    {
        MemoryStream ms = new MemoryStream(connectionData);
        XmlReader reader = new XmlTextReader(ms);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        using (FastBufferWriter writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(mapname), Allocator.Temp))
        {
            writer.WriteValueSafe(mapname);
            networkManager.CustomMessagingManager.SendNamedMessage("MapNameSync", clientId, writer, NetworkDelivery.Reliable);
        }

        if (gameStarted)
        {
            callback(false, null, false, null, null);
        }
        else
        {
            players.Add(clientId);
            callback(false, null, true, null, null);
        }
    }

    public void OnMapChanged()
    {
        SceneMaterial.filepath = Application.dataPath + "/maps/" + mapname + ".gmap";
        if (networkManager.IsHost)
        {
            using (FastBufferWriter writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(mapname), Allocator.Temp))
            {
                writer.WriteValueSafe(mapname);
                networkManager.CustomMessagingManager.SendNamedMessageToAll("MapNameSync", writer, NetworkDelivery.Reliable);
            }
        }
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

        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        XmlElement root = doc.CreateElement("Root");
        doc.AppendChild(root);

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());
        networkManager.StartClient();

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("MapNameSync", (senderClientId, reader) =>
        {
            reader.ReadValueSafe(out mapname);
            string mapPath = Application.dataPath + "/maps";
            if (!Directory.Exists(mapPath))
            {
                Directory.CreateDirectory(mapPath);
            }
            SceneMaterial.filepath = mapPath + "/" + mapname + ".gmap";
            if (!File.Exists(mapPath + "/" + mapname + ".gmap"))
            {
                networkManager.CustomMessagingManager.SendNamedMessage("RequireMap",senderClientId,new FastBufferWriter(0,Allocator.Temp),NetworkDelivery.Reliable);
            }
        });

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("MapData", (senderClientId, reader) => { 
            using (FileStream fs = new FileStream(Application.dataPath + "/maps/" + mapname + ".gmap", FileMode.Create))
            {
                byte[] buffer = new byte[reader.Length];
                reader.ReadBytesSafe(ref buffer,reader.Length);
                fs.Write(buffer,0,buffer.Length);
            }
        });
    }
}
