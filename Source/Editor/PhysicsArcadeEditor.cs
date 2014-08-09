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

using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicsArcade))]
public class PhysicsArcadeEditor : Editor
{
  String[] cachedLayerName = new String[32];
  private GUIStyle rightLabel;


  [MenuItem("Edit/Add PhysicsArcade GameObject")]
  static void AddPhysicsGameObject()
  {
    GameObject gameObject = new GameObject("PhysicsArcade");
    gameObject.AddComponent<PhysicsArcade>();
  }

  static void CheckExecutionOrder(PhysicsArcade t)
  {
    if (t.mCheckedExecutionOrder)
      return;
    t.mCheckedExecutionOrder = true;

    MonoScript monoScript = MonoScript.FromMonoBehaviour(t);
    monoScript.hideFlags = HideFlags.HideAndDontSave; // ??????

    if (MonoImporter.GetExecutionOrder(monoScript) >= 0)
    {
      MonoImporter.SetExecutionOrder(monoScript, -100);
    }

  }

  public override void OnInspectorGUI()
  {

    PhysicsArcade t = (PhysicsArcade)this.target;

    CheckExecutionOrder(t);

    for (int i = 0; i < 32; i++)
    {
      cachedLayerName[i] = LayerMask.LayerToName(i);
    }

    EditorGUIUtility.labelWidth = 115f;
    rightLabel = GUI.skin.GetStyle("RightLabel");

    InspectCommonProperties(t);
    InspectCollisionMatrix(t);
    InspectGroups(t);
  }

  private void InspectCommonProperties(PhysicsArcade t)
  {

    GUILayout.BeginVertical();

    t.Gravity = EditorGUILayout.Vector2Field("Gravity", t.Gravity);

    t.TimeStep = EditorGUILayout.FloatField("Time Step", t.TimeStep);

    t.RaycastTriggers = EditorGUILayout.Toggle("Raycast Hit triggers", t.RaycastTriggers);

    GUILayout.EndVertical();

  }

  private bool inspectCollisionMatrix = false;

  private void InspectCollisionMatrix(PhysicsArcade t)
  {
    inspectCollisionMatrix = EditorGUILayout.Foldout(inspectCollisionMatrix, "Collision Matrix");

    if (inspectCollisionMatrix == false)
      return;

    EditorGUILayout.BeginHorizontal(GUILayout.Height(100));
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
    Rect headerSize = GUILayoutUtility.GetLastRect();

    int cols = 0;

    for (int i = 0; i < 32; i++)
    {
      int k = 31 - i;

      if (cachedLayerName[k].Length == 0)
        continue;

      Vector2 c = new Vector2(headerSize.xMin + 68 + (cols * 14.0f) + 0.5f, headerSize.yMax - 60.0f);
      GUIUtility.RotateAroundPivot(90, new Vector2(c.x + 50.0f, c.y + 10.0f));
      GUI.Label(new Rect(c.x, c.y, 100, 20), cachedLayerName[k], rightLabel);
      GUI.matrix = Matrix4x4.identity;
      cols++;
    }

    GUILayout.BeginVertical();

    for (int i = 0; i < 32; i++)
    {
      if (String.IsNullOrEmpty(cachedLayerName[i]))
        continue;

      GUILayout.BeginHorizontal();
      GUILayout.Label(cachedLayerName[i], rightLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 5));

      int c = 0;
      for (int e = (31 - i); e > -1; e--)
      {

        if (String.IsNullOrEmpty(cachedLayerName[e]))
          continue;

        if (c == cols)
          break;
        c++;

        int k = 31 - e;

        bool value = !t.GetLayerCollision(i, k);
        bool newValue = EditorGUILayout.Toggle(value, GUILayout.Width(10));

        if (newValue != value)
        {
          t.IgnoreCollision(i, k, !newValue);

          Debug.Log(String.Format("A{0},{1} = {2}", i, k, t.GetLayerCollision(i, k)));
          Debug.Log(String.Format("B{0},{1} = {2}", k, i, t.GetLayerCollision(i, k)));
        }
      }

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      cols--;
    }

    GUILayout.EndVertical();
  }

  private bool inspectGroups = false;

  private void InspectGroups(PhysicsArcade t)
  {
    inspectGroups = EditorGUILayout.Foldout(inspectGroups, "Groups");

    if (inspectGroups == false)
      return;

    int nbGroups = t.groups.Count;

    bool guiEnabled = GUI.enabled;
    GUILayout.BeginVertical();
    int highestId = 1;
    for (int i = 0; i < nbGroups; i++)
    {
      GUILayout.BeginHorizontal();
      PhysicsArcadeCollisionGroup collisionGroup = t.groups[i];

      collisionGroup.enabled = GUILayout.Toggle(collisionGroup.enabled, collisionGroup.enabled ? "Enabled" : "Disabled", GUI.skin.button, GUILayout.Height(16));
      collisionGroup.id = EditorGUILayout.IntField(collisionGroup.id);
      if (collisionGroup.id >= highestId)
        highestId = collisionGroup.id;

      if (nbGroups == 1)
      {
        GUI.enabled = false;
      }

      if (GUILayout.Button("x"))
      {
        t.groups.RemoveAt(i);
        break;
      }

      GUI.enabled = guiEnabled;


      GUILayout.EndHorizontal();
    }

    if (GUILayout.Button("Add"))
    {
      t.groups.Add(new PhysicsArcadeCollisionGroup()
      {
        enabled = true,
        id = highestId + 1
      });
    }

    GUILayout.EndVertical();


  }
}
