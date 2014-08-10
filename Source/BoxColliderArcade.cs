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

  //
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

  //
  public Vector2 center;

  //
  public DirectionArcade collision = DirectionArcade.All;

  //
  public DirectionArcade touching = DirectionArcade.None;

  //
  public bool touchingDown
  {
    get { return (touching & DirectionArcade.Down) != 0; }
  }

  public bool touchingUp
  {
    get { return (touching & DirectionArcade.Up) != 0; }
  }

  public bool touchingHorizontal
  {
    get { return (touching & DirectionArcade.Horizontal) != 0; }
  }

  public bool touchingVertical
  {
    get { return (touching & DirectionArcade.Vertical) != 0; }
  }

  //
  public bool trigger;

  //
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

  //
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

  //
  [SerializeField]
  private int mGroup;

  //
  [SerializeField]
  private Vector2 mSize = new Vector2(1.0f, 1.0f);

  //
  [SerializeField]
  private Vector2 mHalfSize = new Vector2(0.5f, 0.5f);

  //
  private GameObject mGameObject;

  //
  internal Transform mTransform;

  //
  private bool mApplicationIsQuitting;

  //
  private bool mCachedEnabled;

  // 
  public bool isEnabled
  {
    get { return mCachedEnabled; }
  }

  //
  private RigidbodyArcade mRigidbody;


  void OnEnable()
  {
    mCachedEnabled = true;
    mGameObject = gameObject;
    mTransform = gameObject.transform;
    mRigidbody = GetComponent<RigidbodyArcade>();
    PhysicsArcade.Internal.instance.Add(this);
  }

  void OnDisable()
  {
    mCachedEnabled = false;

    if (mApplicationIsQuitting == false)
    {
      PhysicsArcade.Internal.instance.Remove(this);
    }
  }

  void OnApplicationQuit()
  {
    mApplicationIsQuitting = true;
  }

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

