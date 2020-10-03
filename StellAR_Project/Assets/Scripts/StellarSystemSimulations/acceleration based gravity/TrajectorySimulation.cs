﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrajectorySimulation : MonoBehaviour
{
    
    public GameObject mainObject;
    //private Vector3 mainPos;
    //private float mainMass;

    public Vector3 initialVelocity;

    public static Vector3[] linePositions;
    public static bool destroyLine;
    public static bool drawLine;
    public int lineVertices;

    private Vector3[] velosities;
    private Vector3[] positions;
    private float[] massess;

    private float[] radius;

    private bool[] dead;

    private bool[] isStatic;

    private int length;

    private float timer;

    Vector3[] prel_acc;

    private bool startSimulation;


  void Awake(){
      Time.fixedDeltaTime = 0.01f;
      linePositions = new Vector3[lineVertices];
  } 


    void CopyObjects(){
       
        length=CelestialObject.Objects.Count;
       
        velosities=new Vector3[length];
        positions=new Vector3[length];
        massess=new float[length];
        radius=new float[length];
        dead=new bool[length];
        isStatic=new bool[length];

        Transform T = (Transform) mainObject.GetComponent(typeof(Transform));
        Rigidbody rb = (Rigidbody) mainObject.GetComponent(typeof(Rigidbody));
        positions[0] = rb.position;
        massess[0] = rb.mass;
        //velosities[0] = rb.velocity;
        radius[0] = T.localScale.x;

    
        CelestialObject a = (CelestialObject) mainObject.GetComponent(typeof(CelestialObject));
        if(a.staticBody)
        {
            isStatic[0]=true;
            }
        else{
            velosities[0] = a.velocity;
        //velosities[0] = a.pausedVelocity;
            }
            
        

        int count = 1;
        for(int i=0; i<length; i++){
            GameObject go = CelestialObject.Objects[i].gameObject;
            if(go == mainObject){
                continue; }
            else {
            Transform Ti = (Transform) go.GetComponent(typeof(Transform));
            Rigidbody rbi = (Rigidbody) go.GetComponent(typeof(Rigidbody));
            positions[count] = rbi.position;
            massess[count] = rbi.mass;
            //velosities[count] = rbi.velocity;
            radius[count] = Ti.localScale.x;
            CelestialObject ai = (CelestialObject) go.GetComponent(typeof(CelestialObject));
            if(ai.staticBody)
                isStatic[count]=true;
            else
                velosities[count] = ai.velocity;
                //velosities[count] = ai.pausedVelocity;
            count++;
            }
        }
    }

    void CalcNextVelo(int index, Vector3 new_pos, float timeStep){
        velosities[index]=(new_pos-positions[index])/timeStep;
    }

    void CalcNextVeloWithAcc(int index, Vector3 acc, float timeStep){
        velosities[index]+=(acc*timeStep)*Mathf.Sqrt(2.0f/massess[index]);
    }

     void CalcVeloStart(){
        velosities[0] = velosities[0] + initialVelocity*Mathf.Sqrt(2.0f/massess[0]);
    }

    Vector3 CalcAccNBody(int index){
        
        Vector3 dirResultant = new Vector3(0f,0f,0f);
        
        for (int i=0; i<length; i++){
            if(i!=index && !dead[i]){
                float distance = (positions[i]-positions[index]).magnitude;
                
                if(distance == double.PositiveInfinity)
                    return new Vector3(0f,0f,0f);

                Vector3 direction = (positions[i]-positions[index]).normalized;
                dirResultant += NBodyPhysics.gravityConstant*direction*massess[i]/Mathf.Pow(distance,2.0f);
            }    
        }
        return dirResultant;
    }

      Vector3 CalcAccSource(int index){
        Vector3 dirResultant = new Vector3(0f,0f,0f);
    
        for (int i=0; i<length; i++){
            if(i!=index && !dead[i] && isStatic[i]){
                float distance = (positions[i]-positions[index]).magnitude;

                if(distance == double.PositiveInfinity)
                    return new Vector3(0f,0f,0f);

                Vector3 direction = (positions[i]-positions[index]).normalized;
                dirResultant += NBodyPhysics.gravityConstant*direction*massess[i]/Mathf.Pow(distance,2.0f);
            }    
        }
        return dirResultant;
    }

    Vector3 CalcPos(int index, float timeStep, Vector3 acc){
        return positions[index] +velosities[index]*timeStep + acc*Mathf.Pow(timeStep,2.0f)/2.0f; 
    }

     Vector3 CalcPos(int index, float timeStep){
        return positions[index]+velosities[index]*timeStep; 
    }

    void CheckBoundary(int index){
        Vector3 firstPos = positions[index];
        bool firstDead = false;
        for(int i=0; i<length; i++){
            if((i!=index)&&(!dead[i])){
                Vector3 secondPos = positions[i]; 
    
                float distance = (secondPos-firstPos).magnitude;
                float radiusDistance= radius[index]+radius[i];
                
                if(distance <= radiusDistance*0.5f){ 
                    if(!isStatic[i]){
                        dead[i] = true;
                        firstDead = true;}
                    else{
                        firstDead = true;
                    }
                }
            }
        }
        if(firstDead)
            dead[index]=true;
    }
        

    // Update is called once per frame
    void Update()
    {
        if (mainObject != null)
        {
            if (SimulationPauseControl.gameIsPaused)
            {
                if (Input.GetKeyDown(KeyCode.T) || Input.touchCount > 0)
                {
                    CalcTrajectory(Time.fixedDeltaTime);
                    Array.Reverse(linePositions);
                    drawLine = true;


                }
                //As of now the below code is counting on the user being sure that when they press they place, no backsies. C
                if (Input.GetKeyDown(KeyCode.Return) || ((Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Ended))
                {
                    SetInitialVel();
                    destroyLine = true;
                    SimulationPauseControl.gameIsPaused = false;
                    //mainObject = null;
                    //this.GetComponent<TrajectoryLineAnimation>().main = null;

                }
            }
        }
    }


    void CalcTrajectory(float time){
        CopyObjects();
        CalcVeloStart();
        int count=0;
        for(int j=0; j<lineVertices; j++){
            prel_acc = new Vector3[length];
            
            linePositions[count]=positions[0];
            count++;
                
            for(int i=0; i<length; i++){
                if((!isStatic[i]) && (!dead[i])){
                        if(ToggleGravityMode.nBodyGravity){
                            prel_acc[i]=CalcAccNBody(i);
                        }
                        else{
                            prel_acc[i]=CalcAccSource(i);
                        }
                
                }
            }
             for(int i=0; i<length; i++){ 
                if((!isStatic[i]) && (!dead[i])){
                    
                    CalcNextVeloWithAcc(i,prel_acc[i],time);
                    Vector3 new_pos = CalcPos(i,time);
                    
                    

                    //Vector3 new_pos = CalcPos(i,time,prel_acc[i]); 
                    //CalcNextVelo(i,new_pos,time);
                    
                    positions[i] = new_pos;
                }
            }
            for(int i=0; i<length; i++){
                if((!isStatic[i]) && (!dead[i])){
                        CheckBoundary(i);
                    }
            }

        }
    }

    void SetInitialVel(){
        CelestialObject a = (CelestialObject) mainObject.GetComponent(typeof(CelestialObject));
         a.velocity += initialVelocity*Mathf.Sqrt(2.0f/massess[0]);
        //a.pausedVelocity += initialVelocity;*/
    }
    }
