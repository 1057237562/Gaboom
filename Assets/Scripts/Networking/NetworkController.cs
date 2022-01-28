using Gaboom.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
public class NetworkController : MonoBehaviour,INetworkUpdateSystem
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

    Queue<Action> pendingPackage = new Queue<Action>();

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        transport = GetComponent<UNetTransport>();
    }

    const int messageSize = 1024;
    //const int maxiumSize = 60000;

    public void StartHost()
    {
        if (networkManager.IsHost) return;
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

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler("RequireMap", (senderClientId, reader) =>
        {
            string filename = Application.dataPath + "/maps/" + mapname + ".gmap";
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (fs.Length < messageSize)
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                using (FastBufferWriter fastBufferWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(buffer), Allocator.Temp))
                {
                    fastBufferWriter.TryBeginWrite(FastBufferWriter.GetWriteSize(buffer));
                    fastBufferWriter.WriteValueSafe(buffer.Length);
                    fastBufferWriter.WriteBytes(buffer);
                    networkManager.CustomMessagingManager.SendNamedMessage("MapData", senderClientId, fastBufferWriter, NetworkDelivery.Reliable);
                }
                fs.Close();
                fs.Dispose();
            }
            else
            {
                int i = 0;
                for (; i < fs.Length / messageSize; i++)
                {
                    pendingPackage.Enqueue(new Action(() =>
                    {
                        byte[] buffer = new byte[messageSize];
                        fs.Read(buffer, 0, messageSize);
                        using (FastBufferWriter fastBufferWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(buffer), Allocator.Temp))
                        {
                            fastBufferWriter.TryBeginWrite(FastBufferWriter.GetWriteSize(buffer));
                            fastBufferWriter.WriteValueSafe(buffer.Length);
                            fastBufferWriter.WriteBytes(buffer);
                            networkManager.CustomMessagingManager.SendNamedMessage("MapData", senderClientId, fastBufferWriter, NetworkDelivery.Reliable);
                        }
                    }));
                }
                pendingPackage.Enqueue(new Action(() =>
                {
                    byte[] buffer = new byte[fs.Length - messageSize * i];
                    fs.Read(buffer, 0, buffer.Length);
                    using (FastBufferWriter fastBufferWriter = new FastBufferWriter(FastBufferWriter.GetWriteSize(buffer), Allocator.Temp))
                    {
                        fastBufferWriter.TryBeginWrite(FastBufferWriter.GetWriteSize(buffer));
                        fastBufferWriter.WriteValueSafe(buffer.Length);
                        fastBufferWriter.WriteBytes(buffer);
                        networkManager.CustomMessagingManager.SendNamedMessage("MapData", senderClientId, fastBufferWriter, NetworkDelivery.Reliable);
                    }
                    fs.Close();
                    fs.Dispose();
                }));
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
        if (!networkManager.IsHost) return;
        networkManager.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
        networkManager.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        gameStarted = true;
        Destroy(this);
    }

    public void StartClient()
    {
        if (networkManager.IsClient) return;
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
            using (FileStream fs = new FileStream(Application.dataPath + "/maps/" + mapname + ".gmap", FileMode.Append))
            {
                reader.ReadValueSafe(out int size);
                byte[] buffer = new byte[size];
                reader.ReadBytesSafe(ref buffer, size);
                fs.Write(buffer,0,buffer.Length);
            }
        });
    }

    public void NetworkUpdate(NetworkUpdateStage updateStage)
    {
        if(pendingPackage.Count >0)
        {
            pendingPackage.Dequeue().Invoke();
        }
    }
}
