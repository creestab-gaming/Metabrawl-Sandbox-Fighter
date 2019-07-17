using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_Data;

public class S_Input : MonoBehaviour
{
    [SerializeField] public static int MOVE_BUFFER;

    S_Player player;
    ControlScheme controls;
    ActionType input;
    int fBuf;

    ActionType _inStart;
    int _bufStart;


    // Start is called before the first frame update
    void Start()
    {
        ResetPlayer();
        ResetBuffer();
    }

    // Update is called once per frame
    void Update()
    {
        //Check axis possitions
        if (GENERAL_DEBUG && Input.GetKeyDown(controls.alt))
        {
            Debug.Log("   Left Analog Horizontal: " + Input.GetAxis("P1_LHorz"));
            Debug.Log("   Left Analog Vertical: " + Input.GetAxis("P1_LVert"));
            Debug.Log("   Right Analog Horizontal: " + Input.GetAxis("P1_RHorz"));
            Debug.Log("   Right Analog Vertical: " + Input.GetAxis("P1_RVert"));
            Debug.Log("   Left Analog Horizontal: " + Input.GetAxis("P2_LHorz"));
            Debug.Log("   Left Analog Vertical: " + Input.GetAxis("P2_LVert"));
            Debug.Log("   Right Analog Horizontal: " + Input.GetAxis("P2_RHorz"));
            Debug.Log("   Right Analog Vertical: " + Input.GetAxis("P2_RVert"));
        }

        if (input != ActionType.none) TickBuffer();

        //Placeholders to identify when the buffer receives a new input
        _inStart = input;
        _bufStart = fBuf;

        //Light attack input
        if (Input.GetKeyDown(controls.light))
        {
            //Is it a shield grab?
            if (Input.GetKey(controls.block))
            {
                SetBuffer(ActionType.grab);
            }
            //Is the main stick past the heavy threshold?
            else if (Input.GetAxis(controls.mHorz) < controls.lightToHeavy && Input.GetAxis(controls.mVert) < controls.lightToHeavy)
            {
                SetBuffer(ProcessInput(Action.Heavy, false));
            }
            //Its a light attack
            SetBuffer(ProcessInput(Action.Light, false));
        }
        //Block input
        else if (Input.GetKeyDown(controls.block))
        {
            SetBuffer(ProcessInput(Action.Block, false));
        }
        //Special move input
        else if (Input.GetKeyDown(controls.special))
        {
            SetBuffer(ProcessInput(Action.Special, false));
        }
        //Heavy attack input
        else if (Input.GetKeyDown(controls.heavy))
        {
            SetBuffer(ProcessInput(Action.Heavy, false));
        }
        //C-stick input
        else if(Input.GetAxis(controls.cHorz) != 0 || Input.GetAxis(controls.cVert) != 0)
        {
            SetBuffer(ProcessInput(controls.cStickUse, true));
        }

        //Check if the buffer has changed
        if(fBuf > _bufStart || input != _inStart) { player.SetInput(input); }
    }

    //Determine which ActionType to buffer
    public ActionType ProcessInput(Action _input, bool c)
    {
        int _orientation = player.curState.orientation;
        bool _airborne = player.curState.airborne;

        //Determining axis to reference
        string _horz; string _vert;
        if (c)
        {
            _horz = controls.cHorz;
            _vert = controls.cVert;
        }
        else
        {
            _horz = controls.mHorz;
            _vert = controls.mVert;
        }

        //Horizontal input
        if (Mathf.Abs(Input.GetAxis(_horz)) >= Mathf.Abs(Input.GetAxis(_vert)))
        {
            //Facing right
            if (_orientation == 1)
            {
                //Right on stick
                if (Input.GetAxis(_horz) > 0)
                {
                    if (_input == Action.Light) { if (_airborne) { return ActionType.fAir; } return ActionType.fLight; }
                    if (_input == Action.Heavy) { if (_airborne) { return ActionType.fAir; } return ActionType.fStrong; }
                    if (_input == Action.Special) { return ActionType.fSpec; }
                    if (_input == Action.Block)
                    {
                        if (!_airborne || player.curState.stun > 0) return ActionType.fRoll;
                        else return ActionType.floatDodge;
                    }
                }
                //Left on stick
                else
                {
                    if (_input == Action.Light) { if (_airborne) { return ActionType.bAir; } player.FlipOrientation(); return ActionType.fLight; }
                    if (_input == Action.Heavy) { if (_airborne) { return ActionType.bAir; } player.FlipOrientation(); return ActionType.fStrong; }
                    if (_input == Action.Special) { player.FlipOrientation(); return ActionType.fSpec; }
                    if (_input == Action.Block)
                    {
                        if (!_airborne || player.curState.stun > 0) return ActionType.bRoll;
                        else return ActionType.floatDodge;
                    }
                }
            }
            //Facing left
            else
            {
                //Right on stick
                if (Input.GetAxis(_horz) > 0)
                {
                    if (_input == Action.Light) { if (_airborne) { return ActionType.bAir; } player.FlipOrientation(); return ActionType.fLight; }
                    if (_input == Action.Heavy) { if (_airborne) { return ActionType.bAir; } player.FlipOrientation(); return ActionType.fStrong; }
                    if (_input == Action.Special) { player.FlipOrientation(); return ActionType.fSpec; }
                    if (_input == Action.Block)
                    {
                        if (!_airborne || player.curState.stun > 0) return ActionType.bRoll;
                        else return ActionType.floatDodge;
                    }
                }
                //Left on stick
                else
                {
                    if (_input == Action.Light) { if (_airborne) { return ActionType.fAir; } return ActionType.fLight; }
                    if (_input == Action.Heavy) { if (_airborne) { return ActionType.fAir; } return ActionType.fStrong; }
                    if (_input == Action.Special) { return ActionType.fSpec; }
                    if (_input == Action.Block)
                    {
                        if (!_airborne || player.curState.stun > 0) return ActionType.fRoll;
                        else return ActionType.floatDodge;
                    }
                }
            }
        }
        //Vertical input
        if(Mathf.Abs(Input.GetAxis(_horz)) < Mathf.Abs(Input.GetAxis(_vert)))
        {
            //Up on stick
            if (Input.GetAxis(_vert) > 0)
            {
                if (_input == Action.Light) { if (_airborne) { return ActionType.uAir; } return ActionType.uLight; }
                if (_input == Action.Heavy) { if (_airborne) { return ActionType.uAir; } return ActionType.uStrong; }
                if (_input == Action.Special) { return ActionType.uSpec; }
                if (_input == Action.Block)
                {
                    if (player.curState.stun > 0) return ActionType.techHop;
                    else return ActionType.floatDodge;
                }
            }
            //Down on stick
            else
            {
                if (_input == Action.Light) { if (_airborne) { return ActionType.dAir; } return ActionType.dLight; }
                if (_input == Action.Heavy) { if (_airborne) { return ActionType.dAir; } return ActionType.dStrong; }
                if (_input == Action.Special) { return ActionType.dSpec; }
                if (_input == Action.Block) { return ActionType.fallDodge; }
            }
        }
        //Neutral input
        if (_airborne)
        {
            if (_input == Action.Block) return ActionType.floatDodge;
            if (_input == Action.Light || _input == Action.Heavy) return ActionType.nAir;
            if (_input == Action.Special) return ActionType.nSpec;
        }
        //Jab input
        if (_input == Action.Light)
        {
            if (player.curState.holding) return ActionType.pummel;
            else return ActionType.jab;
        }

        //Processing error
        if (GENERAL_DEBUG) Debug.Log("Could not process action input");
        return ActionType.none;
    }

    public void TickBuffer()
    {
        if (fBuf <= 0)
        {
            ResetBuffer();
            player.ClearInput();
        }
        else fBuf--;
    }
    public void SetBuffer(ActionType _input)
    {
        input = _input;
        fBuf = MOVE_BUFFER;
    }
    public void ResetBuffer()
    {
        input = ActionType.none;
        fBuf = 0;
    }

    public void ResetPlayer()
    {
        player = gameObject.GetComponent<S_Player>();
        controls = player.controls;
    }
}
