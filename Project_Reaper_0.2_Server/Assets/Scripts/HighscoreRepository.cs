using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class HighscoreRepository
{

    public class HighscoreResult
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public int Highscore { get; set; }
    }

    public async void StorePlayerScores(List<Player> playerList)
    {
        try
        {
            using (var connection = DbConnection.GetConfiguredConnection())
            {
                await connection.OpenAsync();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO scores(playerId, points, team) VALUES(@playerId, @score, @team)";
                    cmd.Parameters.Add("@playerId", MySqlConnector.MySqlDbType.Int32);
                    cmd.Parameters.Add("@score", MySqlConnector.MySqlDbType.Int32);
                    cmd.Parameters.Add("@team", MySqlConnector.MySqlDbType.VarChar);
                    foreach (Player player in playerList)
                    {
                        cmd.Parameters["@playerId"].Value = player.user.PlayerId;
                        cmd.Parameters["@score"].Value = player.Score;
                        cmd.Parameters["@team"].Value = player.isReaper ? "Reaper" : "Runner";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not store highscores in database: {ex.Message}");
        }
    }

    public async Task<List<HighscoreResult>> RetrieveTopRankedPlayers(int limit)
    {
        List<HighscoreResult> result = new List<HighscoreResult>();
        try
        {
            using (var connection = DbConnection.GetConfiguredConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT p.PlayerId, p.Name, s.Team, SUM(s.Points) as highscore " +
                    "FROM scores as s " +
                    "INNER JOIN player as p WHERE p.PlayerId = s.PlayerId " +
                    "GROUP BY s.PlayerId, s.Team " +
                    "ORDER BY highscore desc " +
                    "LIMIT @limit;";
                command.Parameters.AddWithValue("@limit", limit);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        int playerId = reader.GetInt32("playerId");
                        string name = reader.GetString("name");
                        string team = reader.GetString("team");
                        int highscore = reader.GetInt32("highscore");
                        result.Add(new HighscoreResult { PlayerId = playerId, Name = name, Team = team, Highscore = highscore });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not load highscores from database: {ex.Message}");
        }
        return result;
    }

    [MessageHandler((ushort)ClientToServerId.leaderboardRequest, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private async static void LeaderboardRequest(ushort fromClientId, Message message)
    {
        var repository = new HighscoreRepository();
        List<HighscoreResult> list = await repository.RetrieveTopRankedPlayers(10);
        SendLeaderboardResult(fromClientId, list);
    }

    private static void SendLeaderboardResult(ushort clientId, List<HighscoreResult> list)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.leaderboardResponse);
        message.AddStrings(list.Select(x => x.Name).ToArray());
        message.AddStrings(list.Select(x => x.Team).ToArray());
        message.AddInts(list.Select(x => x.Highscore).ToArray());
        NetworkManager.Singleton.DatabaseAPIServer.Send(message, clientId);
    }

}