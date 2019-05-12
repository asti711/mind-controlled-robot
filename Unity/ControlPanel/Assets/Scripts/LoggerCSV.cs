using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
/// <summary>
/// Class to log and save game events to .csv file
/// </summary>
public class LoggerCSV : MonoBehaviour
{

	public static LoggerCSV instance = null;

    public List<string[]> rows = new List<string[]>();

    private float timer;
    private float saveInterval = 10f;

    public bool inSession;
    public int gameMode = 0;
    public int participantID = -1;
    public int counterBalanceID = -1;

    public static readonly int NORMAL_MODE = 0;
    public static readonly int BCI_MODE = 1;
    public static readonly int UNASIGNED = -1;

    public static readonly string EVENT_START_NORMAL = "Start Normal Mode";
    public static readonly string EVENT_END_NORMAL = "End Normal Mode";

    public static readonly string EVENT_UNABLE = "Unable to Complete Stage";

    public static readonly string EVENT_START_BCI = "Start BCI Mode";
    public static readonly string EVENT_END_BCI = "End BCI Mode";

    public static readonly string EVENT_PAUSE_START = "Start Pause";
	public static readonly string EVENT_PAUSE_END = "End Pause";

    public static readonly string EVENT_TRAINSTAGE_START = "Start BCI Training Stage";
    public static readonly string EVENT_TRAINSTAGE_END = "End BCI Training Stage";

	public static readonly string EVENT_TRAINING_N = "Training Neutral";
	public static readonly string EVENT_TRAINING_R = "Training Right";
	public static readonly string EVENT_TRAINING_L = "Training Left";

	public static readonly string EVENT_TRAINING_CLEAR_N = "Neutral Cleared";
	public static readonly string EVENT_TRAINING_CLEAR_R = "Right Cleared";
	public static readonly string EVENT_TRAINING_CLEAR_L = "Left Cleared";

	public static readonly string EVENT_TRAINING_ACCEPT = "Training Data Accepted";
	public static readonly string EVENT_TRAINING_REJECT = "Training Data Rejected";

	public static readonly string EVENT_TRAINING_TRIAL_PASS_R = "Right Training Trial Passed";
	public static readonly string EVENT_TRAINING_TRIAL_PASS_L = "Left Training Trial Passed";

	public static readonly string EVENT_TIMEOUT = "Timed Out";

	public static readonly string EVENT_FAMI_START = "Start Familiarization";
    public static readonly string EVENT_FAMI_END = "Completed Familiarization";
    public static readonly string EVENT_FAMI_PROMT = "Trial Prompt Created";
    public static readonly string EVENT_FAMI_BLOCK_POS = "Block xPos at Start Navigation";
    public static readonly string EVENT_FAMI_PASS = "Familiarization Trial Passed";

	public static readonly string EVENT_BLOCK_ROTATE = "Block Rotated";
	public static readonly string EVENT_BLOCK_LEFT = "Block Left";
	public static readonly string EVENT_BLOCK_RIGHT = "Block Right";
    public static readonly string EVENT_BLOCK_CREATE = "Block Created";
    public static readonly string EVENT_BLOCK_DROP = "Block Dropped";

    public static readonly string EVENT_SCORE = "Score";
    public static readonly string EVENT_GAME_OVER = "Game Over";


//------------------------------Singleton Control Functions------------------------------//

    //Occurs at the beginning of the game
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			this.CreateTitles();
			DontDestroyOnLoad(gameObject);
		}
		else if (instance != this)
		{
            Destroy(this.gameObject);
		}

	}

	/// <summary>
	/// Returns LoggerCSV singleton
	/// </summary>
	public static LoggerCSV GetInstance()
	{
		return instance;
	}

	/// <summary>
	/// Saves CSV file every saveInterval
	/// </summary>
	private void Update()
    {
        timer += Time.deltaTime;
        //Only save if in session
        if(timer>saveInterval && inSession){
            SaveCSV();
            timer = 0f;
        }
    }


	//------------------------------CSV Functions------------------------------//

	/// <summary>
	/// Creates Title Row
	/// </summary>
	private void CreateTitles()
	{
		string[] titles = { "External Time", "Internal Time", "Event", "AUX" };
		rows.Add(titles);
	}

	/// <summary>
	/// Adds event with no auxiliary information 
	/// </summary>
    /// <param name="event_log">Event to be logged</param>
	public void AddEvent(string event_log)
	{
        AddEvent(event_log, null);
	}

	/// <summary>
	/// Adds event with auxiliary information
	/// </summary>
	/// <param name="event_log">Event to be logged</param>
	/// <param name="aux">Axiliary information to be logged with event</param>
	public void AddEvent(string event_log, string aux){
        string[] toAdd = { DateTime.Now.ToString(), Time.time.ToString(), event_log, aux };
		rows.Add(toAdd);
    }

	/// <summary>
	/// Prints current logs in unity console
	/// </summary>
	public void PrintLogger()
	{
		for (int i = 0; i < rows.Count; i++)
		{
			string[] r = rows[i];
			string toPrint = "";
			for (int j = 0; j < r.Length; j++)
			{
				toPrint += r[j] + "    ";
			}
			Debug.Log("Row " + i.ToString() + ": " + toPrint);
		}
	}

	/// <summary>
	/// Clears all logged information and resets participant IDs and game mode
	/// </summary>
	public void ResetCSV(){
        participantID = UNASIGNED;
        counterBalanceID = UNASIGNED;
        gameMode = NORMAL_MODE;
		rows = new List<string[]>();
		CreateTitles();
    }

	/// <summary>
	/// Saves logged data as a .csv file, location depends on OS.
	/// See Unity reference for Application.persistentDataPath
	/// </summary>
	public void SaveCSV()
    {
        string[][] output = new string[rows.Count][]; // my data is held in rows (List<string[]>)
    	//Convert rows into string [][]
        for (int i = 0; i < output.Length; i++)
    	{
    		output[i] = rows[i];
    	}
    	int len = output.GetLength(0);
    	string divider = ",";

    	StringBuilder sb = new StringBuilder();

        //convert output into csv format
    	for (int index = 0; index < len; index++)
    		sb.AppendLine(string.Join(divider, output[index]));


    	string filePath = getPath(); //Returns Application.persistentDataPath + "gameData.csv"

        //Save Data
    	StreamWriter outStream = File.CreateText(filePath);
    	outStream.WriteLine(sb);
    	outStream.Close();

    }
	/// <summary>
    /// Returns path + name of file in format:
    /// path/participantID_controlMode_groupID_BrainBlocks.csv
	/// </summary>
	// Following method is used to retrive the relative path as device platform
	private string getPath()
	{
		string mode;
		if (gameMode == NORMAL_MODE)
			mode = "_Normal_";
		else
			mode = "_BCI_";

        string final = Application.persistentDataPath;
        if (final.EndsWith("brain_blocks"))
            final = final.Substring(0, final.Length-12);
        return final + participantID.ToString() + mode + counterBalanceID.ToString() 
                                    + "_BrainBlocks" + ".csv";
	}
	
}
