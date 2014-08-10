using UnityEngine;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{

  public Projectile msOriginal;


  private static List<Projectile> msFreeProjectiles;

  void Awake()
  {
    msFreeProjectiles = new List<Projectile>(16);
  }

  public Projectile GetProjectile()
  {
    if (msFreeProjectiles.Count != 0)
    {
      int last = msFreeProjectiles.Count - 1;
      var projectile = msFreeProjectiles[last];
      msFreeProjectiles.RemoveAt(last);
      return projectile;
    }
    var thing = Object.Instantiate(msOriginal);
    return thing as Projectile;
  }

  public void Add(Projectile projectile)
  {
    msFreeProjectiles.Add(projectile);
  }

  public void Remove(Projectile projectile)
  {
    msFreeProjectiles.Remove(projectile);
  }

}
