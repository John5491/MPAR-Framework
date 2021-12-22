using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using System.Text;

public class PasswordNetworkManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;
    [SerializeField] private GameObject touchTransformObject;
    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject arParentObject;
    [SerializeField] private GameObject tempCamera;

    private static Dictionary<ulong, PlayerData> clientData;

    private void Start()
    {
        //transformGizmoObject.SetActive(false);
        NetworkManager.Singleton.OnServerStarted += HandleServerStart;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= HandleServerStart;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void Host()
    {
        clientData = new Dictionary<ulong, PlayerData>();
        // networkManager.GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress = IPManager.GetIP(ADDRESSFAM.IPv4);
        networkManager.GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress = TestLocationService.GetLocalIPAddress();
        PrepareConnectionData();
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    public void Client()
    {
        PrepareConnectionData();
        networkManager.GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress = addressInputField.text;

        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        passwordEntryUI.SetActive(true);
        leaveButton.SetActive(false);
        arParentObject.SetActive(false);
        tempCamera.SetActive(true);
        NetworkManager.Singleton.Shutdown();
    }

    public static PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientData.TryGetValue(clientId, out PlayerData playerData)) return playerData;

        return null;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        string payload = Encoding.ASCII.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        bool approvalConnection = connectionPayload.password == passwordInputField.text;

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (approvalConnection)
        {
            clientData[clientId] = new PlayerData(connectionPayload.playerName, connectionPayload.password);
        }

        callback(true, null, approvalConnection, spawnPos, spawnRot);
    }

    private void PrepareConnectionData()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            password = passwordInputField.text,
            playerName = nameInputField.text
        });

        byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    private void HandleServerStart()
    {
        //transformGizmoObject.SetActive(true);
        if (NetworkManager.Singleton.IsHost)
        {
            //Instantiate(transformGizmoObject).GetComponent<NetworkObject>().Spawn();
            //Instantiate(ARCursor).GetComponent<NetworkObject>().Spawn();
            //HandleClientConnect(NetworkManager.Singleton.LocalClientId);
            Instantiate(touchTransformObject).GetComponent<NetworkObject>().Spawn();
        }
    }

    private void HandleClientConnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            arParentObject.SetActive(true);
            passwordEntryUI.SetActive(false);
            leaveButton.SetActive(true);
            tempCamera.SetActive(false);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            clientData.Remove(clientId);
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            arParentObject.SetActive(false);
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            tempCamera.SetActive(true);
        }
    }
}
