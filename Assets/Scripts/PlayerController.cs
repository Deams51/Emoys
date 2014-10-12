using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
    public GUIText countText;
	private int count;
    EmotionalCharacter character;

	void Start (){
		count = 0;
        countText = new GUIText();
		SetCountText();

        EmotivEngine engine = this.GetComponent("EmotivEngine") as EmotivEngine;
        character = engine.character;
	}

	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () {
        Vector3 movement;
        Vector3 movementUp = new Vector3(0,0,0);
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space)) movementUp += new Vector3(0.0f, 10.0f, 0.0f); 
        EmotivEngine engine = this.GetComponent("EmotivEngine") as EmotivEngine;
        character = engine.character;

            movement = new Vector3(moveHorizontal, 0.0f, moveVertical) + movementUp;
            movement = movement * speed * Time.deltaTime;
        
        rigidbody.AddForce(movement);
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive(false);
			count += 1;
			SetCountText();
		}
	}

	void SetCountText(){
		countText.text = "Count: " + count.ToString ();
	}
}
