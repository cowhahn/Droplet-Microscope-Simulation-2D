using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public int gridNumber;
    public int refreshFrequency;
    public float dimension;
    public bool overRelaxation;
    private float[,] _horizontalVelocities;
    private float[,] _verticalVeloctities;
    private float _gravity = -9.8f;
    private float _GridDimension;

    void InitializeGrid()
    {
        _horizontalVelocities = new float[gridNumber + 1, gridNumber];
        _verticalVeloctities = new float[gridNumber, gridNumber + 1];
    }
    void UpdateExternalForces()
    {
        for (int i = 0; i < gridNumber - 1; i++)
        {
            for (int j = 1; j < gridNumber - 1; j++)
            {
                _verticalVeloctities[i, j] += _gravity * Time.fixedDeltaTime;
            }
        }
    }
    void DrawGrid()
    {
        
    }
    void Start()
    {
        Time.fixedDeltaTime = 1 / refreshFrequency;
        _GridDimension = dimension / gridNumber;
        InitializeGrid();
        UpdateExternalForces();
    } 
    void FixedUpdate()
    {
        //UpdateExternalForces();
    }
}
