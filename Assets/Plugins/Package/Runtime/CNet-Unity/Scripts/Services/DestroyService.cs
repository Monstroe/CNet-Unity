using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class DestroyService : MonoBehaviour, NetService
{
    void Awake()
    {
        RegisterService((int)ServiceType.Destroy);
    }

    void OnDisable()
    {
        UnregisterService((int)ServiceType.Destroy);
    }

    public void RegisterService(int serviceID)
    {
        NetManager.Instance.RegisterService(serviceID, this);
    }

    public void UnregisterService(int serviceID)
    {
        NetManager.Instance.UnregisterService(serviceID);
    }

    public void ReceiveData(NetPacket packet)
    {
        try
        {
            int netID = (int)packet.ReadShort();

            if (!NetManager.Instance.NetObjects.TryGetValue(netID, out SyncedObject syncedObj))
            {
                Debug.LogError("<color=red><b>CNet</b></color>: DestroyService could not find NetObject with ID " + netID);
            }

            Destroy(syncedObj.gameObject);

            if (NetManager.Instance.IsHost)
            {
                NetManager.Instance.Send(packet, PacketProtocol.TCP);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: DestroyService error - " + e.Message);
        }
    }
}