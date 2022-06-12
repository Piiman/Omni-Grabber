using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO simplificar esto, en especial como decide avanzar al siguiente paso y eliminar banderas
public class scrGarraMov : MonoBehaviour
{
    //Coords de posicion inicial de la garra
    [Header("Claw Start positions")]
    public Vector3 vec3StartCoords = new Vector3(0,0,0);

    //Coords maximas de la garra
    [Header("Claw Maximum position")]
    public Vector3 vec3MaxCoords = new Vector3(0,0,0);

    //Velocidad maxima y aceleracion de la garra
    [Header("Claw Speed")]
    public float floatMaxSpeed = 0.0f;
    public float floatAcceleration = 0.0f;

    [Header("Claw open/close wait times")]
    public float floatOpenCatch = 0.0f;
    public float floatCloseCatch = 0.0f;
    public float floatOpenRelease = 0.0f;
    public float floatCloseRelease = 0.0f;

    //Objetos de brazos de garra
    [Header("Claw arms")]
    public GameObject clawArmRight;
    public GameObject clawArmLeft;

    //Resorte de garra
    [Header("Arms Spring")]
    public int intClosingForce = 0;
    public int intClosingDamp = 0;
    public int intTargetPosition = 0;

    //Motor de garra
    [Header("Arms Motor")]
    public int intOpeningVelocity = 0;
    public int intOpeningForce = 0;


    //Limites de garra
    [Header("Arms Limits")]
    public float floatMaxOpening = 0.0f;
    public float floatMinOpening = 0.0f;
    public float floatLimitBounce = 0.0f;
    public float floatBounceMinVelocity = 0.0f;

    //State determina que que fase se encuentra la garra(despues de 9 regresa a 1)
    //0: Inicio/moivimiento izq/der
    //1: Movimiento atras/adelante
    //2: Abrir garra
    //3: Movimiento abajo
    //4: Cerrar garra
    //5: Movimiento arriba
    //6: Movimiento en profundidad hacia posicion inicial Z
    //7: Movimiento horizonta hacia posicion inicial X
    //8: Abrir garra
    //9: Cerrar garra
    private int intState;

    //Velocidad actual de la garra
    private float floatSpeed;

    //flags para estado del input
    private bool boolHold;
    private bool boolBreak;
    private bool boolOpenClaw;
    private bool boolWait;

    void Start()
    {
        //init vars & flags
        intState = 0;
        floatSpeed = 0.0f;
        boolHold = false;
        boolBreak = false;
        boolOpenClaw = false;
        boolWait = false;

        //init left claw
        voidInitArm(clawArmLeft);

        //init right claw
        voidInitArm(clawArmRight);
    }

    void Update()
    {
        //Si se presiona el boton de salto entra en estado de aceleracion
        if (Input.GetButtonDown("Jump") && !boolHold && intState <=1) {
            boolHold = true;
        }

        //Si se suelta el boton de salto entra en estado de frenado
        if (Input.GetButtonUp("Jump") && boolHold && intState <= 1) {
            voidSetBreakState();
        }

        //TODO: add si la garra choca esto solo puede pasar en fase 3
        if(false){
            //add despues de x segundos avanza
        }

        //Si se esta presionando el boton de salto (estado de aceleracion) aumenta la aceleracion
        if (boolHold && floatSpeed <= floatMaxSpeed) {
            floatSpeed += floatAcceleration;
        }

        //Si la garra esta frenando desacelera, de lo contrario avanza
        if (boolBreak) {
            floatSpeed -= floatAcceleration;

            //Si la velocidad es 0 o negativa pasa al sig estado
            if(floatSpeed <= 0.0){
                floatSpeed = 0;
                boolBreak = false;
                intState++;

                //Si el estado siguiente es 10 reinicia a 0 y ve a coordenadas original
                if(intState >= 10) {
                    gameObject.transform.position = vec3StartCoords;
                    intState = 0;
                } 
                //Si el estado es 2 o uno siguiente, para que la garra se mueva hasta la posicion maxima de ese
                //estado(en este caso son las posiciones originales).
                else if(intState >= 2){
                    boolHold = true;
                }
            }
        } else {
            switch(intState){
                //---------------Inicio fase control de jugador---------------
                case 0:
                    voidMoveClaw(Vector3.right,gameObject.transform.position.x, vec3MaxCoords.x);
                    break;
                case 1:
                    voidMoveClaw(Vector3.forward,gameObject.transform.position.z, vec3MaxCoords.z);
                    break;
                //---------------Fin fase control de jugador---------------
                //---------------Inicio fase de agarre---------------
                case 2:
                    //TODO: en lugar de esperar a que cierre/abra por completo esperar n segundos a que abra/cierre
                    if(!boolOpenClaw){
                        voidOpenClaw();
                    }

                    if(boolCheckOpen() && !boolWait){
                        boolWait = true;
                        StartCoroutine(proceedAfter(floatOpenCatch));
                    }
                    break;
                case 3:
                    voidMoveClaw(Vector3.down,vec3MaxCoords.y,gameObject.transform.position.y);
                    break;
                case 4:
                    if(boolOpenClaw){
                        voidCloseClaw();
                    }

                    if(boolCheckClose() && !boolWait){
                        boolWait = true;
                        StartCoroutine(proceedAfter(floatCloseCatch));
                    }
                    break;
                case 5:
                    voidMoveClaw(Vector3.up,gameObject.transform.position.y, vec3StartCoords.y);
                    break;
                //---------------Fin fase de agarre---------------
                //---------------Inicio fase de regreso---------------
                case 6:
                    voidMoveClaw(Vector3.back,vec3StartCoords.z,gameObject.transform.position.z);
                    break;
                case 7:
                    voidMoveClaw(Vector3.left,vec3StartCoords.x,gameObject.transform.position.x);
                    break;
                //---------------Fin fase de regreso---------------
                //---------------Inicio fase de soltar---------------
                case 8:
                    if(!boolOpenClaw){
                        voidOpenClaw();
                    }

                    if(boolCheckOpen()  && !boolWait){
                        boolWait = true;
                        StartCoroutine(proceedAfter(floatOpenRelease));
                    }
                    break;
                case 9:
                    if(boolOpenClaw){
                        voidCloseClaw();
                    }

                    if(boolCheckClose()  && !boolWait){
                        boolWait = true;
                        StartCoroutine(proceedAfter(floatCloseRelease));
                    }
                    break;
                //---------------Fin fase de soltar---------------
            }
        }
    }

    void voidMoveClaw(Vector3 vec3Direction, float floatPosition, float floatMaxPosition){
        gameObject.transform.Translate(vec3Direction * floatSpeed * Time.deltaTime);
        if(floatPosition >= floatMaxPosition){
            voidSetBreakState();
        }
    }

    void voidSetBreakState() {
        boolHold = false;
        boolBreak = true;
    }

    void voidOpenClaw() {
        HingeJoint hingeLeftClaw = clawArmLeft.GetComponent<HingeJoint>();
        HingeJoint hingeRightClaw = clawArmRight.GetComponent<HingeJoint>();

        hingeLeftClaw.useMotor = true;
        hingeRightClaw.useMotor = true;
        boolOpenClaw = true;
    }

    bool boolCheckOpen() {
        //to easy code
        bool leftOpen = Mathf.Approximately(clawArmLeft.transform.rotation.eulerAngles.z, floatMaxOpening);
        bool rightOpen = Mathf.Approximately(clawArmRight.transform.rotation.eulerAngles.z, floatMaxOpening);
    
        return (leftOpen && rightOpen);
    }

    bool boolCheckClose() {
        //to easy code
        bool leftClosed = Mathf.Approximately(clawArmLeft.transform.rotation.eulerAngles.z, floatMaxOpening);
        bool rightClosed = Mathf.Approximately(clawArmRight.transform.rotation.eulerAngles.z, floatMaxOpening);
    
        return (leftClosed && rightClosed);
    }

    void voidCloseClaw() {
        HingeJoint hingeLeftClaw = clawArmLeft.GetComponent<HingeJoint>();
        HingeJoint hingeRightClaw = clawArmRight.GetComponent<HingeJoint>();

        hingeLeftClaw.useMotor = false;
        hingeRightClaw.useMotor = false;
        boolOpenClaw = false;
    }

    void voidInitArm(GameObject clawArm) {
        HingeJoint hingeClaw = clawArm.GetComponent<HingeJoint>();

        //Spring
        JointSpring hingeSpring = hingeClaw.spring;
        hingeSpring.spring = intClosingForce;
        hingeSpring.damper = intClosingDamp;
        hingeSpring.targetPosition = intTargetPosition;
        hingeClaw.spring = hingeSpring;
        hingeClaw.useSpring = true;

        //Motor
        var hingeMotor = hingeClaw.motor;
        hingeMotor.force = intOpeningForce;
        hingeMotor.targetVelocity = intOpeningVelocity;
        hingeMotor.freeSpin = false;
        hingeClaw.motor = hingeMotor;
        hingeClaw.useMotor = false;

        //limits
        JointLimits hingeLimits = hingeClaw.limits;
        hingeLimits.min = floatMinOpening;
        hingeLimits.bounciness = floatLimitBounce;
        hingeLimits.bounceMinVelocity = floatBounceMinVelocity;
        hingeLimits.max = floatMaxOpening;
        hingeClaw.limits = hingeLimits;
        hingeClaw.useLimits = true;
    }

    IEnumerator proceedAfter(float floatWaitTime)
    {
        yield return new WaitForSeconds(floatWaitTime);
        voidSetBreakState();
        boolWait = false;
    }
}