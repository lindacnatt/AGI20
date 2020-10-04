﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLineAnimation : MonoBehaviour
{
    public LineRenderer traj;
    private Rigidbody rb;
    private int count = 0;
    public GameObject main;
    private int length; 

    // Start is called before the first frame update

  
    void Start()
    {
        traj = GetComponent<LineRenderer>();
        length=TrajectorySimulation.linePositions.Length;
        
    }

    // Update is called once per frame

    public void DrawLine(){
        this.GetComponent<LineRenderer>().enabled = true;
        count =0;
        traj.positionCount = length;
        int index=0;
        foreach(Vector3 pos in TrajectorySimulation.linePositions){
            traj.SetPosition(index,pos);
            index++;
        }
        TrajectorySimulation.drawLine=false;


    }

    void Update(){
        if(main != null)
        {
            rb = (Rigidbody)main.GetComponent(typeof(Rigidbody)); //if it is in start and is assigned to a gameobject it will throw an error as it should not get a planet until it is placed.
            if (TrajectorySimulation.drawLine)
            {
                DrawLine();
            }
        }
        
    }

    void FixedUpdate()
    {
        
        if (main != null)
        {
            if (!SimulationPauseControl.gameIsPaused)
            {
                if (TrajectorySimulation.destroyLine)
                {
              
                    Vector3 linePos = TrajectorySimulation.linePositions[length - 1 - count];
                    float distance = (rb.position - linePos).magnitude;
            
                    if (count < length-1) //otherwise the linepos array thingy above got out of range as it became 2000 - 2001
                    {
                        
                        if (distance <= 1.0f)
                        {
                           
                            traj.positionCount -= 1;
                            count++;
                        }
                    }

                    else
                    {
                  
                        TrajectorySimulation.destroyLine = false;
                        
                        //traj.positionCount=0;
                        count = 0;
                    }

                }
                else
                {
                    traj.positionCount = 0;
                }

            }
        }

    }
}
