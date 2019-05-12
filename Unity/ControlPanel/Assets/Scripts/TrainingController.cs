using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls user progression through the different elements of the Training stage.
/// Users must train neutral, each action twice, and complete the trial for each action
/// </summary>
public class TrainingController : MonoBehaviour {
    //Control
    public TrainingCube cube;
    uint userId;
    public bool firstTime = true;
    public bool training, leftFirst, debug = false;
    //public Text lp, rp;
    public string trainType;
	EmoEngine engine;
    TrainingUI UI;

    //XX Start For testing without EMOTIV
    //private void Update()
    //{
    //    if (!training)
    //    {
    //        if (Input.GetKey(KeyCode.LeftArrow))
    //            cube.SetAciton(cube.ACTION_LEFT);
    //        else if (Input.GetKey((KeyCode.RightArrow)))
    //            cube.SetAciton(cube.ACTION_RIGHT);
    //        else
    //            cube.SetAciton(cube.ACTION_NEUTRAL);
    //    }
    //}
	//XX End

	// private void Update()
	// {
	//lp.text = "Left Power: " + EmoMentalCommand.GetMentalCommandActionPower()[5];
	//rp.text = "Right Power: " + EmoMentalCommand.GetMentalCommandActionPower()[6];
	//}

	//------------------------------Emotiv Event Functions------------------------------//

	/// <summary>
	/// Binds local functions to EmoEngine functions
	/// </summary>
	void BindEvents(){
        engine.UserAdded += OnUserAdded;
		engine.MentalCommandTrainingStarted += OnTrainingStarted;
        engine.MentalCommandTrainingSucceeded += OnTrainingSuccess;
        engine.MentalCommandTrainingFailed += OnTrainingFailed;
		engine.MentalCommandTrainingCompleted += OnTrainingAccepted;
        engine.MentalCommandEmoStateUpdated += OnMentalCommandEmoStateUpdated;
    }
	/// <summary>
	/// Unbinds local functions from EmoEngine functions
	/// </summary>	
    void UnbindEvents(){
	    engine.UserAdded -= OnUserAdded;
		engine.MentalCommandTrainingStarted -= OnTrainingStarted;
        engine.MentalCommandTrainingSucceeded -= OnTrainingSuccess;
        engine.MentalCommandTrainingFailed -= OnTrainingFailed;
		engine.MentalCommandTrainingCompleted -= OnTrainingAccepted;
		engine.MentalCommandEmoStateUpdated -= OnMentalCommandEmoStateUpdated;
    }

	/// <summary>
	/// Event function called when EmoEngine detects new mental command
    /// Updates current action for UI and Training Cube
	/// </summary>
	void OnMentalCommandEmoStateUpdated(object sender, EmoStateUpdatedEventArgs args){
        if (training)
        {
            cube.SetAciton(cube.ACTION_RESET);
            return;
        }
        //do not update during training
        EdkDll.IEE_MentalCommandAction_t action = args.emoState.MentalCommandGetCurrentAction();
        //Move Block and Update UI text
        switch (action)
        {
            case EdkDll.IEE_MentalCommandAction_t.MC_NEUTRAL:
                cube.SetAciton(cube.ACTION_NEUTRAL);
                UI.UpdateCurrentActionText("Current Action: Neutral");
                break;
            case EdkDll.IEE_MentalCommandAction_t.MC_RIGHT:
                cube.SetAciton(cube.ACTION_RIGHT);
                UI.UpdateCurrentActionText("Current Action: Right");
                break;
            case EdkDll.IEE_MentalCommandAction_t.MC_LEFT:
                cube.SetAciton(cube.ACTION_LEFT);
                UI.UpdateCurrentActionText("Current Action: Left");
				break;

        }
    }

	/// <summary>
	/// Event function called when EmoEngine detects error in training
	/// </summary>
	void OnTrainingFailed(object sender, EmoEngineEventArgs args)
	{
		Debug.Log("In Failed");
		UI.UpdateStatusText(trainType + " Failed Due to Noisy Signal, Please Try Again");
		cube.SetAciton(cube.ACTION_RESET);
		UI.ActivateButtons(true);
		training = false;
	}

	/// <summary>
	/// Event function called by EmoEngine when EPOC is connected
	/// </summary>
	void OnUserAdded(object sender, EmoEngineEventArgs args){
        userId = args.userId;
    }
	/// <summary>
	/// Event function called by EmoEngine before training period starts
	/// </summary>
	public void OnTrainingStarted(object sender, EmoEngineEventArgs args)
    {
        UI.UpdateStatusText( "Training " + trainType);
        training = true;
        UI.ActivateButtons(false);
    }

	/// <summary>
	/// Event function called by EmoEngine after training period ends
	/// </summary>    
    public void OnTrainingSuccess(object sender, EmoEngineEventArgs args){
	    Debug.Log("In Success");
		engine.MentalCommandSetTrainingControl(userId, EdkDll.IEE_MentalCommandTrainingControl_t.MC_ACCEPT);
	}

	/// <summary>
	/// Event function called by EmoEngine when training data is accepted
	/// </summary>
	void OnTrainingAccepted(object sender, EmoEngineEventArgs args){
		Debug.Log("In Accepted");
        UI.UpdateStatusText( "Success! Training " + trainType + " Concluded");
		training = false;
		UI.ActivateButtons(true);

        if (!firstTime || trainType == "Neutral")
        {
            ResetCube();
        }
        else
            cube.SetAciton(cube.ACTION_RESET);
		//Shows information about training trials after
		// accepting the first right or left data
		if (firstTime && trainType != "Neutral")
		{
			firstTime = false;
			UI.trialInfoPanel.SetActive(true);

		}
	}

	/// <summary>
	/// Deactivates Right and Left mental commands
	/// </summary>
	void DeactivateRL(){
        EmoMentalCommand.EnableMentalCommandAction(EdkDll.IEE_MentalCommandAction_t.MC_RIGHT, false);
        EmoMentalCommand.EnableMentalCommandAction(EdkDll.IEE_MentalCommandAction_t.MC_LEFT, false);
        EmoMentalCommand.EnableMentalCommandActionsList();
    }
	/// <summary>
	/// Erases ment command training data
	/// </summary>
    /// <param name="action">Action to be erased</param>
	void EraseAction(EdkDll.IEE_MentalCommandAction_t action){
		EdkDll.IEE_MentalCommandSetTrainingAction((uint)EmoUserManagement.currentUser, action);
		EdkDll.IEE_MentalCommandSetTrainingControl((uint)EmoUserManagement.currentUser,
												   EdkDll.IEE_MentalCommandTrainingControl_t.MC_ERASE);
    }


	//------------------------------UI OnClick Functions------------------------------//

	/// <summary>
	/// Custom Start function called by Start_Training_Button
	/// </summary>
	public void CustomStart()
    {
        LoggerCSV logger = LoggerCSV.GetInstance();
        logger.AddEvent(LoggerCSV.EVENT_TRAINSTAGE_START);

        UI = GetComponent<TrainingUI>();
        cube = GameObject.Find("Block").GetComponent<TrainingCube>();
        leftFirst = logger.counterBalanceID == 1
                          || logger.counterBalanceID == 2;

		engine = EmoEngine.Instance;
        DeactivateRL();
        UI.InitUI();
		BindEvents();
    }

	/// <summary>
	/// Initiates trainging of mental command, called by Left_Button, Right_Button, and Neutral_Button
	/// </summary>
    /// <param name="type">Command to be trained ("Neutral","Left","Right")</param>
	public void TrainAction(string type)
	{
        LoggerCSV logger = LoggerCSV.GetInstance();
		trainType = type;
        EdkDll.IEE_MentalCommandAction_t toTrain = EdkDll.IEE_MentalCommandAction_t.MC_NEUTRAL;
        cube.SetAciton(cube.ACTION_RESET);
        switch (type)
        {
            case "Left":
                toTrain = EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
                logger.AddEvent(LoggerCSV.EVENT_TRAINING_L);
                break;
            case "Right":
				toTrain = EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
				logger.AddEvent(LoggerCSV.EVENT_TRAINING_R);
				break;
            default:
                logger.AddEvent(LoggerCSV.EVENT_TRAINING_N);
                break;
        }

		StartCoroutine(UI.UpdateSlider());

        EmoMentalCommand.EnableMentalCommandAction(toTrain, true);
        EmoMentalCommand.EnableMentalCommandActionsList();
        EmoMentalCommand.StartTrainingMentalCommand(toTrain);
	}

	/// <summary>
    /// Initiates clearing of mental command, called by Clear_Left_Button, 
    /// Clear_Right_Button, Clear_and Neutral_Button
	/// </summary>
	/// <param name="type">Command to be cleared ("Neutral","Left","Right")</param>
	public void ClearTrainingCheck(string type)
    {
        switch (type)
        {
            case "Left":
                trainType = "clear left";
                break;
            case "Right":
                trainType = "clear right";
                break;
            default:
                trainType = "clear neutral";
                break;
        }
        UI.clearPanel.SetActive(true);

    }

	/// <summary>
	/// Clear mental command accepted by Accept_Clear_Button,
	/// Clears all data if neutral is cleared
	/// </summary>
	/// <param name="type">Command to be trained ("Neutral","Left","Right")</param>
	public void ClearTraining(){
        LoggerCSV logger = LoggerCSV.GetInstance();
        string statusText = "Neutral";
		EdkDll.IEE_MentalCommandAction_t action = EdkDll.IEE_MentalCommandAction_t.MC_NEUTRAL;
        switch (trainType)
        {
            case "clear left":
                statusText = "Left";
                action = EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
                trainType = "clear left";
                logger.AddEvent(LoggerCSV.EVENT_TRAINING_CLEAR_L);
                break;
            case "clear right":
                statusText = "Right";
                action = EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
                trainType = "clear right";
                logger.AddEvent(LoggerCSV.EVENT_TRAINING_CLEAR_R);
                break;
            default:
                logger.AddEvent(LoggerCSV.EVENT_TRAINING_CLEAR_N);
                UI.UpdateStatusText("Current Aciton: None");
                trainType = "clear neutral";
                Debug.Log(action);
                EraseAction(action);
                //Clear left and right if enabled
                //Left
                if (EmoMentalCommand.MentalCommandActionsEnabled[5]){
                    Debug.Log("left active - clear");
                    EraseAction(EdkDll.IEE_MentalCommandAction_t.MC_LEFT);
                }
                //Right
                if (EmoMentalCommand.MentalCommandActionsEnabled[6]){
					Debug.Log("right active - clear");
					EraseAction(EdkDll.IEE_MentalCommandAction_t.MC_LEFT);
                }
                DeactivateRL();
                //Update UI and Training cube
                UI.UpdateStatusText("Cleared " + statusText + " Training Data");
                UI.UpdateUI(trainType);
                cube.SetAciton(cube.ACTION_RESET);
                return;
		}

		EraseAction(action);
        //Deactivate cleared action
        EmoMentalCommand.EnableMentalCommandAction(action, false);
        EmoMentalCommand.EnableMentalCommandActionsList();
		//Update UI and Training cube
		UI.UpdateUI(trainType);
        UI.ActivateButtons(true);
        cube.SetAciton(cube.ACTION_RESET);
        UI.UpdateStatusText( "Cleared " + statusText + " Training Data");
    }


    /// <summary>
    /// Loads Next Scene, called by Next_Scene_Button
    /// </summary>
    public void NextScene(){
        LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_TRAINSTAGE_END);
        UnbindEvents();
        SceneManager.LoadScene(2);
    }

	/// <summary>
	/// Starts/Ends training trials, Called by Right_Trial_Button and Left_Trial_Button
	/// </summary>
	/// <param name="left">Left command?</param>
	public void StartEndTrial(bool left){
        cube.SetAciton(cube.ACTION_RESET);
        if (left){
            //Trial is ongoing
            if (UI.leftTrial)
                UI.UpdateUI("left trial stop");
			//Trial hasn't started
			else
                UI.UpdateUI("left trial start");
        }
        else{
            //Trial is ongoing
            if (UI.rightTrial)
                UI.UpdateUI("right trial stop");
            //Trial hasn't started
            else
                UI.UpdateUI("right trial start");
        }
    }

    /// <summary>
    /// Resets the training cube to the center position
    /// </summary>
	public void ResetCube(){
		cube.SetAciton(cube.ACTION_RESET);
		UI.UpdateUI(trainType);
    }

}