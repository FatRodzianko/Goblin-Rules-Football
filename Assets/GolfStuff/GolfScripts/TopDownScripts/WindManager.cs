using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager instance;

    public Vector2 WindDirection = Vector2.zero;
    private Vector2 _windDirection = Vector2.zero;
    [SerializeField] public int WindPower = 0;
    [SerializeField] private int _windPower = 0;


    // followed event instructions from here https://answers.unity.com/questions/1206632/trigger-event-on-variable-change.html
    public delegate void WindDirectionChanged(Vector2 dir);
    public event WindDirectionChanged DirectionChanged;

    public delegate void WindPowerChanged(int power);
    public event WindPowerChanged PowerChanged;

    private void Awake()
    {
        MakeInstance();

        DirectionChanged = DirectionChangedFunction;
        PowerChanged = PowerChangedFunction;
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (WindPower != _windPower && PowerChanged != null)
        {
            _windPower = WindPower;
            PowerChanged(_windPower);
        }
        if (WindDirection != _windDirection && DirectionChanged != null)
        {
            _windDirection = WindDirection;
            DirectionChanged(_windDirection);
        }
    }
    void DirectionChangedFunction(Vector2 dir)
    {
        Debug.Log("DirectionChangedFunction: " + dir.ToString());
    }
    void PowerChangedFunction(int power)
    {
        Debug.Log("PowerChangedFunction: " + power.ToString());
    }
}
