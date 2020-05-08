
using UnityEngine;
using static maskshow;
using static maskshow2;

public class skeletonAnimator_left : MonoBehaviour
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
    private float setScale = 0.05f;
    public Animator ani;
    private bool waist = false;
    //Expand
    // Start is called before the first frame update

    private bool isMaskOk = true; //只运行一次的标签

    void Start()
    {
        //hands.transform.position = new Vector3(0.3f, -9.62f, 10.91f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMaskOk && isMusicOk_l){ //wait till t is ok

            isMaskOk = false;
            int i;
            Debug.Log("skeleton Initial left");
            hands = GetComponent<Animation>();

            //initial mark
            for (i = 0; i < notesNum_r; i++)
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
            Debug.Log("Start SKELETON left");
            //music.Play(0);
        }
        if (isMusicOk_l)
        {
             int i;
            UpdateTranslate();

            if (!hasAnim)
            {
                ani.SetInteger("Finger", 0);
            }
            //Start Finger Animation
            int st;
            st = (order / 2 - 4) > 0 ? ((order / 2) - 4) : 0;
            for (i = st; i < order / 2 + 6 && i < notesNum_l; i++)
            {
                UpdateFinger(sounds_l[i].finger, sounds_l[i].start - 0.04f, sounds_l[i].end, i);
                //Control animation start
             }
        }
       

    }
    private void UpdateTranslate()
    {
        int mode;
        float distance;
        //trans_order is the key order (work for singular key at a time while order work for double or more keys at the same time)
        if (trans_order < notesNum_l && (Time.time - inital_time) >= sounds_l4hands[trans_order].start) //start time minus translate time;
        {
            //hands.transform.Translate(new Vector3(sounds_r4hands[trans_order].distance, 0f, 0f)*100.0f*Time.deltaTime, Space.World);
            distance = sounds_l4hands[trans_order].accumdistance;
            mode = sounds_l4hands[trans_order].mode;
            switch (mode)
            {
                case 1:
                    distance = distance + 0.16f;
                    break;
                case 2:
                    distance = distance - 0.16f;
                    break;
                case 3:
                    distance = distance + 0.36f;
                    break;
                case 4:
                    distance = distance - 0.36f;
                    break;

            }
            hands.transform.Translate(0f, 0f, distance *setScale, Space.Self);
            trans_order++;
        }
    }
    private void UpdateFinger(int number, float starttime, float endtime, int index)
    {
        int mode = sounds_l4hands[index].mode;
        int expand  = sounds_l4hands[index].expand;
        int isHead = sounds_l4hands[index].isHead;
        if ((Time.time - inital_time) >= starttime) //给予initial time
        {
            if ((Time.time - inital_time) >= endtime)
            {
                if (mark[index] == 1)
                {
                    //hasAnim = false;
                    mark[index] = 2;                    
                    ani.SetBool("up"+number, true);                    
                    order++;
                }
            }
            else if (mark[index] == 0)
            {
                if (isHead == 1) // recover from last-group pose// only for regular head!
                {
                    ani.SetInteger("expand6", 0);
                    ani.SetInteger("expand7", 0);
                    ani.SetInteger("expand8", 0);
                    ani.SetInteger("expand9", 0);
                    ani.SetInteger("expand10", 0);
                    waist = !waist;
                }
                //expand
                ani.SetInteger("expand"+number, expand);
                //waist movement
                ani.SetInteger("Finger", number);
                ani.SetBool("waist", waist);
                //waist = !waist;

                //mode 
                ani.SetInteger("mode", mode);

                //finger mv
                ani.SetBool("up" + number, false);
                //hasAnim = true;
                mark[index] = 1;
                order++;
            }
        }
    }
    public void Sounds2hands(SoundNote[] sound)
    {

        float distance;
        float translate_time;
        int mode = 0;
        int i;
        //notesNum_rs
        sounds_l4hands = sounds_l;
        last_distance = 0f;
        for (i = 0; i < notesNum_l; i++)
        { 
            //First select mode
            mode = SelectMode(i);
            sounds_l4hands[i].mode = mode;

            //Then calculate distance
            distance = CountDistance(i);
            sounds_l4hands[i].distance = distance;

            //initial expand;
            sounds_l4hands[i].expand = 0;

            translate_time = 0.01f;
            sounds_l4hands[i].start = sounds_l[i].start - translate_time-0.04f;
        }
        //Then accumulate distance
        CountAccumDistance();
        print("ACCM DISTANCE");
        for (i = 0; i < notesNum_l; i++)
        {
            //print(sounds_l4hands[i].accumdistance);
            print("EXPAND Value");
            print(sounds_l4hands[i].expand);
            print("Finger & isHead");
            print(sounds_l4hands[i].finger);
            print(sounds_l4hands[i].isHead);
        }
    }
    public int SelectMode(int i)
    {
        int mode = 0;
        int pitch = sounds_l[i].pitch;
        int finger = sounds_l[i].finger;
        int lastpitch = 0;
        int lastfinger = 0;
        if (i > 0)
        {
            lastpitch = sounds_l[i - 1].pitch;
            lastfinger = sounds_l[i - 1].finger;
        }
        //Debug.Log("SelectMode");

        if (finger == 9 && lastfinger == 6 && pitch >= lastpitch) 
        {
            mode = 1; //Cross to this note; 6-9
            
            //Debug.Log("mode = 1");
        }
        else if (i > 0 && sounds_l4hands[i-1].mode == 1)
        {
            mode = 2;
            //Debug.Log("mode = 2");
        }
        else if (finger == 6 && lastfinger == 9 && pitch < lastpitch)
        {
            mode = 3;
            //Debug.Log("mode = 3");
        }
        else if (i > 0 && sounds_l4hands[i - 1].mode == 3)
        {
            mode = 4;
            //Debug.Log("mode = 4");
        }
        return mode;
    }
    public float CountDistance(int i)
    {
        int pitch = sounds_l[i].pitch;
        int finger = sounds_l[i].finger;
        //int lastpitch = 0;
        //int lastfinger = 0;
        float distance = 0f;
        int octave;
        int keys;
        //60-1 62-2 64-3 65-4 67-5 (inital position)
        octave = (pitch - 41) / 12;
        keys = (pitch - 41) - octave * 12;
        if (keys < 0)
        {
            keys = keys + 12;
            octave--;
        }
        if (keys > 6)
        {
            keys++;
        }
        keys = keys / 2; // black key distance same to white key at its left.

        //(this time distance relative to initial position) minus (last time distance relative to initial position)
        //10-1 9-2 8-3
        finger = 11 - finger;
        distance = octave * octaveWidth + (keys+1-finger) * whiteKeyWidth - last_distance;
        last_distance = octave * octaveWidth + (keys + 1 - finger) * whiteKeyWidth;
        return distance;
    }
    public void CountAccumDistance()
    {
        int i;
        float st_time = 0f;
        float ac_distance = 0f;

        for (i = 0; i<notesNum_l; i++)
        {
            if(i != 0 && sounds_l4hands[i].start-st_time < 0.25f) //Is is not a group head!
            {
                //print("Something");
                sounds_l4hands[i].isHead = 0; 
                ac_distance += sounds_l4hands[i].distance;
                sounds_l4hands[i].accumdistance = 0;
                
            }
            else //Is is a group head!
            {
                sounds_l4hands[i].isHead = 1;
                sounds_l4hands[i].accumdistance = sounds_l4hands[i].distance + ac_distance;
                ac_distance = 0;
                st_time = sounds_l4hands[i].start;
                if (i != 0)
                {
                    //summarize hand posture
                    int last_head;
                    int cur_head = i; //curhead is the current note
                    int last_finger;
                    int cur_finger;
                    int dis_num;
                    int j;
                    last_head = cur_head-1;
                    while (sounds_l4hands[last_head].isHead==0) //not a group head
                    {
                        last_head--; //insert non-head label
                    } 
                    last_finger = sounds_l4hands[last_head].finger;
                    for (j = last_head; j<cur_head; j++)
                    {
                        cur_finger = sounds_l4hands[j].finger;
                        dis_num = (int)(sounds_l4hands[j+1].distance/whiteKeyWidth); // translate positions
                        //print("DisNUm next head index + lasthead--");
                        //print(i);
                        //print(j);
                        //print(dis_num);
                        if (dis_num == -1 && (sounds_l4hands[j+1].finger- cur_finger)==1)
                        {
                            //print(j);
                            //print(dis_num);
                            if(sounds_l4hands[j].finger == 6)
                            {
                                sounds_l4hands[j].expand = 1;
                            }
                            else
                            {
                                sounds_l4hands[j+1].expand = 1;
                            }
                             //for lefthand toward finger10 is positive

                        }

                        // two heads connected, expand = 1 (hand cover one wwidth to F10)
                    }
                }
            }

        }
        bool lastChange = false;
        for (i = 0; i < notesNum_l; i++)
        {           
            if (i>0 && sounds_l4hands[i].isHead==1) //is group head && should reset posture
            {
                if (lastChange)
                {
                    //print("MINUS WKEY");
                    sounds_l4hands[i].accumdistance -= whiteKeyWidth;
                    lastChange = false; //reset
                }
                //last hand posture final avariance
                if(sounds_l4hands[i - 1].expand == 1)
                {
                    //print("ADD WKEY");
                    sounds_l4hands[i].accumdistance += whiteKeyWidth; // less left movement
                    sounds_l4hands[i].isHead = 2; //Is head && remain the old posture 
                    lastChange = true;
                }
            }
        }
    }
}
