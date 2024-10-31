using System.Collections.Generic;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    
    
    public class GameManager : MonoBehaviour
    {
        public List<GameObject> activeSins;
        public List<GameObject> potentialSins;
        public int remainingSin;
        public int winThreshold;//what is the maximum amount of sin that can remain and you still win
        //public Scene credits;

        //prefabs
        public GameObject sinPrefab;
        public GameObject potentialSinPrefab;

        public GameObject quicktimeEventPanel;
        #region Singleton

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static GameManager _instance;

        #endregion
        
        public void Awake()
        {
            activeSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
            Debug.Log("Number of sins: " +activeSins.Count);
            calculateRemainingSin();
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            Debug.Log("The total amount of sin in the game is " + remainingSin);
        }

        private int calculateRemainingSin()
        {
            remainingSin = 0;
            foreach (GameObject sin in activeSins)
            {
                remainingSin += sin.GetComponent<Sin>().weight;
            }

            return remainingSin;
        }
        public void CollectSin(GameObject sin)
        {

            //deal with stats in GameManager

            int sinWeight = sin.GetComponent<Sin>().weight;
            remainingSin -= sinWeight;

            //deal with stats in Player
            PlayerVariables.Instance.CollectSin(sinWeight);

            //deal with prefabs and active/potential sin loop
            activeSins.Remove(sin);
            //TODO instantiate a new potential sin gameobject at the location of sin and add it to potentialSins
            GameObject newPotentialSin = Instantiate(potentialSinPrefab, sin.transform.position, Quaternion.identity);
            potentialSins.Add(newPotentialSin);
            Debug.Log("Number of potential sins: " + potentialSins.Count);


            //TODO have an event that is called to let all enemies know that a sin was collected
            //deal with win condition, later we will remove this and put it in a different script for finishing a level
            Debug.Log("Remaining sin " + remainingSin);
            if (remainingSin <= winThreshold)
            {
                Debug.Log("It is now possible to win");
            }


            //Everything is done, remove the sin object
            Destroy(sin);
        }

        public void releaseSin(int weight)
        {
            //making sure that if you have filled every possible potential sin location, we can return safely
            if (potentialSins.Count == 0)
            {
                Debug.Log("It is impossible to hold more sins");
                return;
            }
            
            //choosing the location
            int location = Random.Range(0, potentialSins.Count);
            
            //remove the old potential sin
            GameObject potentialSin = potentialSins[location];
            GameObject newSin = GameObject.Instantiate(sinPrefab, potentialSin.transform.position, Quaternion.identity);
            
            potentialSins.RemoveAt(location);
            Destroy(potentialSin);
            
            //create a new sin
            newSin.GetComponent<Sin>().weight = weight;
            activeSins.Add(newSin);

            remainingSin += weight;
            
            Debug.Log("Number of potential sins: " + potentialSins.Count);
            Debug.Log("Remaining sin " + remainingSin);
        }

        public bool checkForGameComplete(int modifier)
        {
            calculateRemainingSin();
            int trueSinCount = remainingSin + modifier;
            if (trueSinCount <= winThreshold)
            {
                
                Debug.Log("You win!");
                SceneManager.LoadScene("winScreenPlaytest1");
                return true;
               
            }

            return false;
        }
    }
}

