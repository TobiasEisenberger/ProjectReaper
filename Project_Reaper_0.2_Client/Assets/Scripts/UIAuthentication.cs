using Riptide;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Authenticator : MonoBehaviour
{

    public TMP_Text messageBox;
    public TMP_InputField username;
    public TMP_InputField password;
    public Button submitButton;
    public Button skipAuthButton;

    private static Authenticator Singleton;

    void Start()
    {
        NetworkManager.Singleton.DbAPIClient.Connected += DidConnect;
        NetworkManager.Singleton.DbAPIClient.ConnectionFailed += FailedToConnect;

        NetworkManager.Singleton.ConnectDbAPI();

        if (Singleton == null)
            Singleton = this;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.DbAPIClient.Connected -= DidConnect;
        NetworkManager.Singleton.DbAPIClient.ConnectionFailed -= FailedToConnect;
    }

    public void OnSubmit()
    {
        if (!IsValidInput(out string errorMessages))
        {
            UpdateInfoMessage(errorMessages);
            return;
        }

        UpdateInfoMessage("Processing authentication...");
        ToggleLoginForm(false);
        SendAuthenticationRequest();
    }

    public void OnAuthSkipSubmit()
    {
        LoadMainMenu();
    }

    private bool IsValidInput(out string validationMessage)
    {
        validationMessage = string.Empty;
        if (string.IsNullOrEmpty(username.text))
        {
            validationMessage = "Username required!\n";
        }

        if (string.IsNullOrEmpty(password.text))
        {
            validationMessage += "Password required!\n";
        }

        return validationMessage == string.Empty;
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UpdateInfoMessage("Connection succeeded.\nServices available!");
        ToggleLoginForm(true);
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UpdateInfoMessage("Connection Failed. Reattempting!");
        StartCoroutine(ReattemptConnect());
    }

    private void ToggleLoginForm(bool isActive)
    {
        username.interactable = isActive;
        password.interactable = isActive;
        submitButton.interactable = isActive;
    }

    private IEnumerator ReattemptConnect()
    {
        yield return new WaitForSecondsRealtime(3);
        UpdateInfoMessage("Trying to connect to services...");
        NetworkManager.Singleton.ConnectDbAPI();
    }

    private void UpdateInfoMessage(string message)
    {
        messageBox.text = message;
    }

    private void SendAuthenticationRequest()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.signInRequest);
        message.AddString(username.text);
        message.AddBytes(CreatePasswordHash(password.text));
        NetworkManager.Singleton.DbAPIClient.Send(message);
    }

    private byte[] CreatePasswordHash(string password)
    {
        var sha = new SHA512CryptoServiceProvider();
        return sha.ComputeHash(Encoding.Default.GetBytes(password));
    }

    private static void LoadMainMenu()
    {
        SceneManager.LoadScene("Level2Complete");
    }

    [MessageHandler((ushort)ServerToClientId.authenticationResponse, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private static void AuthenticationResponse(Message message)
    {
        bool isAuthenticated = message.GetBool();
        Debug.Log($"Received authentication response with result: {isAuthenticated}");
        if (isAuthenticated)
        {
            User.name = Singleton.username.text;
            User.isAuthenticated = true;
            LoadMainMenu();
        }
        else
        {
            Singleton.UpdateInfoMessage("User exists and/or password wrong!");
            Singleton.ToggleLoginForm(true);
        }
    }

    [MessageHandler((ushort)ServerToClientId.authenticationNotReachable, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private static void AuthenticationNotReachable(Message message)
    {
        Singleton.UpdateInfoMessage("Authentication currently not possible!");
        Singleton.ToggleLoginForm(false);
        NetworkManager.Singleton.HasServerDbAPISupport = message.GetBool();
        Singleton.skipAuthButton.gameObject.SetActive(true);
    }

}
