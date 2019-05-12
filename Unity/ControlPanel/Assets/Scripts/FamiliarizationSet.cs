using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls block sets for familiarization trials.
/// </summary>
public class FamiliarizationSet : MonoBehaviour
{

    public GameObject ghost;

    private bool orientation;
    private bool completed;

    private readonly float snapPos = 16f;

    private readonly Vector2 ghostStandByPos = Vector2.down * 10;

    private EmoEngine engine;
    private int mentalAction=0;
    public float emotivLag;
    public float blinkProcessInterval = .5f;
    public float actionProcessInterval = .75f;

//------------------------------Unity Functions------------------------------//
    /// <summary>
    /// Start this instance.
    /// </summary>
	private void Start()
    {
        if(LoggerCSV.GetInstance().gameMode == LoggerCSV.BCI_MODE){
			emotivLag = 0f;
			engine = EmoEngine.Instance;
            BindEvents();
        }
        completed = false;
        orientation = true;
        ghost = GameObject.Find(tag + "_ghost");
    }

    /// <summary>
    /// Checks for user input
    /// </summary>
    void Update()
    {
        if(LoggerCSV.GetInstance().gameMode == LoggerCSV.BCI_MODE)
            emotivLag += Time.deltaTime;

        if (!FamiliarizationController.paused)
        {
            if (orientation)
            {
                CheckRotate();
                CheckSnap();
            }
            else
            {
                CheckMoveLeft();
                CheckMoveRight();

                CheckFallDown();
            }
            if (!completed)
                UpdateGhost();
        }

    }

//------------------------------User Input Listener Functions------------------------------//



	/// <summary>
	/// Listens for and applies rotate action
	/// </summary>
	void CheckRotate()
	{
		// Rotate
        if (CustomInput("rotate"))
		{

			transform.Rotate(0, 0, -90);
			// See if valid
			if (LegalGridPos())
				// It's valid. Update grid.
				UpdateGrid();
			else
				// It's not valid. revert.
				transform.Rotate(0, 0, 90);
            
            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_BLOCK_ROTATE);
		}
	}

	/// <summary>
    /// Listens for and applies snap action (positions block at the top of the game field)
	/// </summary>
    void CheckSnap()
    {
        //Snap orientated group to top of play field
        if (CustomInput("down"))
        {
            //Don't allow snap if orientation is wrong
            if (!FindObjectOfType<FamiliarizationController>().CorrectOrientation()){
                return;
            }
            //Check for correct orientation
            orientation = false;
            bool snap = true;
            while (snap)
            {
                //Check if block is at the top
                foreach (Transform child in transform)
                {
                    if (Grid.ToGrid(child.position).y == snapPos)
                    {
                        snap = false;
                    }
                }
                //Move down one and update if still snapping
                if (snap)
                {
                    transform.position += new Vector3(0, -1, 0);
                    UpdateGrid();
                }
            }
            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_FAMI_BLOCK_POS,
                                             FamiliarizationController.PositionAverage(transform).ToString());
            SwapGhosts();

        }
    }

	/// <summary>
	/// Listens for and applies move left action
	/// </summary>
	void CheckMoveLeft()
    {
        // Move Left
        if (CustomInput("left"))
        {
            // Modify position
            transform.position += new Vector3(-1, 0, 0);

            // See if valid
            if (LegalGridPos())
                // Its valid. Update grid.
                UpdateGrid();
            else
                // Its not valid. revert.
                transform.position += new Vector3(1, 0, 0);

            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_BLOCK_LEFT,
                                             FamiliarizationController.PositionAverage(transform).ToString());
            
        }
    }

	/// <summary>
	/// Listens for and applies move right action
	/// </summary>
	void CheckMoveRight()
    {
        // Move Right
        if (CustomInput("right"))
        {
            // Modify position
            transform.position += new Vector3(1, 0, 0);

            // See if valid
            if (LegalGridPos())
                // It's valid. Update grid.
                UpdateGrid();
            else
                // It's not valid. revert.
                transform.position += new Vector3(-1, 0, 0);
            LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_BLOCK_RIGHT,
                                             FamiliarizationController.PositionAverage(transform).ToString());


		}
    }

	/// <summary>
	/// Listens for and applies drop action
	/// </summary>
	void CheckFallDown()
    {
        // Fall
        if (CustomInput("down"))
        {
            if (!FindObjectOfType<FamiliarizationController>().CorrectPosition())
                return;
			
            //Remove ghost
            ghost.transform.position = ghostStandByPos;
            completed = true;

			StartCoroutine(GoDown());
        }
    }

    /// <summary>
    /// Coroutine to drop block
    /// </summary>
    IEnumerator GoDown()
    {
        // Modify position
        transform.position += new Vector3(0, -1, 0);

        // See if valid
        while (LegalGridPos())
        {
            // It's valid. Update grid.
            UpdateGrid();
            transform.position += new Vector3(0, -1, 0);
        }
        // It's not valid. revert.
        transform.position += new Vector3(0, 1, 0);

        yield return new WaitForSeconds(.25f);

        Grid.grid = new Transform[Grid.w, Grid.h];

        // Start next trial
        FindObjectOfType<FamiliarizationController>().CreateNext();

    }

	//------------------------------Grid Helper Functions------------------------------//


	/// <summary>
	/// Checks if positioning is allowed based on Grid.cs
	/// </summary>
	/// <returns><c>true</c>, if grid position was legal, <c>false</c> otherwise.</returns>
	bool LegalGridPos()
    {
		foreach (Transform child in transform)
		{
			Vector2 v = Grid.ToGrid(child.position);

			// Is the set leaving the playing field
			if (!Grid.InsideBorder(v))
				return false;

			// Block in grid cell (and not part of same group)?
			if (Grid.grid[(int)v.x, (int)v.y] != null &&
				Grid.grid[(int)v.x, (int)v.y].parent != transform)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Updates Grid.cs with game object positions
	/// </summary>
	void UpdateGrid()
	{
		// Remove old children from grid
		for (int y = 0; y < Grid.h; ++y)
			for (int x = 0; x < Grid.w; ++x)
				if (Grid.grid[x, y] != null)
					if (Grid.grid[x, y].parent == transform)
						Grid.grid[x, y] = null;

		// Add new children to grid
		foreach (Transform child in transform)
		{
			Vector2 v = Grid.ToGrid(child.position);
			Grid.grid[(int)v.x, (int)v.y] = child;
		}
	}

	//------------------------------Ghost Helper Functions------------------------------//

	/// <summary>
	/// Reorients and repositions ghost based on current block.
	/// </summary>
	void UpdateGhost()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        bool dropping = true;
        while (dropping)
        {
            foreach (Transform child in ghost.transform)
            {
                Vector2 v = Grid.ToGrid(child.position);
                if (Grid.grid[(int)v.x, (int)v.y] != null &&
                    Grid.grid[(int)v.x, (int)v.y].parent != transform)
                {
                    dropping = false;
                    //Revert
                    ghost.transform.position += Vector3.up;
                }
                else if ((int)v.y == 0) dropping = false;
            }
            if (dropping)
                //Continue Dropping
                ghost.transform.position += Vector3.down;
        }
    }

    /// <summary>
    /// Swaps the ghosts when switching between navigation and rotation mode
    /// </summary>
    private void SwapGhosts()
    {
        ghost.transform.position = ghostStandByPos;
        if (orientation)
        {
            ghost = GameObject.Find(tag + "_ghost");
        }
        else
        {
            ghost = GameObject.Find(tag + "_ghost_light");
        } 
        UpdateGhost();
    }

	//------------------------------Emotiv Functions------------------------------//
	/// <summary>
	/// Binds local functions to EmoEngine functions
	/// </summary>
	void BindEvents(){
        engine.MentalCommandEmoStateUpdated += OnMentalCommandEmoStateUpdated;
    }

	/// <summary>
	/// Event function called when EmoEngine detects new mental command
	/// Updates current action for UI and moves current block
	/// </summary>
	void OnMentalCommandEmoStateUpdated(object sender, EmoStateUpdatedEventArgs args)
	{
		Debug.Log("Familiarization: State Updated");

		EdkDll.IEE_MentalCommandAction_t action = args.emoState.MentalCommandGetCurrentAction();
		switch (action)
		{
			case EdkDll.IEE_MentalCommandAction_t.MC_NEUTRAL:
                mentalAction = 0;
                Debug.Log("Mental Command: Neutral");
				break;
			case EdkDll.IEE_MentalCommandAction_t.MC_RIGHT:
                mentalAction = 1;
                Debug.Log("Mental Command: right");
                break;
			case EdkDll.IEE_MentalCommandAction_t.MC_LEFT:
                mentalAction = 2;
                Debug.Log("Mental Command: left");
				break;

		}
	}

//------------------------------Input helper Functions------------------------------//

    /// <summary>
    /// Custom Input function that works across control modes
    /// </summary>
    /// <returns><c>true</c>, if input was occuring, <c>false</c> otherwise.</returns>
    /// <param name="type">Input to check</param>
	private bool CustomInput(string type)
	{
		if (LoggerCSV.GetInstance().gameMode == LoggerCSV.BCI_MODE)
		{
            if(type == "down") return Input.GetKeyDown(KeyCode.DownArrow);

            switch (type){
                case "rotate":
                    if (EmoFacialExpression.isBlink && emotivLag > blinkProcessInterval){
                        emotivLag = 0f;
                        return true;
                    }
                    break;
				case "left":
				    if(mentalAction == 2 && emotivLag > actionProcessInterval){
						emotivLag = 0f;
                        return true;
                    }
                    break;
				case "right":
					if(mentalAction == 1 && emotivLag > actionProcessInterval){
						emotivLag = 0f;
                        return true;
                    }
                    break;
                default:
                    Debug.Log("CustomInput() used incorrectly with: " + type);
                    break;
            }
            return false;
		}
		else
		{
			switch (type)
			{
				case "rotate":
					return Input.GetKeyDown(KeyCode.Space);
				case "left":
					return Input.GetKeyDown(KeyCode.LeftArrow);
				case "right":
					return Input.GetKeyDown(KeyCode.RightArrow);
				case "down":
					return Input.GetKeyDown(KeyCode.DownArrow);
				default:
                    Debug.Log("CustomInput() used incorrectly with: " + type);
					return false;
			}
		}

	}


}


