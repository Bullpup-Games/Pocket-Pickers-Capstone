using System;
using System.Collections;
using _Scripts.Card;
using _Scripts.Enemies;
using _Scripts.Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class KillBox : MonoBehaviour
    {
        private int _environmentLayer;
        [SerializeField] private float overlapCheckRadius = 0.1f;

        private void Awake()
        {
            // Get the integer layer index of Environment layer
            _environmentLayer = LayerMask.NameToLayer("Environment");
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            // Check if an environment collider overlaps with this position
            // Construct a layer mask for the environment layer
            var environmentLayerMask = 1 << _environmentLayer;

            // Perform an overlap check at the killbox position
            var environmentHit = Physics2D.OverlapCircle(transform.position, overlapCheckRadius, environmentLayerMask);

            // If environment is detected overlapping, return and do not kill
            if (environmentHit != null)
                return;

            var colPos = col.gameObject.transform.position;
            var dir = colPos - gameObject.transform.position;
            var layerMaskForRaycast = 1 << _environmentLayer; // Raycast should check for environment layer
            var distance = Vector3.Distance(colPos, transform.position);
            var hit = Physics2D.Raycast(colPos, dir, distance, layerMaskForRaycast);

            if (hit.collider is not null) return;

            if (col.gameObject.CompareTag("Player"))
                StartCoroutine(WaitBeforeKill(col));

            if (col.gameObject.CompareTag("Card"))
                StartCoroutine(WaitBeforeKill(col));

            if (col.gameObject.CompareTag("enemy"))
                StartCoroutine(WaitBeforeKill(col));
        }
        
        private IEnumerator WaitBeforeKill(Collider2D col)
        {
            yield return new WaitForSeconds(0.15f);
            
            if (col is null) yield return null;

            try
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    // TODO: Hook up to the cleanup handler when it is made
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    LavaSoundManager.Instance.PlayLavaSizzleClip();
                    GameManager.Instance.Die();
                } 
            
                if (col.gameObject.CompareTag("Card"))
                {
                    if (Card.Card.Instance is not null && CardManager.Instance.cardPrefab is not null)
                    {
                        CardEffectHandler.Instance.DestroyEffect(Card.Card.Instance.transform.position);
                        LavaSoundManager.Instance.PlayLavaSizzleClip();
                        Card.Card.Instance.DestroyCard();
                    }
                } 
            
                if (col.gameObject.CompareTag("enemy"))
                {
                    var enemy = col.gameObject.GetComponent<IEnemyStateManagerBase>();
                    LavaSoundManager.Instance.PlayLavaSizzleClip();
                    enemy.KillEnemyWithoutGeneratingSin();
                }
            }
            catch (Exception e)
            {
                Debug.Log("CATCH!");
            }
        }
    }
}