using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using _Scripts;
using _Scripts.Player;
//using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
//using System.Text.Json;
//using System.Text.Json.Serialization;



//Used to run persistance functions, setup and cleanup functions, and keep track of saved data
public class SaveManager : MonoBehaviour
{
    
    /*
     * The plan:
     * x 1. We should have a Json file that is stored in this function
     * x 2. Die and EscapeLevel should both run, and then delegate to Cleanup
     * 3. Cleanup should grab all relevant data, and save it in the JSON file, overwriting what was previously there
     * 4. Setup should read the JSON file and set all data in the scene based on the save file
     * 5. We should also have a seperate JSON file for default save, which will be the first thing that will be loaded in
     * 
     */

    //public File SaveFile; //JSON file 
    
    public string saveFilePath;// = Path.Combine(Application.persistentDataPath, "Save.txt"); 
    
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


    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/save.txt";
        if (!Directory.Exists(Application.persistentDataPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }
    }

    public void Cleanup()
    {
        
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
        
       //save the data
        try
        {
            Debug.Log(saveFilePath);
            using (StreamWriter sw = new StreamWriter(saveFilePath, false))
            {
                //Debug.Log("Writing " + saveTime + " to save copy file");
                sw.Write(SaveString);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during saving: " + ex.Message);
        }

        //TODO transition to a seperate scene passed in by the escape or die function
        //transition to the main menu scene
        SceneManager.LoadScene("MainMenuPlayTest1");
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

        //grab saved data from file
        string saveDataJson = "";
        try
        {
            using (StreamReader sr = new StreamReader(saveFilePath))
            {
                saveDataJson = sr.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during saving: " + ex.Message);
        }
        
        //parse saved data into SaveData object
        SaveData saveData =JsonToSaveData(saveDataJson);
        
        //set all of the sins
        foreach (SinData sinData in saveData.Sins)
        {
            //GameManager.Instance.InstantiateSin(sinData.Weight, sinData.location);
            DataToSin(sinData);
        }
        //set the potential sins
        foreach (Vector3 potentialSin in saveData.PotentialSins)
        {
            GameManager.Instance.InstantiatePotentialSin(potentialSin);
        }

        //set player variables
       SetPlayerData(saveData.playerData);
       
       
        return;
    }

    #region settingSins

    private SinData SinToData(GameObject sin)
    {
        SinData sinData = new SinData();
        sinData.Weight = sin.GetComponent<Sin>().weight;
        sinData.location = sin.GetComponent<Sin>().location;
        return sinData;
    }

    private void DataToSin(SinData sinData)
    {
        GameManager.Instance.InstantiateSin(sinData.Weight,sinData.location);
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

    #region JsonParsing
    private string SaveDataToJson(SaveData saveData)
    {
        
        //Strings
        String json = JsonUtility.ToJson(saveData);
        Debug.Log(json);
        return json;
        /*
        return JsonUtility.ToJson(saveData);
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
        */
    }

    private SaveData JsonToSaveData(string json)
    {
        SaveData saveData = new SaveData();
        
        JsonUtility.FromJsonOverwrite(json, saveData);
        Debug.Log("sins contains " + saveData.Sins.Count + " sins");
        
        return saveData;
    }
    
    #endregion
    [Serializable]
    private class SinData
    {
        public int Weight;
        public Vector3 location;

        public string ToString()
        {
            return Weight.ToString() + location.ToString();
        }
    }

    [Serializable]
    private class PlayerData
    {
        public int SinHeld;
        public int SinAccrued;
        public int SinThreshold;

        public string ToString()
        {
            return SinHeld.ToString() + SinAccrued.ToString() + SinThreshold.ToString();
        }
    }
    [Serializable]
    private class SaveData
    {
        public List<SinData> Sins;
        public List<Vector3> PotentialSins;
        public PlayerData playerData;

        public string ToString()
        {
            string result = "";
            foreach (SinData sin in Sins)
            {
                result += sin.ToString() + "\n";
            }

            foreach (Vector3 potentialSin in PotentialSins)
            {
                result += potentialSin.ToString() + "\n";
            }
            result += playerData.ToString();
            return result;
        }
    }

   
}


