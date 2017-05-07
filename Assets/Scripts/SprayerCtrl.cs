using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
using System;


public class SprayerCtrl : MonoBehaviour {

    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;
    public GameObject particle;
	public ParticleSystemRenderer partRender;

	// these lists are used to contain the particles which match
	// the trigger conditions each frame.
	List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
	List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();
	bool isPaint = false;

	Color paintColor;
	TextMesh tm;

	public void Paint(){
		isPaint = true;
	}

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        part.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        collisionEvents = new List<ParticleCollisionEvent>();
		paintColor = Color.yellow;
		tm = GetComponent<TextMesh> ();

		//partRender = part.GetComponent<ParticleSystemRenderer>();

    }

    void Update()
    {
		var main = part.main;

		if (Input.GetKeyDown (KeyCode.Space) || Input.GetKeyDown (KeyCode.Joystick1Button7)) {
			isPaint = true;
		} else if (Input.GetKeyDown (KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.R)) {//x
			paintColor = Color.red;
			//partRender.material.color = paintColor;
			//print (partRender.material.color);

			main.startColor = paintColor;
			//part.Emit(emitParams, 60);

		}
		else if (Input.GetKeyDown (KeyCode.Joystick1Button3)|| Input.GetKeyDown(KeyCode.Y)) { //y
			paintColor = Color.yellow;
			main.startColor = paintColor;
		}
		else if (Input.GetKeyDown (KeyCode.Joystick1Button5)|| Input.GetKeyDown(KeyCode.B)) { //b
			paintColor = Color.blue;
			main.startColor = paintColor;
		}
		else if (Input.GetKeyDown (KeyCode.Joystick1Button10)|| Input.GetKeyDown(KeyCode.G)) {//a
			paintColor = Color.green;
			main.startColor = paintColor;
		}
		else if (Input.GetKeyDown (KeyCode.Joystick1Button11)|| Input.GetKeyDown(KeyCode.M)) {//=
			paintColor = Color.magenta;
			main.startColor = paintColor;
		}
		if(isPaint)
        {
            // start
			isPaint = false;
            part.Play();
            StartCoroutine(wait());
        }
		detectPressedKeyOrButton ();
    }

	public void detectPressedKeyOrButton()
	{
		foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown (kcode)) {
				Debug.Log ("KeyCode down: " + kcode);
				//tm.text = kcode.ToString();
			}
		}
	}

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        part.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        while (i < numCollisionEvents)
        {
			if (other.name.Contains ("MHN")) {
				if (rb) {
					Vector3 pos = collisionEvents [i].intersection;
					Vector3 force = collisionEvents [i].velocity * 10;
					//rb.AddForce(force);
					print (i + " has rigidbody:" + other.name);
					other.GetComponent<Renderer> ().material.color = paintColor;
				} else {
					print (i + " no rigidbody:" + other.name);
					//GameObject go = GameObject.Instantiate(particle);
					//go.transform.position = collisionEvents[i].intersection;
					other.GetComponent<Renderer> ().material.color = paintColor;
				}
			} else {
				print ("other collider:" + other.name);
			}

            i++;
        }
    }

	void OnParticleTrigger()
	{
		// get the particles which matched the trigger conditions this frame
		int numEnter = part.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
		//int numExit = part.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

		// iterate through the particles which entered the trigger and make them red
		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = enter[i];
			print(i + " trigger:" + enter[i]);
			//other.GetComponent<Renderer> ().material.color = Color.yellow;
			p.velocity = new Vector3();
			p.startColor = paintColor;
			enter[i] = p;
		}

		// iterate through the particles which exited the trigger and make them green
//		for (int i = 0; i < numExit; i++)
//		{
//			ParticleSystem.Particle p = exit[i];
//			p.startColor = new Color32(0, 255, 0, 255);
//			exit[i] = p;
//		}

		// re-assign the modified particles back into the particle system
		part.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
		//part.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
	}
}
