using UnityEngine;

/// <summary>
/// One game's settings on the Single Player / Host Game panel. Football and Golf each have their
/// own, and <see cref="GameSetupController"/> drives whichever one the tabs have made visible.
/// </summary>
public abstract class GameSetupPanel : MonoBehaviour
{
    /// <summary>
    /// Shows or hides the rows that only make sense when hosting, such as the lobby name. Called
    /// whenever the player moves between SINGLE PLAYER and HOST GAME, and once on startup.
    /// </summary>
    public abstract void SetHosting(bool isHosting);

    /// <summary>
    /// Reads the rows and starts the game: a loopback single-player session, or a Steam lobby.
    /// </summary>
    public abstract void CreateGame(bool isHosting);
}
