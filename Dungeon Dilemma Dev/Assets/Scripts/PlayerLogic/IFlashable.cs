using System.Collections;
using UnityEngine;

public interface IFlashable
{
    public void TakeDamageFlash(Vector2 source, bool isStatusEffect);
}