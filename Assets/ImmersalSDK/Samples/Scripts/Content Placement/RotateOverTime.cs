﻿/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_Axis = new Vector3(0f, 1, 0f);
    [SerializeField]
    private float m_Speed = 40f;

    void Update()
    {
        transform.Rotate(m_Axis, m_Speed * Time.deltaTime);
    }
}
