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

[AddComponentMenu("Physics Arcade/Box Collider Arcade")]
[DisallowMultipleComponent]
public class BoxColliderArcade : MonoBehaviour
{

  // size - Size of the Box collider
  // Default: 1, 1
  public Vector2 size
  {
    get
    {
      return mSize;
    }
    set
    {
      mSize = value;
      mHalfSize = value * 0.5f;
    }
  }

  // Offset of the Box collider, relative to the GameObject position.
  // Default: 0, 0
  public Vector2 center;

  // Allowable collisions
  // Default: All (Left, Right, Up and Down)
  public DirectionArcade collision = DirectionArcade.All;

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

  // Is this collider a trigger, i.e. Collisions are allowed through but
  // reported to a OnTriggerArcade function.
  // Default: false
  public bool trigger;

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

  // Is the BoxCollider enabled.
  // Same as MonoBehaviour.enabled
  public bool isEnabled
  {
    get { return mCachedEnabled; }
  }

  // The bounds of this BoxCollider
  public Bounds bounds
  {
    get
    {
      if (mTransform == null)
        mTransform = gameObject.transform;

      Vector3 position = mTransform.position;
      return new Bounds(position + new Vector3(center.x, center.y, 0.0f), Vector2.Scale(mSize, mTransform.localScale));
    }
  }

  // Internal - BoxCollider Group
  // See: BoxCollider.group
  [SerializeField]
  private int mGroup;

  // Internal - BoxCollider Size
  // See: BoxCollider.size
  [SerializeField]
  private Vector2 mSize = new Vector2(1.0f, 1.0f);

  // Internal - BoxCollider's cached Size / 2
  [SerializeField]
  private Vector2 mHalfSize = new Vector2(0.5f, 0.5f);

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

  public bool MovingIntersection(BoxColliderArcade other, Vector2 nextWorldPosition, ref PhysicsArcadeHit hit)
  {
    Vector2 otherPos = (Vector2)other.mTransform.position + other.center;
    Vector2 pos = nextWorldPosition + center;

    Vector2 size = Vector2.Scale(mHalfSize, mTransform.localScale);
    Vector2 otherSize = Vector2.Scale(other.mHalfSize, other.mTransform.localScale);

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

  public bool CanCollide(DirectionArcade direction)
  {
    return (collision & direction) != 0;
  }

}

