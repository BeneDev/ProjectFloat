using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [SerializeField] Transform particleParent;
    [SerializeField] int particleInitCount = 50;

    [SerializeField] GameObject muzzleFlash;
    Stack<GameObject> freeMuzzleFlashs = new Stack<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < particleInitCount; i++)
        {
            GameObject newMuzzleFlash = Instantiate(muzzleFlash, transform.position, Quaternion.Euler(new Vector3(-90f, 0f, 0f)), particleParent);
            newMuzzleFlash.SetActive(false);
            freeMuzzleFlashs.Push(newMuzzleFlash);
        }
    }

    public GameObject GetMuzzleFlash(Vector3 pos, Vector3 dir)
    {
        print(freeMuzzleFlashs.Count);
        GameObject ps = freeMuzzleFlashs.Pop();
        ps.SetActive(true);
        ps.transform.position = pos;
        ps.transform.up = dir;
        ParticleSystem pSys = ps.GetComponent<ParticleSystem>();
        pSys.Play();
        StartCoroutine(GetParticleBack(pSys.main.duration, ps, freeMuzzleFlashs));
        return ps;
    }

    IEnumerator GetParticleBack(float duration, GameObject ps, Stack<GameObject> stackToPush)
    {
        yield return new WaitForSeconds(duration);
        ps.SetActive(false);
        stackToPush.Push(ps);
    }
}
