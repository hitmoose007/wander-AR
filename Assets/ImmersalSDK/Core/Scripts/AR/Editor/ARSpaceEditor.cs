/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

#if UNITY_EDITOR
using UnityEditor;

namespace Immersal.AR
{
    [CustomEditor(typeof(ARSpace))]
    public class ARSpaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Place AR Maps and all content under this object", MessageType.Info);
            DrawDefaultInspector();
        }
    }
}
#endif