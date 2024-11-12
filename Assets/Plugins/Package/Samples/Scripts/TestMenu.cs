using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestMenu : MonoBehaviour
{
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Room Panel")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text playersInRoomText;

    private int playersInRoom = 0;

    public void Connect()
    {
        NetManager.Instance.Connect();
    }

    public void OnConnected()
    {
        SwitchToRoomPanel();
    }

    public void Disconnect()
    {
        NetManager.Instance.Disconnect();
    }

    public void OnDisconnected()
    {
        ResetMenu();
        SwitchToConnectPanel();
    }

    public void SwitchToRoomPanel()
    {
        connectPanel.SetActive(false);
        roomPanel.SetActive(true);
    }

    public void SwitchToConnectPanel()
    {
        connectPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public void ResetMenu()
    {
        playersInRoom = 0;
        playersInRoomText.gameObject.SetActive(false);
        playersInRoomText.text = "Players in room: " + playersInRoom;
        createButton.interactable = true;
        joinButton.interactable = true;
        leaveButton.interactable = false;
        closeButton.interactable = false;
        roomCodeInput.interactable = true;
        roomCodeInput.text = "";
        startButton.interactable = false;
    }

    public void CreateRoom()
    {
        ((RoomService)NetManager.Instance.NetServices[(int)ServiceType.Room]).CreateRoom();
    }

    public void OnRoomCreated(int roomCode)
    {
        roomCodeInput.text = roomCode.ToString();
        playersInRoom++;
        playersInRoomText.gameObject.SetActive(true);
        playersInRoomText.text = "Players in room: " + playersInRoom;
        createButton.interactable = false;
        joinButton.interactable = false;
        closeButton.interactable = true;
        roomCodeInput.interactable = false;
    }

    public void JoinRoom()
    {
        if (int.TryParse(roomCodeInput.text, out int roomCode))
        {
            ((RoomService)NetManager.Instance.NetServices[(int)ServiceType.Room]).JoinRoom(roomCode);
        }
        else
        {
            Debug.LogError("Invalid room code");
        }
    }

    public void OnRoomJoined(int roomCode, Guid[] playerIDs)
    {
        roomCodeInput.text = roomCode.ToString();
        playersInRoom += playerIDs.Length + 1;
        playersInRoomText.gameObject.SetActive(true);
        playersInRoomText.text = "Players in room: " + playersInRoom;
        createButton.interactable = false;
        joinButton.interactable = false;
        leaveButton.interactable = true;
        roomCodeInput.interactable = false;
    }

    public void LeaveRoom()
    {
        ((RoomService)NetManager.Instance.NetServices[(int)ServiceType.Room]).LeaveRoom();
    }

    public void OnRoomLeft()
    {
        ResetMenu();
    }

    public void CloseRoom()
    {
        ((RoomService)NetManager.Instance.NetServices[(int)ServiceType.Room]).CloseRoom();
    }

    public void OnRoomClosed()
    {
        ResetMenu();
    }

    public void OnMemberJoined(Guid playerID)
    {
        playersInRoom++;
        playersInRoomText.text = "Players in room: " + playersInRoom;

        if (playersInRoom > 1 && NetManager.Instance.IsHost)
        {
            startButton.interactable = true;
        }
    }

    public void OnMemberLeft(Guid playerID)
    {
        playersInRoom--;
        playersInRoomText.text = "Players in room: " + playersInRoom;
        if (playersInRoom == 1 && NetManager.Instance.IsHost)
        {
            startButton.interactable = false;
        }
    }

    public void StartRoom()
    {
        ((RoomService)NetManager.Instance.NetServices[(int)ServiceType.Room]).StartRoom();
    }

    public void OnRoomStarted()
    {
        Debug.Log("Room started");
        SceneManager.LoadScene("CNet-Game");
    }
}
