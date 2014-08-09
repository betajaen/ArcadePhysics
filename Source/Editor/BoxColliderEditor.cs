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

[CustomEditor(typeof(BoxColliderArcade))]
public class BoxColliderArcadeEditor : Editor
{

  public override void OnInspectorGUI()
  {
    BoxColliderArcade t = (BoxColliderArcade)this.target;

    t.size = EditorGUILayout.Vector2Field("Size", t.size);

    t.center = EditorGUILayout.Vector2Field("Center", t.center);

    t.trigger = EditorGUILayout.Toggle("Trigger", t.trigger);

    t.group = EditorGUILayout.IntField("Group", t.group);

    EditorGUILayout.Foldout(true, "Collision Edges");

    GUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    GUILayout.BeginVertical(GUILayout.Width(100));

    GUILayout.BeginHorizontal();
    bool oldValue = GetCollisionDirection(t, DirectionArcade.Up);
    bool newValue = GUILayout.Toggle(oldValue, "U", GUI.skin.button, GUILayout.Height(20));
    SetCollisionDirection(t, DirectionArcade.Up, oldValue, newValue);
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    oldValue = GetCollisionDirection(t, DirectionArcade.Left);
    newValue = GUILayout.Toggle(oldValue, "L", GUI.skin.button, GUILayout.Width(20), GUILayout.Height(60));
    SetCollisionDirection(t, DirectionArcade.Left, oldValue, newValue);

    oldValue = GetCollisionAll(t);
    newValue = GUILayout.Toggle(oldValue, System.String.Empty, GUI.skin.textArea, GUILayout.Width(60), GUILayout.Height(60));
    SetCollisionDirectionAll(t, oldValue, newValue);

    oldValue = GetCollisionDirection(t, DirectionArcade.Right);
    newValue = GUILayout.Toggle(oldValue, "R", GUI.skin.button, GUILayout.Width(20), GUILayout.Height(60));
    SetCollisionDirection(t, DirectionArcade.Right, oldValue, newValue);

    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    oldValue = GetCollisionDirection(t, DirectionArcade.Down);
    newValue = GUILayout.Toggle(oldValue, "D", GUI.skin.button, GUILayout.Height(20));
    SetCollisionDirection(t, DirectionArcade.Down, oldValue, newValue);
    GUILayout.EndHorizontal();

    GUILayout.EndVertical();
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
  }

  static bool GetCollisionAll(BoxColliderArcade collider)
  {
    return collider.collision == DirectionArcade.All;
  }

  private static void SetCollisionDirectionAll(BoxColliderArcade collider, bool oldValue, bool newValue)
  {
    if (oldValue != newValue)
    {
      if (newValue)
        collider.collision = DirectionArcade.All;
      else
        collider.collision = DirectionArcade.None;
    }
  }

  static bool GetCollisionDirection(BoxColliderArcade collider, DirectionArcade direction)
  {
    return (collider.collision & direction) != 0;
  }

  static void SetCollisionDirection(BoxColliderArcade collider, DirectionArcade direction, bool oldValue, bool newValue)
  {
    if (oldValue != newValue)
    {
      if (newValue)
        collider.collision |= direction;
      else
        collider.collision &= ~direction;
    }
  }

  public void OnSceneGUI()
  {
    BoxColliderArcade t = (BoxColliderArcade)this.target;

    Color res = Handles.color;

    Handles.color = Color.green;

    Bounds bounds = t.bounds;
    Vector3 min = bounds.min, max = bounds.max;

    if ((t.collision & DirectionArcade.Left) != 0)
      Handles.color = Color.green;
    else
      Handles.color = Color.grey;

    Handles.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z));

    if ((t.collision & DirectionArcade.Up) != 0)
      Handles.color = Color.green;
    else
      Handles.color = Color.grey;

    Handles.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z));

    if ((t.collision & DirectionArcade.Right) != 0)
      Handles.color = Color.green;
    else
      Handles.color = Color.grey;

    Handles.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z));

    if ((t.collision & DirectionArcade.Down) != 0)
      Handles.color = Color.green;
    else
      Handles.color = Color.grey;

    Handles.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(min.x, min.y, min.z));

    Handles.color = res;
  }

}

