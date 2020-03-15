using UnityEngine;
using static maskshow;

public class skeleton : MonoBehaviour
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
    private float whiteKeyWidth;a
    private float last_distance;
    //music 2.22
    private AudioSource music;
    //2.23
    private int last_number = 111;

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
        music = GetComponent<AudioSource>(); // 2.22

        //initial hands
        hands.wrapMode = WrapMode.Once;

        //hands.Play("waist");
        hasAnim = false;

        Debug.Log(Time.time); //2.22
        music.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        int i;
    
        UpdateTranslate();
        for (i = order / 2; i < order / 2 + 6 && i < notesNum_r; i++)
        {

            UpdateFinger(sounds_r[i].finger, sounds_r[i].start - 0.04f, sounds_r[i].end, i);
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
        if (trans_order < notesNum_r && Time.time >= sounds_r4hands[trans_order].start) //start time minus translate time;
        {
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
                if (number == 1)
                {
                    if (last_number == 3)
                    {
                        hands.Play("cross F1");
                        animName = "cross F1";
                    }
                    else 
                    {
                        hands.Play("Finger 1");
                        animName = "Finger 1";
                    }                      
                                    
                }
                else if (number == 2)
                {
                    hands.Play("Finger 2");
                    animName = "Finger 2";
                }
                else if (number == 3)
                {
                    if (last_number == 1)
                    {
                        hands.Play("weave F1");
                        animName = "weave F1";
                    }
                    else 
                    {
                        hands.Play("Finger 3");
                        animName = "Finger 3";
                    }

                    //hands.transform.Translate(-0.6f, 0f, 0f, Space.World);
                    //hands.Play("Finger 1d-4");
                    //animName = "Finger 1d-4";
                }
                else if (number == 4)
                {
                    hands.Play("Finger 4");
                    animName = "Finger 4";
                }
                else if (number == 5)
                {
                    hands.Play("Finger 5");
                    animName = "Finger 5";
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
                last_number = number; //2.23
            }
        }
    }
    public void Sounds2hands(SoundNote[] sound)
    {

        float distance;
        float translate_time;
        int i;
        //notesNum_r
        sounds_r4hands = sounds_r;
        last_distance = 0f;
        for (i = 0; i < notesNum_r; i++)
        {
            distance = CountDistance(i);
            Debug.Log(distance);
            //2.25
            sounds_r4hands[i].distance = distance ;


            //2.26 动画有多少秒，手指动画开始播放时间就提前于红块动画开始时间多少秒
            translate_time = 0.04f;

            if(i > 0 && sounds_r[i].finger == 1 && sounds_r[i - 1].finger == 3) //cross
            {
                translate_time = 0.16f; 

            }
            else if(i > 0 && sounds_r[i].finger == 3 && sounds_r[i - 1].finger == 1) //weave
            {
                translate_time = 0.20f; 
            }           
            
            sounds_r4hands[i].start = sounds_r[i].start - translate_time; 
        }

    }
    public float CountDistance(int i)
    {
        int pitch = sounds_r[i].pitch;
        int finger = sounds_r[i].finger;
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
                        
        Debug.Log("keys");
        Debug.Log(keys);
        //this time distance relative to initial position minus last time distance relative to initial position
        distance = octave * octaveWidth + (keys+1-finger) * whiteKeyWidth - last_distance;
        last_distance = octave * octaveWidth + (keys + 1 - finger) * whiteKeyWidth;
        if(i == 3 || i == 6) //cross 4-F3 7-F3 
        {
            distance = distance + 0.16f; //移动到f1    
        }
        else if(i == 4 || i == 7) 
        {
            distance = distance - 0.16f; //弥补少移动的     
        }
        else if(i == 13 || i == 16) //weave 14-F3 17-F3 
        {
            distance = distance + 0.36f; //移动到f3 
        }
        else if(i == 14 || i == 17) 
        {
            distance = distance - 0.36f;  //弥补少移动的 
        }
        
        return distance;
    }
}
