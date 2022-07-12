using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrClawArm : MonoBehaviour
{
    public void Open() {
        HingeJoint hingeClaw = gameObject.GetComponent<HingeJoint>();

        hingeClaw.useMotor = true;
    }

    public void Close() {
        HingeJoint hingeClaw = gameObject.GetComponent<HingeJoint>();

        hingeClaw.useMotor = false;
    }

    void OnCollisionEnter(Collision col){
        if(col.relativeVelocity == new Vector3(0,0,0)){
            gameObject.transform.parent.GetComponent<scrGarraMov>().voidSetBreakState();
        }
    }
}
