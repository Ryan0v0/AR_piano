using UnityEngine;
using static maskshow2;
using static maskshow;

public class skeleton_left_jx : MonoBehaviour
{
    //private new Animation animation;
    private float current_time;
    private int order = 0;
    private int trans_order = 0;
    private int[] mark = new int[20000];
    //hands appear and hold
    public Animation hands;
    private string animName = "Initial name";
    private bool hasAnim = false;
    //hands translate rule
    private SoundNote[] sounds_l4hands = null;
    private float octaveWidth;
    private float whiteKeyWidth;
    private float last_distance;
	
	private bool isMaskOk = true; //只运行一次的标签
   // private AudioSource music;
    // Start is called before the first frame update
	private int last_number = 111;
    void Start()
    {
        //hands.transform.position = new Vector3(0.3f, -9.62f, 10.91f);
        hands.Play("Finger 7");

    }

    // Update is called once per frame
    void Update()
    {
        int i;
		if(isMaskOk&&isMusicOk_l&&isMusicOk_r){ //mask为真后进行一次初始化并变为假

            isMaskOk = false;
			
			Debug.Log("skeleton_left Initial");
			//music = GetComponent<AudioSource>();
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
		}
        if(isMusicOk_l && isMusicOk_r)
        {
			UpdateTranslate();
			for (i = order / 2; i < order / 2 + 6 && i < notesNum_l; i++)
			{

				UpdateFinger(sounds_l[i].finger, sounds_l[i].start - 0.04f, sounds_l[i].end, i);
				//Control animation start
			}

			if (hasAnim && hands[animName].time > (hands[animName].length / 2))
			{

				hands[animName].speed = 0;

			}
            //Debug.Log("First time update");
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
                Debug.Log("NUMBER");
                Debug.Log(number);
                if (number == 6)
				{
					if (last_number == 9)
                    {
                        hands.Play("Finger9-6");
						animName = "Finger9-6";
                    }
					else
					{
                
						hands.Play("Finger 6");
						animName = "Finger 6";
					}
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
                    if (last_number == 6)
                    {
						hands.Play("Finger6-9");
						animName = "Finger6-9";
                    }
                    else 
                    {
						hands.Play("Finger 9");
						animName = "Finger 9";
					}
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
        sounds_l4hands = sounds_l;
        last_distance = 0f;
        for (i = 0; i < notesNum_l; i++)
        {
            distance = CountDistance(i);
            //Debug.Log(distance);
            sounds_l4hands[i].distance = distance;
            translate_time = 0f;
            sounds_l4hands[i].start = sounds_l[i].start - translate_time - 0.04f;
        }

    }
    public float CountDistance(int i)
    {
        int pitch = sounds_l[i].pitch;
        int finger = sounds_l[i].finger;
        int mode = sounds_l[i].mode;
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
        //this time distance relative to initial position minus last time distance relative to initial position
        //10-1 9-2 8-3
        finger = 11 - finger;
        distance = octave * octaveWidth + (keys + 1 - finger) * whiteKeyWidth - last_distance;
        last_distance = octave * octaveWidth + (keys + 1 - finger) * whiteKeyWidth;
        return distance;
    }
}
