using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public abstract class NetService : MonoBehaviour
{
    public abstract void ReceiveData(NetPacket packet);
}
