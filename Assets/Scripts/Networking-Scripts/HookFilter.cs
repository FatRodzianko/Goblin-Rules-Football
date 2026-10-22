using FishNet;

// FishNet calls a SyncVar's OnChange twice on the host: once as the server, then again as the host's client, with the old
// and new values equal. Mirror called the hook once there. Football's hooks were written for Mirror, so they skip the
// second call. Remote clients still get their one call.
public static class HookFilter
{
    public static bool ShouldRun(bool asServer)
    {
        return asServer || !InstanceFinder.IsServerStarted;
    }
}
