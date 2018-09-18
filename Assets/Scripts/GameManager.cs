using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [SerializeField] Transform particleParent;
    [SerializeField] int particleInitCount = 50;
    [SerializeField] Transform ballParent;
    [SerializeField] int ballPoolingCount = 300;

    [SerializeField] GameObject muzzleFlash;
    Stack<GameObject> freeMuzzleFlashs = new Stack<GameObject>();

    [SerializeField] GameObject ball;
    Stack<GameObject> freeBalls = new Stack<GameObject>();
    [SerializeField] float ballLifeTime = 5f;

    [SerializeField] Material[] possibleBallMaterials;

    private void Awake()
    {
        for (int i = 0; i < particleInitCount; i++)
        {
            GameObject newMuzzleFlash = Instantiate(muzzleFlash, transform.position, Quaternion.Euler(new Vector3(-90f, 0f, 0f)), particleParent);
            newMuzzleFlash.SetActive(false);
            freeMuzzleFlashs.Push(newMuzzleFlash);
        }
        for (int i = 0; i < ballPoolingCount; i++)
        {
            GameObject newBall = Instantiate(ball, transform.position, Quaternion.identity, ballParent);
            if(possibleBallMaterials.Length > 0)
            {
                newBall.GetComponent<MeshRenderer>().material = possibleBallMaterials[Random.Range(0, possibleBallMaterials.Length)];
            }
            newBall.SetActive(false);
            freeBalls.Push(newBall);
        }
    }

    //TODO maybe give here parameter for balls speed
    public GameObject GetBall(Vector3 pos, Vector3 dir)
    {
        GameObject b = freeBalls.Pop();
        b.transform.position = pos;
        b.transform.forward = dir;
        b.SetActive(true);
        StartCoroutine(GetBallBack(b, ballLifeTime));
        return b;
    }

    IEnumerator GetBallBack(GameObject ball, float duration)
    {
        yield return new WaitForSeconds(duration);
        ball.SetActive(false);
        freeBalls.Push(ball);
    }

    public GameObject GetMuzzleFlash(Vector3 pos, Vector3 dir)
    {
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
