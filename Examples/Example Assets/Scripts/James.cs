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

  private const int kUp = 1;
  private const int kDown = 2;
  private const int kLeft = 4;
  private const int kRight = 8;


  void Start()
  {
    cam = Camera.main;
    trans = transform;
  }

  void Update()
  {

  }

  void FixedUpdate()
  {
    DoMove();
    DoWand();
  }

  void DoMove()
  {

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
