/*
    ArcadePhysics
    -------------
    
    Copyright (c) 2014 Robin Southern

                                                                                  
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
                                                                                  
    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.
                                                                                  
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE. 
    
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RigidbodyArcade))]
public class RigidbodyArcadeEditor : Editor
{

  public override void OnInspectorGUI()
  {

    RigidbodyArcade t = (RigidbodyArcade)this.target;

    t.acceleration = EditorGUILayout.Vector2Field("Acceleration", t.acceleration);

    t.velocity = EditorGUILayout.Vector2Field("Velocity", t.velocity);

    t.maxVelocity = EditorGUILayout.Vector2Field("Max Velocity", t.maxVelocity);

    t.drag = EditorGUILayout.Vector2Field("Drag", t.drag);

    t.gravityScale = EditorGUILayout.FloatField("Gravity Scale", t.gravityScale);

    t.elasticity = EditorGUILayout.FloatField("Elasticity", t.elasticity);

    t.interpolatePosition = EditorGUILayout.Toggle("Interpolate Position", t.interpolatePosition);

    t.kinematic = EditorGUILayout.Toggle("Kinematic", t.kinematic);

    if (t.kinematic == false && t.platform)
      t.platform = false;

    if (GUI.enabled)
    {
      GUI.enabled = t.kinematic;
    }

    t.platform = EditorGUILayout.Toggle("Moving Platform", t.platform);

    if (GUI.enabled)
    {
      GUI.enabled = t.platform;
    }

    t.platformAttachX = EditorGUILayout.Toggle("Attach X", t.platformAttachX);

    t.platformAttachX = EditorGUILayout.Toggle("Attach Y", t.platformAttachX);

    GUI.enabled = true;

  }
}

