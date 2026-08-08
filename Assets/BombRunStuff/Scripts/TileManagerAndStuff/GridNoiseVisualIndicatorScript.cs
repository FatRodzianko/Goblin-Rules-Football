using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNoiseVisualIndicatorScript : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private NoiseVisualObjectPool _noiseVisualObjectPool;

    [Header("Sprites/animation")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<Sprite> _sprites = new List<Sprite>();
    private float _animationDelay = 0.025f;
    private Action<GameObject> _animationEnded;

    public void AnimationEnded()
    {
        _noiseVisualObjectPool.ReleaseObject(this.gameObject);
        //Destroy(this.gameObject);
    }
    public void InitializeNoiseVisual(NoiseVisualObjectPool pool, float animationDelay, Action<GameObject> completed)
    {
        _noiseVisualObjectPool = pool;
        _animationDelay = animationDelay;
        this._animationEnded = completed;
    }
    public void StartAnimation()
    {
        //_animator.Play("NoiseShake");
        StartCoroutine(AnimateNoiseTile());
    }
    IEnumerator AnimateNoiseTile()
    {
        if (_sprites.Count < 1)
            yield break;
        float frameCount = _sprites.Count;
        this._spriteRenderer.color = new Color(1f, 1f, 1f, 0.33f);
        for (int i = 0; i < _sprites.Count; i++)
        {
            this._spriteRenderer.sprite = _sprites[i];
            //this._spriteRenderer.color = new Color(1f, 1f, 1f, 0.33f * ((frameCount - i) / frameCount));
            Color newColor = this._spriteRenderer.color;
            newColor.a = newColor.a * (1 - (1 / frameCount));
            this._spriteRenderer.color = newColor;
            //Debug.Log("AnimateNoiseTile: Setting alpha to: " + newColor.a + ":" + this._spriteRenderer.color.a + " for frame: " + i + ". Sprite color: " + this._spriteRenderer.color);
            yield return new WaitForSeconds(_animationDelay / (frameCount / 4));
        }
        //AnimationEnded();
        _animationEnded(this.gameObject);
    }
}
