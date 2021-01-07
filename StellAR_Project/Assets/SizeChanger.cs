﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random=UnityEngine.Random;

public class SizeChanger : MonoBehaviour
{
    private GameObject Planet = null;
    private bool Gas = false;
    public Slider SizeSlider;
    private float scaler = 2.5f / Mathf.Log(100);
    //float solidScaler, gasScaler = 1f;
    // Start is called before the first frame update
    private void OnDisable()
    {
        Planet = null;
        SizeSlider.value = 0;
       
    }
    void OnEnable()
    {

        if (GameObject.FindGameObjectWithTag("Planet"))
        {
            Debug.Log(true);
            Planet = GameObject.FindGameObjectWithTag("Planet");
            Gas = false;
            SizeSlider.value = 1.00f; //Defaults to Earth
            SizeSlider.minValue = 0.3f; //Slightly smaller than Mercury
            SizeSlider.maxValue = 2.50f; //Super-terrans
            SizeUpdate(SizeSlider.value);
        }

        else if (GameObject.FindGameObjectWithTag("GasPlanet"))
        {
            Planet = GameObject.FindGameObjectWithTag("GasPlanet");
            Gas = true;
            SizeSlider.value = 11.20f; //Defaults to Jupiter
            SizeSlider.minValue = 2.50f;
            SizeSlider.maxValue = 14.00f; //Jupiter is 11.2, 95% of all exo planets nasa has confirmed has a radius lower than 13.25
            SizeUpdate(SizeSlider.value);
        }
        
     
    }
   

    public void SizeUpdate(float value) {
        value = Mathf.Log(value*100)/5;
        if (Gas && Planet)
        {
            value *= scaler/* * 0.5f*/;
            Planet.transform.localScale = new Vector3(value, value, value); //localscale adjusts diameter, to keep consistency with rocky icospheres we halve it to get a radius
            
        }
        else if(!Gas && Planet)
        {
            IcoPlanet ico = Planet.GetComponent<IcoPlanet>();
            ico.shapeSettings.radius = value/2 * scaler;
            ico.UpdateMesh();
        }
    }
    public void RandomSize(){
        if (Gas){
            float number = Random.Range(2.50f, 14.00f);
            SizeSlider.value = number;
            SizeUpdate(number);
        }
        else {
            float number = Random.Range(0.3f, 2.50f);
            SizeSlider.value = number;
            SizeUpdate(number);
        }
        
    }
}
