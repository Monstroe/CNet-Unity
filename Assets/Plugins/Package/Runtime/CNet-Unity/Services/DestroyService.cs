using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class DestroyService : NetService
{
    public override void ReceiveData(NetPacket packet)
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