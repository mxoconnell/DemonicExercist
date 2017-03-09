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
    [SerializeField] GameObject loseScreen;
    [SerializeField] GameObject exampleArrow;
    [SerializeField] GameObject exampleZombie;
    [SerializeField] GameObject pathWay;
    [SerializeField] GameObject player;
    [SerializeField] public List<GameObject> arrowTargets;
    [SerializeField] UnityEngine.UI.Text Score;
    public List<GameObject> movingArrows;
    public List<GameObject> movingZombies;      // Coming to attack player
    public List<GameObject> stationaryZombies;  // Doing aerobics
    public int arrowSpeed = 1;
    public float zombieSpeed = 0.5f;
    public int level = 1; 
    private float correctThreshold = 0.3f;
    private float turnIntoRunnerDistance = 2.5f;
    private bool hasLost = false;
    private enum Directions { Left=0, Down=1, Up=2, Right=3 }

    private void OnEnable() {
        Assert.IsNotNull(exampleArrow);
        Assert.IsNotNull(exampleZombie);
        Assert.IsNotNull(arrowTargets);
        Assert.IsNotNull(Score);

    }


    // Use this for initialization
    void Start () {
	    InvokeRepeating("SpawnArrow", 1, 0.5f);
        InvokeRepeating("SpawnZombie", 2, 1f);
	}
	
	// Update is called once per frame
	void Update () {
        
        int time = (int)Time.timeSinceLevelLoad*10;
        if(!hasLost) {            
            Score.text = "Score: " + (float)time/10;
        }

        if(level*10 > time)
            level++;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
           KeyPressed(0);
        if (Input.GetKeyDown(KeyCode.DownArrow))
           KeyPressed(1);
        if (Input.GetKeyDown(KeyCode.UpArrow))
           KeyPressed(2);
        if (Input.GetKeyDown(KeyCode.RightArrow))
           KeyPressed(3);
        if(Input.GetKeyDown(KeyCode.R))
            Debug.Log("Rrrrrr!");

		for(int i=movingArrows.Count-1; i >= 0; i--) {
            // Move arrows
            GameObject currentArrow = movingArrows[i];
            currentArrow.transform.position += new Vector3(-1*arrowSpeed*Time.deltaTime,0,0);
            // Check if any got too far
            if(currentArrow.transform.position.x < arrowTargets[int.Parse(currentArrow.name)].transform.position.x - correctThreshold) { 
                ArrowMissed(currentArrow.name);
                movingArrows.RemoveAt(i);
                Destroy(currentArrow);
            }
        }
        if(!hasLost)
            for(int i=movingZombies.Count-1; i >= 0; i--) {
                // Move arrows
                GameObject currentZombie = movingZombies[i];
                int isFacingLeft = bool.Parse(currentZombie.name) ? -1 : 1;
                currentZombie.transform.position += new Vector3(isFacingLeft*zombieSpeed*Time.deltaTime,0,0);
                // Check if any got too far
                if( (isFacingLeft == -1 && pathWay.transform.position.x > currentZombie.transform.position.x) ||
                    (isFacingLeft != -1 && pathWay.transform.position.x < currentZombie.transform.position.x)) { 
                        if( Mathf.Abs(pathWay.transform.position.x-currentZombie.transform.position.x) > 0) { 
                            Debug.Log("loser");
                            loseScreen.SetActive(true);
                            player.GetComponent<Animator>().SetTrigger("Lose");
                            hasLost = true;
                        }
               }
            }
	}
    
    void ArrowMissed(string arrowType) {
        Debug.Log("Missed an arrow of type:" + arrowType);
        zombieSpeed *= 1.5f;
    }

    void SpawnZombie() {
        float rand = Random.value;
        bool isFacingLeft = true;
        if(Random.Range(0,1000) > level)
            return;
        else if (rand < .25)
            return;
        else if(rand<.5)
            isFacingLeft = false;

        Vector3 newPosition = new Vector3( isFacingLeft ? 5.5f : -7, 0, Random.Range(-7.5f, -6));
        Quaternion newRotation = isFacingLeft ? exampleZombie.transform.rotation : Quaternion.Euler(0,90,0);
        GameObject newZombie = (GameObject) Instantiate(exampleZombie, newPosition, newRotation);
        newZombie.name = isFacingLeft.ToString();
        movingZombies.Add(newZombie);
    }

    void SpawnArrow() {
        // This will slow down early arrows then stagger them slightly during later game
        if(Random.Range(0,1000) > level)
            return;
        else if (Random.Range(0,4) < 1)
            return;

        GameObject newArrow = (GameObject) Instantiate(exampleArrow, exampleArrow.transform.position, exampleArrow.transform.rotation);
        int arrowType = (int)Random.Range(0,3.999f);
        int rotationAmount = 0;
        if(arrowType==0) { } else if(arrowType==1) {rotationAmount = 90;} else if(arrowType==2) {rotationAmount = -90;} else if(arrowType==3) {rotationAmount = -180;}
        newArrow.transform.Rotate(0,0,rotationAmount);
        newArrow.name = arrowType.ToString();
        movingArrows.Add(newArrow);
    }

    // Turns zombies into aerobics people.
    // if isLeft than zombies who !isFacingLeft are affected
    void FreezeZombies(bool isLeft) {
        for(int i=movingZombies.Count-1; i >= 0; i--) {
            if(isLeft == !bool.Parse(movingZombies[i].name)) {
                if(Mathf.Abs(movingZombies[i].transform.position.x) > turnIntoRunnerDistance)
                    continue;
                movingZombies[i].transform.rotation = Quaternion.Euler(new Vector3(0,-180, 0));
                movingZombies[i].GetComponent<Animator>().SetTrigger("Run");
                stationaryZombies.Add(movingZombies[i]);
                movingZombies.RemoveAt(i); 
            }
        }
    }

    // Key order: Left, Down, Up, Right
    void DoAnimations(int key) {
        Directions direction = (Directions) key;
        ///Debug.Log(key + ": went int and this went out:" + direction);
        Debug.Log( "Sending: " + direction);
        player.GetComponent<Animator>().SetTrigger(direction.ToString());
        foreach( GameObject z in stationaryZombies)
            z.GetComponent<Animator>().SetTrigger(direction.ToString());
    }

    // Key order: Left, Down, Up, Right
    void KeyPressed(int key) {
        // User pressed a key, animate everyone
            DoAnimations(key);
        // User pressed a key, freeze zombies
        // If left or right, call freeze zombies telling ifLeft
        if(key==0 || key==3)
            FreezeZombies(key==0);
        // User pressed a key, determine where the arrow needs to be for a success:
        float startX = arrowTargets[key].transform.position.x + correctThreshold;
        ///Debug.Log(arrowTargets[key].transform.position.x + "      !     " +correctThreshold + "   " + startX);
        float endX = arrowTargets[key].transform.position.x - correctThreshold;
        // Determine if a moving arrow is in that location threshold:
        ///Debug.Log("You pressed key type:" + key);
        for(int i=movingArrows.Count-1; i >= 0; i--) {
            GameObject currentArrow = movingArrows[i];
            int currentArrowType = int.Parse(currentArrow.name);
            if(key==currentArrowType) {
                ///Debug.Log("We found a matching type!");
               /// Debug.Log(startX + "  "  + currentArrow.transform.position.x + "    " +endX );
                if(currentArrow.transform.position.x > endX && currentArrow.transform.position.x < startX) {
                    ///Debug.Log("Correct!");
                    movingArrows.RemoveAt(i);
                    Destroy(currentArrow);
                    arrowTargets[key].GetComponent<Animator>().SetTrigger("correct");
                }
            }
        }
    }
}
