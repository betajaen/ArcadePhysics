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

[Flags]
public enum DirectionArcade
{
  None = 0,
  Up = 1,
  Down = 2,
  Left = 4,
  Right = 8,
  Horizontal = Left | Right,
  Vertical = Up | Down,
  All = Horizontal | Vertical
}


[AddComponentMenu("Physics Arcade/Rigidbody Arcade")]
[DisallowMultipleComponent]
public class RigidbodyArcade : MonoBehaviour
{

  // 
  public Vector2 lastPosition;

  // 
  public Vector2 nextPosition;

  // 
  public Vector2 interpolatedPosition;

  // 
  public bool interpolatePosition = true;

  // 
  public Vector2 acceleration;

  // 
  public Vector2 velocity;

  //
  public Vector2 maxVelocity = new Vector2(15.0f, 15.0f);

  //
  public Vector2 drag = new Vector2(0.05f, 0.05f);

  //
  public float gravityScale = 1.0f;

  //
  public float elasticity = 0.0f;

  //
  public DirectionArcade touching;

  //
  public DirectionArcade lastTouching;

  //
  public bool kinematic;

  //
  public bool platform;

  //
  public bool platformAttachX;

  //
  public bool platformAttachY = true;

  //
  public int group
  {
    get { return mCollider.group; }
  }

  //
  public ColliderArcade colliderArcade
  {
    get { return mCollider; }
  }

  private GameObject mGameObject;
  private Transform mTransform;
  private bool mApplicationIsQuitting;
  internal bool mTransformUpdateNeeded;
  internal Vector2 mTranslation;
  internal Vector2 mDelta;
  internal Vector2 mDeltaAbs;
  internal Vector2 mDeltaSign;
  private ColliderArcade mCollider;
  private List<ColliderArcade> mFriendlyIntersectingColliders;

  void Awake()
  {
    if (mFriendlyIntersectingColliders == null)
    {
      mFriendlyIntersectingColliders = new List<ColliderArcade>(4);
    }
  }

  void OnApplicationQuit()
  {
    mApplicationIsQuitting = true;
  }

  void OnEnable()
  {
    mGameObject = gameObject;
    mTransform = gameObject.transform;
    mCollider = GetComponent<ColliderArcade>();
    PhysicsArcade.Internal.instance.Add(this);
  }

  void OnDisable()
  {
    if (mApplicationIsQuitting == false)
    {
      mCollider.OnRigidbodyRemoved();
      PhysicsArcade.Internal.instance.Remove(this);
    }
  }

  public bool FixedUpdateArcade()
  {
    lastPosition = mTransform.position;
    nextPosition = lastPosition;

    Vector2 gravity = gravityScale * PhysicsArcade.Internal.instance.Gravity;
    float time = PhysicsArcade.Internal.delta;

    float velDeltaX = (GetVelocity(velocity.x, acceleration.x + gravity.x, drag.x, maxVelocity.x, time) - velocity.x) * 0.5f;
    velocity.x += velDeltaX;
    mTranslation.x = velocity.x * time;
    velocity.x += velDeltaX;

    if (mCollider.touchingDown == false)
    {
      acceleration.y += gravity.y;
    }

    float velDeltaY = (GetVelocity(velocity.y, acceleration.y, drag.y, maxVelocity.y, time) - velocity.y) * 0.5f;
    velocity.y += velDeltaY;
    mTranslation.y = velocity.y * time;
    velocity.y += velDeltaY;

    acceleration.x = 0.0f;
    acceleration.y = 0.0f;

    nextPosition = lastPosition + mTranslation;

    mDelta = nextPosition - lastPosition;

    mDeltaAbs.x = Mathf.Abs(mDelta.x);
    mDeltaAbs.y = Mathf.Abs(mDelta.y);

    mDeltaSign.x = Mathf.Sign(mDelta.x);
    mDeltaSign.y = Mathf.Sign(mDelta.y);

    mTransformUpdateNeeded = mDeltaAbs.y > Mathf.Epsilon; // || mDeltaAbs.x > Mathf.Epsilon

    return mTransformUpdateNeeded;
  }

  public void FixedPostUpdateArcade(List<ColliderArcade> colliders)
  {
    int colliderCount = colliders.Count;

    PhysicsArcadeHit hit = new PhysicsArcadeHit();
    Vector2 adjustment = new Vector2();
    mCollider.touching = DirectionArcade.None;

    for (int i = 0; i < colliderCount; i++)
    {
      ColliderArcade collider = colliders[i];
      if (collider == mCollider)
        continue;

      bool layerCollisionIgnored = PhysicsArcade.Internal.instance.GetLayerCollision(mGameObject.layer, collider.gameObject.layer);

      if (layerCollisionIgnored)
        continue;

      if (ColliderArcade.IntersectionMoving(mCollider, collider, nextPosition, ref hit) == false)
        continue;
      //if (mCollider.MovingIntersection(collider, nextPosition, ref hit) == false)
      // continue;

      if (hit.axis == 0)
      {
        int direction = hit.delta.x < 0.0f ? -1 : 1;
        if (direction == 1)
        {
          if (mCollider.CanEdgeCollide(DirectionArcade.Right))
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mCollider.touching |= DirectionArcade.Right;
              adjustment.x += hit.delta.x;
              velocity.x = 0.0f;
            }
            else
            {
              mFriendlyIntersectingColliders.Remove(collider);
            }
          }
          else
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mFriendlyIntersectingColliders.Add(collider);
            }
          }
        }
        else
        {
          if (mCollider.CanEdgeCollide(DirectionArcade.Left))
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mCollider.touching |= DirectionArcade.Left;
              adjustment.x += hit.delta.y;
              velocity.x = 0.0f;
            }
            else
            {
              mFriendlyIntersectingColliders.Remove(collider);
            }
          }
          else
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mFriendlyIntersectingColliders.Add(collider);
            }
          }
        }
      }
      else
      {
        int direction = hit.delta.y < 0.0f ? -1 : 1;
        if (direction == 1)
        {
          if (mCollider.CanEdgeCollide(DirectionArcade.Down))
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mCollider.touching |= DirectionArcade.Down;
              adjustment.y += hit.delta.y;
              velocity.y = 0.0f;
            }
            else
            {
              mFriendlyIntersectingColliders.Remove(collider);
            }
          }
          else
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mFriendlyIntersectingColliders.Add(collider);
            }
          }
        }
        else
        {
          if (mCollider.CanEdgeCollide(DirectionArcade.Up))
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mCollider.touching |= DirectionArcade.Up;
              adjustment.y += hit.delta.y;
              velocity.y = 0.0f;
            }
            else
            {
              mFriendlyIntersectingColliders.Remove(collider);
            }
          }
          else
          {
            if (mFriendlyIntersectingColliders.Contains(collider) == false)
            {
              mFriendlyIntersectingColliders.Add(collider);
            }
          }
        }
      }

    }

    nextPosition += adjustment;

    mDelta = nextPosition - lastPosition;

    mDeltaAbs.x = Mathf.Abs(mDelta.x);
    mDeltaAbs.y = Mathf.Abs(mDelta.y);

    mDeltaSign.x = Mathf.Sign(mDelta.x);
    mDeltaSign.y = Mathf.Sign(mDelta.y);

    mTransformUpdateNeeded = mDeltaAbs.y > Mathf.Epsilon || mDeltaAbs.x > Mathf.Epsilon;
  }

  public void UpdateArcade()
  {
    if (mTransformUpdateNeeded)
    {
      if (false && interpolatePosition)
      {
        // TODO: Interpolate position
        // TODO: Switch off mTransformUpdateNeeded when ratio is 1 otherwise keep it on
      }
      else
      {
        mTransformUpdateNeeded = false;
        interpolatedPosition = nextPosition;
        mTransform.position = nextPosition;
      }
    }
  }

  private static float GetVelocity(float velocity, float acceleration, float drag, float max_velocity, float time)
  {
    if (Mathf.Abs(acceleration) > Mathf.Epsilon)
    {
      velocity += acceleration * time;
    }
    else if (drag > 0.0f)
    {
      float drag_t = drag * time;
      if (velocity - drag_t > 0.0f)
        velocity -= drag_t;
      else if (velocity + drag_t < 0.0f)
        velocity += drag_t;
      else
        velocity = 0.0f;
    }
    if (velocity > max_velocity)
      velocity = max_velocity;
    else if (velocity < -max_velocity)
      velocity = -max_velocity;
    return velocity;
  }


}
