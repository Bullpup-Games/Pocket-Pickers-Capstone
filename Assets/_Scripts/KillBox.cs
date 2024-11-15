using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Card;
using _Scripts.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
public class KillBox : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask cardLayer;
    [SerializeField] private LayerMask enemyLayer;
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == playerLayer)
        {
            // TODO: Hook up to the cleanup handler when it is made
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        
        if (col.gameObject.layer == cardLayer)
        {
            Card.Instance.DestroyCard();
            return;
        }

        if (col.gameObject.layer == enemyLayer)
        {
            var enemy = col.gameObject.GetComponent<IEnemyStateManagerBase>();
            enemy.KillEnemy();
        }
        
    }
}
