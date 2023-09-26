using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Steamworks;

public class LobbyManagerGolf : MonoBehaviour
{
    public static LobbyManagerGolf instance;
    [SerializeField] EventSystem eventSystem;

    [Header("Lobby UI Elements")]
    [SerializeField] private TextMeshProUGUI LobbyNameText;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject PlayerListItemPrefab;
    [SerializeField] private GameObject PlayerListItem3v3Prefab;
    [SerializeField] private Button ReadyUpButton;
    [SerializeField] private Button StartGameButton;

    private void Awake()
    {
        MakeInstance();
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
        
    }
}
