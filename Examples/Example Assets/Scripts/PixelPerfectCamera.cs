using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
  public float PixelsToUnits = 100.0f;
  public float Zoom = 1.0f;

  private Camera cam;
  private float invPixelToUnits;

  void Start()
  {
    invPixelToUnits = 1.0f / PixelsToUnits;
    cam = GetComponent<Camera>();
    cam.orthographicSize = Screen.height * 0.5f * invPixelToUnits / Zoom;
  }

  void Update()
  {
    cam.orthographicSize = Screen.height * 0.5f * invPixelToUnits / Zoom;
  }

}
