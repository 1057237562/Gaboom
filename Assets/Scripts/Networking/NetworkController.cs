using Gaboom.Scene;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkController : NetworkManager
{

    public struct MapSyncMessage : NetworkMessage
    {
        public string mapname;
        public long mapsize;
    }

    public struct MapData : NetworkMessage
    {
        public byte[] data;
    }

    public struct RequireMap : NetworkMessage
    {
    }

    public struct SummonCamera : NetworkMessage {
    }

    public Text address_hfield;
    public Text port_hfield;
    public Text password_hfield;
    public Text address_cfield;
    public Text port_cfield;
    public Text password_cfield;

    public Slider progressbar;

    public List<ulong> players;

    public string mapname;

    long fileLength;
    public static bool gameStarted = false;

    //Queue<Action> pendingPackage = new Queue<Action>();
    public GameObject networkCamera;

    const int messageSize = 1024;
    //const int maxiumSize = 60000;

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        MapSyncMessage message = new MapSyncMessage()
        {
            mapname = mapname, mapsize = fileLength
        };
        conn.Send(message);
    }

    public void BtnStartHost()
    {
        if (mode == NetworkManagerMode.Host) return;
        networkAddress = address_hfield.text;
        ushort port = 25565;
        ushort.TryParse(port_hfield.text, out port);
        ((TelepathyTransport)transport).port = port;

        XmlDocument doc = new XmlDocument();

        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        XmlElement root = doc.CreateElement("Root");
        doc.AppendChild(root);

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        NetworkClient.RegisterHandler<MapSyncMessage>((message) =>{ });
        NetworkServer.RegisterHandler<RequireMap>((conn, message) => {
            StartCoroutine(SendMapData(conn));
        }, true);

        NetworkServer.RegisterHandler<SummonCamera>((conn, message) => {
            GameObject cam = Instantiate(networkCamera);
            NetworkServer.AddPlayerForConnection(conn,cam);
        });

        StartHost();
    }

    IEnumerator SendMapData(NetworkConnection conn)
    {
        string filename = Application.dataPath + "/maps/" + mapname + ".gmap";
        FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        byte[] buffer;
        if (fs.Length < messageSize)
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            MapData mapData = new MapData() { data = buffer };
            conn.Send(mapData);
            fs.Close();
            fs.Dispose();
        }
        else
        {
            int i = 0;
            for (; i < fs.Length / messageSize; i++)
            {
                buffer = new byte[messageSize];
                fs.Read(buffer, 0, messageSize);
                MapData mapData = new MapData() { data = buffer };
                conn.Send(mapData);
                progressbar.gameObject.SetActive(true);
                progressbar.value = (float)i / (fs.Length / messageSize);
            }
            buffer = new byte[fs.Length - messageSize * i];
            fs.Read(buffer, 0, buffer.Length);
            MapData data = new MapData() { data = buffer };
            conn.Send(data);
            fs.Close();
            fs.Dispose();
            progressbar.gameObject.SetActive(false);
        }
        yield return 0;
    }

    /*private void ApprovalCheck(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
    {
        MemoryStream ms = new MemoryStream(connectionData);
        XmlReader reader = new XmlTextReader(ms);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        using (FastBufferWriter writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(mapname) + FastBufferWriter.GetWriteSize(fileLength), Allocator.Temp))
        {
            writer.WriteValueSafe(mapname);
            writer.WriteValueSafe(fileLength);
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
    }*/

    public void OnMapChanged()
    {
        SceneMaterial.filepath = Application.dataPath + "/maps/" + mapname + ".gmap";
        fileLength = new FileInfo(SceneMaterial.filepath).Length;
        if (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
        {
            MapSyncMessage message = new MapSyncMessage() { mapname = mapname,mapsize = fileLength};
            NetworkServer.SendToAll(message);
        }
    }

    public void StartGame()
    {
        if (mode != NetworkManagerMode.Host && mode != NetworkManagerMode.ServerOnly) return;
        //networkManager.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
        //networkManager.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        ServerChangeScene("GameScene");
        gameStarted = true;
    }

    public void BtnStartClient()
    {
        if (mode != NetworkManagerMode.Offline) return;
        networkAddress = address_cfield.text;
        ushort port = 25565;
        ushort.TryParse(port_cfield.text,out port);
        ((TelepathyTransport)transport).port = port;

        XmlDocument doc = new XmlDocument();

        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        XmlElement root = doc.CreateElement("Root");
        doc.AppendChild(root);

        StringWriter writer = new StringWriter();
        XmlTextWriter xw = new XmlTextWriter(writer);

        doc.WriteTo(xw);

        //networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(writer.ToString());

        NetworkClient.RegisterHandler<MapSyncMessage>((message) => {
            mapname = message.mapname;
            fileLength = message.mapsize;
            string mapPath = Application.dataPath + "/maps";
            if (!Directory.Exists(mapPath))
            {
                Directory.CreateDirectory(mapPath);
            }
            SceneMaterial.filepath = mapPath + "/" + mapname + ".gmap";
            if (!File.Exists(mapPath + "/" + mapname + ".gmap") || new FileInfo(SceneMaterial.filepath).Length != fileLength)
            {
                NetworkClient.Send(new RequireMap());
            }
        }, true);

        NetworkClient.RegisterHandler<MapData>((data) => {
            string filepath = Application.dataPath + "/maps/" + mapname + ".gmap";
            using (FileStream fs = new FileStream(filepath, FileMode.Append))
            {
                fs.Write(data.data, 0, data.data.Length);
            }
            FileInfo info = new FileInfo(filepath);
            if (info.Length != fileLength)
            {
                progressbar.gameObject.SetActive(true);
                progressbar.value = (float)info.Length / fileLength;
            }
            else
            {
                progressbar.gameObject.SetActive(false);
            }
        }, true);

        StartClient();
    }

    public void SpawnNetworkCamera()
    {
        NetworkClient.Send(new SummonCamera());
    }
}