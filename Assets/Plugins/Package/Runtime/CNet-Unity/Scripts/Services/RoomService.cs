using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CNet;

public class RoomService : MonoBehaviour, NetService
{
    public UnityEvent<int> OnRoomCreate;
    public UnityEvent<int, Guid[]> OnRoomJoin;
    public UnityEvent OnRoomLeft;
    public UnityEvent OnRoomStart;
    public UnityEvent OnRoomClose;
    public UnityEvent<Guid> OnMemberJoin;
    public UnityEvent<Guid> OnMemberLeft;

    public enum RoomServiceType
    {
        ID = 0,
        RoomCode = 1,
        RoomMembers = 2,
        MemberJoined = 3,
        MemberLeft = 4,
        RoomStart = 5,
        RoomClosed = 6,
        Invalid = 7
    }

    enum ServiceSendType
    {
        CreateRoom = 0,
        JoinRoom = 1,
        LeaveRoom = 2,
        StartRoom = 3,
        CloseRoom = 4
    }

    void Awake()
    {
        RegisterService((int)ServiceType.Room);
    }

    void OnDisable()
    {
        UnregisterService((int)ServiceType.Room);
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
            int netID = packet.ReadShort();

            switch (netID)
            {
                case (int)RoomServiceType.ID:
                    {
                        if (Guid.TryParse(packet.ReadString(), out Guid id))
                        {
                            NetManager.Instance.ID = id;
                        }
                        else
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService could not parse ID");
                            return;
                        }

                        Debug.Log("<color=red><b>CNet</b></color>: ID received - " + NetManager.Instance.ID);
                        break;
                    }
                case (int)RoomServiceType.RoomCode:
                    {
                        if (NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Host is already in a room, cannot create another room without leaving the current one");
                            return;
                        }

                        NetManager.Instance.RoomCode = packet.ReadInt();
                        NetManager.Instance.InRoom = true;
                        NetManager.Instance.IsHost = true;
                        Debug.Log("<color=red><b>CNet</b></color>: Room code received - " + NetManager.Instance.RoomCode);
                        OnRoomCreate.Invoke(NetManager.Instance.RoomCode);
                        break;
                    }
                case (int)RoomServiceType.RoomMembers:
                    {
                        if (NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive room members while already being in another room");
                            return;
                        }

                        if (NetManager.Instance.IsHost)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Only a guest can receive room members");
                            return;
                        }

                        int roomCode = packet.ReadInt();
                        int count = packet.ReadInt();
                        List<Guid> members = new List<Guid>();
                        for (int i = 0; i < count; i++)
                        {
                            if (Guid.TryParse(packet.ReadString(), out Guid memberID))
                            {
                                if (memberID == NetManager.Instance.ID)
                                {
                                    continue;
                                }

                                members.Add(memberID);
                            }
                            else
                            {
                                Debug.LogError("<color=red><b>CNet</b></color>: RoomService could not parse room member ID");
                                return;
                            }
                        }

                        NetManager.Instance.RoomCode = roomCode;
                        NetManager.Instance.RoomMembers.AddRange(members);
                        NetManager.Instance.InRoom = true;
                        NetManager.Instance.IsHost = false;
                        Debug.Log("<color=red><b>CNet</b></color>: Room members received - " + NetManager.Instance.RoomMembers.Count);
                        OnRoomJoin.Invoke(NetManager.Instance.RoomCode, NetManager.Instance.RoomMembers.ToArray());
                        break;
                    }
                case (int)RoomServiceType.MemberJoined:
                    {
                        if (!NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive member joined without being in a room");
                            return;
                        }

                        if (Guid.TryParse(packet.ReadString(), out Guid memberID))
                        {
                            NetManager.Instance.RoomMembers.Add(memberID);
                        }
                        else
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService could not parse new member ID");
                            return;
                        }

                        Debug.Log("<color=red><b>CNet</b></color>: Member joined - " + memberID);
                        OnMemberJoin.Invoke(memberID);
                        break;
                    }
                case (int)RoomServiceType.MemberLeft:
                    {
                        if (!NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive member left without being in a room");
                            return;
                        }

                        if (Guid.TryParse(packet.ReadString(), out Guid memberID))
                        {
                            NetManager.Instance.RoomMembers.Remove(memberID);
                            if (memberID == NetManager.Instance.ID)
                            {
                                NetManager.Instance.Reset();
                                OnRoomLeft.Invoke();
                            }
                            else
                            {
                                OnMemberLeft.Invoke(memberID);
                            }
                        }
                        else
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService could not parse member left ID");
                            return;
                        }

                        Debug.Log("<color=red><b>CNet</b></color>: Member left - " + memberID);
                        break;
                    }
                case (int)RoomServiceType.RoomStart:
                    {
                        if (!NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive room start without being in a room");
                            return;
                        }

                        Debug.Log("<color=red><b>CNet</b></color>: Room started");
                        OnRoomStart.Invoke();
                        break;
                    }
                case (int)RoomServiceType.RoomClosed:
                    {
                        if (!NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive room closed without being in a room");
                            return;
                        }

                        NetManager.Instance.Reset();
                        Debug.Log("<color=red><b>CNet</b></color>: Room closed");
                        OnRoomClose.Invoke();
                        break;
                    }
                case (int)RoomServiceType.Invalid:
                    {
                        NetManager.Instance.Reset();
                        Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Invalid service type: " + packet.ReadString());
                        break;
                    }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService error - " + e.Message);
        }
    }

    public void CreateRoom()
    {
        if (NetManager.Instance.InRoom)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Host is already in a room, cannot create another room without leaving the current one");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Room);
            packet.Write((short)ServiceSendType.CreateRoom);
            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }

    public void JoinRoom(int roomID)
    {
        if (NetManager.Instance.InRoom)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot join a room while already in one");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Room);
            packet.Write((short)ServiceSendType.JoinRoom);
            packet.Write(roomID);
            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }

    public void LeaveRoom()
    {
        if (!NetManager.Instance.InRoom)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot leave a room while not in one");
            return;
        }

        if (NetManager.Instance.IsHost)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Host cannot leave the room");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Room);
            packet.Write((short)ServiceSendType.LeaveRoom);
            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }

    public void CloseRoom()
    {
        if (!NetManager.Instance.InRoom)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot close a room while not in one");
            return;
        }

        if (!NetManager.Instance.IsHost)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Only the host can close the room");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Room);
            packet.Write((short)ServiceSendType.CloseRoom);
            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }

    public void StartRoom()
    {
        if (!NetManager.Instance.InRoom)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot start a room while not in one");
            return;
        }

        if (!NetManager.Instance.IsHost)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Only the host can start the room");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Room);
            packet.Write((short)ServiceSendType.StartRoom);
            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }
}
