using System.Collections;
using System.Collections.Generic;
using _Scripts.Player;
using UnityEngine;

public class Sin : MonoBehaviour
{
    // Start is called before the first frame update

    public int weight;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerVariables>().collectSin(this.weight);
            Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
            Destroy(gameObject);
        }
    }
}
