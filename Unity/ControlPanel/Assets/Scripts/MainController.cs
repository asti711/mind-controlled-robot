using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for Main gameplay Scene
/// </summary>
public class MainController : MonoBehaviour {

    public static int score;

    public TextMeshProUGUI scoreText;
    public Button pauseButton;

	public GameObject finishedMessage;
    public GameObject midwayMessage;
    public GameObject pauseMessage;

    private bool checkingTime;
    private bool midWayReached;
    public float allotedTime;
    public float halfAllottedTime;

    public static bool paused = false;

//------------------------------Unity & Main Scene Control Functions------------------------------//

    /// <summary>
    /// Initializes relevant variables
    /// </summary>
	void Start () {
        paused = false;
        halfAllottedTime = allotedTime / 2;
        midWayReached = false;
        checkingTime = true;
        score = 0;
        UI_Game();
	}
	
    /// <summary>
    /// Checks if allotted time has been exceeded, updates score
    /// </summary>
	void Update () {
        if(checkingTime)
            CheckTime();
        UpdateScore();
		
	}

	/// <summary>
	/// Check if halfway or end has been reached
	/// </summary>
	void CheckTime(){
        halfAllottedTime -= Time.deltaTime;
        if(halfAllottedTime<0){
            paused = true;
            checkingTime = false;
            if(!midWayReached){
                LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_PAUSE_START);
				midWayReached = true;
                halfAllottedTime = allotedTime / 2;
                UI_Pause("midway");
            }
            else{
                LoggerCSV logger = LoggerCSV.GetInstance();
                if (logger.gameMode == LoggerCSV.BCI_MODE)
                    LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_END_BCI);
                else
                    logger.AddEvent(LoggerCSV.EVENT_END_NORMAL);
                UI_Pause("finished");
                logger.inSession = false;
                logger.SaveCSV();
                logger.ResetCSV();
            }
        }
    }

	//------------------------------UI OnClick Functions------------------------------//


	/// <summary>
	/// Finishs the game. Called by Done_Button
	/// </summary>
	public void FinishGame(){
        if (LoggerCSV.GetInstance().gameMode == LoggerCSV.BCI_MODE)
		{
			GameObject master = GameObject.Find("Persistent_Master");
            Destroy(GameObject.Find("Contact_Quality"));
            master.GetComponent<EmotivControl>().End();
			Destroy(master.GetComponent<EmotivControl>());
			Destroy(master.GetComponent<EmoFacialExpression>());
		}
        SceneManager.LoadScene(0);
    }

	/// <summary>
	/// Ends the midway message and continues gameplay. Called by End_Midway_Button
	/// </summary>
	public void EndMidwayMessage(){
        LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_PAUSE_END);
        UI_Game();
        checkingTime = true;
        paused = false;
    }

	/// <summary>
	/// Starts the pause. Called by Pause_Button
	/// </summary>
	public void StartPause(){
        LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_PAUSE_START);
		checkingTime = false;
        paused = true;
        UI_Pause("pause");
	}

	/// <summary>
	/// Ends the pause. Called by End_Pause_Button
	/// </summary>
	public void EndPause(){
		LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_PAUSE_END);
		checkingTime = true;
        paused = false;
        UI_Game();
	}

	//------------------------------UI Helper Functions------------------------------//

	/// <summary>
	/// Sets UI elements for gameplay view
	/// </summary>
	private void UI_Game()
	{
		midwayMessage.SetActive(false);
		finishedMessage.SetActive(false);
		pauseMessage.SetActive(false);
		pauseButton.gameObject.SetActive(true);
		scoreText.gameObject.SetActive(true);
	}

	/// <summary>
	/// Sets UI elements for paused-game view
	/// </summary>
    /// <param name="type">Type of pause ("midway", "pause", "finished")</param>
	private void UI_Pause(string type)
	{
		scoreText.gameObject.SetActive(false);
		pauseButton.gameObject.SetActive(false);
		switch (type)
		{
			case "midway":
				midwayMessage.SetActive(true);
				return;
			case "pause":
				pauseMessage.SetActive(true);
				return;
			case "finished":
				finishedMessage.SetActive(true);
				return;
		}
	}

	/// <summary>
    /// Updates the score UI element.
    /// </summary>
	public void UpdateScore()
	{
		scoreText.text = "Score: " + score.ToString();
	}
}
