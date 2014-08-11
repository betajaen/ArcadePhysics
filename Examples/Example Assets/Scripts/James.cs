using UnityEngine;
using System.Collections;

public class James : MonoBehaviour
{

  public GameObject wand;
  public float wandDistance = 0.32f;

  private Camera mCamera;
  private Transform mTransform;
  private Vector2 mScreenCoords;
  private float mWandAngle;
  private Quaternion mWandRotation;
  private int mMovementFlags;

  private RigidbodyArcade mRigidBody;
  private float mWandPower;
  private float mWandTimer;
  public ProjectileManager mProjectileManager;

  void Start()
  {
    mCamera = Camera.main;
    mTransform = transform;
    mRigidBody = GetComponent<RigidbodyArcade>();
    mRigidBody.drag.x = mRigidBody.maxVelocity.x * 4.0f;
    mProjectileManager = GetComponent<ProjectileManager>();
    mWandPower = 1.0f;
    mWandTimer = 0.0f;
  }

  void FixedUpdate()
  {
    DoWand();
    DoMove();
  }

  void DoMove()
  {
    mRigidBody.acceleration.x = 0.0f;

    if (Input.GetKey(KeyCode.A))
      mRigidBody.acceleration.x = -mRigidBody.maxVelocity.x * 4.0f;

    if (Input.GetKey(KeyCode.D))
      mRigidBody.acceleration.x = mRigidBody.maxVelocity.x * 4.0f;

    mWandTimer += Time.fixedDeltaTime;

    if (Input.GetMouseButton(0) && mWandTimer > 0.01f)
    {
      mWandTimer = 0.0f;
      var bg = mWandRotation * new Vector2(1.0f, 0.0f);
      mRigidBody.acceleration.x -= bg.x * mWandPower * (mRigidBody.colliderArcade.touchingDown ? 1.0f : 10.0f);
      mRigidBody.acceleration.y -= bg.y * mWandPower * 10.0f;
      var projectile = mProjectileManager.GetProjectile();
      projectile.Reuse(mTransform.position + mWandRotation * new Vector2(0.42f, 0.0f), mWandRotation * new Vector3(2.56f, 0.0f));
    }
  }

  void DoWand()
  {
    var sc = mCamera.ScreenToWorldPoint(Input.mousePosition);
    var t = sc - mTransform.position;

    var angle = Mathf.Atan2(t.y, t.x) * Mathf.Rad2Deg;
    mWandRotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

    wand.transform.position = mTransform.position + mWandRotation * new Vector3(0.32f, 0.0f);
    wand.transform.rotation = mWandRotation;
  }



}
