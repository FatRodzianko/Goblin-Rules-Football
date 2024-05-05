using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MapMakerEscapeMenu : MonoBehaviour
{
    [SerializeField] bool _isMenuOpen = false;
    [SerializeField] GameObject _escMenuPanel;
    [SerializeField] MapMakerBuilder _builder;
    [SerializeField] MapMakerGolfControls _playerInput;
    [SerializeField] MinimizeMaximizeManager _minimizeMaximizeManager;
    // Start is called before the first frame update
    void Start()
    {
        _builder = MapMakerBuilder.GetInstance();
        if (!_minimizeMaximizeManager)
            _minimizeMaximizeManager = this.GetComponent<MinimizeMaximizeManager>();
        _playerInput = _builder.PlayerInput;
        _playerInput.EscMenu.Escape.performed += EscapeKeyPressed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        _playerInput.EscMenu.Escape.performed -= EscapeKeyPressed;
    }
    public bool IsMenuOpen
    {
        get
        {
            return _isMenuOpen;
        }
    }
    public void EscapeKeyPressed(InputAction.CallbackContext ctx)
    {
        if (_isMenuOpen)
            CloseEscapeMenu();
        else
        {
            OpenEscapeMenu();
        }
    }
    public void OpenEscapeMenu()
    {
        _escMenuPanel.SetActive(true);
        _playerInput.MapMaker.Disable();
        _minimizeMaximizeManager.MinimizeShortCut();
        _isMenuOpen = true;
    }
    public void CloseEscapeMenu()
    {
        _escMenuPanel.SetActive(false);
        _playerInput.MapMaker.Enable();
        _minimizeMaximizeManager.MaximizeShortCut();
        _isMenuOpen = false;
    }
    public void ExitMapMaker()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
