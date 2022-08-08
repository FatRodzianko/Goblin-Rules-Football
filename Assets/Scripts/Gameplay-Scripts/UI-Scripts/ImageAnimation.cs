using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class ImageAnimation : MonoBehaviour
{

	public Sprite[] sprites;
	public int spritePerFrameUnroll = 6;
	public int spritePerFrameReroll = 4;
	public bool loop = false;
	public bool destroyOnEnd = false;
	public bool unroll = false;
	public bool reroll = false;
	public bool halfTime = false;
	public bool endOfGame = false;
	public float lastFrameTime = 0f;

	public int index = 0;
	[SerializeField] private Image image;
	private int frame = 0;

	[Header("UI Objects")]
	[SerializeField] GameObject[] TheFinalScoreTextObjects;
	[SerializeField] GameObject[] HalfTimeTextObjects;
	[SerializeField] GameObject TextObjects;
	[SerializeField] GameObject HalfTimeObjects;
	[SerializeField] GameObject ParentPanel;
	[SerializeField] GameObject TeamWinnerText;
	[SerializeField] GameObject FirstButton;
	bool activateWinningText = false;
	[SerializeField] bool activateFirstButton = false;

	void Awake()
	{
		image = GetComponent<Image>();
	}
    private void Update()
    {
		if (Time.unscaledTime < (lastFrameTime + 0.02f))
			return;
		else
			lastFrameTime = Time.unscaledTime;

		if (!loop && index == sprites.Length) return;
		if (unroll)
		{
			frame++;
			if (frame < spritePerFrameUnroll) return;
			image.sprite = sprites[index];

			frame = 0;
			index++;
			if (index >= sprites.Length)
			{
				if (loop) index = 0;
				if (destroyOnEnd) Destroy(gameObject);
				ActivateTextEndOfGame(halfTime);
				unroll = false;
			}
		}
		else if (reroll)
		{
			frame++;
			if (frame < spritePerFrameReroll) return;
			frame = 0;
			index--;
			try
			{
				image.sprite = sprites[index];
			}
			catch (Exception e)
			{
				Debug.Log("ImageAnimation: " + e);
			}

			if (index <= 0)
			{
				if (loop) index = 0;
				if (destroyOnEnd) Destroy(gameObject);
				reroll = false;
				this.gameObject.SetActive(false);
				ParentPanel.SetActive(false);
			}
		}
	}
    /*void FixedUpdate()
	{
		if (!loop && index == sprites.Length) return;
		if (unroll)
		{
			frame++;
			if (frame < spritePerFrameUnroll) return;
			image.sprite = sprites[index];
			
			frame = 0;
			index++;
			if (index >= sprites.Length)
			{
				if (loop) index = 0;
				if (destroyOnEnd) Destroy(gameObject);
				ActivateTextEndOfGame(halfTime);
				unroll = false;
			}
		}
		else if (reroll)
		{
			frame++;
			if (frame < spritePerFrameReroll) return;			
			frame = 0;
			index--;
			try
			{
				image.sprite = sprites[index];
			}
			catch (Exception e)
            {
                Debug.Log("ImageAnimation: " + e);
            }
            
			if (index <= 0)
			{
				if (loop) index = 0;
				if (destroyOnEnd) Destroy(gameObject);
				reroll = false;
				this.gameObject.SetActive(false);
				ParentPanel.SetActive(false);
			}
		}
		
	}*/
    public void UnScrollHalfTime()
	{	
		if (!unroll)
		{
			Debug.Log("UnScrollHalfTime");
			halfTime = true;
			endOfGame = false;
			index = 0;
			reroll = false;
			unroll = true;
			if (!this.gameObject.activeInHierarchy)
				this.gameObject.SetActive(true);
			if (!ParentPanel.activeInHierarchy)
				ParentPanel.SetActive(true);
		}
		
	}
	public void UnScrollEndOfGame(string winnerOfGame)
	{
		if (!unroll)
		{
			Debug.Log("UnScrollEndOfGame");
			if (winnerOfGame != "draw")
				activateWinningText = true;
			else
				activateWinningText = false;
			halfTime = false;
			endOfGame = true;
			index = 0;
			reroll = false;
			unroll = true;
			if (!this.gameObject.activeInHierarchy)
				this.gameObject.SetActive(true);
			if (!ParentPanel.activeInHierarchy)
				ParentPanel.SetActive(true);
		}
		
	}
	public void Unscroll()
	{ 

	}
	public void ReRollScroll()
	{
		if (!reroll)
		{
			Debug.Log("ReRollScroll: half time: " + halfTime.ToString());
			DeActivateTextEndOfGame(halfTime);
			index = sprites.Length - 1;
			unroll = false;
			reroll = true;
		}
	}
	void ActivateTextEndOfGame(bool halfTime)
	{
		if (halfTime)
		{
			TextObjects.SetActive(true);
			//foreach (GameObject gameObject in HalfTimeTextObjects)
			//gameObject.SetActive(true);
			var eventSystem = EventSystem.current;
			if (activateFirstButton)
			{
				eventSystem.SetSelectedGameObject(FirstButton, new BaseEventData(eventSystem));
				eventSystem.firstSelectedGameObject = FirstButton;
				Debug.Log("ActivateTextEndOfGame: The current selected object in the eventsystem is: " + eventSystem.currentSelectedGameObject.name.ToString());
			}
		}
		else
		{
			TextObjects.SetActive(true);
			if(activateWinningText)
				TeamWinnerText.GetComponent<TouchDownTextGradient>().ActivateGradient();
			foreach (GameObject gameObject in TheFinalScoreTextObjects)
				gameObject.SetActive(true);
			// Reset the selected button to the main menu button?
			GameObject mainMenuButton = GameObject.FindGameObjectWithTag("mainMenuButton");
			var eventSystem = EventSystem.current;
			eventSystem.SetSelectedGameObject(mainMenuButton, new BaseEventData(eventSystem));
			eventSystem.firstSelectedGameObject = mainMenuButton;
			Debug.Log("ActivateTextEndOfGame: The current selected object in the eventsystem is: " + eventSystem.currentSelectedGameObject.name.ToString());
		}
	}
	void DeActivateTextEndOfGame(bool halfTime)
	{
		if (halfTime)
		{
			TextObjects.SetActive(false);
			/*foreach (GameObject gameObject in HalfTimeTextObjects)
				gameObject.SetActive(false);*/
		}
		else
		{
			TextObjects.SetActive(true);
			/*foreach (GameObject gameObject in TheFinalScoreTextObjects)
				gameObject.SetActive(false);*/
		}
	}
}