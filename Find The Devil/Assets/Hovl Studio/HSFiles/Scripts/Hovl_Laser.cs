﻿using System.Collections;
using System.Collections.Generic;
// using System.Runtime.Serialization.Formatters; // This using directive is not needed for this script
using System;
using UnityEngine;

public class Hovl_Laser : MonoBehaviour
{
 
    public Transform laserStartTransform;
    public Transform laserEndTransform; 
    
    public GameObject HitEffect;
    public float HitOffset = 0;
    public bool useLaserRotation = false; 

    public float MaxLength; 
    private LineRenderer Laser;

    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);

   
    private bool LaserSaver = false; 
    private bool UpdateSaver = false; 

    private ParticleSystem[] Effects; 
    private ParticleSystem[] HitParticles; 

    void Start()
    {
        Laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        
        if (HitEffect != null)
        {
            HitParticles = HitEffect.GetComponentsInChildren<ParticleSystem>();
        }
        
        if (laserStartTransform == null)
        {
            laserStartTransform = transform;
        }
        
        if (HitParticles != null)
        {
            foreach (var AllPs in HitParticles)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }

    void Update()
    {
        Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));
        Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));
        
        if (Laser != null && UpdateSaver == false)
        {
            Vector3 startPoint = laserStartTransform.position;
            Vector3 endPoint;
            
            if (laserEndTransform != null)
            {
                endPoint = laserEndTransform.position;
                
                if (HitEffect != null)
                {
                    HitEffect.transform.position = endPoint;
                    if (useLaserRotation)
                        HitEffect.transform.rotation = laserEndTransform.rotation;
                    else
                        HitEffect.transform.LookAt(endPoint ); 

            
                    foreach (var AllPs in Effects)
                    {
                        if (!AllPs.isPlaying) AllPs.Play();
                    }
                    
                    if (HitParticles != null)
                    {
                        foreach (var AllPs in HitParticles)
                        {
                            if (AllPs.isPlaying) AllPs.Stop();
                        }
                    }
                }

                Length[0] = MainTextureLength * Vector3.Distance(startPoint, endPoint);
                Length[2] = NoiseTextureLength * Vector3.Distance(startPoint, endPoint);
            }
            else
            {
                RaycastHit hit;
                Vector3 rayDirection = laserStartTransform.forward; 

                if (Physics.Raycast(startPoint, rayDirection, out hit, MaxLength))
                {
                    endPoint = hit.point;
                    
                    if (HitEffect != null)
                    {
                        HitEffect.transform.position = hit.point ;
                        if (useLaserRotation)
                            HitEffect.transform.rotation = laserStartTransform.rotation; 
                        else
                            HitEffect.transform.LookAt(hit.point );
                        
                        if (HitParticles != null)
                        {
                             foreach (var AllPs in HitParticles)
                            {
                                if (!AllPs.isPlaying) AllPs.Play();
                            }
                        }
                    }
                    
                    foreach (var AllPs in Effects)
                    {
                        if (!AllPs.isPlaying) AllPs.Play();
                    }
                    
                    Length[0] = MainTextureLength * Vector3.Distance(startPoint, hit.point);
                    Length[2] = NoiseTextureLength * Vector3.Distance(startPoint, hit.point);
                    
                }
                else
                {
                    endPoint = startPoint + rayDirection * MaxLength;
                    
                    foreach (var AllPs in Effects)
                    {
                        if (!AllPs.isPlaying) AllPs.Play();
                    }
                    Length[0] = MainTextureLength * MaxLength;
                    Length[2] = NoiseTextureLength * MaxLength;
                }
            }
            
            Laser.SetPosition(0, startPoint);
            Laser.SetPosition(1, endPoint);
            
            if (Laser.enabled == false && LaserSaver == false)
            {
                LaserSaver = true;
                Laser.enabled = true;
            }
        }
    }
    
    public void DisablePrepare()
    {
        if (Laser != null)
        {
            Laser.enabled = false;
        }
        UpdateSaver = true; 
        
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
        if (HitParticles != null)
        {
            foreach (var AllPs in HitParticles)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
        
        LaserSaver = false;
    }
}