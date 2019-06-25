using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_Data;

[System.Serializable]
public class PlayerSensorData
{
    [SerializeField] S_Sensor[] _sensors;
    [SerializeField] ActionType _instance;   //The action that uses the sensor

    public S_Sensor[] GetSensors { get { return _sensors; } }
    public ActionType GetInstance { get { return _instance; } }
}

public class S_Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] List<PlayerSensorData> sensors;
    [SerializeField] TextAsset movesetFile;
    [SerializeField] TextAsset controlsFile;
    private GameObject rig;
    private Rigidbody rb;
    private BoxCollider box;

    public Dictionary<SlotType, Move> moveset;
    public ControlScheme controls;
    public PlayerState startState;
    public PlayerState curState;
    public List<PlayerModifier> stateMods;

    // Start is called before the first frame update
    void Start()
    {
        Spawn();


    }

    // Update is called once per frame
    void Update()
    {
        //Updating player modifications
        foreach(PlayerModifier _mod in stateMods) _mod.Tick();
        stateMods.RemoveAll(x => x.time == 0);
        RefreshMods();


    }

    public void Spawn()
    {
        Init(movesetFile.name, controlsFile.name);
    }

    public void Init(string _moveset, string _controls)
    {
        moveset = ReadMoves(_moveset);
        controls = ReadControls(_controls);
    }

    //Changes the character state to account for a modification
    public void AddMod(PlayerModifier _mod)
    {
        stateMods.Add(_mod);
    }
    //Applies all currently stored modifications to the current state
    public void RefreshMods()
    {
        ResetState();
        ApplyMods();
    }
    //Resets the current player state to the starting state
    public void ResetState()
    {
        curState = startState;
    }

    //Applies all currently stored modifications to the current state
    public void ApplyMods()
    {
        foreach(PlayerModifier _mod in stateMods)
        {
            if (_mod.model != null) curState.color = _mod.color;
            if (_mod.color != Color.gray) curState.color = _mod.color;
            curState.size += _mod.size;
            curState.weight += _mod.weight;
            curState.runSpeed += _mod.runSpeed;
            curState.gJumpSpeed += _mod.gJumpSpeed;
            curState.aJumpSpeed += _mod.aJumpSpeed;
            curState.fallSpeed += _mod.fallSpeed;
            curState.maxJumps += _mod.maxJumps;
            curState.dps += _mod.dps;
        }
    }    
    //
    public void ClearMods()
    {
        stateMods = new List<PlayerModifier>();
    }
}
