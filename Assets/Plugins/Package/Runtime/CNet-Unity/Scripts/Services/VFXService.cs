using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class VFXService : MonoBehaviour, NetService
{
    void Awake()
    {
        RegisterService((int)ServiceType.VFX);
    }

    void OnDisable()
    {
        UnregisterService((int)ServiceType.VFX);
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