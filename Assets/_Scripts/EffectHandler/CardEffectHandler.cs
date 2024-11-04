using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectHandler : MonoBehaviour
{

    public GameObject pinkPoof;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportEffect(Vector2 position)
    {
        Instantiate(pinkPoof, position, Quaternion.identity);
    }
}
