using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform start;
    public Transform end;
    public float speed;
    float t = 0.5f;
    public delegate void AnimationHandler();
    public static AnimationManager instance;
    public new Animation animation;
    public AnimationClip f1;
    public AnimationClip f2;
    public AnimationClip f3;
    public AnimationClip f4;
    public AnimationClip f5;
    public AnimationHandler animationHandler;
    Vector3 targetPosition = new Vector3(0, -8.8f, 10.91f);
   

    void Start()
    {
        instance = this;
        animation = GetComponent<Animation>();
        
    }
    public void Playf1()

    {

        animation.Play(f1.name);

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        animation.Play();
    }
}
