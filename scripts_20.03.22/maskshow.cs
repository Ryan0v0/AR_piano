
using UnityEngine;
//using static maskshow2;
public class maskshow : MonoBehaviour
{
    public GameObject[] mask = null;
    private float current_time;
    private float start_time;
    private TextAsset t;
    private TextAsset finger_output;
    private AudioSource music;
    public bool isPlay_r = false;

    // Start is called before the first frame update
    public class SoundNote
    {
        public float start;
        public float end;
        public int pitch;
        public int finger;
        public float distance;
        public float accumdistance;
        public int isHead;
        public int mode;
        public int expand;

    }
    private int order = 0;
    private int[] mark = new int[20000];
    public static SoundNote[] sounds_r = null;
    public static int notesNum_r;
    private string[] elements;
    public static bool isMusicOk_r = false;
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
        if(isMusicOk_r){
            int i;
            int st;
            st = (order / 2 - 4) > 0 ? ((order / 2) - 4) : 0;
            for (i = st; i<order/2 + 6 && i< notesNum_r; i++)
            {
                UpdateMask(sounds_r[i].pitch, sounds_r[i].start, sounds_r[i].end, i);
            }
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
                    //Debug.Log("fade");
                    //Debug.Log(index);
                   
                   mask[number - 21].SetActive(false);
                   mark[index] = 2;
                   order++;
                }
            }
            else if (mark[index] == 0)
            {
                //Debug.Log(Time.time);
                mask[number - 21].SetActive(true);
                mark[index] = 1;
                order++;
            }
        }
    }
    public void onPause()
    {
        Time.timeScale = 0;
        //music.Pause();

        isPlay_r = false;

    }
    public void onPlay()
    {
        Time.timeScale = 1f;
        //music.Play();

        isPlay_r = true;
        //masks2.SetActive(false);		 
        //fpArms_skin_1.SetActive(false);

    }
    public void ConsoleResult(int value)
    {
        //这里用 if else if也可，看自己喜欢
        //分别对应：第一项、第二项....以此类推
        int i;
        switch (value)
        {
            case 0:
                print("第1页");
                break;
            case 1:
                {
                    //this.GetComponent<AudioSource>().clip = audios[0];
                    t = Resources.Load("0") as TextAsset;
                    //music = this.GetComponent<AudioSource>();
                    break;
                }
            case 2:
                {
                    //this.GetComponent<AudioSource>().clip = audios[1];
                    t = Resources.Load("0") as TextAsset;
                    //music = this.GetComponent<AudioSource>();
                    break;

                }
            case 3:
                {
                    print("第4页");
                    print("Both Scale_right");
                    t = Resources.Load("Both Scale_right") as TextAsset;
                    break;
                }
            case 4:
                {
                    //t = Resources.Load("jht+right") as TextAsset;
                    finger_output = Resources.Load("output0_right") as TextAsset;
                    t = Resources.Load("right_test") as TextAsset;
                    break;
                }
            //如果只设置的了4项，而代码中有第五个，是永远调用不到的
            //需要对应在 Dropdown组件中的 Options属性 中增加选择项即可
            case 5:
                break;
        }
        start_time = Time.time;
        //music = this.GetComponent<AudioSource>();
        if (t == null)
        {
            Debug.Log("NULL!");
        }
        string s = t.text;
        string[] array = s.Split('\n');

        for (i = 0; i < array.Length; i++)
        {
            mark[i] = 0; //都没执行过
            elements = array[i].Split('|');
            sounds_r[i] = new SoundNote()
            {
                pitch = int.Parse(elements[0]),
                start = float.Parse(elements[1]),
                end = float.Parse(elements[2]),
                finger = int.Parse(elements[3])
            };
        }
        notesNum_r = array.Length;
        //先将块全部消失
        for (i = 0; i < 88; i++)
        {
            mask[i] = transform.GetChild(i).gameObject;
            mask[i].SetActive(false);
        }
        Debug.Log("start maskshow");
        isMusicOk_r = true;
        Time.timeScale = 0;
    }
}
