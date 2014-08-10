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
using System.Collections.Generic;
using UnityEngine;

public struct PhysicsArcadeHit
{
  public Vector2 delta;
  public int axis;
}

[AddComponentMenu("Physics Arcade/Physics Arcade")]
[DisallowMultipleComponent]
public class PhysicsArcade : MonoBehaviour
{

  public float TimeStep = 0.03333333333f;

  public Vector2 Gravity = new Vector2(0.0f, -10.0f);

  public bool RaycastTriggers = false;

  public bool[] collisionIgnoreMatrix = new bool[1024];

  public List<PhysicsArcadeCollisionGroup> groups = new List<PhysicsArcadeCollisionGroup>
  {
    new PhysicsArcadeCollisionGroup()
    {
      enabled = true,
      id = 0
    }
  };

  private bool mIterating;

  [SerializeField]
  public bool mCheckedExecutionOrder;

  void Awake()
  {
    if (groups == null || groups.Count == 0)
    {
      if (groups == null)
        groups = new List<PhysicsArcadeCollisionGroup>(4);
      PhysicsArcadeCollisionGroup collisionGroup = new PhysicsArcadeCollisionGroup();
      collisionGroup.id = 0;
      collisionGroup.enabled = true;
      groups.Add(collisionGroup);
    }

    if (collisionIgnoreMatrix == null)
    {
      collisionIgnoreMatrix = new bool[1024];

      for (int i = 0; i < 1024; i++)
      {
        collisionIgnoreMatrix[i] = true;
      }
    }

  }

  void OnEnable()
  {
    if (Internal.instance != null)
    {
      Debug.LogException(new Exception("Only one PhysicsArcade MonoBehaviour can be active at a time!"));
    }
    Internal.instance = this;
  }

  void OnDisable()
  {
    Internal.instance = null;
  }

  void Update()
  {
    Internal.delta = TimeStep;
    Internal.accumulator += Time.deltaTime;
    int steps = Mathf.FloorToInt(Internal.accumulator / TimeStep);

    if (steps > 0)
    {
      Internal.accumulator -= steps * TimeStep;
    }

    Internal.alpha = Internal.accumulator / TimeStep;

    mIterating = true;

    FixedUpdateArcade(steps);

    UpdateArcade();

    mIterating = false;
  }

  void FixedUpdateArcade(int substeps)
  {
    int count = groups.Count;
    PhysicsArcadeCollisionGroup group;
    for (int groupIndex = 0; groupIndex < count; groupIndex++)
    {
      group = groups[groupIndex];
      if (group.enabled == false)
        continue;
      group.mIterating = true;

      for (int substep = 0; substep < substeps; substep++)
      {
        bool collisionCheckPass = false;

        var it = group.rigidbodies.iterator;
        int rbCount = it.Count;

        for (int i = 0; i < rbCount; i++)
        {
          RigidbodyArcade rigidbody = it[i];
          collisionCheckPass |= rigidbody.FixedUpdateArcade();
        }

        var colit = group.colliders.iterator;

        if (true) // collisionCheckPass && group.resolveCollisions)
        {
          // Resolve collisions and apply transforms
          for (int i = 0; i < rbCount; i++)
          {
            RigidbodyArcade rigidbody = it[i];
            rigidbody.FixedPostUpdateArcade(colit);
          }
        }

        group.rigidbodies.SwapBuffers();
      }

      group.mIterating = false;
    }

  }

  void UpdateArcade()
  {
    int count = groups.Count;
    PhysicsArcadeCollisionGroup group;
    for (int i = 0; i < count; i++)
    {
      group = groups[i];
      if (group.enabled == false)
        continue;
      group.mIterating = true;

      var it = group.rigidbodies.iterator;
      int rbCount = it.Count;

      for (int j = 0; j < rbCount; j++)
      {
        RigidbodyArcade rigidbody = it[j];
        rigidbody.UpdateArcade();
      }

      // group
      group.mIterating = false;
    }
  }

  public void IgnoreCollision(int layerA, int layerB)
  {
    IgnoreCollision(layerA, layerB, true);
  }

  public void IgnoreCollision(int layerA, int layerB, bool value)
  {
    collisionIgnoreMatrix[layerA + (32 * layerB)] = value;
    collisionIgnoreMatrix[layerB + (32 * layerA)] = value;
    //Physics2D.IgnoreLayerCollision(layerA, layerB, value);
  }

  public bool GetLayerCollision(int layerA, int layerB)
  {
    return collisionIgnoreMatrix[layerA + (32 * layerB)];
  }

  public PhysicsArcadeCollisionGroup GetGroup(int id)
  {
    int count = groups.Count;
    PhysicsArcadeCollisionGroup group;
    for (int i = 0; i < count; i++)
    {
      group = groups[i];
      if (group.id == id)
        return group;
    }
#if UNITY_EDITOR
    group = new PhysicsArcadeCollisionGroup();
    group.id = id;
    group.enabled = true;
    groups.Add(group);
    Debug.LogWarning(String.Format("A PhysicsArcade group (id = {0}) was created automatically. This message will be replaced by an exception in runtime mode. Please create the group in the Editor in the PhysicsArcade object in the future to prevent this message!", id));
    return group;
#else
    throw new Exception("Collider is tied to a group that does not exist!");
#endif
  }


  public void Add(BoxColliderArcade boxCollider)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(boxCollider.group);

    if (group.mIterating)
    {
      group.colliders.modified = true;
      group.colliders.modifier.Add(boxCollider);
    }
    else
    {
      group.colliders.modifier.Add(boxCollider);
      group.colliders.iterator.Add(boxCollider);
    }
  }

  public void Remove(BoxColliderArcade boxCollider)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(boxCollider.group);

    if (group.mIterating)
    {
      group.colliders.modified = true;
      group.colliders.modifier.Remove(boxCollider);
    }
    else
    {
      group.colliders.modifier.Remove(boxCollider);
      group.colliders.iterator.Remove(boxCollider);
    }
  }

  public void Regroup(BoxColliderArcade boxCollider, int oldGroupId)
  {
    PhysicsArcadeCollisionGroup oldGroup = GetGroup(oldGroupId);
    PhysicsArcadeCollisionGroup newGroup = GetGroup(boxCollider.group);

    if (oldGroup == newGroup)
      return;

    if (newGroup.mIterating)
    {
      newGroup.colliders.modified = true;
      newGroup.colliders.modifier.Add(boxCollider);
    }
    else
    {
      newGroup.colliders.modifier.Add(boxCollider);
      newGroup.colliders.iterator.Add(boxCollider);
    }

    if (oldGroup.mIterating)
    {
      newGroup.colliders.modified = true;
      oldGroup.colliders.modifier.Remove(boxCollider);
    }
    else
    {
      oldGroup.colliders.modifier.Remove(boxCollider);
      oldGroup.colliders.iterator.Remove(boxCollider);
    }

  }

  public void Add(RigidbodyArcade rigidbodyArcadeObject)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(rigidbodyArcadeObject.group);

    if (group.mIterating)
    {
      group.rigidbodies.modified = true;
      group.rigidbodies.modifier.Add(rigidbodyArcadeObject);
    }
    else
    {
      group.rigidbodies.modifier.Add(rigidbodyArcadeObject);
      group.rigidbodies.iterator.Add(rigidbodyArcadeObject);
    }
  }

  public void Remove(RigidbodyArcade rigidbodyArcadeObject)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(rigidbodyArcadeObject.group);

    if (mIterating)
    {
      group.rigidbodies.modified = true;
      group.rigidbodies.modifier.Remove(rigidbodyArcadeObject);
    }
    else
    {
      group.rigidbodies.modifier.Remove(rigidbodyArcadeObject);
      group.rigidbodies.iterator.Remove(rigidbodyArcadeObject);
    }
  }

  public static class Internal
  {
    public static float accumulator;
    public static float alpha;
    public static float delta;
    public static PhysicsArcade instance;
  }

  #region internal classes

  public class PhysicsDoubleBuffer<T> where T : MonoBehaviour
  {
    public bool modified;

    public List<T> iterator
    {
      get { return mItems[mIteratorIndex]; }
    }

    public List<T> modifier
    {
      get { return mItems[mModifierIndex]; }
    }

    private int mIteratorIndex, mModifierIndex;
    private readonly List<T>[] mItems;

    public PhysicsDoubleBuffer()
    {
      mItems = new List<T>[2];
      mItems[0] = new List<T>(8);
      mItems[1] = new List<T>(8);
      mIteratorIndex = 0;
      mModifierIndex = 1;
    }

    public void SwapBuffers()
    {
      if (modified)
      {
        List<T> source = modifier, dest = iterator;
        dest.Clear();
        dest.Capacity = source.Capacity;
        int count = source.Count;
        for (int i = 0; i < count; i++)
        {
          dest.Add(source[i]);
        }
        modified = false;
      }

      int e = mIteratorIndex;
      mIteratorIndex = mModifierIndex;
      mModifierIndex = e;
    }

  }

  #endregion

}

[Serializable]
public class PhysicsArcadeCollisionGroup
{
  [SerializeField]
  public int id;

  [SerializeField]
  public bool enabled;

  [SerializeField]
  public bool resolveCollisions = true;

  [NonSerialized]
  public readonly PhysicsArcade.PhysicsDoubleBuffer<BoxColliderArcade> colliders;

  [NonSerialized]
  public readonly PhysicsArcade.PhysicsDoubleBuffer<RigidbodyArcade> rigidbodies;

  [NonSerialized]
  internal bool mIterating;

  public PhysicsArcadeCollisionGroup()
  {
    colliders = new PhysicsArcade.PhysicsDoubleBuffer<BoxColliderArcade>();
    rigidbodies = new PhysicsArcade.PhysicsDoubleBuffer<RigidbodyArcade>();
  }
}
