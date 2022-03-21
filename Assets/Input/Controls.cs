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
                    ""processors"": ""StickDeadzone(min=0.125,max=0.925)"",
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
                }
            ],
            ""bindings"": [
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
                    ""id"": ""f559df9f-36b2-485b-a083-b8dc1b98c26a"",
                    ""path"": ""<XInputController>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""Move"",
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
                    ""id"": ""47ada124-1e30-4be2-bc86-ea7cac7bde1a"",
                    ""path"": ""<XInputController>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
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
                    ""id"": ""81faf0b6-0649-4aa7-9acc-305e21491f70"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c36f43d0-767d-4fe5-a1c9-6e443c000e3c"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
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
                    ""id"": ""3e92bc36-8c2d-422a-ab1a-a4d04e30123d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Slide"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""292813fb-1c0d-430b-a017-e38f06ca8112"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
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
                    ""id"": ""c875824e-8a7e-43a2-bff4-a02879ae8cc3"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Dive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3920a0d0-ed57-4c25-bf4b-05b5a5e33253"",
                    ""path"": ""<XInputController>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
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
                    ""id"": ""779edb31-f4d3-44cf-8370-2f516c20646e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Block"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""43a098ed-d8f8-462b-83f9-208d8dd40666"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
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
                    ""id"": ""412416db-3334-4650-aa19-affa57dd485a"",
                    ""path"": ""<XInputController>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickFootball"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""QESwitchGoblins"",
            ""id"": ""33cc8087-1502-4497-8bdf-0aae50d5c5e8"",
            ""actions"": [
                {
                    ""name"": ""SwitchE"",
                    ""type"": ""Button"",
                    ""id"": ""645949b3-5e11-4994-bb9e-09bb6e5b263f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SwitchQ"",
                    ""type"": ""Button"",
                    ""id"": ""6e197114-5723-4f60-9609-7193af1f1157"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""57cce0c2-ee55-4c9e-aa17-dafe77b070eb"",
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
                    ""id"": ""0b9dc878-1ac9-4aa9-b5af-16745d1f9927"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SwitchE"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3458ddf3-e58e-45cd-ae3f-dde5d9b3fae6"",
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
                    ""id"": ""1630c9ad-6008-497f-aa1c-c290033c10c7"",
                    ""path"": ""<XInputController>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SwitchQ"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""KickOff"",
            ""id"": ""6200b16f-c2ab-46d6-beba-10e66e97c416"",
            ""actions"": [
                {
                    ""name"": ""KickoffAngleDown"",
                    ""type"": ""Button"",
                    ""id"": ""c3e8d6cb-fa18-4a2d-8782-d558d6110166"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""KickoffAngleUp"",
                    ""type"": ""Button"",
                    ""id"": ""fa483f34-a5d1-4a28-a528-5625f7ef7a6d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""dd20070b-0c46-4e32-93a3-ebe616a5c65f"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickoffAngleDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d13bfd50-6b32-4cae-9a34-94251591b252"",
                    ""path"": ""<XInputController>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickoffAngleDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a8d8921a-3798-4b94-8be4-2bfddf7fb8db"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickoffAngleUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e438d57c-d8b6-40ae-ac73-abd7d4eb4163"",
                    ""path"": ""<XInputController>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickoffAngleUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""KickAfterPositioning"",
            ""id"": ""911f4aa8-2976-47b2-8bf9-923dda5fd402"",
            ""actions"": [
                {
                    ""name"": ""KickAfterPositioning-Right"",
                    ""type"": ""Button"",
                    ""id"": ""895f1fd8-dbc4-4f7e-9abb-808e03531f05"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""KickAfterPositioning-Left"",
                    ""type"": ""Button"",
                    ""id"": ""676c79c3-a86a-40db-85ea-0433fd8b9827"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SubmitPosition"",
                    ""type"": ""Button"",
                    ""id"": ""30e2c770-de22-418b-b714-061f92ca4248"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""7f604f11-12b0-4f6d-a584-5ebb66826ad4"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickAfterPositioning-Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5b884ca7-b665-4e8c-976b-5ca4e25bf41e"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickAfterPositioning-Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""699a6a32-86a3-461f-8d27-4865725bbc91"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickAfterPositioning-Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a8a8693e-749f-430b-805f-450fde97d837"",
                    ""path"": ""<XInputController>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickAfterPositioning-Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""432cd9c7-b2a5-401a-b2cb-34f6213c9214"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SubmitPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8a6b6bc-24f1-4d3d-9c26-2c0551bd6b01"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SubmitPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""CoinTossKickReceive"",
            ""id"": ""d5ad9933-d3c8-4edb-8c4e-b3456231221c"",
            ""actions"": [
                {
                    ""name"": ""SubmitCoin"",
                    ""type"": ""Button"",
                    ""id"": ""2b94af5d-c306-4758-8a93-671351ddc2fa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectTails"",
                    ""type"": ""Button"",
                    ""id"": ""2f188232-0ce9-4482-b337-e22d22235267"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectHeads"",
                    ""type"": ""Button"",
                    ""id"": ""161061fc-8b66-47ac-b458-65980f0a994e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""22b346f7-7d04-44cc-a88d-af5279771d88"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""SubmitCoin"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0e5f1208-7570-4c1a-b9d0-1bd3269f597a"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SubmitCoin"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c1e7bd57-5ca1-4dd2-914d-1c16c5806f90"",
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
                    ""id"": ""38794d6d-51d1-48eb-b3e8-0b99fa2bcd36"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectTails"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""565b7e65-b3a2-4542-946f-d4097f5c7fb9"",
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
                    ""id"": ""0f6a2941-3324-4978-a06f-d0e03ddee7c7"",
                    ""path"": ""<XInputController>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectHeads"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""KickAfterKicking"",
            ""id"": ""e0cb3e75-95b3-41b9-a383-9e6b2bf377a2"",
            ""actions"": [
                {
                    ""name"": ""KickAfterSubmit"",
                    ""type"": ""Button"",
                    ""id"": ""f6724e12-c3ed-4f95-906a-5ada47f70d2c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""990a44da-4b16-4e48-be40-14a2c5c8cecd"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""KickAfterSubmit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3f612bdc-cdd2-476a-a62b-053b0513555d"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""KickAfterSubmit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PowerUps"",
            ""id"": ""75c8ba50-df2f-4147-ba74-0d5155082826"",
            ""actions"": [
                {
                    ""name"": ""PowerUp1"",
                    ""type"": ""Button"",
                    ""id"": ""e6ddc253-cbcd-4842-84d7-d80b079e241c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PowerUp2"",
                    ""type"": ""Button"",
                    ""id"": ""8ab417a8-0d84-4c14-b000-81515aecf728"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PowerUp3"",
                    ""type"": ""Button"",
                    ""id"": ""90cd0521-a802-4dc7-bd10-2b76eaf61e09"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PowerUp4"",
                    ""type"": ""Button"",
                    ""id"": ""5109777a-bd0f-4e60-9da7-3f0c2ceecdb2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4386d07e-be96-430e-93e3-82e6e7479b5f"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""PowerUp1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""15a6266e-e4ba-4f45-9b72-2917e87ba5ff"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""PowerUp1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d02b4353-6c37-4cb3-a02e-29fe61caee8a"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""PowerUp2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1130be0c-7d8e-4655-bf5f-31beb3c9f37e"",
                    ""path"": ""<XInputController>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""PowerUp2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68300691-e92b-4ad6-ad0e-33fd494384f0"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""PowerUp3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4967b179-6b53-404d-a153-6b4ba10d8a13"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""PowerUp3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03ab04af-1edc-4cb0-b9c4-d4fa8786e186"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""PowerUp4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80b81770-41e4-4af1-a1e1-6e6656c00bd9"",
                    ""path"": ""<XInputController>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""PowerUp4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""SelectPowerUps"",
            ""id"": ""c7f83cf0-5df4-405c-be9f-122c5ef69152"",
            ""actions"": [
                {
                    ""name"": ""SubmitSelection"",
                    ""type"": ""Button"",
                    ""id"": ""ae2a74e4-087f-43c8-83af-4bff1f15d61b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectLeft"",
                    ""type"": ""Button"",
                    ""id"": ""76fd207a-130c-44c2-af16-ade2a90f9a77"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectRight"",
                    ""type"": ""Value"",
                    ""id"": ""c4b0d989-fe85-4cef-83fc-0837f1a2406b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectLeftOrRight"",
                    ""type"": ""Value"",
                    ""id"": ""14efd74a-9ae4-40ac-bc29-4e6ff3c067f1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""StickDeadzone(min=0.5,max=0.925)"",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectLeftOrRightComposite"",
                    ""type"": ""Value"",
                    ""id"": ""b37cf925-f95b-4a9d-9a55-a744c97fbe9d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""StickDeadzone(min=0.5,max=0.925)"",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""987c0a17-d29b-47f6-a4a9-376fc1c43ab1"",
                    ""path"": ""<XInputController>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SubmitSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""167853a7-77b5-4761-88ba-3fa94ff12823"",
                    ""path"": ""<XInputController>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone(min=0.5,max=0.925)"",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2771d0bf-4638-4e7e-8ba6-478aac64842c"",
                    ""path"": ""<XInputController>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone(min=0.5,max=0.925)"",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8d236c7-c004-4918-ba5c-5fac95a5b3e8"",
                    ""path"": ""<XInputController>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeftOrRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""2e702dbc-2f94-47d7-a3e9-29773867d5fe"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone(min=0.125,max=0.925)"",
                    ""groups"": """",
                    ""action"": ""SelectLeftOrRightComposite"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""db0d2fbc-f8cf-479a-9df3-9b7497ed1abf"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeftOrRightComposite"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""26bfd77d-194d-47fa-97f8-fcb6559729e7"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeftOrRightComposite"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""79085596-61c8-4cb0-b5d2-1b14632bd43c"",
                    ""path"": ""<XInputController>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeftOrRightComposite"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""1752c55b-37a1-4802-bbec-67d2d3f2f842"",
                    ""path"": ""<XInputController>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""GamePad"",
                    ""action"": ""SelectLeftOrRightComposite"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
        },
        {
            ""name"": ""GamePad"",
            ""bindingGroup"": ""GamePad"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
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
        m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
        m_Player_Attack = m_Player.FindAction("Attack", throwIfNotFound: true);
        m_Player_Slide = m_Player.FindAction("Slide", throwIfNotFound: true);
        m_Player_Dive = m_Player.FindAction("Dive", throwIfNotFound: true);
        m_Player_Block = m_Player.FindAction("Block", throwIfNotFound: true);
        m_Player_KickFootball = m_Player.FindAction("KickFootball", throwIfNotFound: true);
        // QESwitchGoblins
        m_QESwitchGoblins = asset.FindActionMap("QESwitchGoblins", throwIfNotFound: true);
        m_QESwitchGoblins_SwitchE = m_QESwitchGoblins.FindAction("SwitchE", throwIfNotFound: true);
        m_QESwitchGoblins_SwitchQ = m_QESwitchGoblins.FindAction("SwitchQ", throwIfNotFound: true);
        // KickOff
        m_KickOff = asset.FindActionMap("KickOff", throwIfNotFound: true);
        m_KickOff_KickoffAngleDown = m_KickOff.FindAction("KickoffAngleDown", throwIfNotFound: true);
        m_KickOff_KickoffAngleUp = m_KickOff.FindAction("KickoffAngleUp", throwIfNotFound: true);
        // KickAfterPositioning
        m_KickAfterPositioning = asset.FindActionMap("KickAfterPositioning", throwIfNotFound: true);
        m_KickAfterPositioning_KickAfterPositioningRight = m_KickAfterPositioning.FindAction("KickAfterPositioning-Right", throwIfNotFound: true);
        m_KickAfterPositioning_KickAfterPositioningLeft = m_KickAfterPositioning.FindAction("KickAfterPositioning-Left", throwIfNotFound: true);
        m_KickAfterPositioning_SubmitPosition = m_KickAfterPositioning.FindAction("SubmitPosition", throwIfNotFound: true);
        // CoinTossKickReceive
        m_CoinTossKickReceive = asset.FindActionMap("CoinTossKickReceive", throwIfNotFound: true);
        m_CoinTossKickReceive_SubmitCoin = m_CoinTossKickReceive.FindAction("SubmitCoin", throwIfNotFound: true);
        m_CoinTossKickReceive_SelectTails = m_CoinTossKickReceive.FindAction("SelectTails", throwIfNotFound: true);
        m_CoinTossKickReceive_SelectHeads = m_CoinTossKickReceive.FindAction("SelectHeads", throwIfNotFound: true);
        // KickAfterKicking
        m_KickAfterKicking = asset.FindActionMap("KickAfterKicking", throwIfNotFound: true);
        m_KickAfterKicking_KickAfterSubmit = m_KickAfterKicking.FindAction("KickAfterSubmit", throwIfNotFound: true);
        // PowerUps
        m_PowerUps = asset.FindActionMap("PowerUps", throwIfNotFound: true);
        m_PowerUps_PowerUp1 = m_PowerUps.FindAction("PowerUp1", throwIfNotFound: true);
        m_PowerUps_PowerUp2 = m_PowerUps.FindAction("PowerUp2", throwIfNotFound: true);
        m_PowerUps_PowerUp3 = m_PowerUps.FindAction("PowerUp3", throwIfNotFound: true);
        m_PowerUps_PowerUp4 = m_PowerUps.FindAction("PowerUp4", throwIfNotFound: true);
        // SelectPowerUps
        m_SelectPowerUps = asset.FindActionMap("SelectPowerUps", throwIfNotFound: true);
        m_SelectPowerUps_SubmitSelection = m_SelectPowerUps.FindAction("SubmitSelection", throwIfNotFound: true);
        m_SelectPowerUps_SelectLeft = m_SelectPowerUps.FindAction("SelectLeft", throwIfNotFound: true);
        m_SelectPowerUps_SelectRight = m_SelectPowerUps.FindAction("SelectRight", throwIfNotFound: true);
        m_SelectPowerUps_SelectLeftOrRight = m_SelectPowerUps.FindAction("SelectLeftOrRight", throwIfNotFound: true);
        m_SelectPowerUps_SelectLeftOrRightComposite = m_SelectPowerUps.FindAction("SelectLeftOrRightComposite", throwIfNotFound: true);
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
    private readonly InputAction m_Player_Sprint;
    private readonly InputAction m_Player_Attack;
    private readonly InputAction m_Player_Slide;
    private readonly InputAction m_Player_Dive;
    private readonly InputAction m_Player_Block;
    private readonly InputAction m_Player_KickFootball;
    public struct PlayerActions
    {
        private @Controls m_Wrapper;
        public PlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Sprint => m_Wrapper.m_Player_Sprint;
        public InputAction @Attack => m_Wrapper.m_Player_Attack;
        public InputAction @Slide => m_Wrapper.m_Player_Slide;
        public InputAction @Dive => m_Wrapper.m_Player_Dive;
        public InputAction @Block => m_Wrapper.m_Player_Block;
        public InputAction @KickFootball => m_Wrapper.m_Player_KickFootball;
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
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
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
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // QESwitchGoblins
    private readonly InputActionMap m_QESwitchGoblins;
    private IQESwitchGoblinsActions m_QESwitchGoblinsActionsCallbackInterface;
    private readonly InputAction m_QESwitchGoblins_SwitchE;
    private readonly InputAction m_QESwitchGoblins_SwitchQ;
    public struct QESwitchGoblinsActions
    {
        private @Controls m_Wrapper;
        public QESwitchGoblinsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @SwitchE => m_Wrapper.m_QESwitchGoblins_SwitchE;
        public InputAction @SwitchQ => m_Wrapper.m_QESwitchGoblins_SwitchQ;
        public InputActionMap Get() { return m_Wrapper.m_QESwitchGoblins; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(QESwitchGoblinsActions set) { return set.Get(); }
        public void SetCallbacks(IQESwitchGoblinsActions instance)
        {
            if (m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface != null)
            {
                @SwitchE.started -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchE;
                @SwitchE.performed -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchE;
                @SwitchE.canceled -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchE;
                @SwitchQ.started -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchQ;
                @SwitchQ.performed -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchQ;
                @SwitchQ.canceled -= m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface.OnSwitchQ;
            }
            m_Wrapper.m_QESwitchGoblinsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SwitchE.started += instance.OnSwitchE;
                @SwitchE.performed += instance.OnSwitchE;
                @SwitchE.canceled += instance.OnSwitchE;
                @SwitchQ.started += instance.OnSwitchQ;
                @SwitchQ.performed += instance.OnSwitchQ;
                @SwitchQ.canceled += instance.OnSwitchQ;
            }
        }
    }
    public QESwitchGoblinsActions @QESwitchGoblins => new QESwitchGoblinsActions(this);

    // KickOff
    private readonly InputActionMap m_KickOff;
    private IKickOffActions m_KickOffActionsCallbackInterface;
    private readonly InputAction m_KickOff_KickoffAngleDown;
    private readonly InputAction m_KickOff_KickoffAngleUp;
    public struct KickOffActions
    {
        private @Controls m_Wrapper;
        public KickOffActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @KickoffAngleDown => m_Wrapper.m_KickOff_KickoffAngleDown;
        public InputAction @KickoffAngleUp => m_Wrapper.m_KickOff_KickoffAngleUp;
        public InputActionMap Get() { return m_Wrapper.m_KickOff; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(KickOffActions set) { return set.Get(); }
        public void SetCallbacks(IKickOffActions instance)
        {
            if (m_Wrapper.m_KickOffActionsCallbackInterface != null)
            {
                @KickoffAngleDown.started -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleDown;
                @KickoffAngleDown.performed -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleDown;
                @KickoffAngleDown.canceled -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleDown;
                @KickoffAngleUp.started -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleUp;
                @KickoffAngleUp.performed -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleUp;
                @KickoffAngleUp.canceled -= m_Wrapper.m_KickOffActionsCallbackInterface.OnKickoffAngleUp;
            }
            m_Wrapper.m_KickOffActionsCallbackInterface = instance;
            if (instance != null)
            {
                @KickoffAngleDown.started += instance.OnKickoffAngleDown;
                @KickoffAngleDown.performed += instance.OnKickoffAngleDown;
                @KickoffAngleDown.canceled += instance.OnKickoffAngleDown;
                @KickoffAngleUp.started += instance.OnKickoffAngleUp;
                @KickoffAngleUp.performed += instance.OnKickoffAngleUp;
                @KickoffAngleUp.canceled += instance.OnKickoffAngleUp;
            }
        }
    }
    public KickOffActions @KickOff => new KickOffActions(this);

    // KickAfterPositioning
    private readonly InputActionMap m_KickAfterPositioning;
    private IKickAfterPositioningActions m_KickAfterPositioningActionsCallbackInterface;
    private readonly InputAction m_KickAfterPositioning_KickAfterPositioningRight;
    private readonly InputAction m_KickAfterPositioning_KickAfterPositioningLeft;
    private readonly InputAction m_KickAfterPositioning_SubmitPosition;
    public struct KickAfterPositioningActions
    {
        private @Controls m_Wrapper;
        public KickAfterPositioningActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @KickAfterPositioningRight => m_Wrapper.m_KickAfterPositioning_KickAfterPositioningRight;
        public InputAction @KickAfterPositioningLeft => m_Wrapper.m_KickAfterPositioning_KickAfterPositioningLeft;
        public InputAction @SubmitPosition => m_Wrapper.m_KickAfterPositioning_SubmitPosition;
        public InputActionMap Get() { return m_Wrapper.m_KickAfterPositioning; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(KickAfterPositioningActions set) { return set.Get(); }
        public void SetCallbacks(IKickAfterPositioningActions instance)
        {
            if (m_Wrapper.m_KickAfterPositioningActionsCallbackInterface != null)
            {
                @KickAfterPositioningRight.started -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningRight;
                @KickAfterPositioningRight.performed -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningRight;
                @KickAfterPositioningRight.canceled -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningRight;
                @KickAfterPositioningLeft.started -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningLeft;
                @KickAfterPositioningLeft.performed -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningLeft;
                @KickAfterPositioningLeft.canceled -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnKickAfterPositioningLeft;
                @SubmitPosition.started -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnSubmitPosition;
                @SubmitPosition.performed -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnSubmitPosition;
                @SubmitPosition.canceled -= m_Wrapper.m_KickAfterPositioningActionsCallbackInterface.OnSubmitPosition;
            }
            m_Wrapper.m_KickAfterPositioningActionsCallbackInterface = instance;
            if (instance != null)
            {
                @KickAfterPositioningRight.started += instance.OnKickAfterPositioningRight;
                @KickAfterPositioningRight.performed += instance.OnKickAfterPositioningRight;
                @KickAfterPositioningRight.canceled += instance.OnKickAfterPositioningRight;
                @KickAfterPositioningLeft.started += instance.OnKickAfterPositioningLeft;
                @KickAfterPositioningLeft.performed += instance.OnKickAfterPositioningLeft;
                @KickAfterPositioningLeft.canceled += instance.OnKickAfterPositioningLeft;
                @SubmitPosition.started += instance.OnSubmitPosition;
                @SubmitPosition.performed += instance.OnSubmitPosition;
                @SubmitPosition.canceled += instance.OnSubmitPosition;
            }
        }
    }
    public KickAfterPositioningActions @KickAfterPositioning => new KickAfterPositioningActions(this);

    // CoinTossKickReceive
    private readonly InputActionMap m_CoinTossKickReceive;
    private ICoinTossKickReceiveActions m_CoinTossKickReceiveActionsCallbackInterface;
    private readonly InputAction m_CoinTossKickReceive_SubmitCoin;
    private readonly InputAction m_CoinTossKickReceive_SelectTails;
    private readonly InputAction m_CoinTossKickReceive_SelectHeads;
    public struct CoinTossKickReceiveActions
    {
        private @Controls m_Wrapper;
        public CoinTossKickReceiveActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @SubmitCoin => m_Wrapper.m_CoinTossKickReceive_SubmitCoin;
        public InputAction @SelectTails => m_Wrapper.m_CoinTossKickReceive_SelectTails;
        public InputAction @SelectHeads => m_Wrapper.m_CoinTossKickReceive_SelectHeads;
        public InputActionMap Get() { return m_Wrapper.m_CoinTossKickReceive; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CoinTossKickReceiveActions set) { return set.Get(); }
        public void SetCallbacks(ICoinTossKickReceiveActions instance)
        {
            if (m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface != null)
            {
                @SubmitCoin.started -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSubmitCoin;
                @SubmitCoin.performed -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSubmitCoin;
                @SubmitCoin.canceled -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSubmitCoin;
                @SelectTails.started -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectTails;
                @SelectTails.performed -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectTails;
                @SelectTails.canceled -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectTails;
                @SelectHeads.started -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectHeads;
                @SelectHeads.performed -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectHeads;
                @SelectHeads.canceled -= m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface.OnSelectHeads;
            }
            m_Wrapper.m_CoinTossKickReceiveActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SubmitCoin.started += instance.OnSubmitCoin;
                @SubmitCoin.performed += instance.OnSubmitCoin;
                @SubmitCoin.canceled += instance.OnSubmitCoin;
                @SelectTails.started += instance.OnSelectTails;
                @SelectTails.performed += instance.OnSelectTails;
                @SelectTails.canceled += instance.OnSelectTails;
                @SelectHeads.started += instance.OnSelectHeads;
                @SelectHeads.performed += instance.OnSelectHeads;
                @SelectHeads.canceled += instance.OnSelectHeads;
            }
        }
    }
    public CoinTossKickReceiveActions @CoinTossKickReceive => new CoinTossKickReceiveActions(this);

    // KickAfterKicking
    private readonly InputActionMap m_KickAfterKicking;
    private IKickAfterKickingActions m_KickAfterKickingActionsCallbackInterface;
    private readonly InputAction m_KickAfterKicking_KickAfterSubmit;
    public struct KickAfterKickingActions
    {
        private @Controls m_Wrapper;
        public KickAfterKickingActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @KickAfterSubmit => m_Wrapper.m_KickAfterKicking_KickAfterSubmit;
        public InputActionMap Get() { return m_Wrapper.m_KickAfterKicking; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(KickAfterKickingActions set) { return set.Get(); }
        public void SetCallbacks(IKickAfterKickingActions instance)
        {
            if (m_Wrapper.m_KickAfterKickingActionsCallbackInterface != null)
            {
                @KickAfterSubmit.started -= m_Wrapper.m_KickAfterKickingActionsCallbackInterface.OnKickAfterSubmit;
                @KickAfterSubmit.performed -= m_Wrapper.m_KickAfterKickingActionsCallbackInterface.OnKickAfterSubmit;
                @KickAfterSubmit.canceled -= m_Wrapper.m_KickAfterKickingActionsCallbackInterface.OnKickAfterSubmit;
            }
            m_Wrapper.m_KickAfterKickingActionsCallbackInterface = instance;
            if (instance != null)
            {
                @KickAfterSubmit.started += instance.OnKickAfterSubmit;
                @KickAfterSubmit.performed += instance.OnKickAfterSubmit;
                @KickAfterSubmit.canceled += instance.OnKickAfterSubmit;
            }
        }
    }
    public KickAfterKickingActions @KickAfterKicking => new KickAfterKickingActions(this);

    // PowerUps
    private readonly InputActionMap m_PowerUps;
    private IPowerUpsActions m_PowerUpsActionsCallbackInterface;
    private readonly InputAction m_PowerUps_PowerUp1;
    private readonly InputAction m_PowerUps_PowerUp2;
    private readonly InputAction m_PowerUps_PowerUp3;
    private readonly InputAction m_PowerUps_PowerUp4;
    public struct PowerUpsActions
    {
        private @Controls m_Wrapper;
        public PowerUpsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @PowerUp1 => m_Wrapper.m_PowerUps_PowerUp1;
        public InputAction @PowerUp2 => m_Wrapper.m_PowerUps_PowerUp2;
        public InputAction @PowerUp3 => m_Wrapper.m_PowerUps_PowerUp3;
        public InputAction @PowerUp4 => m_Wrapper.m_PowerUps_PowerUp4;
        public InputActionMap Get() { return m_Wrapper.m_PowerUps; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PowerUpsActions set) { return set.Get(); }
        public void SetCallbacks(IPowerUpsActions instance)
        {
            if (m_Wrapper.m_PowerUpsActionsCallbackInterface != null)
            {
                @PowerUp1.started -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp1;
                @PowerUp1.performed -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp1;
                @PowerUp1.canceled -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp1;
                @PowerUp2.started -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp2;
                @PowerUp2.performed -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp2;
                @PowerUp2.canceled -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp2;
                @PowerUp3.started -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp3;
                @PowerUp3.performed -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp3;
                @PowerUp3.canceled -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp3;
                @PowerUp4.started -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp4;
                @PowerUp4.performed -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp4;
                @PowerUp4.canceled -= m_Wrapper.m_PowerUpsActionsCallbackInterface.OnPowerUp4;
            }
            m_Wrapper.m_PowerUpsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @PowerUp1.started += instance.OnPowerUp1;
                @PowerUp1.performed += instance.OnPowerUp1;
                @PowerUp1.canceled += instance.OnPowerUp1;
                @PowerUp2.started += instance.OnPowerUp2;
                @PowerUp2.performed += instance.OnPowerUp2;
                @PowerUp2.canceled += instance.OnPowerUp2;
                @PowerUp3.started += instance.OnPowerUp3;
                @PowerUp3.performed += instance.OnPowerUp3;
                @PowerUp3.canceled += instance.OnPowerUp3;
                @PowerUp4.started += instance.OnPowerUp4;
                @PowerUp4.performed += instance.OnPowerUp4;
                @PowerUp4.canceled += instance.OnPowerUp4;
            }
        }
    }
    public PowerUpsActions @PowerUps => new PowerUpsActions(this);

    // SelectPowerUps
    private readonly InputActionMap m_SelectPowerUps;
    private ISelectPowerUpsActions m_SelectPowerUpsActionsCallbackInterface;
    private readonly InputAction m_SelectPowerUps_SubmitSelection;
    private readonly InputAction m_SelectPowerUps_SelectLeft;
    private readonly InputAction m_SelectPowerUps_SelectRight;
    private readonly InputAction m_SelectPowerUps_SelectLeftOrRight;
    private readonly InputAction m_SelectPowerUps_SelectLeftOrRightComposite;
    public struct SelectPowerUpsActions
    {
        private @Controls m_Wrapper;
        public SelectPowerUpsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @SubmitSelection => m_Wrapper.m_SelectPowerUps_SubmitSelection;
        public InputAction @SelectLeft => m_Wrapper.m_SelectPowerUps_SelectLeft;
        public InputAction @SelectRight => m_Wrapper.m_SelectPowerUps_SelectRight;
        public InputAction @SelectLeftOrRight => m_Wrapper.m_SelectPowerUps_SelectLeftOrRight;
        public InputAction @SelectLeftOrRightComposite => m_Wrapper.m_SelectPowerUps_SelectLeftOrRightComposite;
        public InputActionMap Get() { return m_Wrapper.m_SelectPowerUps; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SelectPowerUpsActions set) { return set.Get(); }
        public void SetCallbacks(ISelectPowerUpsActions instance)
        {
            if (m_Wrapper.m_SelectPowerUpsActionsCallbackInterface != null)
            {
                @SubmitSelection.started -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSubmitSelection;
                @SubmitSelection.performed -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSubmitSelection;
                @SubmitSelection.canceled -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSubmitSelection;
                @SelectLeft.started -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeft;
                @SelectLeft.performed -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeft;
                @SelectLeft.canceled -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeft;
                @SelectRight.started -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectRight;
                @SelectRight.performed -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectRight;
                @SelectRight.canceled -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectRight;
                @SelectLeftOrRight.started -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRight;
                @SelectLeftOrRight.performed -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRight;
                @SelectLeftOrRight.canceled -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRight;
                @SelectLeftOrRightComposite.started -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRightComposite;
                @SelectLeftOrRightComposite.performed -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRightComposite;
                @SelectLeftOrRightComposite.canceled -= m_Wrapper.m_SelectPowerUpsActionsCallbackInterface.OnSelectLeftOrRightComposite;
            }
            m_Wrapper.m_SelectPowerUpsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SubmitSelection.started += instance.OnSubmitSelection;
                @SubmitSelection.performed += instance.OnSubmitSelection;
                @SubmitSelection.canceled += instance.OnSubmitSelection;
                @SelectLeft.started += instance.OnSelectLeft;
                @SelectLeft.performed += instance.OnSelectLeft;
                @SelectLeft.canceled += instance.OnSelectLeft;
                @SelectRight.started += instance.OnSelectRight;
                @SelectRight.performed += instance.OnSelectRight;
                @SelectRight.canceled += instance.OnSelectRight;
                @SelectLeftOrRight.started += instance.OnSelectLeftOrRight;
                @SelectLeftOrRight.performed += instance.OnSelectLeftOrRight;
                @SelectLeftOrRight.canceled += instance.OnSelectLeftOrRight;
                @SelectLeftOrRightComposite.started += instance.OnSelectLeftOrRightComposite;
                @SelectLeftOrRightComposite.performed += instance.OnSelectLeftOrRightComposite;
                @SelectLeftOrRightComposite.canceled += instance.OnSelectLeftOrRightComposite;
            }
        }
    }
    public SelectPowerUpsActions @SelectPowerUps => new SelectPowerUpsActions(this);
    private int m_KeyboardandMouseSchemeIndex = -1;
    public InputControlScheme KeyboardandMouseScheme
    {
        get
        {
            if (m_KeyboardandMouseSchemeIndex == -1) m_KeyboardandMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard and Mouse");
            return asset.controlSchemes[m_KeyboardandMouseSchemeIndex];
        }
    }
    private int m_GamePadSchemeIndex = -1;
    public InputControlScheme GamePadScheme
    {
        get
        {
            if (m_GamePadSchemeIndex == -1) m_GamePadSchemeIndex = asset.FindControlSchemeIndex("GamePad");
            return asset.controlSchemes[m_GamePadSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnSlide(InputAction.CallbackContext context);
        void OnDive(InputAction.CallbackContext context);
        void OnBlock(InputAction.CallbackContext context);
        void OnKickFootball(InputAction.CallbackContext context);
    }
    public interface IQESwitchGoblinsActions
    {
        void OnSwitchE(InputAction.CallbackContext context);
        void OnSwitchQ(InputAction.CallbackContext context);
    }
    public interface IKickOffActions
    {
        void OnKickoffAngleDown(InputAction.CallbackContext context);
        void OnKickoffAngleUp(InputAction.CallbackContext context);
    }
    public interface IKickAfterPositioningActions
    {
        void OnKickAfterPositioningRight(InputAction.CallbackContext context);
        void OnKickAfterPositioningLeft(InputAction.CallbackContext context);
        void OnSubmitPosition(InputAction.CallbackContext context);
    }
    public interface ICoinTossKickReceiveActions
    {
        void OnSubmitCoin(InputAction.CallbackContext context);
        void OnSelectTails(InputAction.CallbackContext context);
        void OnSelectHeads(InputAction.CallbackContext context);
    }
    public interface IKickAfterKickingActions
    {
        void OnKickAfterSubmit(InputAction.CallbackContext context);
    }
    public interface IPowerUpsActions
    {
        void OnPowerUp1(InputAction.CallbackContext context);
        void OnPowerUp2(InputAction.CallbackContext context);
        void OnPowerUp3(InputAction.CallbackContext context);
        void OnPowerUp4(InputAction.CallbackContext context);
    }
    public interface ISelectPowerUpsActions
    {
        void OnSubmitSelection(InputAction.CallbackContext context);
        void OnSelectLeft(InputAction.CallbackContext context);
        void OnSelectRight(InputAction.CallbackContext context);
        void OnSelectLeftOrRight(InputAction.CallbackContext context);
        void OnSelectLeftOrRightComposite(InputAction.CallbackContext context);
    }
}
