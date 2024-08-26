using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using CNet;
using UnityEngine;
using UnityEngine.Events;

public class NetManager : MonoBehaviour, IEventNetClient
{
    [Serializable]
    public class Settings
    {
        public int MaxPacketSize { get => maxPacketSize; set => maxPacketSize = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        [SerializeField] private int maxPacketSize = 128;
        [SerializeField] private int bufferSize = 4096;
    }

    public static NetManager Instance { get; private set; }
    public const int MAX_NET_OBJECTS = 32767; // Max size of short

    public NetSystem System { get; private set; }
    public Guid ID { get; private set; }
    public NetEndPoint RemoteEndPoint { get; private set; }
    public string Address { get => address; set => address = value; }
    public int Port { get => port; set => port = value; }
    public Settings TCPSettings { get => tcpSettings; }
    public Settings UDPSettings { get => udpSettings; }
    public bool Connected { get; private set; }
    public bool IsHost { get; private set; }
    public Dictionary<int, NetService> NetServices { get; private set; }
    public Dictionary<int, SyncedObject> NetObjects { get; private set; }
    public List<SyncedObject> NetPrefabs { get => netPrefabs; }


    [Header("Network Settings")]
    [SerializeField] private string address = "localhost";
    [SerializeField] private int port = 7777;
    [SerializeField] private Settings tcpSettings;
    [SerializeField] private Settings udpSettings;

    [Header("Network Services")]
    [SerializeField] private List<NetService> netServices;

    [Header("Network Objects")]
    [SerializeField] private List<SyncedObject> netObjets;
    [SerializeField] private List<SyncedObject> netPrefabs;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("<color=red><b>CNet</b></color>: There are multiple NetManager instances in the scene, destroying one.");
            Destroy(this);
        }

        Initialize();
    }

    private void Initialize()
    {
        //Initialize NetServices
        NetServices = new Dictionary<int, NetService>();
        for (int i = 0; i < netServices.Count; i++)
        {
            NetServices.Add(i, netServices[i]);
        }

        // Initialize NetObjects
        NetObjects = new Dictionary<int, SyncedObject>();
        for (int i = 0; i < netObjets.Count; i++)
        {
            NetObjects.Add(i, netObjets[i]);
            netObjets[i].NetID = i;
        }
    }

    public void Connect()
    {
        Debug.Log("<color=red><b>CNet</b></color>: Starting NetManager...");
        System = new NetSystem(address, port);
        System.TCP.MaxPacketSize = tcpSettings.MaxPacketSize;
        System.TCP.BufferSize = tcpSettings.BufferSize;
        System.UDP.MaxPacketSize = udpSettings.MaxPacketSize;
        System.UDP.BufferSize = udpSettings.BufferSize;
        System.RegisterInterface(this);
        System.Connect();
        isInitialized = true;
        Debug.Log("<color=red><b>CNet</b></color>: NetManager started");
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitialized)
        {
            System.Update();
        }
    }

    void OnDisable()
    {
        Debug.Log("<color=red><b>CNet</b></color>: NetManager closing...");
        System.Disconnect(RemoteEndPoint);
        System.Close(false);
    }

    public void OnConnected(NetEndPoint remoteEndPoint)
    {
        Debug.Log("<color=red><b>CNet</b></color>: NetManager connected");
        RemoteEndPoint = remoteEndPoint;
        Connected = true;
    }

    public void OnDisconnected(NetEndPoint remoteEndPoint, NetDisconnect disconnect)
    {
        Debug.Log("<color=red><b>CNet</b></color>: NetManager disconnected");
        Connected = false;
        isInitialized = false;
    }

    public void OnPacketReceived(NetEndPoint remoteEndPoint, NetPacket packet, PacketProtocol protocol)
    {
        int serviceID = packet.ReadShort();
        if (serviceID > 0 && serviceID < netServices.Count)
        {
            netServices[serviceID].ReceiveData(packet);
        }
        else
        {
            Debug.LogError("<color=red><b>CNet</b></color>: Service with ID " + serviceID + " not found");
        }
    }

    public void OnNetworkError(NetEndPoint remoteEndPoint, SocketException socketException)
    {
        Debug.Log("<color=red><b>CNet</b></color>: Network error - " + socketException.SocketErrorCode.ToString());
    }

    public void Send(NetPacket packet, PacketProtocol protocol)
    {
        System.Send(RemoteEndPoint, packet, protocol);
    }

    public void Send(NetPacket packet, PacketProtocol protocol, float delay)
    {
        StartCoroutine(SendCoroutine(packet, protocol, delay));
    }

    private IEnumerator SendCoroutine(NetPacket packet, PacketProtocol protocol, float delay)
    {
        yield return new WaitForSeconds(delay);
        Send(packet, protocol);
    }

    internal int GenerateNetID()
    {
        int id = UnityEngine.Random.Range(netObjets.Count, MAX_NET_OBJECTS);
        if (NetObjects.ContainsKey(id))
        {
            return GenerateNetID();
        }

        return id;
    }
}
