using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfGameSettingsManager : MonoBehaviour
{
    [SerializeField] public string LobbyName;
    [SerializeField] public int NumberOfPlayers = 1;
    public int MinNumberOfPlayers = 1;
    public int MaxNumberOfPlayers = 10;
    [SerializeField] public bool FriendsOnly;
    [SerializeField] public bool PowerUpsEnabled;
    [SerializeField] public bool WeatherStatuesEnabled;
    [SerializeField] public bool StrokeLimitEnabled;
    [SerializeField] public int StrokeLimit;
    int _strokeLimitMin = 6;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
