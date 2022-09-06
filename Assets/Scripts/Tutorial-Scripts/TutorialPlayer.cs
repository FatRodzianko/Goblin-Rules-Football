using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class TutorialPlayer : MonoBehaviour
{
    CinemachineVirtualCamera myCamera;
	TutorialFootball football;
	public bool isGamePaused = false;

	[Header("My Goblin Team")]
    public List<TutorialGoblinScript> goblinTeam = new List<TutorialGoblinScript>();
    public TutorialGoblinScript selectGoblin;
    public TutorialGoblinScript qGoblin;
    public TutorialGoblinScript eGoblin;
    public bool canSwitchGoblin = true;
    public TutorialTeam myTeam;

	[Header("Team Info")]
	public string teamName;
	public bool doesTeamHaveBall;
	public bool isTeamGrey;

	[Header("Possession Tracker")]
	public float possessionPoints = 0f;
	//[SyncVar] public float gainPossessionPointsRate = 2.5f;
	//public bool isGainingPossesionPointsRoutineRunning = false;
	//IEnumerator GainPossessionPointsRoutine;
	//public bool isLosingPossesionPointsRoutineRunning = false;
	//IEnumerator LosePossessionPointsRoutine;
	//public bool isNoPossessionCooldownRoutineRunning = false;
	//public bool didNoPossessionCooldownRoutineComplete = false;
	//IEnumerator NoPossessionCooldownRoutine;
	public float possessionBonus = 1.0f;

	[Header("Power Ups")]
	public List<TutorialPowerUp> myPowerUps = new List<TutorialPowerUp>();
	public int powerUpSelectedIndexNumber = 0;
	public bool canSelectWithRightStickAgain = true;
	public float nextPowerUpSelectTime = 0f;
	public bool wasRightStickUsedToSelect = false;

	[Header("Kick After")]
	public bool areGoblinsRepositionedForKickAfter = false;
	public bool localIsKickingPlayer = false;

	[Header("Input Manager Controls")]
	public bool coinTossControllsEnabled = false;
	public bool kickOrReceiveControlsEnabled = false;
	public bool qeSwitchingEnabled = false;
	public bool kickingControlsEnabled = false;
	public bool kickoffAimArrowControlsEnabled = false;
	public bool goblinMovementEnabled = false;
	public bool gameplayActionsEnabled = false;
	public bool kickAfterPositioningEnabled = false;
	public bool kickAfterKickingEnabled = false;
	public bool powerUpsEnabled = false;
	public bool menuNavigationEnabled = false;
	public bool blockingControlsEnabled = false;
	public bool attackControlsEnabled = false;

	[Header("Controls From Server")]
	public bool coinTossControlsOnServer = false;
	public bool kickOrReceiveControlsOnServer = false;
	public bool qeSwitchingControlsOnServer = false;
	public bool kickingControlsOnServer = false;
	public bool kickOffAimArrowControlsOnServer = false;
	public bool goblinMovementControlsOnServer = false;
	public bool gameplayActionControlsOnServer = false;
	public bool kickAfterPositioningControlsOnServer = false;
	public bool kickAfterKickingControlsOnServer = false;
	public bool powerupsControlsOnServer = false;
	public bool blockingControlsOnServer = false;
	public bool attackContolsOnServer = false;

	// Start is called before the first frame update
	void Start()
    {
        myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FollowSelectedGoblin(Transform goblinToFollow)
    {
        if(myCamera == null)
            myCamera = GameObject.FindGameObjectWithTag("camera").GetComponent<CinemachineVirtualCamera>();
        myCamera.Follow = goblinToFollow.transform;
    }
    public void EnableQEControls()
    {
        InputManager.Controls.QESwitchGoblins.SwitchQ.performed += ctx => SwitchToQGoblin();
        InputManager.Controls.QESwitchGoblins.SwitchE.performed += ctx => SwitchToEGoblin();
		qeSwitchingControlsOnServer = true;
		qeSwitchingEnabled = true;
	}
	public void EnableKickingControlsFirstTime()
	{
		InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
		InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
		kickingControlsOnServer = true;
		kickingControlsEnabled = true;
	}
    public void SwitchToQGoblin()
    {
		Debug.Log("SwitchToQGoblin: " + canSwitchGoblin.ToString());
		TutorialManager.instance.PlayerPressedQE();
		if ((canSwitchGoblin && !selectGoblin.isKicking && !selectGoblin.isDiving) || qGoblin.doesCharacterHaveBall)
		{
			if (doesTeamHaveBall && !qGoblin.canGoblinReceivePass)
				return;
			TutorialGoblinScript currentSelectedGoblin = selectGoblin;
			TutorialGoblinScript currentQGoblin = qGoblin;

			currentSelectedGoblin.UnSelectThisCharacter();
			currentQGoblin.SelectThisCharacter();

			selectGoblin = currentQGoblin;
			qGoblin = currentSelectedGoblin;

			currentQGoblin.SetQGoblin(false);
			currentSelectedGoblin.SetQGoblin(true);

			Debug.Log("SwitchToQGoblin: Does current selected goblin have the ball? " + currentSelectedGoblin.doesCharacterHaveBall.ToString());
			if (currentSelectedGoblin.doesCharacterHaveBall)
			{

				//ChangeBallHandler(currentSelectedGoblin, currentQGoblin);
				//ThrowBallToGoblin(currentSelectedGoblin, currentQGoblin);
				IEnumerator stopSelectedGoblin = currentSelectedGoblin.CantMove();
				IEnumerator stopQGoblin = currentQGoblin.CantMove();
				StartCoroutine(stopSelectedGoblin);
				StartCoroutine(stopQGoblin);
				currentSelectedGoblin.ThrowBall(currentQGoblin);
				IEnumerator stopSwitch = PreventGoblinSwitch();
				StartCoroutine(stopSwitch);
				//FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
				TutorialManager.instance.FirstThrownPass();
			}
			else
			{
				/*if (!this.isSinglePlayer)
					FollowSelectedGoblin(selectGoblin.transform);
				else if (this.isSinglePlayer && this.isLocalPlayer)
					FollowSelectedGoblin(selectGoblin.transform);*/
				FollowSelectedGoblin(selectGoblin.transform);
			}
			Debug.Log("SwitchToQGoblin switching to goblin: " + selectGoblin.name);
			//CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);
		}
		
	}
    public void SwitchToEGoblin()
    {
		Debug.Log("SwitchToEGoblin: " + canSwitchGoblin.ToString());
		TutorialManager.instance.PlayerPressedQE();
		if ((canSwitchGoblin && !selectGoblin.isKicking && !selectGoblin.isDiving) || eGoblin.doesCharacterHaveBall)
		{
			if (doesTeamHaveBall && !eGoblin.canGoblinReceivePass)
				return;
			TutorialGoblinScript currentSelectedGoblin = selectGoblin;
			TutorialGoblinScript currentEGoblin = eGoblin;

			currentSelectedGoblin.UnSelectThisCharacter();
			currentEGoblin.SelectThisCharacter();

			selectGoblin = currentEGoblin;
			eGoblin = currentSelectedGoblin;

			currentEGoblin.SetEGoblin(false);
			currentSelectedGoblin.SetEGoblin(true);

			Debug.Log("SwitchToEGoblin: Does current selected goblin have the ball? " + currentSelectedGoblin.doesCharacterHaveBall.ToString());
			if (currentSelectedGoblin.doesCharacterHaveBall)
			{

				IEnumerator stopSelectedGoblin = currentSelectedGoblin.CantMove();
				IEnumerator stopEGoblin = currentEGoblin.CantMove();
				StartCoroutine(stopSelectedGoblin);
				StartCoroutine(stopEGoblin);
				currentSelectedGoblin.ThrowBall(currentEGoblin);
				IEnumerator stopSwitch = PreventGoblinSwitch();
				StartCoroutine(stopSwitch);
				//FollowSelectedGoblin(GameObject.FindGameObjectWithTag("football").transform);
				TutorialManager.instance.FirstThrownPass();
			}
			else
			{
				/*if (!this.isSinglePlayer)
					FollowSelectedGoblin(selectGoblin.transform);
				else if (this.isSinglePlayer && this.isLocalPlayer)
					FollowSelectedGoblin(selectGoblin.transform);*/
				FollowSelectedGoblin(selectGoblin.transform);
			}
			Debug.Log("SwitchToEGoblin switching to goblin: " + selectGoblin.name);
			//CmdSetSelectedGoblinOnServer(selectGoblin.GetComponent<NetworkIdentity>().netId);
		}
	}
	public IEnumerator PreventGoblinSwitch()
	{
		canSwitchGoblin = false;
		yield return new WaitForSeconds(0.2f);
		canSwitchGoblin = true;
	}
	public void ReportPlayerSpawnedFootball()
	{
		if (football == null)
		{
			football = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
		}
	}
	public void EnableQESwitchingControls(bool activate)
	{
		Debug.Log("EnableQESwitchingControls: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!qeSwitchingEnabled)
			{
				Debug.Log("EnableQESwitchingControls: enabling the controls.");
				InputManager.Controls.QESwitchGoblins.Enable();
				qeSwitchingEnabled = true;
			}

		}
		else
		{
			Debug.Log("EnableQESwitchingControls: DISABLING the controls.");
			InputManager.Controls.QESwitchGoblins.Disable();
			qeSwitchingEnabled = false;
		}
	}
	public void EnableGoblinMovement(bool enableOrDisable)
	{
		if (enableOrDisable)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!goblinMovementEnabled)
			{
				foreach (TutorialGoblinScript goblin in goblinTeam)
				{
					goblin.EnableGoblinMovement(enableOrDisable);
				}
				goblinMovementEnabled = true;
			}
		}
		else
		{
			foreach (TutorialGoblinScript goblin in goblinTeam)
			{
				goblin.EnableGoblinMovement(enableOrDisable);
			}
			goblinMovementEnabled = false;
		}
	}
	public void EnableBlockingControls()
	{
		InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
		InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();
		blockingControlsOnServer = true;
		blockingControlsEnabled = true;
	}
	public void ActivateBlockingControls(bool activate)
	{
		Debug.Log("ActivateBlockingControls: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!blockingControlsEnabled)
			{
				InputManager.Controls.Player.Block.Enable();
				//InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
				//InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
				blockingControlsEnabled = true;
			}
		}
		else
		{
			InputManager.Controls.Player.Block.Disable();
			blockingControlsEnabled = false;
		}
	}
	void StartBlockGoblin()
	{
		Debug.Log("StartBlockGoblin");
		if (selectGoblin)
		{
			if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isGoblinKnockedOut && !selectGoblin.isPunching)
			{
				selectGoblin.StartBlocking();
				TutorialManager.instance.PlayerBlocked();

				try
				{
					GameObject cowboy = GameObject.FindGameObjectWithTag("cowboy");
					if (cowboy != null)
					{
						if (cowboy.GetComponent<CowboyScript>().isVisibileToClient)
						{
							//TryToGiveCowboyYeehaw();
						}
						else
							Debug.Log("StartBlockGoblin: Cowboy is not visible on the client");
					}

				}
				catch (Exception e)
				{
					Debug.Log("StartBlockGoblin: Error trying to find the cowboy object. Error: " + e);
				}
			}
		}
	}
	void StopBlockGoblin()
	{
		Debug.Log("StopBlockGoblin");
		if (selectGoblin)
		{
			if (!selectGoblin.isSliding && !selectGoblin.isDiving)
			{
				selectGoblin.StopBlocking();
			}
		}
	}
	void StartKickPower()
	{
		Debug.Log("StartKickPower");
		if (selectGoblin.doesCharacterHaveBall)
			selectGoblin.StartKickPower();
	}
	void EndKickPower()
	{
		Debug.Log("EndKickPower");
		if (selectGoblin.doesCharacterHaveBall)
			selectGoblin.StopKickPower();
	}
	public void ActivateKickingControls(bool activate)
	{
		Debug.Log("ActivateKickingControls: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!kickingControlsEnabled)
			{
				InputManager.Controls.Player.KickFootball.Enable();
				//InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
				//InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
				kickingControlsEnabled = true;
			}
		}
		else
		{
			InputManager.Controls.Player.KickFootball.Disable();
			kickingControlsEnabled = false;
		}
	}
	public void EnableAttackControls()
	{
		InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
		attackControlsEnabled = true;
		attackContolsOnServer = true;
	}
	void GoblinAttack()
	{
		if (selectGoblin)
		{
			if (!selectGoblin.doesCharacterHaveBall && !selectGoblin.isPunching)
			{
				selectGoblin.Attack();
			}
		}
	}
	public void EnableGamplayActions(bool activate)
	{
		Debug.Log("EnableGameplayActions: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!gameplayActionsEnabled)
			{
				Debug.Log("EnableGameplayActions: controls enabled for player " + this.name);
				InputManager.Controls.Player.Slide.Enable();
				InputManager.Controls.Player.Dive.Enable();
				InputManager.Controls.Player.Block.Enable();
				InputManager.Controls.Player.Attack.Enable();

				/*InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
                InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
                InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
                InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
                InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();*/
				//gameplayActionsEnabled = true;
			}

		}
		else
		{
			InputManager.Controls.Player.Slide.Disable();
			InputManager.Controls.Player.Dive.Disable();
			InputManager.Controls.Player.Block.Disable();
			InputManager.Controls.Player.Attack.Disable();
			gameplayActionsEnabled = false;
		}
	}
	public void ActivateAttackControls(bool activate)
	{
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			InputManager.Controls.Player.Attack.Enable();
			//InputManager.Controls.Player.KickFootball.performed += _ => StartKickPower();
			//InputManager.Controls.Player.KickFootball.canceled += _ => EndKickPower();
			attackControlsEnabled = true;
		}
		else
		{
			InputManager.Controls.Player.Attack.Disable();
			attackControlsEnabled = false;
		}
	}
	public void EnableSlideControls()
	{
		InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
	}
	public void ActivateSlideControls(bool activate)
	{
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			InputManager.Controls.Player.Slide.Enable();
		}
		else
		{
			InputManager.Controls.Player.Slide.Disable();
		}
	}
	void SlideGoblin()
	{
		Debug.Log("SlideGoblin");
		if (selectGoblin)
		{
			if (!selectGoblin.doesCharacterHaveBall && !selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isPunching)
			{
				selectGoblin.SlideGoblin();
			}
		}
	}
	public void HandlePossessionBonus(float oldValue, float newValue)
	{
		possessionBonus = newValue;
		//UpdatePossessionSpeedBonusForGoblinTeam(newValue);
	}
	public void HandlePossessionPoints(float oldValue, float newValue)
	{
		possessionPoints = newValue;
	}
	public void UsePowerUp(int powerUpNumber)
	{
		Debug.Log("UsePowerUp: Player is going to use power up: " + powerUpNumber.ToString());
		if (powerUpNumber < myPowerUps.Count && powerUpNumber >= 0)
		{
			Debug.Log("UsePowerUp: Player is able to use power up: " + powerUpNumber.ToString());

			try
			{
				//uint powerUpNetId = myPowerUps[powerUpNumber].GetComponent<NetworkIdentity>().netId;
				//CmdUsePowerUp(powerUpNetId);
				TutorialManager.instance.PlayerUsedPowerUp();
				TutorialManager.instance.UsePowerUp(myPowerUps[powerUpNumber]);
			}
			catch (Exception e)
			{
				Debug.Log("UsePowerUp: Player tried to get network id of a non-existent powerup: " + e);
			}

			
		}
	}
	public void PowerUpPickedUp(TutorialPowerUp powerUptoAdd)
	{

		Debug.Log("PowerUpPickedUp: " + this.name + " to pick up power up " + powerUptoAdd.gameObject.name);
		
		myPowerUps.Add(powerUptoAdd);

		//
		// Update TutorialManager to do this???
		//
		//
		//
		TutorialManager.instance.UpdatePowerUpUIImages(myPowerUps);
		TutorialManager.instance.PlayerPickedUpPowerUp();
		SoundManager.instance.PlaySound("pickup-powerup", 0.75f);
	}
	public void RemoveUsedPowerUps()
	{

		// Make sure to remove "null" powerups? This appears to be an issue sometimes where the server destroys the powerup but it isn't removed from the player's list of powerups? Causing it to still be displayed in the UI
		try
		{
			myPowerUps = myPowerUps.Where(item => item != null).ToList();
		}
		catch (Exception e)
		{
			Debug.Log("RemoveUsedPowerUps: Failed to find and remove NULL powerups from myPowerUps list. Error: " + e);
		}

		//
		// Update TutorialManager to do this???
		TutorialManager.instance.UpdatePowerUpUIImages(myPowerUps);
		//
		//
	}
	public void UpdatePowerUpRemainingUses()
	{
		//
		// Update tutorial manager to do this?
		//PowerUpManager.instance.UpdatePowerUpUIImages(myPowerUps);
		//
		//
	}
	public void EnablePowerUpControls()
	{
		InputManager.Controls.PowerUps.PowerUp1.performed += _ => UsePowerUp(0);
		InputManager.Controls.PowerUps.PowerUp2.performed += _ => UsePowerUp(1);
		InputManager.Controls.PowerUps.PowerUp3.performed += _ => UsePowerUp(2);
		InputManager.Controls.PowerUps.PowerUp4.performed += _ => UsePowerUp(3);
		powerUpsEnabled = true;
	}
	public void ActivatePowerUpControls(bool activate)
	{
		Debug.Log("EnablePowerUpControls: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}

			if (!powerUpsEnabled)
			{
				var uiModule = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
				//uiModule.move = moveReference;
				//uiModule.submit = submitReference;
				uiModule.move = InputActionReference.Create(InputManager.Controls.SelectPowerUps.SelectLeftOrRightComposite);
				uiModule.submit = InputActionReference.Create(InputManager.Controls.SelectPowerUps.SubmitSelection);
				uiModule.enabled = true;
				var eventSystem = EventSystem.current;
				eventSystem.enabled = true;

				InputManager.Controls.PowerUps.Enable();
				InputManager.Controls.SelectPowerUps.Enable();
				powerUpsEnabled = true;
			}
		}
		else
		{
			InputManager.Controls.PowerUps.Disable();
			InputManager.Controls.SelectPowerUps.Disable();
			powerUpsEnabled = false;
		}
	}
	public void EnableGameplayActions(bool activate)
	{
		Debug.Log("EnableGameplayActions: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!gameplayActionsEnabled)
			{
				Debug.Log("EnableGameplayActions: controls enabled for player " + this.name);
				InputManager.Controls.Player.Slide.Enable();
				InputManager.Controls.Player.Dive.Enable();
				InputManager.Controls.Player.Block.Enable();
				InputManager.Controls.Player.Attack.Enable();

				/*InputManager.Controls.Player.Attack.performed += _ => GoblinAttack();
                InputManager.Controls.Player.Slide.performed += _ => SlideGoblin();
                InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
                InputManager.Controls.Player.Block.performed += _ => StartBlockGoblin();
                InputManager.Controls.Player.Block.canceled += _ => StopBlockGoblin();*/
				//gameplayActionsEnabled = true;
			}

		}
		else
		{
			InputManager.Controls.Player.Slide.Disable();
			InputManager.Controls.Player.Dive.Disable();
			InputManager.Controls.Player.Block.Disable();
			InputManager.Controls.Player.Attack.Disable();
			gameplayActionsEnabled = false;
		}
	}
	public void EnableDiveAction()
	{
		InputManager.Controls.Player.Dive.performed += _ => DiveGoblin();
	}
	void DiveGoblin()
	{
		Debug.Log("DiveGoblin");
		if (selectGoblin)
		{
			if (!selectGoblin.isSliding && !selectGoblin.isDiving && !selectGoblin.isPunching)
			{
				selectGoblin.DiveGoblin();
			}
		}
	}
	public void RepositionForKickAfter(bool isKickingPlayer, TutorialGoblinScript scoringGoblin, float yPosition)
	{
		if (!areGoblinsRepositionedForKickAfter)
		{
			Debug.Log("RpcRepositionForKickAfter: " + isKickingPlayer.ToString() + " for player: " + this.name + " y position: " + yPosition.ToString());
			if (isKickingPlayer)
			{
				Vector3 kickingPosition = scoringGoblin.transform.position;
				if (this.isTeamGrey)
				{
					//kickingPosition.x = -30f;
					kickingPosition.x = -40f;
				}
				else
				{
					//kickingPosition.x = 30f;
					kickingPosition.x = 40f;
				}
				kickingPosition.y = yPosition;
				scoringGoblin.transform.position = kickingPosition;

				int yPositionModifier = 1;
				foreach (TutorialGoblinScript goblin in goblinTeam)
				{
					goblin.CheckIfGoblinNeedsToFlipForKickAfter(isKickingPlayer);
					if (goblin == scoringGoblin)
					{
						if (goblin.isCharacterSelected)
							FollowSelectedGoblin(goblin.transform);
						else if (goblin.isQGoblin)
							SwitchToQGoblin();
						else if (goblin.isEGoblin)
							SwitchToEGoblin();

						CmdRequestFootballForKickOffGoblin(goblin);
						goblin.ActivateKickAfterAccuracyBar(isKickingPlayer);
						goblin.UpdateHasGoblinRepositionedForKickAfter();
						continue;
					}
					yPositionModifier *= -1;
					Vector3 newPosition = goblin.transform.position;
					newPosition.x = 0f;
					newPosition.y = 3 * yPositionModifier;
					goblin.transform.position = newPosition;
					goblin.UpdateHasGoblinRepositionedForKickAfter();
				}
			}
			else
			{
				int yPositionModifier = 1;
				foreach (TutorialGoblinScript goblin in goblinTeam)
				{
					goblin.CheckIfGoblinNeedsToFlipForKickAfter(isKickingPlayer);
					Vector3 newPosition = goblin.transform.position;
					if (goblin.goblinType.Contains("skirmisher"))
					{
						Debug.Log("RpcRepositionForKickAfter: skirmisher goblin found: " + goblin.name + " " + goblin.goblinType);
						newPosition.y = yPosition;
						if (this.isTeamGrey)
						{
							//newPosition.x = 41f;
							newPosition.x = 52;
						}
						else
						{
							//newPosition.x = -41f;
							newPosition.x = -52f;
						}
						goblin.transform.position = newPosition;

						if (goblin.isCharacterSelected || selectGoblin == goblin)
							FollowSelectedGoblin(goblin.transform);
						else if (goblin.isQGoblin)
							SwitchToQGoblin();
						else if (goblin.isEGoblin)
							SwitchToEGoblin();
						goblin.UpdateHasGoblinRepositionedForKickAfter();
						continue;
					}
					yPositionModifier *= -1;

					if (this.isTeamGrey)
					{
						//newPosition.x = 43.5f;
						newPosition.x = 55f;
					}
					else
					{
						newPosition.x = -43.5f;
						newPosition.x = -55f;
					}
					newPosition.y = 3 * yPositionModifier;
					goblin.transform.position = newPosition;
					goblin.UpdateHasGoblinRepositionedForKickAfter();
				}
			}

			//EnableKickAfterPositioning(isKickingPlayer);
			//CmdKickAfterPositioningControlsOnServerValues(isKickingPlayer);
			//GameplayManager.instance.ActivateKickAfterPositionControlsPanel(isKickingPlayer);
			areGoblinsRepositionedForKickAfter = true;
		}
	}
	void CmdRequestFootballForKickOffGoblin(TutorialGoblinScript goblin)
	{
		if (!goblin.doesCharacterHaveBall)
		{
			TutorialFootball football = GameObject.FindGameObjectWithTag("football").GetComponent<TutorialFootball>();
			football.MoveFootballToKickoffGoblin(goblin);
		}
	}
	public void EnableKickAfterPositioningControls()
	{
		InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
		InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
		InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
		InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
		kickAfterPositioningEnabled = true;
		kickAfterKickingControlsOnServer = true;
	}
	public void EnableKickAfterSubmissionControls()
	{
		InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();
	}
	void KickAfterPositioningMove(bool moveLeft)
	{
		Debug.Log("KickAfterPositioningMove: left: " + moveLeft.ToString());
		selectGoblin.KickAfterRepositioning(moveLeft);
		TutorialManager.instance.PlayerChangedKickAfterPosition();
	}
	void KickAfterPositioningStop()
	{
		Debug.Log("KickAfterPositioningStop");
		selectGoblin.EndKickAfterRepositioning();
	}
	void SubmitKickAfterPositionToServer()
	{
		Debug.Log("TutorialPlayer.cs: SubmitKickAfterPositionToServer");
		selectGoblin.SubmitKickAfterPositionToServer();
	}
	public void EnableKickAfterPositioning(bool activate)
	{
		Debug.Log("EnableKickAfterPositioning: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!kickAfterPositioningEnabled)
			{
				InputManager.Controls.KickAfterPositioning.Enable();

				/*InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();*/

				kickAfterPositioningEnabled = true;
			}
		}
		else
		{
			InputManager.Controls.KickAfterPositioning.Disable();
			kickAfterPositioningEnabled = false;
		}
	}
	public void EnableKickAfterKicking(bool activate)
	{
		Debug.Log("EnableKickAfterKicking: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			try
			{
				if (TutorialEscMenuManager.instance.isEscMenuOpen)
					return;
			}
			catch (Exception e)
			{
				Debug.Log("Could not access TutorialEscMenuManager instance. Error: " + e);
			}
			if (!kickAfterKickingEnabled)
			{
				InputManager.Controls.KickAfterKicking.Enable();

				/*InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.performed += _ => KickAfterPositioningMove(true);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningLeft.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.performed += _ => KickAfterPositioningMove(false);
                InputManager.Controls.KickAfterPositioning.KickAfterPositioningRight.canceled += _ => KickAfterPositioningStop();
                InputManager.Controls.KickAfterPositioning.SubmitPosition.performed += _ => SubmitKickAfterPositionToServer();*/

				kickAfterKickingEnabled = true;
			}
		}
		else
		{
			InputManager.Controls.KickAfterKicking.Disable();
			kickAfterKickingEnabled = false;
		}
	}
	public void StartKickAfterAttempt()
	{
		selectGoblin.StartKickAfterKickAttempt();
	}
	public void EnableKickAfterKickingControls()
	{
		InputManager.Controls.KickAfterKicking.KickAfterSubmit.performed += _ => SubmitKickAfterKicking();
	}
	void SubmitKickAfterKicking()
	{
		Debug.Log("SubmitKickAfterKicking");
		selectGoblin.SubmitKickAfterKicking();
	}
	public void PauseGamePlayer()
	{
		TutorialManager.instance.PauseGameOnServer();
	}
	public void ResumeGamePlayer()
	{
		TutorialManager.instance.ResumeGameOnServer();
	}
	public void EnableMenuNavigationControls(bool activate)
	{
		Debug.Log("EnableMenuNavigationControls: for player " + this.name + " " + activate.ToString());
		if (activate)
		{
			if (!menuNavigationEnabled)
			{


				var uiModule = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
				//uiModule.move = moveReference;
				//uiModule.submit = submitReference;
				uiModule.move = InputActionReference.Create(InputManager.Controls.UI.Navigate);
				uiModule.submit = InputActionReference.Create(InputManager.Controls.UI.Submit);
				uiModule.enabled = true;
				var eventSystem = EventSystem.current;
				eventSystem.enabled = true;
				InputManager.Controls.UI.Enable();
				menuNavigationEnabled = true;
				//Debug.Log("EnableMenuNavigationControls: Is move enabled: " + moveReference.action.enabled.ToString() + " is submit enabled " + submitReference.action.enabled.ToString() + " is the UI controls enabled?: " + InputManager.Controls.UI.enabled.ToString());
				/*GameObject mainMenuButton = GameObject.FindGameObjectWithTag("mainMenuButton");
                var eventSystem = EventSystem.current;
                eventSystem.SetSelectedGameObject(mainMenuButton, new BaseEventData(eventSystem));*/
			}
		}
		else
		{
			InputManager.Controls.UI.Disable();
			menuNavigationEnabled = false;
		}
	}
}
