
using UnityEngine;
public class maskshow : MonoBehaviour
{
    public GameObject[] mask = null;
    private float start_time;
    private float current_time;
    private TextAsset t;
    private AudioSource music;
    
    // Start is called before the first frame update
    public class SoundNote
    {
        public float start;
        public float end;
        public int pitch;
        public int finger;
        public float distance;
        public int mode;
    }
    private int order = 0;
    private int[] mark = new int[20000];
    public static SoundNote[] sounds_r = null;
    public static int notesNum_r;
    private string[] elements;
    void Start()
    {
        
        mask = new GameObject[88];
        sounds_r = new SoundNote[10000];
        start_time = Time.time;
        int i;
        music = GetComponent<AudioSource>();
        //t = Resources.Load("jht+right") as TextAsset;
        //t = Resources.Load("Chord Left") as TextAsset;
        t = Resources.Load("0") as TextAsset;
        //t = Resources.Load("Ode to Joy_right_cut") as TextAsset;
        //t = Resources.Load("Both Scale_right") as TextAsset;
        //t = Resources.Load("C arp Right") as TextAsset;
        if (t == null)
        {
            Debug.Log("NULL!");
        }
        string s = t.text;
        string[] array = s.Split('\n');
        
        for (i = 0; i<array.Length; i++)
        {
            mark[i] = 0; //都没执行过
            elements = array[i].Split('|');
            sounds_r[i] = new SoundNote() { pitch = int.Parse(elements[0]),
                start = float.Parse(elements[1]), end = float.Parse(elements[2]), finger = int.Parse(elements[3])
            };
        }       
        notesNum_r = array.Length;
        //先将块全部消失
        for (i = 0; i < 88; i++)
        {
            mask[i] = transform.GetChild(i).gameObject;
            mask[i].SetActive(false);
        }
        Debug.Log("start");
        //music.Play(0);
    }
    // Update is called once per frame

    void Update()
    {
        //Debug.Log(Time.time);
        int i;
        int st;
        st = (order / 2 - 4) > 0 ? ((order / 2) - 4) : 0;
        for (i = st; i<order/2 + 6 && i< notesNum_r; i++)
        {
            UpdateMask(sounds_r[i].pitch, sounds_r[i].start, sounds_r[i].end, i);
        }
        
    }
    private void UpdateMask(int number,  float starttime, float endtime, int index)
    {
        if (Time.time >= starttime)
        {
            if(Time.time >= endtime)
            {
                if (mark[index] == 1)
                {
                    Debug.Log("fade");
                    Debug.Log(index);
                   
                   mask[number - 21].SetActive(false);
                   mark[index] = 2;
                   order++;
                }
            }
            else if (mark[index] == 0)
            {
                Debug.Log(Time.time);
                mask[number - 21].SetActive(true);
                mark[index] = 1;
                order++;
            }
        }
    }
}