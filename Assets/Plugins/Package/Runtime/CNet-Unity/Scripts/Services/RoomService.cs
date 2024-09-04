using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;
using UnityEngine.Events;

public class RoomService : NetService
{
    public UnityEvent OnRoomCreate;
    public UnityEvent OnRoomJoin;
    public UnityEvent OnRoomLeft;
    public UnityEvent OnRoomStart;
    public UnityEvent OnRoomClose;
    public UnityEvent OnMemberJoin;
    public UnityEvent OnMemberLeft;

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

    public override void ReceiveData(NetPacket packet)
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
                        OnRoomCreate.Invoke();
                        break;
                    }
                case (int)RoomServiceType.RoomMembers:
                    {
                        if (!NetManager.Instance.InRoom)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Cannot receive room members without being in a room");
                            return;
                        }

                        if (NetManager.Instance.IsHost)
                        {
                            Debug.LogError("<color=red><b>CNet</b></color>: RoomService - Only a guest can receive room members");
                            return;
                        }

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

                        NetManager.Instance.RoomMembers.AddRange(members);
                        Debug.Log("<color=red><b>CNet</b></color>: Room members received - " + NetManager.Instance.RoomMembers.Count);
                        OnRoomJoin.Invoke();
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
                        OnMemberJoin.Invoke();
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
                                OnMemberLeft.Invoke();
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

}
