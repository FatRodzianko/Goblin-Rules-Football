using FishNet;
using FishNet.Object;
using UnityEngine;

// Finds spawned objects by ObjectId, in place of Mirror's NetworkIdentity.spawned[netId]. Uses the server's list while
// the server is running, otherwise the client's. Get returns null instead of throwing when the object isn't spawned:
// on FishNet 4 it may simply not have arrived yet.
public static class ObjectLookup
{
    // Mirror never handed out netId 0, so Football used 0 for "no object". FishNet does use id 0, so "no object" is -1.
    public const int NoObject = -1;

    public static bool TryGet(int objectId, out NetworkObject networkObject)
    {
        networkObject = null;
        if (objectId == NoObject)
            return false;
        if (InstanceFinder.IsServerStarted)
            return InstanceFinder.ServerManager.Objects.Spawned.TryGetValue(objectId, out networkObject);
        if (InstanceFinder.IsClientStarted)
            return InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(objectId, out networkObject);
        return false;
    }
    public static bool TryGet<T>(int objectId, out T component) where T : Component
    {
        component = null;
        if (!TryGet(objectId, out NetworkObject networkObject))
            return false;
        return networkObject.TryGetComponent(out component);
    }
    public static NetworkObject Get(int objectId)
    {
        TryGet(objectId, out NetworkObject networkObject);
        return networkObject;
    }
    public static bool Exists(int objectId)
    {
        return TryGet(objectId, out NetworkObject _);
    }
}
