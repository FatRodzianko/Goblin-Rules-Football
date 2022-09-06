using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCircle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Goblin")
        {
            TutorialManager.instance.PlayerIsInCircle();
            //Destroy(this.gameObject);
            StartCoroutine(DestroyCircle());
        }
    }
    IEnumerator DestroyCircle()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Destroy(this.gameObject);
    }
}
