using System.Collections.Generic;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        public List<GameObject> activeSins;
        public List<GameObject> potentialSins;
        public int remainingSin;
        public int winThreshold;//what is the maximum amount of sin that can remain and you still win


        //prefabs
        public GameObject sinPrefab;
        public GameObject potentialSinPrefab;
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
            foreach (GameObject sin in activeSins)
            {
                remainingSin += sin.GetComponent<Sin>().weight;
            }
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            Debug.Log("The total amount of sin in the game is " + remainingSin);
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
    }
}

