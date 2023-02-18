using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torndao : MonoBehaviour
{
    [SerializeField] BoxCollider2D _myCollider;
    [SerializeField] public float HeightInUnityUnits = 3f;
    [SerializeField] int _tornadoStrength = 1;
    private void Awake()
    {
        if (!_myCollider)
            _myCollider = this.GetComponent<BoxCollider2D>();
        SetTornadoStrength();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "golfBall")
        {
            GolfBallTopDown golfBallScript = collision.GetComponent<GolfBallTopDown>();
            if (golfBallScript.IsInHole)
                return;

            float ballZ = golfBallScript.transform.position.z;
            float ballHeightInUnityUnits = golfBallScript.GetBallHeightYValue(ballZ);

            golfBallScript.HitByTornado(HeightInUnityUnits, ballHeightInUnityUnits,_tornadoStrength);
        }
    }
    void SetTornadoStrength()
    {
        int maxStrengthFromRainLevel = 1;
        int minStrengthFromWindPower = 1;
        if (RainManager.instance.RainState.ToLower().Contains("med"))
            maxStrengthFromRainLevel = 3;
        else if (RainManager.instance.RainState.ToLower().Contains("heavy"))
            maxStrengthFromRainLevel = 5;

        float currentWindPower = WindManager.instance.WindPower;
        if (currentWindPower < 10)
        {
            minStrengthFromWindPower = 1;
        }
        else if (currentWindPower < 15)
        {
            minStrengthFromWindPower = 2;
        }
        else
        {
            minStrengthFromWindPower = 3;
        }

        if (minStrengthFromWindPower > maxStrengthFromRainLevel)
            minStrengthFromWindPower = maxStrengthFromRainLevel;

        if (minStrengthFromWindPower == maxStrengthFromRainLevel)
        {
            _tornadoStrength = minStrengthFromWindPower;
        }
        else
        {
            _tornadoStrength = UnityEngine.Random.Range(minStrengthFromWindPower, maxStrengthFromRainLevel);
        }
        AdjustScaleOfTornado(_tornadoStrength);
        AdjustHeightOfTornado(_tornadoStrength);
    }
    void AdjustScaleOfTornado(int scaleToSet)
    {
        this.transform.localScale = new Vector3(scaleToSet, scaleToSet, 1f);
    }
    void AdjustHeightOfTornado(int scaleFactor)
    {
        HeightInUnityUnits *= scaleFactor;
    }
}
