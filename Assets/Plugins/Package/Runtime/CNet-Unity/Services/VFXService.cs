using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class VFXService : NetService
{
    public override void ReceiveData(NetPacket packet)
    {
        try
        {
            string name = packet.ReadString();
            Vector3 position = (Vector3)packet.DeserializeStruct<NetVector3>();
            float scale = packet.ReadFloat();

            NetFXManager.Instance.PlayVFX(name, position, scale, NetManager.Instance.IsHost);
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: VFXService error - " + e.Message);
        }
    }
}