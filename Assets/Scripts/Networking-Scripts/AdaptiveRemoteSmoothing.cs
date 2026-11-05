using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Managing.Timing;

// Smooths play on unstable connections. A remote object is drawn a few NetworkTransform updates in the past, so
// motion between updates can interpolate; that buffer is fixed on the prefabs, enough on a steady connection. On a
// spiky one updates arrive in bursts, the buffer runs dry in the gaps, and remote objects freeze and then snap. This
// deepens the buffer on the objects this machine does not own while its view of them is unsteady, then eases it back
// one step at a time once things have been calm for a while (an instant drop would discard queued updates and snap).
//
// How "unsteady" is measured depends on which side this is, because the two sides can see it:
//   - A client watches its own round trip, which covers the server's sends to it.
//   - A host has no round trip to itself, so it times the arrival of each remote object's data instead: how far
//     behind the local tick each update lands. The spread of that is the jitter on that client's upload.
// Either way the number is a latency in milliseconds, and everything after it is shared.
//
// Display only: it changes how this machine draws objects it does not own - never the server, the tick, what is
// sent, or any outcome, which all stay server-decided. On a steady connection the jitter never crosses the raise
// threshold, so the buffer stays at the prefab value and the game renders exactly as before.
public class AdaptiveRemoteSmoothing : MonoBehaviour
{
    // The prefab value for the goblins and the football, restored when the connection has been calm.
    public const ushort BaseInterpolation = 2;
    const ushort RaisedInterpolation = 4;
    const ushort HighInterpolation = 6;
    // Jitter is the spread (max - min) of the latencies sampled over the window. The cellular tests put choppy
    // stretches at 100-150 ms of spread; a good home connection stays under about 20.
    const long RaiseJitterMs = 80;
    const long RaiseHighJitterMs = 160;
    // Sampled twice a second over 16 samples.
    const float SampleIntervalSeconds = 0.5f;
    const int WindowSize = 16;
    // The buffer steps back down only after the jitter has stayed below the current level this long. Stepping down
    // discards queued updates, which draws as a snap, so this errs long rather than bounce inside one bad stretch.
    const float LowerAfterSeconds = 15f;

    // Shown next to the ping by DevDirectConnect, so a test can see the buffer react.
    public static ushort CurrentInterpolation { get; private set; } = BaseInterpolation;

    // One number per sample tick, in milliseconds: a client's round trip, or a host's spread of arrival times over
    // that tick. A host's arrivals come in far too fast to window individually - a hundred a second would fill the
    // window in a sixth of a second and never see a bad stretch - so they are folded into the tick they land in.
    readonly List<long> _samples = new List<long>();
    // The arrival times a host has seen since the last sample tick. Comparing them only within one tick keeps the
    // measure honest: each client stamps its own tick count, so the raw numbers carry an offset that FishNet nudges
    // over time, but across half a second that offset is fixed and cancels out of the spread.
    long _arrivalMin;
    long _arrivalMax;
    bool _hasArrivals;
    // A host's samples are already spreads, so the jitter is the worst of them; a client's are plain round trips,
    // whose spread is the jitter.
    bool _samplesAreSpreads;
    // Football's objects that this machine does not own.
    readonly List<NetworkTransform> _remotes = new List<NetworkTransform>();
    // Remote transforms a host has subscribed to for arrival times.
    readonly HashSet<NetworkTransform> _timedArrivals = new HashSet<NetworkTransform>();

    float _nextSampleTime;
    float _calmSince;
    bool _wasInSession;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        GameObject host = new GameObject("AdaptiveRemoteSmoothing");
        host.AddComponent<AdaptiveRemoteSmoothing>();
        DontDestroyOnLoad(host);
    }

    void Update()
    {
        // Unscaled: the game pauses with timeScale, and the connection doesn't.
        if (Time.unscaledTime < _nextSampleTime)
            return;
        _nextSampleTime = Time.unscaledTime + SampleIntervalSeconds;

        bool isServer = InstanceFinder.ServerManager != null && InstanceFinder.ServerManager.Started;
        bool isClient = InstanceFinder.ClientManager != null && InstanceFinder.ClientManager.Started;
        if (!isServer && !isClient)
        {
            EndSession();
            return;
        }
        _wasInSession = true;

        CollectRemotes();
        _samplesAreSpreads = isServer;
        if (isServer)
        {
            TimeArrivals();
            if (_hasArrivals)
            {
                AddSample(_arrivalMax - _arrivalMin);
                _hasArrivals = false;
            }
        }
        else
        {
            AddSample(InstanceFinder.TimeManager.RoundTripTime);
        }

        if (_samples.Count < 4)
            return;

        long min = long.MaxValue;
        long max = 0;
        foreach (long sample in _samples)
        {
            if (sample < min) min = sample;
            if (sample > max) max = sample;
        }
        long jitter = _samplesAreSpreads ? max : max - min;

        ushort desired = jitter > RaiseHighJitterMs ? HighInterpolation
            : jitter > RaiseJitterMs ? RaisedInterpolation
            : BaseInterpolation;

        if (desired >= CurrentInterpolation)
            _calmSince = Time.unscaledTime;
        if (desired > CurrentInterpolation)
        {
            // Raising is safe at any moment: the queue just grows into the new depth.
            CurrentInterpolation = desired;
            int applied = Apply();
            Debug.Log("AdaptiveRemoteSmoothing: jitter " + jitter + " ms, buffer up to " + CurrentInterpolation + " on " + applied + " transforms.");
        }
        else if (desired < CurrentInterpolation && Time.unscaledTime - _calmSince >= LowerAfterSeconds)
        {
            CurrentInterpolation = CurrentInterpolation == HighInterpolation ? RaisedInterpolation : BaseInterpolation;
            _calmSince = Time.unscaledTime;
            // A step down to the base value is the last one the block below would skip, so always apply here.
            int applied = Apply();
            Debug.Log("AdaptiveRemoteSmoothing: jitter " + jitter + " ms, buffer down to " + CurrentInterpolation + " on " + applied + " transforms.");
        }

        // Applied every sample, not only on change: an object spawned mid-match starts at the prefab value and
        // needs the current one.
        if (CurrentInterpolation != BaseInterpolation)
            Apply();
    }

    // Forget everything when the session ends, so the next one starts fresh. The objects despawn with the session,
    // so there is nothing to set back.
    void EndSession()
    {
        if (!_wasInSession)
            return;
        _wasInSession = false;
        foreach (NetworkTransform networkTransform in _timedArrivals)
        {
            if (networkTransform != null)
                networkTransform.OnDataReceived -= ArrivalTimed;
        }
        _timedArrivals.Clear();
        _samples.Clear();
        _hasArrivals = false;
        _remotes.Clear();
        CurrentInterpolation = BaseInterpolation;
    }

    // Football's goblins and ball only. Golf's objects are left at their prefab buffers: its ball is placed by
    // whoever owns it and repositioned between holes, and its tornado crawls, so neither gains from a deeper
    // buffer the way a goblin chasing a carrier does.
    void CollectRemotes()
    {
        _remotes.Clear();
        foreach (GoblinScript goblin in FindObjectsOfType<GoblinScript>())
            AddRemote(goblin);
        AddRemote(Football.Instance);
    }

    void AddRemote(FishNet.Object.NetworkBehaviour owned)
    {
        if (owned == null || owned.IsOwner)
            return;
        NetworkTransform networkTransform = owned.GetComponent<NetworkTransform>();
        if (networkTransform != null)
            _remotes.Add(networkTransform);
    }

    int Apply()
    {
        foreach (NetworkTransform networkTransform in _remotes)
            networkTransform.SetInterpolation(CurrentInterpolation);
        return _remotes.Count;
    }

    // A host subscribes to each remote object's arrivals once. The subscription is what measures its jitter, so it
    // is only taken for objects whose data this machine receives.
    void TimeArrivals()
    {
        foreach (NetworkTransform networkTransform in _remotes)
        {
            if (_timedArrivals.Add(networkTransform))
                networkTransform.OnDataReceived += ArrivalTimed;
        }
    }

    // How far behind the local tick this update landed. Its spread over the window is the sending client's jitter:
    // steady sends land a consistent number of ticks late, and a stalled upload lands a batch of them far later.
    void ArrivalTimed(NetworkTransform.TransformData previous, NetworkTransform.TransformData next)
    {
        TimeManager timeManager = InstanceFinder.TimeManager;
        if (timeManager == null || next == null || next.Tick == TimeManager.UNSET_TICK)
            return;
        long lateTicks = (long)timeManager.LocalTick - next.Tick;
        if (lateTicks < 0)
            return;
        long lateMs = (long)(lateTicks * timeManager.TickDelta * 1000d);
        if (!_hasArrivals)
        {
            _hasArrivals = true;
            _arrivalMin = lateMs;
            _arrivalMax = lateMs;
            return;
        }
        if (lateMs < _arrivalMin) _arrivalMin = lateMs;
        if (lateMs > _arrivalMax) _arrivalMax = lateMs;
    }

    void AddSample(long latencyMs)
    {
        _samples.Add(latencyMs);
        while (_samples.Count > WindowSize)
            _samples.RemoveAt(0);
    }
}
