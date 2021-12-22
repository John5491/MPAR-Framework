public struct PlayerData
{
    public string PlayerName { get; private set; }
    public string RoomCode { get; private set; }

    public PlayerData(string playerName, string roomCode) {
        PlayerName = playerName;
        RoomCode = roomCode;
    }
}
