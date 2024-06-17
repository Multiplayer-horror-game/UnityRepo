using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            CreateRelay();
        });
        joinButton.onClick.AddListener(() =>
        {
            JoinRelay(joinCodeInputField.text);
        });
    }

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Vivox setup
        await VivoxService.Instance.InitializeAsync();
        await VivoxService.Instance.LoginAsync();
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            joinCodeText.text = joinCode;

            VivoxService.Instance.ChannelJoined += OnChannelJoined;
            VivoxService.Instance.ChannelLeft += OnChannelLeft;

            await VivoxService.Instance.JoinEchoChannelAsync(joinCode, ChatCapability.AudioOnly);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            await VivoxService.Instance.JoinEchoChannelAsync(joinCode, ChatCapability.AudioOnly);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    void OnChannelJoined(string channelName)
    {
        Debug.Log("Player " + AuthenticationService.Instance.PlayerId + " joined the channel " + channelName);
    }

    void OnChannelLeft(string channelName)
    {
        Debug.Log("Player " + AuthenticationService.Instance.PlayerId + " left the channel " + channelName);
    }

    private void OnApplicationQuit()
    {
        // Log out from Vivox service
        VivoxService.Instance.LogoutAsync();
    }
}
