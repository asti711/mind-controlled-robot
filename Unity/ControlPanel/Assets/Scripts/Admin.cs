using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
/// <summary>
/// Controls the Admin button and input field in Trail/Familiarization stage.
/// Researcher enters in password to terminate session prior to completion.
/// </summary>
public class Admin : MonoBehaviour {

    public Button btnAdmin;
    public TMP_InputField ifAdmin;

    private string password = "missChib";

	/// <summary>
	/// Checks the password. Called by Admin_InputField
	/// </summary>
	/// <param name="s">String to check against passoword</param>
	public void CheckPassword(string s){
        if(s == password){
            LoggerCSV logger = LoggerCSV.GetInstance();
            logger.AddEvent(LoggerCSV.EVENT_UNABLE);
            if (logger.gameMode == LoggerCSV.BCI_MODE){
                logger.AddEvent(LoggerCSV.EVENT_END_BCI);
            }
            else{
                logger.AddEvent(LoggerCSV.EVENT_END_NORMAL);
            }

            //Reset Persistnent master
            if(logger.gameMode == LoggerCSV.BCI_MODE){
                GameObject master = GameObject.Find("Persistent_Master");
                Destroy(master.GetComponent<EmotivControl>());
                Destroy(master.GetComponent<EmoFacialExpression>());
            }
            //Reset Logger
			logger.inSession = false;
			logger.SaveCSV();
			logger.ResetCSV();
			SceneManager.LoadScene(0);
        }
        else{
            btnAdmin.gameObject.SetActive(true);
            ifAdmin.text = string.Empty;
            ifAdmin.gameObject.SetActive(false);
        }

    }
}
