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

    [Header("Lobby Player List UI")]
    [SerializeField] GameObject _lobbyPlayersPanel;
    [SerializeField] GameObject _lobbyListContent;
    [SerializeField] List<GolfPlayerListItem> _lobbyPlayerListItems = new List<GolfPlayerListItem>();

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
        _lobbyPlayersPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddLobbyPlayerListItem(GameObject newPlayerListItem)
    {
        GolfPlayerListItem playerListItem = newPlayerListItem.GetComponent<GolfPlayerListItem>();
        if (!_lobbyPlayerListItems.Contains(playerListItem))
            _lobbyPlayerListItems.Add(playerListItem);
        newPlayerListItem.transform.parent = this._lobbyListContent.transform;
        newPlayerListItem.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
    }
    public void RemoveLobbyPlayerListItem(GameObject newPlayerListItem)
    {
        try
        {
            GolfPlayerListItem playerListItem = newPlayerListItem.GetComponent<GolfPlayerListItem>();
            if (_lobbyPlayerListItems.Contains(playerListItem))
                _lobbyPlayerListItems.Remove(playerListItem);

            Destroy(newPlayerListItem);
        }
        catch (Exception e)
        {
            Debug.Log("RemoveLobbyPlayerListItem: error removing object from list??? Error: " + e);
        }
        
    }
    public void HideUIStuff()
    {
        _lobbyPlayersPanel.SetActive(false);
    }
}
