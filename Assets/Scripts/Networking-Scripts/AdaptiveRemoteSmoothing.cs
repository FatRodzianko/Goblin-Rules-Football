using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Transforming;

// Smooths Football play on unstable connections (Nov 4, 2026). A remote goblin is drawn a couple of NetworkTransform
// updates in the past, so motion between updates can interpolate; that buffer is fixed at 2 on the prefabs, enough on
// a steady connection. On a spiky one (the Nov 3 cellular tests: smooth at 50-100 ms ping, choppy at 150-250 ms)
// updates arrive in bursts, the buffer runs dry in the gaps, and remote goblins freeze and then snap. This watches
// the local client's ping jitter and deepens the buffer on the goblins this machine doesn't own, and on the football,
// while the connection is unstable, then eases it back one step at a time once it has been calm for a while (an
// instant drop would discard queued updates and visibly snap).
//
// Display only: it changes how this machine draws objects it doesn't own - never the server, the tick, what is sent,
// or any outcome, which all stay server-decided. The host never runs it (it has no round trip to itself), and on a
// stable connection the jitter never crosses the raise threshold, so the buffer stays at the prefab value and the
// game renders exactly as before.
public class AdaptiveRemoteSmoothing : MonoBehaviour
{
    // The prefab value for the goblins and the football, restored when the connection has been calm.
    public const ushort BaseInterpolation = 2;
    const ushort RaisedInterpolation = 4;
    const ushort HighInterpolation = 6;
    // Jitter is the spread (max - min) of the round trips sampled over the window. The cellular tests put choppy
    // stretches at 100-150 ms of spread; a good home connection stays under about 20.
    const long RaiseJitterMs = 80;
    const long RaiseHighJitterMs = 160;
    // Sampled twice a second over 16 samples: the window remembers the last 8 seconds.
    const float SampleIntervalSeconds = 0.5f;
    const int WindowSize = 16;
    // The buffer steps back down only after the jitter has stayed below the current level this long. Stepping down
    // discards queued updates, which draws as a snap, so this errs long: during the Nov 4 jitter sim, 5 seconds had
    // the buffer bouncing 6->4->6 inside one unstable stretch and the client visibly jerked at each step down.
    const float LowerAfterSeconds = 15f;

    // Shown next to the ping by DevDirectConnect, so a test can see the buffer react.
    public static ushort CurrentInterpolation { get; private set; } = BaseInterpolation;

    readonly List<long> _samples = new List<long>();
    float _nextSampleTime;
    float _calmSince;
    bool _wasActiveClient;

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

        bool isActiveClient = InstanceFinder.ClientManager != null && InstanceFinder.ClientManager.Started
            && (InstanceFinder.ServerManager == null || !InstanceFinder.ServerManager.Started);
        if (!isActiveClient)
        {
            // Session over (or this machine is the host): forget the window so the next session starts fresh. The
            // objects themselves despawn with the session, so there is nothing to set back.
            if (_wasActiveClient)
            {
                _samples.Clear();
                CurrentInterpolation = BaseInterpolation;
                _wasActiveClient = false;
            }
            return;
        }
        _wasActiveClient = true;

        _samples.Add(InstanceFinder.TimeManager.RoundTripTime);
        if (_samples.Count > WindowSize)
            _samples.RemoveAt(0);
        if (_samples.Count < 4)
            return;

        long min = long.MaxValue;
        long max = 0;
        foreach (long sample in _samples)
        {
            if (sample < min) min = sample;
            if (sample > max) max = sample;
        }
        long jitter = max - min;

        ushort desired = jitter > RaiseHighJitterMs ? HighInterpolation
            : jitter > RaiseJitterMs ? RaisedInterpolation
            : BaseInterpolation;

        if (desired >= CurrentInterpolation)
            _calmSince = Time.unscaledTime;
        if (desired > CurrentInterpolation)
        {
            // Raising is safe at any moment: the queue just grows into the new depth.
            CurrentInterpolation = desired;
            Debug.Log("AdaptiveRemoteSmoothing: ping jitter " + jitter + " ms, buffer up to " + CurrentInterpolation + " on " + Apply() + " transforms.");
        }
        else if (desired < CurrentInterpolation && Time.unscaledTime - _calmSince >= LowerAfterSeconds)
        {
            CurrentInterpolation = CurrentInterpolation == HighInterpolation ? RaisedInterpolation : BaseInterpolation;
            _calmSince = Time.unscaledTime;
            // A step down to the base value is the last one the block below would skip, so always apply here.
            Debug.Log("AdaptiveRemoteSmoothing: ping jitter " + jitter + " ms, buffer down to " + CurrentInterpolation + " on " + Apply() + " transforms.");
        }

        // Applied every sample, not only on change: a goblin spawned mid-match (a takeover bot's) starts at the
        // prefab value and needs the current one.
        if (CurrentInterpolation != BaseInterpolation)
            Apply();
    }

    int Apply()
    {
        int applied = 0;
        foreach (GoblinScript goblin in FindObjectsOfType<GoblinScript>())
        {
            if (goblin == null || goblin.IsOwner)
                continue;
            NetworkTransform goblinTransform = goblin.GetComponent<NetworkTransform>();
            if (goblinTransform != null)
            {
                goblinTransform.SetInterpolation(CurrentInterpolation);
                applied++;
            }
        }
        if (Football.Instance != null)
        {
            NetworkTransform ballTransform = Football.Instance.GetComponent<NetworkTransform>();
            if (ballTransform != null)
            {
                ballTransform.SetInterpolation(CurrentInterpolation);
                applied++;
            }
        }
        return applied;
    }
}
