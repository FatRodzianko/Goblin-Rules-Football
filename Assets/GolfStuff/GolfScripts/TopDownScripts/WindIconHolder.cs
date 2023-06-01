using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindIconHolder : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] SpriteRenderer _windDirectionArrow;
    [SerializeField] SpriteRenderer _windNumberRightDigit;
    [SerializeField] SpriteRenderer _windNumberLeftDigit;

    [Header("Wind Direction Sprites")]
    [SerializeField] Sprite _north;
    [SerializeField] Sprite _northEast;
    [SerializeField] Sprite _east;
    [SerializeField] Sprite _southEast;
    [SerializeField] Sprite _south;
    [SerializeField] Sprite _southWest;
    [SerializeField] Sprite _west;
    [SerializeField] Sprite _northWest;

    [Header("Number Sprites")]
    [SerializeField] Sprite[] _numberSprites;

    [Header("Colors")]
    [SerializeField] Color _lowWind;
    [SerializeField] Color _medWind;
    [SerializeField] Color _highWind;
    [SerializeField] Color _veryHighWind;


    // Start is called before the first frame update
    void Start()
    {
        WindManager.instance.DirectionChanged += UpdateWindDirection;
        WindManager.instance.PowerChanged += UpdateWindPower;
        //WindManager.instance.BasePowerChanged += UpdateWindPower;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateWindDirection(Vector2 dir)
    {
        Debug.Log("UpdateWindDirection: " + dir.ToString());
        if (dir == new Vector2(0f, 1f))
        {
            _windDirectionArrow.sprite = _north;
        }
        if (dir == new Vector2(1f, 1f))
        {
            _windDirectionArrow.sprite = _northEast;
        }
        if (dir == new Vector2(1f, 0f))
        {
            _windDirectionArrow.sprite = _east;
        }
        if (dir == new Vector2(1f, -1f))
        {
            _windDirectionArrow.sprite = _southEast;
        }
        if (dir == new Vector2(0f, -1f))
        {
            _windDirectionArrow.sprite = _south;
        }
        if (dir == new Vector2(-1f, -1f))
        {
            _windDirectionArrow.sprite = _southWest;
        }
        if (dir == new Vector2(-1f, 0f))
        {
            _windDirectionArrow.sprite = _west;
        }
        if (dir == new Vector2(-1f, 1f))
        {
            _windDirectionArrow.sprite = _northWest;
        }
    }
    void UpdateWindPower(int power)
    {
        SetDigits(power);
        SetColor(power);
    }
    void SetDigits(int power)
    {
        Debug.Log("SetDigits: " + power.ToString());
        if (power < 10)
        {
            _windNumberLeftDigit.enabled = false;
            _windNumberRightDigit.sprite = _numberSprites[power];
        }
        else
        {
            _windNumberLeftDigit.enabled = true;
            int leftDigit = power / 10;
            int rightDigit = power % 10;

            _windNumberLeftDigit.sprite = _numberSprites[leftDigit];
            _windNumberRightDigit.sprite = _numberSprites[rightDigit];

        }
    }
    void SetColor(int power)
    {
        if (power < 5)
        {
            _windDirectionArrow.color = _lowWind;
        }
        else if (power < 10)
        {
            _windDirectionArrow.color = _medWind;
        }
        else if (power < 18)
        {
            _windDirectionArrow.color = _highWind;
        }
        else
        {
            _windDirectionArrow.color = _veryHighWind;
        }

    }
}
