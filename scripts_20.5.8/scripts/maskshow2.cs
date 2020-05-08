using UnityEngine;
using static maskshow;

/** Author:  Guo Ruoxi,Wang Jiaxin

**/
public class maskshow2 : MonoBehaviour
{
    public GameObject[] mask = null;
    private float current_time;
    private TextAsset t;
    private AudioSource music;
    public AudioClip[] audios;

    public bool isPlay_l = false;
    public static bool isMusicOk_l = false; //菜单选择后加载mp3 和txt

    public int order = 0;
    public int[] mark = new int[20000];
    public static SoundNote[] sounds_l = null;
    private string[] elements;
    public static int notesNum_l;
    // Start is called before the first frame update
    void Start()
    {

        mask = new GameObject[88];
        sounds_l = new SoundNote[10000];
 
    }
    // Update is called once per frame

    void Update()
    {
        if(isMusicOk_l){
            int i;
            int st;
            st = (order / 2 - 4) > 0 ? ((order / 2) - 4) : 0;
            for (i = st; i < order / 2 + 6 && i < notesNum_l; i++)
            {
                UpdateMask(sounds_l[i].pitch, sounds_l[i].start, sounds_l[i].end, i);
            }
        }

    }
    private void UpdateMask(int number, float starttime, float endtime, int index)
    {
        if ((Time.time - inital_time) >= starttime)
        {
            if ((Time.time - inital_time) >= endtime)
            {
                if (mark[index] == 1)
                {
                    
                    mask[number - 21].SetActive(false);
                    mark[index] = 2;
                    order++;
                }
            }
            else if (mark[index] == 0)
            {
                mask[number - 21].SetActive(true);
                mark[index] = 1;
                order++;
            }
        }
    }
    public void onPause()
    {
        Time.timeScale = 0;
        music.Pause();

        isPlay_l = false;
        //masks2.SetActive(true);
        //fpArms_skin_1.SetActive(true);

    }

    public void onPlay()
    {
        Time.timeScale = 1f;
        music.Play();
        isPlay_l = true;
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
                    this.GetComponent<AudioSource>().clip = audios[0];
                    t = Resources.Load("C arp Left") as TextAsset;                    
                    //music = this.GetComponent<AudioSource>();
                    break;
                }
            case 2:
                {
                    this.GetComponent<AudioSource>().clip = audios[1];
                    t = Resources.Load("Chord Left") as TextAsset;
                    //music = this.GetComponent<AudioSource>();
                    break;

                }
            case 3:
                {
                    this.GetComponent<AudioSource>().clip = audios[2];
                    print("第4页");
                    print("Both Scale_left");
                    t = Resources.Load("Both Scale_left") as TextAsset;
                    break;

                }
            case 4:
                {
                    this.GetComponent<AudioSource>().clip = audios[3];
                    //t = Resources.Load("jht+left") as TextAsset;
                    //t = Resources.Load("left_test") as TextAsset;
                    t = Resources.Load("left_test_demo") as TextAsset;
                    break;

                }
            //如果只设置的了4项，而代码中有第五个，是永远调用不到的
            //需要对应在 Dropdown组件中的 Options属性 中增加选择项即可
            case 5:
                {
                    this.GetComponent<AudioSource>().clip = audios[4];
                    t = Resources.Load("wedding_left") as TextAsset;
                    break;
                }
            case 6:
                break;
        }
        music = this.GetComponent<AudioSource>();
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
            sounds_l[i] = new SoundNote()
            {
                pitch = int.Parse(elements[0]),
                start = float.Parse(elements[1]),
                end = float.Parse(elements[2]),
                finger = int.Parse(elements[3])
            };
        }
        notesNum_l = array.Length;
        //先将块全部消失
        for (i = 0; i < 88; i++)
        {
            mask[i] = transform.GetChild(i).gameObject;
            mask[i].SetActive(false);
        }
        Debug.Log("start maskshow2");
        Debug.Log(sounds_l[1].finger);
        isMusicOk_l = true;
    }
}
