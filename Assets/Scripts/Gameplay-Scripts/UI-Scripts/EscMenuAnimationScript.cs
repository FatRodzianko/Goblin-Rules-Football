using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using TMPro;

public class EscMenuAnimationScript : MonoBehaviour
{
    public Sprite[] sprites;
    public int spritePerFrameUnroll = 6;
    public int spritePerFrameReroll = 4;
    public bool loop = false;
    public bool destroyOnEnd = false;
    public bool unroll = false;
    public bool reroll = false;
	public float lastFrameTime = 0f;


    public int index = 0;
    [SerializeField] private Image image;
    private int frame = 0;

    [Header("UI Objects")]
	[SerializeField] private GameObject EscMenuCanvas;
	[SerializeField] GameObject[] EscapeMenuObjects;
	[SerializeField] GameObject ParentPanel;
	[SerializeField] GameObject FirstButton;
	[SerializeField] Button PauseGameButton;

	void Awake()
    {
        image = GetComponent<Image>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
				ActivateEscMenuObjects(true);
				unroll = false;
			}
		}
		else if (reroll)
		{
			ActivateEscMenuObjects(false);
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
				EscMenuCanvas.SetActive(false);
			}
		}
	}
	void FixedUpdate()
	{
		/*if (!loop && index == sprites.Length) return;
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
				ActivateEscMenuObjects(true);
				unroll = false;
			}
		}
		else if (reroll)
		{
			ActivateEscMenuObjects(false);
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
				EscMenuCanvas.SetActive(false);
			}
		}*/

	}
	void ActivateEscMenuObjects(bool activate)
	{
		Debug.Log("ActivateEscMenuObjects: " + activate.ToString());
		foreach (GameObject gameObject in EscapeMenuObjects)
			gameObject.SetActive(activate);
		if (!GameplayManager.instance.isSinglePlayer)
		{
			if (GameplayManager.instance.isGamePaused)
				PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Resume Game";
			else
				PauseGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause Game";
			PauseGameButton.gameObject.SetActive(activate);

		}	
		else
			PauseGameButton.gameObject.SetActive(false);
		if (activate)
		{
			var eventSystem = EventSystem.current;
			eventSystem.SetSelectedGameObject(FirstButton, new BaseEventData(eventSystem));
			eventSystem.firstSelectedGameObject = FirstButton;
		}
	}
	public void ReRollScroll()
	{
		if (!reroll)
		{
			index = sprites.Length - 1;
			unroll = false;
			reroll = true;
		}
	}
}
