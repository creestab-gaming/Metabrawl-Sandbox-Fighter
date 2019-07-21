using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_Data;

[System.Serializable]
public class PlayerSensorData
{
    [SerializeField] S_Sensor[] _sensors;   //Hitbox Sensors
    [SerializeField] ActionType _instance;  //The action that uses the sensor

    public S_Sensor[] GetSensors { get { return _sensors; } }
    public ActionType GetInstance { get { return _instance; } }
}

public class S_Player : MonoBehaviour
{
    [SerializeField] Animator anim;                 //The animator for this player
    [SerializeField] List<PlayerSensorData> sensors;//All hitbox sensors
    [SerializeField] public int port;  //Outlet of which the player is being controlled, determines player #
    [SerializeField] public TextAsset characterFile;    //Text file containing player/character data
    [SerializeField] public TextAsset controlsFile;     //Text file containing input bindings
    private Rigidbody rb;       //Rigid body of player rig
    private BoxCollider box;    //Box collider associated with player

    public Model model;
    public Enchantment enchantment;
    public Dictionary<ActionType, Move> moveset;        //Keymap for each slot type and a bound action
    public ControlScheme controls;                  //A struct of all inputs and a bound action
    public PlayerState startState;                  //Player variables on spawn
    public PlayerState curState;                    //Player variables accounting for state modifiers
    public List<PlayerModifier> stateMods;          //List of all state modifiers effecting player

    //State modifications and scaling
    private static float zStart;            //Initial z-axis shift
    private static float zOffset = 1f;      //Difference in z-axis between orientations

    //Buffered input tracking
    private ActionType input;       //Tracking of buffered input
    private float mVert;            //Vertical force to be applied per frame

    // Start is called before the first frame update
    void Start()
    {
        moveset = new Dictionary<ActionType, Move>();
        startState = new PlayerState();

        if(port > 0)
        {
            S_Player _temp = ReadCharacter(characterFile.name);
            model = _temp.model;
            startState = model.modifiers;
            enchantment = _temp.enchantment;
            moveset = _temp.moveset;
            controls = ReadControls(controlsFile.name);

            Spawn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Reduce frames remaining for all temporary modifications, then remove any that are expired
        TickMods();
        ApplyMods();

        //Reset animation speed when not running
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run")) { anim.speed = 1; }

        //Process current state
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run")
                || anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch") || anim.GetCurrentAnimatorStateInfo(0).IsName("Block"))
        {
            //Drops block when not holding the input anymore
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Block") && Input.GetKeyUp(controls.block)) { anim.Play("Idle"); }

            curState.actable = true;
        }
        //Actable in air
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Airborne") || anim.GetCurrentAnimatorStateInfo(0).IsName("AirJump"))
        {
            curState.actable = true;
        }
        //Jumpsquat processing
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("JumpSquat"))
        {
            curState.actable = false;       //Cannot act in jumpsquat
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= (5 / 6))  //Last frame of jumpsquat
            {
                if(GENERAL_DEBUG == true) Debug.Log("End of JumpSquat");
                curState.actable = true;    //Allows acting after squat. Debating if this is the place to do this.
                if (Input.GetKey(controls.jump)) Jump('f');     //Full hop if holding jump at end of jumpsquat
                else Jump('s');                                 //Shorthop if not holding jump

                if (input == ActionType.jump) input = ActionType.none;  //Removes excess jump inputs during jumpsquat
            }
        }
        else
        {
            curState.actable = false;   //Not in an actable state
        }

        mVert = 0;


        //Action Processing
        if (curState.actable)
        {
            if (input != ActionType.none)
            {
                //if(GENERAL_DEBUG) Debug.Log("Action pulled from buffer: " + input + "   Update #" + debugFrames);

                ////////////////////
                //Possible actions//
                ////////////////////

                if (curState.airborne)
                {
                    //Bufferable aerial move processing
                    if (input == ActionType.jump) { Jump('a'); }
                    else if (input == ActionType.nAir) { anim.Play("NeutralAir"); }
                    else if (input == ActionType.fAir) { anim.Play("ForwardAir"); }
                    else if (input == ActionType.bAir) { anim.Play("BackAir"); }
                    else if (input == ActionType.uAir) { anim.Play("UpAir"); }
                    else if (input == ActionType.dAir) { anim.Play("DownAir"); }
                    else if (input == ActionType.nSpec) { anim.Play("NeutralSpecial"); }
                    else if (input == ActionType.fSpec) { anim.Play("ForwardSpecial"); }
                    else if (input == ActionType.uSpec) { anim.Play("UpSpecial"); }
                    else if (input == ActionType.dSpec) { anim.Play("DownSpecial"); }
                    else if (input == ActionType.floatDodge) { anim.Play("AirDodge"); }
                    else if (input == ActionType.fallDodge) { anim.Play("FallDodge"); }
                }
                else
                {
                    //Bufferable grounded move processing
                    if (input == ActionType.jump) { anim.Play("JumpSquat"); if (GENERAL_DEBUG) Debug.Log("Preparing for jump"); }
                    else if (input == ActionType.jab) { anim.Play("Jab"); }
                    else if (input == ActionType.fLight) { anim.Play("ForwardLight"); }
                    else if (input == ActionType.uLight) { anim.Play("UpLight"); }
                    else if (input == ActionType.dLight) { anim.Play("DownLight"); }
                    else if (input == ActionType.fStrong) { anim.Play("ForwardStrong"); }
                    else if (input == ActionType.uStrong) { anim.Play("UpStrong"); }
                    else if (input == ActionType.dStrong) { anim.Play("DownStrong"); }
                    else if (input == ActionType.nSpec) { anim.Play("NeutralSpecial"); }
                    else if (input == ActionType.fSpec) { anim.Play("ForwardSpecial"); }
                    else if (input == ActionType.uSpec) { anim.Play("UpSpecial"); }
                    else if (input == ActionType.dSpec) { anim.Play("DownSpecial"); }
                    else if (input == ActionType.floatDodge) { anim.Play("AirDodge"); }
                    else if (input == ActionType.fallDodge) { anim.Play("SpotDodge"); }
                    else if (input == ActionType.fRoll) { anim.Play("ForwardRoll"); }
                    else if (input == ActionType.bRoll) { anim.Play("BackwardRoll"); }
                    else if (input == ActionType.grab) { anim.Play("Grabbing"); }
                }

                //Buffered action executed
                ClearInput();
            }
            else
            {
                if (curState.airborne) //Aerial control
                {
                    //Plays idle airborne animation if actable and not playing an animation
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Airborne") && !anim.GetCurrentAnimatorStateInfo(0).IsName("AirJump")) anim.Play("Airborne");

                    //Fastfall processing
                    if (!curState.fastfall)
                    {
                        //If player is trying to fastfall while already falling but not fastfalling
                        if ((Input.GetKeyDown(controls.down) || Input.GetAxis(controls.mVert) < -.7) && rb.velocity.y < 0 && rb.velocity.y > -(curState.fallSpeed + (curState.weight * 2)))
                        {
                            curState.fastfall = true;   //Player is now fastfalling
                            mVert = -(curState.fallSpeed + (curState.weight * 2));  //Applied vertical force modified by fastfall mod
                            rb.velocity.Set(rb.velocity.x, -curState.fallSpeed, 0); //Immediately initiate a downward momentum
                        }
                        else mVert = -curState.fallSpeed;   //Player not fastfalling
                    }
                    else mVert = -(curState.fallSpeed + (curState.weight * 2));     //Player already fastfalling

                    //Key Movement
                    if (Input.GetKey(controls.right))
                    {
                        gameObject.transform.position -= gameObject.transform.right * curState.orientation * curState.aDrift;
                    }
                    else if (Input.GetKey(controls.left))
                    {
                        gameObject.transform.position += gameObject.transform.right * curState.orientation * curState.aDrift;
                    }
                    //Analog Movement
                    else
                    {
                        if (Input.GetAxis(controls.mHorz) > 0)       //Inputting right
                        {
                            gameObject.transform.position -= gameObject.transform.right * curState.orientation * curState.runSpeed * Input.GetAxis(controls.mHorz);
                        }
                        else if (Input.GetAxis(controls.mHorz) < 0)  //Inputting left
                        {
                            gameObject.transform.position -= gameObject.transform.right * curState.orientation * curState.runSpeed * Input.GetAxis(controls.mHorz);
                        }
                    }
                }
                else    //Grounded control
                {
                    mVert = -1;     //Slight downward force to ensure grounded state
                    if (rb.velocity.x != 0 || rb.velocity.y != 0) rb.velocity.Set(0, 0, 0); //If any momentum while grounded, nullify

                    //Grab actions
                    /*if (IsHoldingPlayer())
                    {

                    }*/

                    //Enter Block
                    else if (Input.GetKeyDown(controls.block) && !anim.GetCurrentAnimatorStateInfo(0).IsName("Block")) { anim.Play("Block"); }

                    //Key move crouch
                    else if (Input.GetKey(controls.down))
                    {
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch")) { anim.Play("Crouch"); } //Crouch if not already crouching
                    }
                    //Key move right
                    else if (Input.GetKey(controls.right))
                    {
                        if (curState.orientation != 1)
                        {
                            curState.orientation = 1;   //If the player is not facing the direction of input, turn around
                        }
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Running"))
                        {
                            anim.Play("Running");       //If the player is not already running, run
                        }
                        gameObject.transform.position -= gameObject.transform.right * curState.runSpeed;    //Move the player
                    }
                    //Key move left
                    else if (Input.GetKey(controls.left))
                    {
                        if (curState.orientation != -1)
                        {
                            curState.orientation = -1;  //If the player is not facing the direction of input, turn around
                        }
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                        {
                            anim.Play("Run");           //If the player is not already running, run
                        }
                        gameObject.transform.position -= gameObject.transform.right * curState.runSpeed;    //Move the player
                    }
                    //Analog Movement
                    else if (Mathf.Abs(Input.GetAxis(controls.mHorz)) > Mathf.Abs(Input.GetAxis(controls.mVert))) //Move if horizontal input is greater than vertical input
                    {
                        if (Input.GetAxis(controls.mHorz) > 0) { curState.orientation = 1; }
                        else { curState.orientation = -1; }

                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run")) anim.Play("Run");
                        anim.speed = Mathf.Abs(Input.GetAxis(controls.mHorz));

                        gameObject.transform.position -= gameObject.transform.right * curState.runSpeed * Mathf.Abs(Input.GetAxis(controls.mHorz));
                    }
                    else if (Input.GetAxis(controls.mVert) < -.5)
                    {
                        //Crouch
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch")) anim.Play("Crouch");
                    }
                    else
                    {
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) anim.Play("Idle");
                    }
                }
            }
        }
        //Process influence to inactable states (Performing a move, teching, DI, mashing, ext.)
        else
        {

        }

        //Process orientation
        if (curState.orientation == 1)
        {
            gameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);   //Face right
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, zStart + zOffset);    //adjust Z-axis possition
        }
        else if (curState.orientation == -1)
        {
            gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);     //Face left
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, zStart - zOffset);    //adjust Z-axis possition
        }

        //Processing forces into applied momentum
        if (curState.airborne) rb.AddForce(new Vector3(0, mVert - (Mathf.Log10(Mathf.Abs(rb.velocity.y) + (curState.weight*10)) * curState.fallSpeed), 0.0f));
        else rb.AddForce(new Vector3(0, mVert - curState.weight, 0));
        //Velocity failsafe if values are too high
        if (rb.velocity.y < -(curState.fallSpeed + (curState.weight * 3)) && curState.stun == 0) { rb.velocity = new Vector3(rb.velocity.x, -(curState.fallSpeed + (curState.weight * 3)), 0); }
    }

    //Instantiate the player
    public void Spawn()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        box = gameObject.GetComponent<BoxCollider>();

        curState = startState;
        mVert = 0;
        input = ActionType.none;
        anim.Play("Airborne");
    }

    public void Jump(char type)
    {
        if(GENERAL_DEBUG) Debug.Log("Attempting Jump");

        //Short jump
        if (type == 's')
        {
            rb.velocity = new Vector3(rb.velocity.x, 20.0f / curState.weight, 0.0f);
            mVert = curState.gJumpSpeed / 2;
        }
        //Full jump
        else if (type == 'f')
        {
            rb.velocity = new Vector3(rb.velocity.x, 40.0f / curState.weight, 0.0f);
            mVert = curState.gJumpSpeed;
        }
        //Aerial jump
        else if (type == 'a')
        {
            anim.Play("AirJump");
            rb.velocity = new Vector3(rb.velocity.x, 10.0f / curState.weight, 0.0f);
            mVert = curState.aJumpSpeed;
            curState.jumps--;
        }

        //Going airborne
        if (!curState.airborne)
        {
            curState.airborne = true;
            gameObject.transform.Translate(new Vector3(0, .5f, 0), Space.Self);
            anim.Play("Airborne");
        }
    }

    public void SetInput(ActionType _input)
    {
        input = _input;
    }

    //Remove buffered input
    public void ClearInput()
    {
        input = ActionType.none;
    }

    //Applies all currently stored modifications to the current state
    public void RefreshState()
    {
        ResetState();
        ApplyMods();
    }
    //Resets the current player state to the starting state
    public void ResetState()
    {
        curState = startState;
    }
    //Adds a modification to the list of player modifications
    public void AddMod(PlayerModifier _mod)
    {
        stateMods.Add(_mod);
    }
    //Applies all currently stored modifications to the starting state
    public void ApplyMods()
    {
        foreach(PlayerModifier _mod in stateMods)
        {
            if (_mod.model != null) curState.color = _mod.color;
            if (_mod.color != Color.gray) curState.color = _mod.color;
            if (_mod.element != Element.Null) curState.element = _mod.element;
            curState.size = startState.size + _mod.size;
            curState.weight = startState.weight + _mod.weight;
            curState.runSpeed = startState.runSpeed + _mod.runSpeed;
            curState.gJumpSpeed = startState.gJumpSpeed + _mod.gJumpSpeed;
            curState.aJumpSpeed = startState.aJumpSpeed + _mod.aJumpSpeed;
            curState.aDrift = startState.aDrift + _mod.aDrift;
            curState.fallSpeed = startState.fallSpeed + _mod.fallSpeed;
            curState.maxJumps = startState.maxJumps + _mod.maxJumps;
            curState.dps = startState.dps + _mod.dps;
        }
    }
    //
    public void TickMods()
    {
        foreach (PlayerModifier _mod in stateMods) _mod.Tick(); //Iterate through all state modifiers and reduce all frame timers by 1
        stateMods.RemoveAll(x => x.time == 0);                  //Purge all expired state modifiers
    }
    //
    public void ClearMods()
    {
        stateMods = new List<PlayerModifier>();
    }

    //Turns the player around
    public void FlipOrientation()
    {
        curState.orientation = -curState.orientation;
    }

    public List<PlayerSensorData> GetSensors
    { get { return sensors; } }
}
