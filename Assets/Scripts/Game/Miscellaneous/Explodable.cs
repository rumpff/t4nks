﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explodable : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
}
