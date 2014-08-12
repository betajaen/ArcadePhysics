using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RigidbodyArcade))]
[RequireComponent(typeof(PointColliderArcade))]
public class Projectile : MonoBehaviour
{
  public ProjectileManager Manager;

  private Transform mTransform;
  private RigidbodyArcade mRigidBody;
  private ColliderArcade mCollider;
  private float mTime;

  void Start()
  {
    Manager.Add(this);
    gameObject.SetActive(false);
    mTransform = transform;
    mRigidBody = GetComponent<RigidbodyArcade>();
    mCollider = mRigidBody.colliderArcade;
  }

  void FixedUpdate()
  {
    mTime += Time.fixedDeltaTime;
    if (mTime > 0.5f)
    {
      gameObject.SetActive(false);
      Manager.Add(this);
    }
  }

  public void Reuse(Vector2 position, Vector2 velocity)
  {
    Manager.Remove(this);
    gameObject.SetActive(true);
    mTransform = transform;
    mRigidBody = GetComponent<RigidbodyArcade>();
    mCollider = mRigidBody.colliderArcade;
    mTime = 0.0f;


    mRigidBody.lastPosition = position;
    mRigidBody.velocity = velocity;
    mTransform.position = position;
  }


}
