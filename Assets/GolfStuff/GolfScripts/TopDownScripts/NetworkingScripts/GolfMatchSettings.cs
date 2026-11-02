using System.Collections.Generic;

// The settings a Golf lobby was created with on the title screen (GolfLobbySettings). The host sends a copy to every
// player as they join, so whoever takes the lobby over if the host leaves can carry on with the same course and rules.
// FishNet writes the public fields itself when this is sent in an RPC.
public struct GolfMatchSettings
{
    public int numberOfPlayers;
    public bool friendsOnly;
    public bool powerUpsEnabled;
    public bool weatherStatuesEnabled;
    public bool strokeLimitEnabled;
    public int strokeLimitNumber;
    public RainManager.RainMode rainMode;
    public WindManager.WindMode windMode;
    public bool parFavorPenalty;
    public string courseId;
    public string courseName;
    public ulong courseWorkshopId;
    public string courseHoleSelection;
    public List<int> customHolesToPlay;
}
