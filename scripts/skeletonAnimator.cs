
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
    private bool waist = false;
    private bool playmusic = true;
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
        Debug.Log("Start SKELETON");
        //music.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        int i;
        if (playmusic)
        {
            music.Play(0);
            playmusic = false;
        }
        //First Translate
        UpdateTranslate();

        if (!hasAnim)
        {
            ani.SetInteger("Finger", 0);
        }
        //Start Finger Animation
        int st;
        st = (order / 2 - 4) > 0 ? ((order / 2) - 4) : 0;
        for (i = st; i < order / 2 + 6 && i < notesNum_r; i++)
        {

            UpdateFinger(sounds_r[i].finger, sounds_r[i].start - 0.04f, sounds_r[i].end, i);
            //Control animation start
        }

    }
    private void UpdateTranslate()
    {
        int mode;
        float distance;
        //trans_order is the key order (work for singular key at a time while order work for double or more keys at the same time)
        if (trans_order < notesNum_r && Time.time >= sounds_r4hands[trans_order].start) //start time minus translate time;
        {
            //hands.transform.Translate(new Vector3(sounds_r4hands[trans_order].distance, 0f, 0f)*100.0f*Time.deltaTime, Space.World);
            distance = sounds_r4hands[trans_order].distance;
            mode = sounds_r4hands[trans_order].mode;
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
            hands.transform.Translate(distance, 0f, 0f, Space.World);
            trans_order++;
        }
    }
    private void UpdateFinger(int number, float starttime, float endtime, int index)
    {
        int mode = sounds_r4hands[index].mode;
        if (Time.time >= starttime)
        {
            if (Time.time >= endtime)
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
                //waist movement
                ani.SetInteger("Finger", number);
                ani.SetBool("waist", waist);
                waist = !waist;
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
        sounds_r4hands = sounds_r;
        last_distance = 0f;
        for (i = 0; i < notesNum_r; i++)
        { 
            //First select mode
            mode = SelectMode(i);
            sounds_r4hands[i].mode = mode;

            //Then calculate distance
            distance = CountDistance(i);
            sounds_r4hands[i].distance = distance;

            translate_time = 0.01f;
            sounds_r4hands[i].start = sounds_r[i].start - translate_time-0.04f;
        }

    }
    public int SelectMode(int i)
    {
        int mode = 0;
        int pitch = sounds_r[i].pitch;
        int finger = sounds_r[i].finger;
        int lastpitch = 0;
        int lastfinger = 0;
        if (i > 0)
        {
            lastpitch = sounds_r[i - 1].pitch;
            lastfinger = sounds_r[i - 1].finger;
        }
        Debug.Log("SelectMode");

        if (finger == 1 && lastfinger == 3 && pitch >= lastpitch) //cross 4-F3 7-F3 
        {
            mode = 1; //Cross to this note;
            
            Debug.Log("mode = 1");
        }
        else if (i > 0 && sounds_r4hands[i-1].mode == 1)
        {
            mode = 2;
            Debug.Log("mode = 2");
        }
        else if (finger == 3 && lastfinger == 1 && pitch < lastpitch)
        {
            mode = 3;
            Debug.Log("mode = 3");
        }
        else if (i > 0 && sounds_r4hands[i - 1].mode == 3)
        {
            mode = 4;
            Debug.Log("mode = 4");
        }
        return mode;
    }
    public float CountDistance(int i)
    {
        int pitch = sounds_r[i].pitch;
        int finger = sounds_r[i].finger;
        int mode = sounds_r[i].mode;
        //int lastpitch = 0;
        //int lastfinger = 0;
        float distance = 0f;
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
