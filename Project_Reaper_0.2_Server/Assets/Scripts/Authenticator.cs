using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Authenticator
{

    private static Dictionary<ushort, User> userList = new Dictionary<ushort, User>();

    public static void EnqueueUser(ushort clientId)
    {
        userList.Add(clientId, new User());
    }

    public static void LogOutUser(ushort clientId)
    {
        userList.Remove(clientId);
    }

    public static User GetAuthenticatedUser(string name)
    {
        return userList.Values.Where(x => x.Name == name && x.IsAuthenticated).FirstOrDefault();
    }

    [MessageHandler((ushort)ClientToServerId.signInRequest, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private static async void SignInRequest(ushort fromClientId, Message message)
    {
        if (!userList.TryGetValue(fromClientId, out User user))
            return;

        string username = message.GetString();
        string password = Convert.ToBase64String(message.GetBytes());
        Debug.Log($"Received authentication request for user: {username} with hash: {password}");

        if (IsUserAlreadyAuthenticated(username))
        {
            NotifyWrongCredentials(fromClientId);
            return;
        }

        try
        {
            using (var connection = DbConnection.GetConfiguredConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"SELECT playerId, name, password FROM player WHERE name = @playername;";
                command.Parameters.AddWithValue("@playername", username);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // User already exists in db
                    if (reader.HasRows)
                    {
                        // Assuming there will always be only one entry with this name (unique user names!)
                        while (reader.Read())
                        {
                            int playerId = reader.GetInt32("playerId");
                            string userNameDb = reader.GetString("name");
                            string playerPasswordDb = reader.GetString("password");
                            Debug.Log($"Found user: {userNameDb} with password: {playerPasswordDb}");

                            if (playerPasswordDb == password)
                            {
                                AcceptUserAuthentication(fromClientId, playerId, userNameDb);
                                LogInByExistingUser(fromClientId);
                            }
                            else
                            {
                                NotifyWrongCredentials(fromClientId);
                            }
                            return;
                        }
                    }
                }
                // Create new user if there is not an already existing one
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO player (Name, Password) VALUES (@name, @password)";
                    cmd.Parameters.AddWithValue("@name", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    await cmd.ExecuteNonQueryAsync();
                    int playerId = (int)cmd.LastInsertedId;
                    AcceptUserAuthentication(fromClientId, playerId, username);
                }
                SendAuthenticationResponse(fromClientId, true);
            }
        }
        catch (Exception ex)
        {
            SendAuthenticationNotAvailable(fromClientId);
            Debug.LogWarning($"An error occurred: {ex.Message}");
        }
    }

    private static bool IsUserAlreadyAuthenticated(string name)
    {
        int sameUserCount = userList.Values.Where(x => x.Name == name && x.IsAuthenticated).Count();
        return sameUserCount > 0;
    }

    private static void AcceptUserAuthentication(ushort clientId, int playerId, string name)
    {
        if (userList.TryGetValue(clientId, out User user))
        {
            user.PlayerId = playerId;
            user.IsAuthenticated = true;
            user.Name = name;
            Debug.Log($"Authenticated user with playerId: {playerId}");
        }
    }

    private static void LogInByExistingUser(ushort clientId)
    {
        Debug.Log("user and password in db matched!");
        SendAuthenticationResponse(clientId, true);
    }

    private static void NotifyWrongCredentials(ushort clientId)
    {
        Debug.Log("user and/or password wrong!");
        SendAuthenticationResponse(clientId, false);
    }

    private static void SendAuthenticationResponse(ushort clientId, bool isAuthenticated)
    {
        Debug.Log($"Sending authentication response to client: {clientId}");
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.authenticationResponse);
        message.AddBool(isAuthenticated);
        NetworkManager.Singleton.DatabaseAPIServer.Send(message, clientId);
    }

    public static void SendAuthenticationNotAvailable(ushort clientId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.authenticationNotReachable);
        message.AddBool(NetworkManager.Singleton.HasServerDbAPISupport);
        NetworkManager.Singleton.DatabaseAPIServer.Send(message, clientId);
    }

}
