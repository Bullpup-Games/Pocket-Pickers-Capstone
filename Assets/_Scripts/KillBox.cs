using System.Collections;
using _Scripts.Enemies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class KillBox : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
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

            if (col.gameObject.CompareTag("Player"))
            {
                // TODO: Hook up to the cleanup handler when it is made
                //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                GameManager.Instance.Die();
            } 
            
            if (col.gameObject.CompareTag("Card"))
            {
                if (CardEffectHandler.Instance is not null && Card.Card.Instance is not null)
                {
                    CardEffectHandler.Instance.DestroyEffect(Card.Card.Instance.transform.position);
                    Card.Card.Instance.DestroyCard();
                }
            } 
            
            if (col.gameObject.CompareTag("enemy"))
            {
                var enemy = col.gameObject.GetComponent<IEnemyStateManagerBase>();
                enemy.KillEnemyWithoutGeneratingSin();
            }
        }
    }
}