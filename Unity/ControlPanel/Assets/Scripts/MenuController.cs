using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls Menu
/// </summary>
public class MenuController : MonoBehaviour {

    public GameObject messagePanel;
    public TextMeshProUGUI messageText;

    private readonly int[] counterBalnaceOptions = { 1, 2, 3, 4 };
	
    /// <summary>
    /// Coroutine to display message to user
    /// </summary>
    /// <param name="message">Message text</param>
    /// <param name="delay">Duration of message popup</param>
    IEnumerator ShowMessage(string message, float delay)
	{
        messageText.text = message;
        messagePanel.SetActive(true);
		yield return new WaitForSeconds(delay);
        messagePanel.SetActive(false);
	}

	//------------------------------UI OnClick Functions------------------------------//

	/// <summary>
	/// Sets the game mode. Called by Game_Mode_Slider
	/// </summary>
	/// <param name="val">Game mode value</param>
	public void SetGameMode(float val)
	{
        LoggerCSV.GetInstance().gameMode = (int)val;

	}
	/// <summary>
	/// Sets the participant ID. Called by Participant_ID_InputField
	/// </summary>
	/// <param name="val">ID value</param>
	public void SetParticipantID(string val){
        int id;
        Int32.TryParse(val, out id);
        LoggerCSV.GetInstance().participantID = id;
    }

	/// <summary>
    /// Sets the counter balance ID (1,2,3, or 4). Called by CounterBalance_ID_InputField
	/// </summary>
	/// <param name="val">ID value</param>
	public void SetCounterBalanceID(string val)
	{
		int id;
		Int32.TryParse(val, out id);
        LoggerCSV.GetInstance().counterBalanceID = id;
	}

	/// <summary>
	/// Starts the game. Called by Start_Button
	/// </summary>
	public void StartGame(){
        if (LoggerCSV.GetInstance().participantID < 1){
            StartCoroutine(ShowMessage("Please Enter a Valid Participant ID", 1.5f));
            return;
        }
        if(Array.IndexOf(counterBalnaceOptions,LoggerCSV.GetInstance().counterBalanceID)<0){
			StartCoroutine(ShowMessage("Please Enter a Valid Participant Group", 1.5f));
			return;
        }
        //Start saving data automatically
        LoggerCSV.GetInstance().inSession = true;
        if (LoggerCSV.GetInstance().gameMode == LoggerCSV.NORMAL_MODE){
            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_START_NORMAL);
            SceneManager.LoadScene(2);
        }
        else{
            GameObject master = GameObject.Find("Persistent_Master");
            master.AddComponent<EmotivControl>();
            master.AddComponent<EmoFacialExpression>();
            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_START_BCI);
            SceneManager.LoadScene(1);
        }
    }

	/// <summary>
	/// Quit the game. Called by Quit_Button
	/// </summary>
	public void Quit()
	{
		Application.Quit();
	}
}