using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HostClientDropdownHandler : MonoBehaviour
{
    [SerializeField] private PasswordNetworkManager uiPassword;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject copied_Notification;

    private void Start()
    {
        onChangeDropdown(0);
    }

    public void onChangeDropdown(int value)
    {
        if (value == 0)
        {
            addressInputField.interactable = false;
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(() =>
            {
                uiPassword.Host();
                // GUIUtility.systemCopyBuffer = IPManager.GetIP(ADDRESSFAM.IPv4);
                GUIUtility.systemCopyBuffer = TestLocationService.GetLocalIPAddress();
                copied_Notification.SetActive(true);
            });
        }
        else if (value == 1)
        {
            addressInputField.interactable = true;
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(() =>
            {
                uiPassword.Client();
            });
        }
    }
}
