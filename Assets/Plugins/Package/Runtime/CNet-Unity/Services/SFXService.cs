using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class SFXService : NetService
{
    public override void ReceiveData(NetPacket packet)
    {
        try
        {
            string name = packet.ReadString();
            float volume = packet.ReadFloat();
            Vector3? pos = null;

            if (packet.UnreadLength > 0)
            {
                pos = (Vector3)packet.DeserializeStruct<NetVector3>();
            }

            NetFXManager.Instance.PlaySFX(name, volume, pos, NetManager.Instance.IsHost);
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SFXService error - " + e.Message);
        }
    }
}