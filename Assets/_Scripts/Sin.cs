using UnityEngine;

namespace _Scripts
{
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
                Debug.Log("Collided with a sin");
                Debug.Log("Colision is with " + col.gameObject.name);
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());

                GameManager.Instance.CollectSin(gameObject);
            }
        }
    }
}