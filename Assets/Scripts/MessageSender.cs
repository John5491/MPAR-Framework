using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

public class MessageSender : NetworkBehaviour
{
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject serverAnnouncementMessage;
    [SerializeField] private GameObject sendButton;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button leaveButton;
    private TMP_InputField messageInputField;
    private GameObject chatboxContent;
    private GameObject passwordNetworkManager;

    private bool hasAnnounce = false;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> roomCode = new NetworkVariable<FixedString64Bytes>();


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PlayerData? playerData = PasswordNetworkManager.GetPlayerData(OwnerClientId);

        if (playerData.HasValue)
        {
            playerName.Value = playerData.Value.PlayerName;
            roomCode.Value = playerData.Value.RoomCode;
        }
    }

    public void AnnounceLeave()
    {
        if (!IsOwner) return;
        ClearChatbox();
        SpawnAnnouncementMessageServerRpc(false);
    }

    // Start is called before the first frame update
    void Awake()
    {
        hasAnnounce = false;
        messageInputField = GameObject.Find("/----------UI---------/UI_Chatbox/ChatBox/InputInterface/InputField_Message").GetComponent<TMP_InputField>();
        chatboxContent = GameObject.Find("/----------UI---------/UI_Chatbox/ChatBox/Chatbox/Viewport/Content");
        sendButton = GameObject.Find("/----------UI---------/UI_Chatbox/ChatBox/InputInterface/SendButton");
        roomCodeText = GameObject.Find("/----------UI---------/UI_Password/Canvas_Password/Text_RoomCode").GetComponent<TMP_Text>();
        usernameText = GameObject.Find("/----------UI---------/UI_Password/Canvas_Password/Text_Username").GetComponent<TMP_Text>();
        leaveButton = GameObject.Find("/----------UI---------/UI_Password/Canvas_Password/Button_Leave").GetComponent<Button>();
        passwordNetworkManager = GameObject.Find("/----------UI---------/UI_Password");

        if (messageInputField != null) messageInputField.interactable = true;
        if (sendButton != null) sendButton.GetComponent<Button>().interactable = true;
        if (sendButton != null) sendButton.GetComponent<Button>().onClick.AddListener(SendMessage);
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(delegate
            {
                AnnounceLeave();
                StartCoroutine(Leave());
            });
        }
    }


    IEnumerator Leave()
    {
        yield return new WaitForSeconds(0.1f);
        while (!hasAnnounce)
        {
            yield return new WaitForSeconds(0.1f);
        }
        passwordNetworkManager.GetComponent<PasswordNetworkManager>().Leave();
    }
    private void Start()
    {
        if (!IsOwner) return;
        usernameText.text = "Username: " + playerName.Value.ToString();
        roomCodeText.text = "Room Code:" + roomCode.Value.ToString();
        SpawnAnnouncementMessageServerRpc(true);
    }

    public void SendMessage()
    {
        if (!IsOwner) return;
        if (string.IsNullOrWhiteSpace(messageInputField.text)) return;
        if (messageInputField.isFocused) return;

        SpawnMessageServerRpc(messageInputField.text);

        messageInputField.text = null;
    }

    private void OnEnable()
    {
        playerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void OnDisable()
    {
        playerName.OnValueChanged -= HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString64Bytes oldPlayerName, FixedString64Bytes newPlayerName)
    {
        return;
    }

    [ServerRpc]
    private void SpawnMessageServerRpc(string messageContent)
    {
        SpawnMessageClientRpc(messageContent);
    }

    [ClientRpc]
    private void SpawnMessageClientRpc(string messageContent)
    {
        if (chatboxContent == null) return;

        GameObject messageObject = Instantiate(messagePrefab, chatboxContent.transform);
        PrepareMessage(messageObject, messageContent);
    }

    private void PrepareMessage(GameObject messageObject, string messageContent)
    {
        TMP_Text username = messageObject.transform.Find("Username").GetComponent<TMP_Text>();
        TMP_Text message = messageObject.transform.Find("Message").GetComponent<TMP_Text>();

        username.text = playerName.Value.ToString();
        message.text = messageContent;
    }

    [ServerRpc]
    private void SpawnAnnouncementMessageServerRpc(bool connect)
    {
        SpawnAnnouncementMessageClientRpc(connect);
    }

    [ClientRpc]
    private void SpawnAnnouncementMessageClientRpc(bool connect)
    {
        GameObject announcmentMsg = Instantiate(serverAnnouncementMessage, chatboxContent.transform);
        TMP_Text message = announcmentMsg.transform.Find("Message").GetComponent<TMP_Text>();
        message.text = (IsOwner ? "You" : (playerName.Value.ToString())) + (connect ? " joined" : " left") + " the rooom.";
        if (connect)
        {
            message.color = Color.green;
        }
        else
        {
            message.color = Color.red;
        }

        if (IsOwner) hasAnnounce = true;
    }

    private void ClearChatbox()
    {
        foreach (Transform msg in chatboxContent.transform)
        {
            Destroy(msg.gameObject);
        }
    }
}
