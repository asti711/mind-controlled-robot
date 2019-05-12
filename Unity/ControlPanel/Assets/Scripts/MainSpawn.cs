using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns blocks and previews of next blocks for the main gameplay stage
/// </summary>
public class MainSpawn : MonoBehaviour {

    public GameObject parent;
    public GameObject[] options;
    public GameObject preview;
    private GameObject next_group;

    /// <summary>
    /// Start this instance. Creates first preview and first block
    /// </summary>
    void Start () {
        CreateFirst();		
        CreateNext();
	}

    /// <summary>
    /// Creates the first preview
    /// </summary>
    public void CreateFirst(){
		int next = Random.Range(0, options.Length);
		next_group = Instantiate(options[next], preview.transform.position, Quaternion.identity, parent.transform);
    }

	/// <summary>
	/// Spawns "preview" group at top of game area.
	/// Randmomly chooses next "preview" group
	/// </summary>
	public void CreateNext(){
        next_group.transform.position = transform.position;
        next_group.AddComponent<MainSet>();
		int next = Random.Range(0, options.Length);
        next_group = Instantiate(options[next], preview.transform.position, Quaternion.identity, parent.transform);
    }
}
