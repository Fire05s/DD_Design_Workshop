using System.Collections;
using PurrNet;
using UnityEngine;

public class Door : NetworkBehaviour
{
    public Direction direction;

    public GameObject doorVisual; // open door
    public GameObject wallVisual; // closed wall
    private Animator doorAnimator;

    private SyncedRoom room;

    private void Start()
    {
        room = GetComponentInParent<SyncedRoom>();
        doorAnimator = doorVisual.GetComponent<Animator>();
    }

    [ObserversRpc(runLocally:true)]
    public void SetOpen(bool open)
    {
        doorVisual.SetActive(false);
        wallVisual.SetActive(!open);
    }

    [ObserversRpc(runLocally: true)]
    public void SetLock(bool isLocked)
    {
        if (wallVisual.activeSelf) return;
        // doorVisual.SetActive(isLocked);
        if (isLocked)
        {
            StartCoroutine(DoorLock());
        }
        else
        {
            StartCoroutine(DoorOpen());
        }
    }

    private IEnumerator DoorLock()
    {
        doorVisual.SetActive(true);
        doorAnimator.SetBool("isLocking", true);
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator DoorOpen()
    {
        doorAnimator.SetBool("isOpening", true);
        yield return new WaitForSeconds(0.6f);
        doorVisual.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Challenger")) room.PlayerEntered(this);
    }
}
