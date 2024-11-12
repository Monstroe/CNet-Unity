using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using CNet;

public enum SyncOn
{
    ClientSide,
    ServerSide
}

public class NetManager : MonoBehaviour, IEventNetClient
{
    [Serializable]
    public struct Settings
    {
        public int MaxPacketSize { get => maxPacketSize; set => maxPacketSize = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        [SerializeField] private int maxPacketSize;
        [SerializeField] private int bufferSize;
    }

    public static NetManager Instance { get; private set; }
    public const int MAX_NET_OBJECTS = 32767; // Max size of short

    public NetSystem System { get; private set; }
    public Guid ID { get; internal set; }
    public int RoomCode { get; internal set; }
    public List<Guid> RoomMembers { get; internal set; }
    public NetEndPoint RemoteEndPoint { get; private set; }
    public string Address { get => address; set => address = value; }
    public int Port { get => port; set => port = value; }
    public Settings TCPSettings { get => tcpSettings; }
    public Settings UDPSettings { get => udpSettings; }
    public bool Connected { get; private set; }
    public bool IsHost { get; internal set; }
    public bool InRoom { get; internal set; }
    public SyncedObject NetPlayer { get => netPlayer; }
    public SyncedObject NetOtherPlayer { get => netOtherPlayer; }
    public Dictionary<int, NetService> NetServices { get; private set; }
    public Dictionary<int, SyncedObject> NetObjects { get; private set; }
    public List<SyncedObject> NetPrefabs { get => netPrefabs; }


    [Header("Network Settings")]
    [SerializeField] private string address = "127.0.0.1";
    [SerializeField] private int port = 7777;
    [SerializeField] private Settings tcpSettings = new Settings { MaxPacketSize = 128, BufferSize = 4096 };
    [SerializeField] private Settings udpSettings = new Settings { MaxPacketSize = 128, BufferSize = 4096 };

    [Header("Network Objects")]
    [Tooltip("This prefab still needs to be added to the Net Prefabs list")]
    [SerializeField] private SyncedObject netPlayer;
    [Tooltip("This prefab still needs to be added to the Net Prefabs list")]
    [SerializeField] private SyncedObject netOtherPlayer;
    [SerializeField] private List<SyncedObject> netObjets;
    [SerializeField] private List<SyncedObject> netPrefabs;

    [Header("Network Events")]
    [Space]
    public UnityEvent<NetEndPoint> OnConnectedEvent;
    public UnityEvent<NetEndPoint, NetDisconnect> OnDisconnectedEvent;
    public UnityEvent<NetEndPoint, NetPacket, PacketProtocol> OnPacketReceivedEvent;
    public UnityEvent<NetEndPoint, SocketException> OnNetworkErrorEvent;

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
            Destroy(gameObject);
        }

        Initialize();
    }

    private void Initialize()
    {
        // Initialize NetObjects
        NetObjects = new Dictionary<int, SyncedObject>();
        for (int i = 0; i < netObjets.Count; i++)
        {
            NetObjects.Add(i, netObjets[i]);
            netObjets[i].NetID = i;
        }

        NetServices = new Dictionary<int, NetService>();
        RoomMembers = new List<Guid>();
        Reset();

        Debug.Log("<color=red><b>CNet</b></color>: NetManager initialized");
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

    public void Disconnect()
    {
        NetPacket packet = new NetPacket(System, PacketProtocol.TCP);
        packet.Write("Goodbye!");
        System.Disconnect(RemoteEndPoint, packet);
        Reset();
    }

    internal void Reset()
    {
        IsHost = false;
        InRoom = false;
        RoomCode = -1;
        RoomMembers.Clear();
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
        if (Connected)
        {
            System.DisconnectForcefully(RemoteEndPoint);
            System.Close(false);
        }
    }

    public void OnConnected(NetEndPoint remoteEndPoint)
    {
        Debug.Log("<color=red><b>CNet</b></color>: NetManager connected");
        RemoteEndPoint = remoteEndPoint;
        Connected = true;
        OnConnectedEvent.Invoke(remoteEndPoint);
    }

    public void OnDisconnected(NetEndPoint remoteEndPoint, NetDisconnect disconnect)
    {
        Debug.Log("<color=red><b>CNet</b></color>: NetManager disconnected - " + disconnect.DisconnectCode.ToString() + (disconnect.DisconnectCode == DisconnectCode.ConnectionClosedWithMessage ? " (" + disconnect.DisconnectData.ReadString() + ")" : ""));
        Connected = false;
        isInitialized = false;
        OnDisconnectedEvent.Invoke(remoteEndPoint, disconnect);
    }

    public void OnPacketReceived(NetEndPoint remoteEndPoint, NetPacket packet, PacketProtocol protocol)
    {
        int serviceID = packet.ReadShort();

        if (NetServices.ContainsKey(serviceID))
        {
            NetServices[serviceID].ReceiveData(packet);
        }
        else
        {
            Debug.LogError("<color=red><b>CNet</b></color>: Service with ID " + serviceID + " not found");
        }

        OnPacketReceivedEvent.Invoke(remoteEndPoint, packet, protocol);
    }

    public void OnNetworkError(NetEndPoint remoteEndPoint, SocketException socketException)
    {
        Debug.Log("<color=red><b>CNet</b></color>: Network error - " + "(" + socketException.SocketErrorCode.ToString() + ") " + socketException.ToString());
        OnNetworkErrorEvent.Invoke(remoteEndPoint, socketException);
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

    public void RegisterService(int id, NetService service)
    {
        NetServices.Add(id, service);
    }

    public void UnregisterService(int id)
    {
        NetServices.Remove(id);
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
