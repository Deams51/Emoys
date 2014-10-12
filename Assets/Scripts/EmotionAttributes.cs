using UnityEngine;
using System.Collections;

public class EmotionAttributes : MonoBehaviour {

    public float appeal = 0.0f;
    public Pad emotion = new Pad(0, 0, 0);
	// Use this for initialization
	void Start () {
        this.renderer.material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
