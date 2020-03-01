
using UnityEngine;
using static maskshow;

public class skeletonAnimator : MonoBehaviour
{
    //private new Animation animation;
    private float current_time;
    private int order = 0;
    private int trans_order = 0;
    private int[] mark = new int[20000];
    //hands appear and hold
    public Animation hands;
    private string animName = "";
    private bool hasAnim = false;
    //hands translate rule
    private SoundNote[] sounds_r4hands = null;
    private float octaveWidth;
    private float whiteKeyWidth;
    private float last_distance;
    public Animator ani;
    private AudioSource music;
    // Start is called before the first frame update
    void Start()
    {
        int i;
        Debug.Log("skeleton Initial");
        hands = GetComponent<Animation>();
        //initial mark
        for (i = 0; i < notesNum_r; i++)
        {
            mark[i] = 0;
        }

        //initial sounds_r4hands
        octaveWidth = 2.09f;
        whiteKeyWidth = octaveWidth / 7;//key width       
        Sounds2hands(sounds_r);
        music = GetComponent<AudioSource>();
        //initial hands
        hands.wrapMode = WrapMode.Once;

        //hands.Play("waist");
        hasAnim = false;
        Debug.Log(Time.time);
        music.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        int i;
        //First Translate
        UpdateTranslate();

        if (!hasAnim)
        {
            ani.SetInteger("Finger", 0);
        }
        //Start Finger Animation
        for (i = order / 2; i < order / 2 + 6 && i < notesNum_r; i++)
        {

            UpdateFinger(sounds_r[i].finger, sounds_r[i].start - 0.04f, sounds_r[i].end, i);
            //Control animation start
        }

        //Control Fingers to hold (duration of each note);
        if (hasAnim && ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5)
        {
            Debug.Log("has Anim");
            //Debug.Log(hands[animName].time);
            //ani.speed = 0;

        }

    }
    private void UpdateTranslate()
    {
        //trans_order is the key order (work for singular key at a time while order work for double or more keys at the same time)
        if (trans_order < notesNum_r && Time.time >= sounds_r4hands[trans_order].start) //start time minus translate time;
        {
            //hands.transform.Translate(new Vector3(sounds_r4hands[trans_order].distance, 0f, 0f)*100.0f*Time.deltaTime, Space.World);
            hands.transform.Translate(sounds_r4hands[trans_order].distance, 0f, 0f, Space.World);
            trans_order++;
        }
    }
    private void UpdateFinger(int number, float starttime, float endtime, int index)
    {
        if (Time.time >= starttime)
        {
            if (Time.time >= endtime)
            {
                if (mark[index] == 1)
                {
                    //mask[number - 21].SetActive(false);
                    //ani.speed = 1;
                    hasAnim = false;
                    mark[index] = 2;
                    order++;
                }
            }
            else if (mark[index] == 0)
            {
                //if (animName == "Finger 1d-4")
                //{
                    //hands.transform.Translate(0.6f, 0f, 0f, Space.World);
                //}
                //Debug.Log(index);
                if (number == 1)
                {
                    //hands.Play("Finger 1");
                    ani.SetInteger("Finger", 1);
                    //animName = "Finger 1";
                }
                else if (number == 2)
                {
                    //hands.Play("Finger 2");
                    ani.SetInteger("Finger", 2);
                   // animName = "Finger 2";
                }
                else if (number == 3)
                {
                    ani.SetInteger("Finger", 3);    
                }
                else if (number == 4)
                {
                    //hands.Play("Finger 4");
                    ani.SetInteger("Finger", 4);
                    //animName = "Finger 4";
                }
                else if (number == 5)
                {
                    //hands.Play("Finger 5");
                    ani.SetInteger("Finger", 5);
                   // animName = "Finger 5";
                }
                else if (number == 55)
                {
                    //hands.transform.Translate(-0.6f, 0f, 0f, Space.World);
                    //hands.Play("Finger 1d-4");
                    //animName = "Finger 1d-4";
                }
                hasAnim = true;
                mark[index] = 1;
                order++;
            }
        }
    }
    public void Sounds2hands(SoundNote[] sound)
    {

        float distance;
        float translate_time;
        int i;
        //notesNum_rs
        sounds_r4hands = sounds_r;
        last_distance = 0f;
        for (i = 0; i < notesNum_r; i++)
        {
            distance = CountDistance(sounds_r[i].pitch, sounds_r[i].finger);
            //Debug.Log(distance);
            sounds_r4hands[i].distance = distance;
            translate_time = 0.01f;
            sounds_r4hands[i].start = sounds_r[i].start - translate_time-0.04f;
        }

    }
    public float CountDistance(int pitch, int finger)
    {
        float distance = 0;
        int octave;
        int keys;
        //60-1 62-2 64-3 65-4 67-5 (inital position)
        octave = (pitch - 60) / 12;
        keys = (pitch - 60) - octave * 12;
        if (keys < 0)
        {
            keys = keys + 12;
            octave--;
        }
        if (keys > 4)
        {
            keys++;
        }
        keys = keys / 2; // black key distance same to white key at its left.
                        
        //Debug.Log("keys");
        //Debug.Log(keys);
        //this time distance relative to initial position minus last time distance relative to initial position
        distance = octave * octaveWidth + (keys+1-finger) * whiteKeyWidth - last_distance;
        last_distance = octave * octaveWidth + (keys + 1 - finger) * whiteKeyWidth;
        return distance;
    }
    public void hold()
    {

    }
}
