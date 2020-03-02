using UnityEngine;
using static maskshow2;
using static maskshow;

public class skeleton_left : MonoBehaviour
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
    private SoundNote[] sounds_l4hands = null;
    private float octaveWidth;
    private float whiteKeyWidth;
    private float last_distance;
    private AudioSource music;
    // Start is called before the first frame update
    void Start()
    {
        int i;
        Debug.Log("skeleton_left Initial");
        music = GetComponent<AudioSource>();
        hands = GetComponent<Animation>();
        //initial mark
        for (i = 0; i < notesNum_l; i++)
        {
            mark[i] = 0;
        }

        //initial sounds_r4hands
        octaveWidth = 2.09f;
        whiteKeyWidth = octaveWidth / 7;//key width       
        Sounds2hands(sounds_l);

        //initial hands
        hands.wrapMode = WrapMode.Once;

        //hands.Play("waist");
        hasAnim = false;

        Debug.Log("Start skeleton_left");
        music.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        int i;
        UpdateTranslate();
        for (i = order / 2; i < order / 2 + 6 && i < notesNum_l; i++)
        {

            UpdateFinger(sounds_l[i].finger, sounds_l[i].start - 0.04f, sounds_l[i].end, i);
            //Control animation start
        }

        if (hasAnim && hands[animName].time > (hands[animName].length / 2))
        {
            //Debug.Log(animName);
            //Debug.Log(hands[animName].time);
            hands[animName].speed = 0;

        }

    }
    private void UpdateTranslate()
    {
        //trans_order is the key order (work for singular key at a time while order work for double or more keys at the same time)
        if (trans_order < notesNum_l && Time.time >= sounds_l4hands[trans_order].start) //start time minus translate time;
        {
            hands.transform.Translate(sounds_l4hands[trans_order].distance, 0f, 0f, Space.World);
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
                    hands[animName].speed = 1;
                    hasAnim = false;
                    mark[index] = 2;
                    order++;
                }
            }
            else if (mark[index] == 0)
            {
                if (animName == "Finger 1d-4")
                {
                    hands.transform.Translate(0.6f, 0f, 0f, Space.World);
                }
                //Debug.Log(index);
                if (number == 6)
                {
                    hands.Play("Finger 6");
                    animName = "Finger 6";
                }
                else if (number == 7)
                {
                    hands.Play("Finger 7");
                    animName = "Finger 7";
                }
                else if (number == 8)
                {
                    hands.Play("Finger 8");
                    animName = "Finger 8";
                    //hands.transform.Translate(-0.6f, 0f, 0f, Space.World);
                    //hands.Play("Finger 1d-4");
                    //animName = "Finger 1d-4";
                }
                else if (number == 9)
                {
                    hands.Play("Finger 9");
                    animName = "Finger 9";
                }
                else if (number == 10)
                {
                    hands.Play("Finger 10");
                    animName = "Finger 10";
                }
                else if (number == 55)
                {
                    hands.transform.Translate(-0.6f, 0f, 0f, Space.World);
                    hands.Play("Finger 1d-4");
                    animName = "Finger 1d-4";
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
        //notesNum_r
        sounds_l4hands = sounds_l;
        last_distance = 0f;
        for (i = 0; i < notesNum_l; i++)
        {
            distance = CountDistance(sounds_l[i].pitch, sounds_l[i].finger);
            Debug.Log(distance);
            sounds_l4hands[i].distance = distance;
            translate_time = 0f;
            sounds_l4hands[i].start = sounds_l[i].start - translate_time - 0.04f;
        }

    }
    public float CountDistance(int pitch, int finger)
    {
        float distance = 0;
        int octave;
        int keys;
        const int initial_pos = 43;
        //60-1 62-2 64-3 65-4 67-5 (inital position)
        octave = (pitch - initial_pos) / 12;
        keys = (pitch - initial_pos) - octave * 12;
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
                        
        Debug.Log("keys");
        Debug.Log(keys);
        //this time distance relative to initial position minus last time distance relative to initial position
        distance = octave * octaveWidth + (keys-10+finger) * whiteKeyWidth - last_distance;
        last_distance = octave * octaveWidth + (keys -10 + finger) * whiteKeyWidth;
        return distance;
    }
}
