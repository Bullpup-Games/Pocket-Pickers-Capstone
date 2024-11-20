using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timeout : MonoBehaviour
{
    // Start is called before the first frame update
    private IEnumerator timeout()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Map");
    }

    void Start()
    {

        StartCoroutine(timeout());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
