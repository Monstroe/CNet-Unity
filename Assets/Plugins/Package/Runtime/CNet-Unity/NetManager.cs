using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using CNet;
using UnityEngine;
using UnityEngine.Events;

public enum UserType
{
    Guest,
    Host
}

public class NetManager : MonoBehaviour, IEventNetClient
{
    [Serializable]
    public struct Settings
    {
        public int MaxPacketSize { get => maxPacketSize; set => maxPacketSize = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int maxPacketSize;
        public int bufferSize;
    }

    [Serializable]
    public class PacketReceivedEvent : UnityEvent<NetEndPoint, NetPacket, PacketProtocol> { }

    public static NetManager Instance { get; private set; }

    //public delegate void PacketHandler(NetPacket packet);

    public NetSystem System { get; private set; }
    public UserType UserType { get; private set; }
    public Guid ID { get; private set; }
    public NetEndPoint RemoteEndPoint { get; private set; }
    public string Address { get => address; set => address = value; }
    public int Port { get => port; set => port = value; }
    public Settings TCPSettings { get => tCPSettings; }
    public Settings UDPSettings { get => uDPSettings; }
    public bool Connected { get; private set; }
    public List<SyncedObject> NetObjects { get => netObjets; }


    [Header("Network Settings")]
    [SerializeField] private string address = "localhost";
    [SerializeField] private int port = 7777;
    [SerializeField] private Settings tCPSettings;
    [SerializeField] private Settings uDPSettings;
    [Header("Network Objects")]
    [SerializeField] private List<SyncedObject> netObjets;
    [Space]
    public PacketReceivedEvent PacketReceived;

    private Dictionary<int, NetService> services;
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
            Debug.LogError("<color=red><b>CNet</b></color>: There are multiple NetManager instances in the scene, destroying one.");
            Destroy(this);
        }
    }

    public void Connect()
    {
        Debug.Log("<color=red><b>CNet</b></color>: Starting NetManager...");
        System = new NetSystem(address, port);
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
    }

    public void OnPacketReceived(NetEndPoint remoteEndPoint, NetPacket packet, PacketProtocol protocol)
    {
        int serviceID = packet.ReadShort();
        if (services.TryGetValue(serviceID, out NetService service))
        {
            service.ReceiveData(packet);
        }
        else
        {
            PacketReceived.Invoke(remoteEndPoint, packet, protocol);
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

    public void RegisterService(int serviceID, NetService service)
    {
        services.Add(serviceID, service);
    }
}
