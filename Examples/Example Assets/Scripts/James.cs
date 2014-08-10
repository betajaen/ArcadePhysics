using UnityEngine;
using System.Collections;

public class James : MonoBehaviour
{

  public GameObject wand;
  public float wandDistance = 0.32f;

  private Camera cam;
  private Transform trans;
  private Vector2 screenCoordsMe;
  private float angle;
  private Quaternion mWandRotation;
  private int movementFlags;

  private RigidbodyArcade rb;
  private float wandPower;
  void Start()
  {
    cam = Camera.main;
    trans = transform;
    rb = GetComponent<RigidbodyArcade>();
    rb.drag.x = rb.maxVelocity.x * 4.0f;
    wandPower = 1.0f;
  }


  void FixedUpdate()
  {
    DoWand();
    DoMove();
  }

  void DoMove()
  {
    rb.acceleration.x = 0.0f;

    if (Input.GetKey(KeyCode.A))
      rb.acceleration.x = -rb.maxVelocity.x * 4.0f;

    if (Input.GetKey(KeyCode.D))
      rb.acceleration.x = rb.maxVelocity.x * 4.0f;

    if (Input.GetMouseButton(0))
    {
      var bg = mWandRotation * new Vector2(1.0f, 0.0f);
      rb.acceleration.x -= bg.x * wandPower * (rb.colliderArcade.touchingDown ? 1.0f : 10.0f);
      rb.acceleration.y -= bg.y * wandPower * 10.0f;
    }

  }

  void DoWand()
  {

    var sc = cam.ScreenToWorldPoint(Input.mousePosition);
    var t = sc - trans.position;

    var angle = Mathf.Atan2(t.y, t.x) * Mathf.Rad2Deg;
    mWandRotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

    wand.transform.position = trans.position + mWandRotation * new Vector3(0.32f, 0.0f);
    wand.transform.rotation = mWandRotation;
  }



}
