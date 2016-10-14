﻿using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles most of player's inputs.
/// </summary>
public class RayPlayerInput : MonoBehaviour {

    public bool enablePlayerInput = true;

    public GameObject projectile;
    public GameObject sporeShotSource;

    GameObject inTrigger;

    GameObject shot;
    GameObject triggerIndicator;
    GameObject ui;

    RayMovement rayMovement;
    CharacterInteraction characterInteraction;

    // Use this for initialization
    void Start () {
	    if (!sporeShotSource)
            sporeShotSource = transform.FindChild("SporeShotSource").gameObject;

        rayMovement = GetComponent<RayMovement>();
        characterInteraction = GetComponent<CharacterInteraction>();

        ui = GameObject.FindGameObjectWithTag("UI");
        triggerIndicator = ui.transform.FindChild("TriggerIndicator").gameObject;
    }
	
	void Update ()
    {
        // Interaction with NPC's (hard coded key for now)
        characterInteraction.TryInteraction = Input.GetKeyDown(KeyCode.E);

        if (!enablePlayerInput) return;

        // Shooting
        if (Input.GetButtonDown("Fire1"))
        {
            if (projectile) Shoot();
        }

        // Horizontal & vertical movement
        rayMovement.CharacterInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Jumping (hard coded key for now)
        rayMovement.Jump = Input.GetKeyDown(KeyCode.Space); // GetKey() enables bunny hopping

        // Running (hard coded key for now)
        rayMovement.Run = Input.GetKey(KeyCode.LeftShift);

        // Trigger activation (hard coded key for now)
        if (Input.GetKeyDown(KeyCode.F) && inTrigger != null)
        {
            if (inTrigger.GetComponent<DoorTrigger>() && inTrigger.GetComponent<DoorTrigger>().smoothTransition)
            {
                enablePlayerInput = false;
                //StartCoroutine(rayMovement.WalkToPreviousLevel(inTrigger, 2));

                //StartCoroutine(SmoothLevelTransition());
                inTrigger.GetComponent<Trigger>().Activate();
            }
            if (inTrigger.GetComponent<ElevatorTrigger>())
                inTrigger.GetComponent<ElevatorTrigger>().requirementMet = transform.name.Contains(inTrigger.GetComponent<ElevatorTrigger>().requiredAuthorization) ? true : false;

            inTrigger.GetComponent<Trigger>().Activate();
        }

        // Up (climb, go up in levels) (hard coded key for now)
        if (Input.GetKey(KeyCode.W))
        {
            //StartCoroutine(rayMovement.GoToHigherGroundLevel());
            //rayMovement.MoveToHigherGroundLevel();

        }

        // Down (go down in levels) (hard coded key for now)
        if (Input.GetKey(KeyCode.S))
        {
            //StartCoroutine(rayMovement.GoToLowerGroundLevel());
            //rayMovement.MoveToLowerGroundLevel();
        }
    }

    void Shoot()
    {
        // Gets mouse position from screen
        Vector2 target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 myPos = new Vector2(sporeShotSource.transform.position.x, sporeShotSource.transform.position.y);

        // Creates the projectile
        shot = (GameObject)Instantiate(projectile, sporeShotSource.transform.position, Quaternion.identity);

        // Uses object pool to spawn a projectile
        //shot = ObjectPool.current.Spawn(projectile, sporeShotSource.transform.position, Quaternion.identity);

        //  Sets projectile's direction towards the mouse position
        shot.GetComponent<SporeShot>().SetDirection(target - myPos);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        // OnTrigger's activate even though script is disabled,
        // so prevent these codes from activating unless this script is enabled
        if (!isActiveAndEnabled || !triggerIndicator) return;

        triggerIndicator.SetActive(false);
        inTrigger = null;
    }

    
    void OnTriggerStay2D(Collider2D col)
    {
        // OnTrigger's activate even though script is disabled,
        // so prevent these codes from activating unless this script is enabled
        if (!isActiveAndEnabled || !triggerIndicator) return;

        // If in trigger that uses Trigger interface, get the trigger
        if (col.GetComponent(typeof(Trigger)) && !triggerIndicator.activeInHierarchy)
        {
            triggerIndicator.SetActive(true);
            //inTrigger = col.GetComponent<Trigger>();
            inTrigger = col.gameObject;
        }
    }
    
    IEnumerator SmoothLevelTransition()
    {
        Debug.Log("SmoothTransition");
        inTrigger.transform.root.GetComponents<MonoBehaviour>().ToList().ForEach(s => Destroy(s));
        inTrigger.transform.root.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(sr => sr.color = new Color(255, 255, 255, 1));
        Vector3 targetPos = Vector3.zero;
        targetPos.x = transform.position.x;
        //Vector3 dif = inTrigger.GetComponent<Collider2D>().bounds.center - inTrigger.transform.root.position;
        GameObject parent = inTrigger.transform.root.gameObject;
        inTrigger.transform.SetParent(null);
        parent.transform.SetParent(inTrigger.transform);
        while(inTrigger.transform.root.position != targetPos)
        //while(inTrigger.GetComponent<Collider2D>().bounds.center != targetPos)
        {
            GetComponentInChildren<Animator>().SetFloat("Speed", 1);
            inTrigger.transform.root.position = Vector3.MoveTowards(inTrigger.transform.root.position, targetPos, 4f * Time.deltaTime);
            yield return null;
        }
        transform.position = new Vector2(41.29f, transform.position.y);
        inTrigger.transform.position = new Vector2(40.93f, 0);
        inTrigger.GetComponent<DoorTrigger>().ActivateScene();
    }
}