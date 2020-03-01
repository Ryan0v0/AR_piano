using UnityEngine;
using static maskshow;
public class maskshow2 : MonoBehaviour
{
    public GameObject[] mask = null;
    private float start_time;
    private float current_time;
    private TextAsset t;
    private AudioSource music;

    // Start is called before the first frame update

    public int order = 0;
    public int[] mark = new int[20000];
    public static SoundNote[] sounds_l = null;
    private string[] elements;
    public static int NotesNum_l;
    void Start()
    {

        mask = new GameObject[88];
        sounds_l = new SoundNote[10000];
        start_time = Time.time;
        int i;
        //music = GetComponent<AudioSource>();
        //t = Resources.Load("Chord Left") as TextAsset;
        //t = Resources.Load("jht+left") as TextAsset;
        t = Resources.Load("0") as TextAsset;
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
        NotesNum_l = array.Length;
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
        for (i = order / 2; i < order / 2 + 6 && i < NotesNum_l; i++)
        {
            UpdateMask(sounds_l[i].pitch, sounds_l[i].start, sounds_l[i].end, i) ;
        }

    }
    private void UpdateMask(int number, float starttime, float endtime, int index)
    {
        if (Time.time >= starttime)
        {
            if (Time.time >= endtime)
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
                Debug.Log(Time.time);
                mask[number - 21].SetActive(true);
                mark[index] = 1;
                order++;
            }
        }
    }
}
