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

[AddComponentMenu("Physics Arcade/Point Collider Arcade")]
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class PointColliderArcade : ColliderArcade
{
  static readonly Vector2 kSkin = new Vector2(0.01f, 0.01f);

  // Can the edge of this BoxCollider collide?
  public override bool CanEdgeCollide(DirectionArcade direction)
  {
    return true;
  }

  // The bounds of this PointCollider
  public override Bounds bounds
  {
    get
    {
      if (mTransform == null)
        mTransform = gameObject.transform;

      Vector3 position = mTransform.position;
      return new Bounds(position + new Vector3(center.x, center.y, 0.0f), Vector2.Scale(kSkin, mTransform.localScale));
    }
  }
}

