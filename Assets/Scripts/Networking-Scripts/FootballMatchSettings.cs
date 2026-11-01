// The settings a Football lobby was created with on the title screen. The host sends a copy to every player as they join
// the lobby, so whoever takes the lobby over if the host leaves can carry on with the same match. FishNet writes the
// public fields itself when this is sent in an RPC.
public struct FootballMatchSettings
{
    public bool is1v1;
    public string lobbyName;
    public int secondsPerHalf;
    public bool powerUpsEnabled;
    public bool randomEventsEnabled;
    public bool spawnObstaclesEnabled;
    public bool mercyRuleEnabled;
    public int mercyRulePointDifferential;
}
