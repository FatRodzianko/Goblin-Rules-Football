// GENERATED AUTOMATICALLY FROM 'Assets/Input/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""55a7f896-16fd-471c-8e4e-00dfa218de30"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""55dae0ab-f74c-450a-af39-cc436a68254b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SwitchQ"",
                    ""type"": ""Button"",
                    ""id"": ""aa787ddd-6c65-4d72-b5b2-4663ff4118ab"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SwitchE"",
                    ""type"": ""Button"",
                    ""id"": ""c67e26c1-c2b2-4915-91ae-6d17099a6142"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""bda818f1-5aa6-4207-b58e-846839fd1004"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""f2927ea9-1d99-4dba-9960-471368835194"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Slide"",
                    ""type"": ""Button"",
                    ""id"": ""38ee9258-7db9-4792-a514-8fca7ed36902"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dive"",
                    ""type"": ""Button"",
                    ""id"": ""abac5bfe-079d-4f67-8227-44a691505551"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Block"",
                    ""type"": ""Button"",
                    ""id"": ""6817865c-d0a2-43f6-8201-00a7260fb2a1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""KickFootball"",
                    ""type"": ""Button"",
                    ""id"": ""f24a79c5-3d8d-418f-9dcb-b615f69ab95d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectHeads"",
                    ""type"": ""Button"",
                    ""id"": ""a5cb37ee-fb69-41c5-b9fa-1d1d669c7e86"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectTails"",
                    ""type"": ""Button"",
                    ""id"": ""3f40d423-1b15-4334-ac2d-2444862699f2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SubmitCoin"",
                    ""type"": ""Button"",
                    ""id"": ""c9b0432a-e8be-4f24-a82e-6b2f3abd5d4d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""2da967f6-9535-4570-9bc6-59ac844a7133"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e3991aee-c947-407f-b590-c61d8bde363a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""dca0926e-6ad5-4cc1-a39e-197c03dd0940"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""27ee4024-ae84-47f7-ab87-2ef7c53ae543"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8b45e870-0a23-4e4b-ad45-c66709e76cff"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrow_Keys"",
                    ""id"": ""0b4b9071-1e12-44e8-befe-38d8a9402a08"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8969137f-7e5f-48d0-a89e-7e703f263516"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""359363fb-9076-4452-b92e-f8626fe91b9f"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""a6d5133e-66f1-4d78-bd7f-eff4fb14893c"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""2b2ff860-c7b8-4eb1-b7d6-45102e99ab90"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""42bf3b80-b367-41b8-8533-c8e032f228c8"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SwitchQ"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9c21b70b-8b79-405c-a071-16d1b0f428ba"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SwitchE"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""75dc912d-82e1-4692-9286-20152d642ff8"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0f2a87bf-bec1-4ec0-af9e-10d3c7ee9641"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1c668a29-f927-42f7-a93f-c92300f65497"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Slide"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3a2449ad-9f93-4c39-b408-16e1e846af7b"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Dive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7bc32c91-f703-4e1f-88a7-52220ed970a2"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Block"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aee29fbb-f9c5-4ae7-86dd-4eb785a0e2d6"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickFootball"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""53f9ab38-37f9-4b7b-9790-dcb08db17122"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SelectHeads"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""88a3727d-4704-4b20-becc-6e4c4e094945"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SelectTails"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1ff50729-4375-481f-afa9-7ee0475450b5"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SubmitCoin"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard and Mouse"",
            ""bindingGroup"": ""Keyboard and Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_SwitchQ = m_Player.FindAction("SwitchQ", throwIfNotFound: true);
        m_Player_SwitchE = m_Player.FindAction("SwitchE", throwIfNotFound: true);
        m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
        m_Player_Attack = m_Player.FindAction("Attack", throwIfNotFound: true);
        m_Player_Slide = m_Player.FindAction("Slide", throwIfNotFound: true);
        m_Player_Dive = m_Player.FindAction("Dive", throwIfNotFound: true);
        m_Player_Block = m_Player.FindAction("Block", throwIfNotFound: true);
        m_Player_KickFootball = m_Player.FindAction("KickFootball", throwIfNotFound: true);
        m_Player_SelectHeads = m_Player.FindAction("SelectHeads", throwIfNotFound: true);
        m_Player_SelectTails = m_Player.FindAction("SelectTails", throwIfNotFound: true);
        m_Player_SubmitCoin = m_Player.FindAction("SubmitCoin", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_SwitchQ;
    private readonly InputAction m_Player_SwitchE;
    private readonly InputAction m_Player_Sprint;
    private readonly InputAction m_Player_Attack;
    private readonly InputAction m_Player_Slide;
    private readonly InputAction m_Player_Dive;
    private readonly InputAction m_Player_Block;
    private readonly InputAction m_Player_KickFootball;
    private readonly InputAction m_Player_SelectHeads;
    private readonly InputAction m_Player_SelectTails;
    private readonly InputAction m_Player_SubmitCoin;
    public struct PlayerActions
    {
        private @Controls m_Wrapper;
        public PlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @SwitchQ => m_Wrapper.m_Player_SwitchQ;
        public InputAction @SwitchE => m_Wrapper.m_Player_SwitchE;
        public InputAction @Sprint => m_Wrapper.m_Player_Sprint;
        public InputAction @Attack => m_Wrapper.m_Player_Attack;
        public InputAction @Slide => m_Wrapper.m_Player_Slide;
        public InputAction @Dive => m_Wrapper.m_Player_Dive;
        public InputAction @Block => m_Wrapper.m_Player_Block;
        public InputAction @KickFootball => m_Wrapper.m_Player_KickFootball;
        public InputAction @SelectHeads => m_Wrapper.m_Player_SelectHeads;
        public InputAction @SelectTails => m_Wrapper.m_Player_SelectTails;
        public InputAction @SubmitCoin => m_Wrapper.m_Player_SubmitCoin;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @SwitchQ.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchQ;
                @SwitchQ.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchQ;
                @SwitchQ.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchQ;
                @SwitchE.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchE;
                @SwitchE.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchE;
                @SwitchE.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitchE;
                @Sprint.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Attack.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Slide.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSlide;
                @Slide.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSlide;
                @Slide.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSlide;
                @Dive.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDive;
                @Dive.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDive;
                @Dive.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDive;
                @Block.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBlock;
                @Block.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBlock;
                @Block.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBlock;
                @KickFootball.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnKickFootball;
                @KickFootball.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnKickFootball;
                @KickFootball.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnKickFootball;
                @SelectHeads.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectHeads;
                @SelectHeads.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectHeads;
                @SelectHeads.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectHeads;
                @SelectTails.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectTails;
                @SelectTails.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectTails;
                @SelectTails.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelectTails;
                @SubmitCoin.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSubmitCoin;
                @SubmitCoin.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSubmitCoin;
                @SubmitCoin.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSubmitCoin;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @SwitchQ.started += instance.OnSwitchQ;
                @SwitchQ.performed += instance.OnSwitchQ;
                @SwitchQ.canceled += instance.OnSwitchQ;
                @SwitchE.started += instance.OnSwitchE;
                @SwitchE.performed += instance.OnSwitchE;
                @SwitchE.canceled += instance.OnSwitchE;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Slide.started += instance.OnSlide;
                @Slide.performed += instance.OnSlide;
                @Slide.canceled += instance.OnSlide;
                @Dive.started += instance.OnDive;
                @Dive.performed += instance.OnDive;
                @Dive.canceled += instance.OnDive;
                @Block.started += instance.OnBlock;
                @Block.performed += instance.OnBlock;
                @Block.canceled += instance.OnBlock;
                @KickFootball.started += instance.OnKickFootball;
                @KickFootball.performed += instance.OnKickFootball;
                @KickFootball.canceled += instance.OnKickFootball;
                @SelectHeads.started += instance.OnSelectHeads;
                @SelectHeads.performed += instance.OnSelectHeads;
                @SelectHeads.canceled += instance.OnSelectHeads;
                @SelectTails.started += instance.OnSelectTails;
                @SelectTails.performed += instance.OnSelectTails;
                @SelectTails.canceled += instance.OnSelectTails;
                @SubmitCoin.started += instance.OnSubmitCoin;
                @SubmitCoin.performed += instance.OnSubmitCoin;
                @SubmitCoin.canceled += instance.OnSubmitCoin;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_KeyboardandMouseSchemeIndex = -1;
    public InputControlScheme KeyboardandMouseScheme
    {
        get
        {
            if (m_KeyboardandMouseSchemeIndex == -1) m_KeyboardandMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard and Mouse");
            return asset.controlSchemes[m_KeyboardandMouseSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnSwitchQ(InputAction.CallbackContext context);
        void OnSwitchE(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnSlide(InputAction.CallbackContext context);
        void OnDive(InputAction.CallbackContext context);
        void OnBlock(InputAction.CallbackContext context);
        void OnKickFootball(InputAction.CallbackContext context);
        void OnSelectHeads(InputAction.CallbackContext context);
        void OnSelectTails(InputAction.CallbackContext context);
        void OnSubmitCoin(InputAction.CallbackContext context);
    }
}
