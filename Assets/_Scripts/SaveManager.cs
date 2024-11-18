using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Scripts;
using _Scripts.Player;
using UnityEngine;

//Used to run persistance functions, setup and cleanup functions, and keep track of saved data
public class SaveManager : MonoBehaviour
{
    
    /*
     * The plan:
     * 1. We should have a Json file that is stored in this function
     * 2. Die and EscapeLevel should both run, and then delegate to Cleanup
     * 3. Cleanup should grab all relevant data, and save it in the JSON file, overwriting what was previously there
     * 4. Setup should read the JSON file and set all data in the scene based on the save file
     * 5. We should also have a seperate JSON file for default save, which will be the first thing that will be loaded in
     * 
     */

    //public File SaveFile; //JSON file 
    
    
    #region Singleton

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(SaveManager)) as SaveManager;

            return _instance;
        }
        set { _instance = value; }
    }

    private static SaveManager _instance;

    #endregion
    
    public void Die()
    {
        /*
         * The plan:
         * 1. Redistribute all sin that has been accumulated in the level
         *      a: this should be sin held + a sin penalty + sin comitted
         *      b: Break this down into several reasonably sized portions
         *      c: instantiate each of these portions into a new sin from the potential sins
         * 2. Reset the player's sin held and sin commited to 0
         * 3. Call cleanup, possibly pass in a cutscene to transition to
         */
        return;
    }

    public void EscapeLevel()
    {
        /*
         * The plan:
         * 1. reset player sin held to 0 (release the sin they collected)
         * 2. Run check to see if you have won the game. If they have:
         *      a: delete their saved data
         *      b: set there saved data to be the default JSON
         *      c: transition to credits
         * 3. Call cleanup, possibly pass in a cutscene to transition to
         * 
         */
        
        //release all of the sin you hold
        PlayerVariables.Instance.sinHeld = 0;
        GameManager.Instance.checkForGameComplete(PlayerVariables.Instance.sinAccrued);
        Cleanup();
        return;
    }

    public void Cleanup()
    {
        /*
         * The plan:
         * 1. get access to the JSON save file
         * 2. Grab hold of the list of active sins, and the list of potential sins, including
         *      a: active sin weight
         *      b: active sin positon
         *      c: potential sin position
         * 3. Get the Player Controller's stats, including
         *      a: sin held
         *      b: sin committed
         *      c: sin threshold
         * 4. Parse all of these into JSON format
         * 5. Overwrite the JSON save file with the new information
         * 6. If we end up having a cutscene, transition to the cutscene that was passed in
         * 7. If we don't have a cutscene, transition to the main menu
         */
        Debug.Log("Escape level, do not complete game");
        
        SaveData saveData = new SaveData();
        
        //grab all sins
        List<SinData> sins = new List<SinData>();//the data we'll put in the save object
        List<GameObject> sinObjects = GameManager.Instance.activeSins;
        
        foreach (GameObject sinObject in sinObjects)
        {
            sins.Add(SinToData(sinObject));
        }
        saveData.Sins = sins;
        
        //grab potential sins
        List<Vector3> potentialSins = new List<Vector3>();
        List<GameObject> potentialSinObjects = GameManager.Instance.potentialSins;

        foreach (GameObject potentialSinObject in potentialSinObjects)
        {
            potentialSins.Add(PotentialSinToVector3(potentialSinObject));
        }
        saveData.PotentialSins = potentialSins;
        
        //add the player's data
        saveData.playerData = FetchPlayerData();
        
        //TODO eventually we will want to save the one time triggers here as well
        
        //save the player's data as a JSON file
        string SaveString = SaveDataToJson(saveData);
        Debug.Log(SaveString);
        
        File.WriteAllText("Assets/_Scripts/Saves/Save.txt", SaveString);
        return;
    }

    public void Setup()
    {
        /*
         * The plan:
         * 1. Get access to the JSON save file
         * 2. Instantiate all active sins with their weights at their positions
         * 2.5. We will need to create an "instantiate sin" function that chooses the correct prefab based on weight.
         * 3. Instantiate all potential sins at their positions
         * 4. Get list of all active sins, put this into the game manager
         * 5. Get list of all potential sins, put this into the game manager
         * 6. Instantiate the player object with the correct stats
         * 7. Calculate total amount of sin in level (call function in Game Manager)
         * 8. Set the sin UI meters to the correct levels
         */
        return;
    }

    #region settingSins

    private SinData SinToData(GameObject sin)
    {
        SinData sinData = new SinData();
        sinData.Weight = sin.GetComponent<Sin>().weight;
        sinData.Position = sin.transform.position;
        return sinData;
    }

    private void DataToSin(SinData sinData)
    {
        GameManager.Instance.InstantiateSin(sinData.Weight,sinData.Position);
    }

    private Vector3 PotentialSinToVector3(GameObject potentialSin)
    {
        return potentialSin.transform.position;
    }
    
    //we don't need a Vector3ToPotentialSin because we can just call the function in GameManager
    #endregion
    
    #region settingPlayer
    

    private PlayerData FetchPlayerData()
    {
        PlayerData playerData = new PlayerData();
        PlayerVariables player = PlayerVariables.Instance;

        playerData.SinHeld = player.sinHeld;
        playerData.SinAccrued = player.sinAccrued;
        playerData.SinThreshold = player.sinThreshold;
        
        return playerData;
    }

    private void SetPlayerData(PlayerData playerData)
    {
        PlayerVariables player = PlayerVariables.Instance;
        player.sinHeld = playerData.SinHeld;
        player.sinAccrued = playerData.SinAccrued;
        player.sinThreshold = playerData.SinThreshold;
    }

    #endregion

    private string SaveDataToJson(SaveData saveData)
    {
        String Json = "{";
        
        //adding the sins
        Json+= ("\"Sins\": [");

        
        //putting a comma between every instance of a sin
        if (saveData.Sins.Count > 0)
        {
            Json += (JsonUtility.ToJson(saveData.Sins[0]));
        }
        for (int i = 1; i < saveData.Sins.Count; i++)
        {
            Json += (",");
            Json += (JsonUtility.ToJson(saveData.Sins[i]));
            
        }
        Json+= ("]");
        
        //adding the potential sins

        Json += ", \"PotentialSins\": [";
        if (saveData.PotentialSins.Count > 0)
        {
            Json += (JsonUtility.ToJson(saveData.PotentialSins[0]));
        }
        
        for (int i = 1; i < saveData.PotentialSins.Count; i++)
        {
            Json += (",");
            Json += (JsonUtility.ToJson(saveData.PotentialSins[i]));
        }

        Json += "]";
        
        //adding the player stats
        Json += ", \"PlayerData\": ";
        
        Json += (JsonUtility.ToJson(saveData.playerData));

        Json += "}";
        //Debug.Log(Json);
        return Json;
    }
    
    private class SaveData
    {
        public List<SinData> Sins;
        public List<Vector3> PotentialSins;
        public PlayerData playerData;
    }

    private class SinData
    {
        public int Weight;
        public Vector3 Position;
    }

    private class PlayerData
    {
        public int SinHeld;
        public int SinAccrued;
        public int SinThreshold;
    }
}


