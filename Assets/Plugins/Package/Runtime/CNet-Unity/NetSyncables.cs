using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

[NetSyncable]
public struct NetVector2
{
    public float x;
    public float y;

    public static explicit operator NetVector2(Vector2? v)
    {
        if (v == null)
        {
            return new NetVector2 { x = 0, y = 0 };
        }
        else
        {
            Vector2 vec = (Vector2)v;
            return new NetVector2 { x = vec.x, y = vec.y };
        }
    }

    public static explicit operator Vector2(NetVector2 v)
    {
        return new Vector2(v.x, v.y);
    }
}

[NetSyncable]
public struct NetVector3
{
    public float x;
    public float y;
    public float z;

    public static explicit operator NetVector3(Vector3? v)
    {
        if (v == null)
        {
            return new NetVector3 { x = 0, y = 0, z = 0 };
        }
        else
        {
            Vector3 vec = (Vector3)v;
            return new NetVector3 { x = vec.x, y = vec.y, z = vec.z };
        }
    }

    public static explicit operator Vector3(NetVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}

[NetSyncable]
public struct NetQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public static explicit operator NetQuaternion(Quaternion? q)
    {
        if (q == null)
        {
            return new NetQuaternion { x = 0, y = 0, z = 0, w = 0 };
        }
        else
        {
            Quaternion quat = (Quaternion)q;
            return new NetQuaternion { x = quat.x, y = quat.y, z = quat.z, w = quat.w };
        }
    }

    public static explicit operator Quaternion(NetQuaternion q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
}