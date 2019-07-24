using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_Data;

public class S_Sensor : MonoBehaviour
{
    //[SerializeField] GameObject player;
    [SerializeField] ColliderState type;
    [SerializeField] Move moveData;
    [SerializeField] int hitNum;

    Vector3 pos;
    Quaternion rot;

    // Start is called before the first frame update
    void Start()
    {
        pos = transform.localPosition;
        rot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = pos;
        transform.localRotation = rot;
    }

    //public GameObject getPlayer { get { return player; } }
    public ColliderState getColliderState
    {
        get { return type; }
        set { type = value; }
    }
    public Move getMove { get { return moveData; } }
    public int getHitNumber { get { return hitNum; } }
}
