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

#region Physics Arcade

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

  public void Add(ColliderArcade collider)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(collider.group);

    if (group.mIterating)
    {
      group.colliders.modified = true;
      group.colliders.modifier.Add(collider);
    }
    else
    {
      group.colliders.modifier.Add(collider);
      group.colliders.iterator.Add(collider);
    }
  }

  public void Remove(ColliderArcade collider)
  {
    PhysicsArcadeCollisionGroup group = GetGroup(collider.group);

    if (group.mIterating)
    {
      group.colliders.modified = true;
      group.colliders.modifier.Remove(collider);
    }
    else
    {
      group.colliders.modifier.Remove(collider);
      group.colliders.iterator.Remove(collider);
    }
  }

  public void Regroup(ColliderArcade collider, int oldGroupId)
  {
    PhysicsArcadeCollisionGroup oldGroup = GetGroup(oldGroupId);
    PhysicsArcadeCollisionGroup newGroup = GetGroup(collider.group);

    if (oldGroup == newGroup)
      return;

    if (newGroup.mIterating)
    {
      newGroup.colliders.modified = true;
      newGroup.colliders.modifier.Add(collider);
    }
    else
    {
      newGroup.colliders.modifier.Add(collider);
      newGroup.colliders.iterator.Add(collider);
    }

    if (oldGroup.mIterating)
    {
      newGroup.colliders.modified = true;
      oldGroup.colliders.modifier.Remove(collider);
    }
    else
    {
      oldGroup.colliders.modifier.Remove(collider);
      oldGroup.colliders.iterator.Remove(collider);
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

#endregion

////////////////////////////////////////////////////////////////////////

#region Collision Group

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
  public readonly PhysicsArcade.PhysicsDoubleBuffer<ColliderArcade> colliders;

  [NonSerialized]
  public readonly PhysicsArcade.PhysicsDoubleBuffer<RigidbodyArcade> rigidbodies;

  [NonSerialized]
  internal bool mIterating;

  public PhysicsArcadeCollisionGroup()
  {
    colliders = new PhysicsArcade.PhysicsDoubleBuffer<ColliderArcade>();
    rigidbodies = new PhysicsArcade.PhysicsDoubleBuffer<RigidbodyArcade>();
  }
}

#endregion

////////////////////////////////////////////////////////////////////////

#region ColliderArcade

public abstract class ColliderArcade : MonoBehaviour
{

  // Offset of the collider, relative to the GameObject position.
  // Default: 0, 0
  public Vector2 center;

  // Current edges of the Box collider that are touching another Box collider
  // Default: None
  public DirectionArcade touching = DirectionArcade.None;

  // Is this collider's down edge touching another collider?
  // Default: false
  public bool touchingDown
  {
    get { return (touching & DirectionArcade.Down) != 0; }
  }

  // Is this collider's up edge touching another collider?
  // Default: false
  public bool touchingUp
  {
    get { return (touching & DirectionArcade.Up) != 0; }
  }

  // Is this collider's left or right edge touching another collider?
  // Default: false
  public bool touchingHorizontal
  {
    get { return (touching & DirectionArcade.Horizontal) != 0; }
  }

  // Is this collider's up or down edge touching another collider?
  // Default: false
  public bool touchingVertical
  {
    get { return (touching & DirectionArcade.Vertical) != 0; }
  }

  // What group index does this collider belong to?
  // Default: 0
  // See: PhysicsArcade.group
  public int group
  {
    get
    {
      return mGroup;
    }
    set
    {
      if (mGroup == value)
        return;
      if (Application.isPlaying)
      {
        int oldValue = mGroup;
        mGroup = value;
        if (mCachedEnabled)
        {
          PhysicsArcade.Internal.instance.Regroup(this, oldValue);
        }
      }
      else
      {
        mGroup = value;
      }
    }
  }


  // Internal - BoxCollider Group
  // See: BoxCollider.group
  [SerializeField]
  private int mGroup;

  // Internal - Cached GameObject reference
  private GameObject mGameObject;

  // Internal - Cached Transform reference
  internal Transform mTransform;

  // Internal - The application is quitting, the Box Collider should avoid cleanup
  private bool mApplicationIsQuitting;

  // Internal - Is this BoxCollider enabled or not.
  private bool mCachedEnabled;

  // Internal - Cached RigidBodyArcade, if it has one, otherwise null.
  private RigidbodyArcade mRigidbody;

  // Enable event
  // BoxCollider is refreshed, and added to the PhysicsArcade frame events, only if the Application isn't quitting.
  void OnEnable()
  {
    mCachedEnabled = true;
    mGameObject = gameObject;
    mTransform = gameObject.transform;
    mRigidbody = GetComponent<RigidbodyArcade>();
    PhysicsArcade.Internal.instance.Add(this);
  }

  // Disabled event
  // BoxCollider removed from the PhysicsArcade frame events, only if the Application isn't quitting.
  void OnDisable()
  {
    mCachedEnabled = false;

    if (mApplicationIsQuitting == false)
    {
      PhysicsArcade.Internal.instance.Remove(this);
    }
  }

  // Application Quit
  // A flag is set to stop Disable events firing, avoiding any possible exceptions from deleted GameObjects.
  void OnApplicationQuit()
  {
    mApplicationIsQuitting = true;
  }

  // OnRigidBody removed
  // When a rigidbody is removed from the GameObject as this BoxCollider, triggered by PhysicsArcade
  public void OnRigidbodyRemoved()
  {
    mRigidbody = null;
  }

  // Can the edge of this BoxCollider collide?
  public abstract bool CanEdgeCollide(DirectionArcade direction);

  public abstract Bounds bounds
  {
    get;
  }

  public static bool IntersectionMoving(ColliderArcade moving, ColliderArcade stationary, Vector2 movingNextPosition, ref PhysicsArcadeHit hit)
  {
    Type movingType = moving.GetType();
    Type stationaryType = stationary.GetType();

    if (movingType == typeof(BoxColliderArcade))
    {
      if (stationaryType == typeof(BoxColliderArcade))
        return IntersectionMovingBoxVsBox(moving as BoxColliderArcade, stationary as BoxColliderArcade,
          movingNextPosition, ref hit);
      if (stationaryType == typeof(PointColliderArcade))
        return IntersectionMovingBoxVsPoint(moving as BoxColliderArcade, stationary as PointColliderArcade,
          movingNextPosition, ref hit);
    }
    else if (movingType == typeof(PointColliderArcade))
    {
      if (stationaryType == typeof(BoxColliderArcade))
        return IntersectionMovingLineVsBox(moving as PointColliderArcade, stationary as BoxColliderArcade,
          movingNextPosition, ref hit);
      if (stationaryType == typeof(PointColliderArcade))
        return IntersectionMovingLineVsPoint(moving as PointColliderArcade, stationary as PointColliderArcade,
          movingNextPosition, ref hit);
    }

    return false;
  }

  private static bool IntersectionMovingBoxVsBox(BoxColliderArcade moving, BoxColliderArcade stationary, Vector2 next, ref PhysicsArcadeHit hit)
  {
    Vector2 otherPos = (Vector2)stationary.mTransform.position + stationary.center;
    Vector2 pos = next + moving.center;

    Vector2 size = Vector2.Scale(moving.mHalfSize, moving.mTransform.localScale);
    Vector2 otherSize = Vector2.Scale(stationary.mHalfSize, stationary.mTransform.localScale);

    float deltaX = pos.x - otherPos.x;
    float penetrationX = (size.x + otherSize.x) - Mathf.Abs(deltaX);

    if (penetrationX <= 0.0f)
    {
      return false;
    }

    float deltaY = pos.y - otherPos.y;
    float penetrationY = (size.y + otherSize.y) - Mathf.Abs(deltaY);

    if (penetrationY <= 0.0f)
    {
      return false;
    }

    if (penetrationX < penetrationY)
    {
      float signX = Mathf.Sign(deltaX);
      hit.delta.x = penetrationX * signX;
      hit.delta.y = 0.0f;
      hit.axis = 0;
    }
    else
    {
      float signY = Mathf.Sign(deltaY);
      hit.delta.x = 0.0f;
      hit.delta.y = penetrationY * signY;
      hit.axis = 1;
    }

    return true;
  }

  private static bool IntersectionMovingBoxVsPoint(BoxColliderArcade moving, PointColliderArcade stationary, Vector2 next, ref PhysicsArcadeHit hit)
  {
    return false;
  }

  private static bool IntersectionMovingLineVsBox(PointColliderArcade moving, BoxColliderArcade stationary, Vector2 next, ref PhysicsArcadeHit hit)
  {
    return false;
  }

  private static bool IntersectionMovingLineVsPoint(PointColliderArcade moving, PointColliderArcade stationary, Vector2 next, ref PhysicsArcadeHit hit)
  {
    return false;
  }

}

#endregion
