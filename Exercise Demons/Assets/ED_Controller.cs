using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// How this game works:
/// The movingArrows spawn every few seconds and move right to left.
/// If a key is pressed we may delete an arrow if in the right spot.
/// If an arrow goes beyond a certain point the zombies get faster.
/// </summary>
public class ED_Controller : MonoBehaviour {

    [SerializeField] GameObject exampleArrow;
    [SerializeField] public List<GameObject> arrowTargets;
    public List<GameObject> movingArrows;
    public int arrowSpeed = 1;
    public float correctThreshold = .2f;

    private void OnEnable() {
        Assert.IsNotNull(exampleArrow);
    }


    // Use this for initialization
    void Start () {
	    InvokeRepeating("SpawnArrow", 1, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
           KeyPressed(0);
        if (Input.GetKeyDown(KeyCode.DownArrow))
           KeyPressed(1);
        if (Input.GetKeyDown(KeyCode.UpArrow))
           KeyPressed(2);
        if (Input.GetKeyDown(KeyCode.RightArrow))
           KeyPressed(3);

		for(int i=0; i < movingArrows.Count; i++) {
            GameObject currentArrow = movingArrows[i];
            currentArrow.transform.position += new Vector3(-1*arrowSpeed*Time.deltaTime,0,0);
        }
	}
    
    void SpawnArrow() {
        GameObject newArrow = (GameObject) Instantiate(exampleArrow, exampleArrow.transform.position, exampleArrow.transform.rotation);
        int arrowType = (int)Random.Range(0,3.999f);
        int rotationAmount = 0;
        if(arrowType==0) { } else if(arrowType==1) {rotationAmount = 90;} else if(arrowType==2) {rotationAmount = -90;} else if(arrowType==3) {rotationAmount = -180;}
        newArrow.transform.Rotate(0,0,rotationAmount);
        newArrow.name = arrowType.ToString();
        movingArrows.Add(newArrow);
    }

    // Key order: Left, Down, Up, Right
    void KeyPressed(int key) {
        // User pressed a key, determine where the arrow needs to be for a success:
        float startX = arrowTargets[key].transform.position.x - correctThreshold;
        float endX = arrowTargets[key].transform.position.x + correctThreshold;
        // Determine if a moving arrow is in that location threshold:
        for(int i=0; i < movingArrows.Count; i++) {
            GameObject currentArrow = movingArrows[i];
            int currentArrowType = int.Parse(currentArrow.name);
            if(key==currentArrowType) {
                if(currentArrow.transform.position.x > endX && currentArrow.transform.position.x < startX) {
                    Destroy(currentArrow);
                    arrowTargets[key].GetComponent<Animator>().SetTrigger("correct");

                }
            }
        }
    }
}
