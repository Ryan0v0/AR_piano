using UnityEngine;
using UnityEngine.UI;
using static maskshow;
public class maskshow2_jx : MonoBehaviour
{
	
    public GameObject[] mask = null;
    private float start_time;
    private float current_time;
    private TextAsset t;
    private AudioSource music;
	public AudioClip[] audios;
    // Start is called before the first frame update
	public bool isPlay = false;
	public static bool isMusicOk = false; //菜单选择后加载mp3 和txt
    public int order = 0;
    public int[] mark = new int[20000];
    public static SoundNote[] sounds_l = null;
    public static int notesNum_l;
    private string[] elements;
    void Start()
    {
		int i;
	//	Time.timeScale = 0;
        mask = new GameObject[88];
        sounds_l = new SoundNote[10000];
     //   start_time = Time.time;      
	//	music = GetComponent<AudioSource>(); 
     // t = Resources.Load("C arp Left") as TextAsset;

// t = Resources.Load("Chord Left") as TextAsset;
/*          if(t == null)
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
        Debug.Log("start");  */
    
    }
    // Update is called once per frame

    void Update()
    {
		if(isMusicOk){
			Debug.Log(Time.time);
			int i;
			for (i = order / 2; i < order / 2 + 6 && i < notesNum_l; i++)
			{
				
				UpdateMask(sounds_l[i].pitch, sounds_l[i].start, sounds_l[i].end, i);
			}
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
                mask[number - 21].SetActive(true);
                mark[index] = 1;
                order++;
            }
        }
    }
	
	public void onPause(){
		Time.timeScale = 0;
		music.Pause();
		
		isPlay = false;
		//masks2.SetActive(true);
		//fpArms_skin_1.SetActive(true);
		
	}
	
	public void onPlay(){
		Time.timeScale = 1f;
		music.Play();
		
		isPlay = true;
		//masks2.SetActive(false);		 
		//fpArms_skin_1.SetActive(false);
		
	}
	
	public void ConsoleResult(int value)
    {
        //这里用 if else if也可，看自己喜欢
        //分别对应：第一项、第二项....以此类推
        switch (value)
        {
            case 0:
                print("第1页");
                break;
            case 1:
			{
				 start_time = Time.time;   
				int i;
				t = Resources.Load("C arp Left") as TextAsset;
				 this.GetComponent<AudioSource>().clip = audios[0];
                music = this.GetComponent<AudioSource>();
				//Time.timeScale = 0;
				//mask = new GameObject[88];
				//sounds_l = new SoundNote[10000];
				start_time = Time.time;      
				music = GetComponent<AudioSource>();
				t = Resources.Load("C arp Left") as TextAsset;
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
				Debug.Log("start"); 
				print("第2页");
				isMusicOk = true;
				break;
			}
            case 2:
			{
				 start_time = Time.time;   
				 this.GetComponent<AudioSource>().clip = audios[1];
                music = this.GetComponent<AudioSource>();
				int i ;
				t = Resources.Load("Chord Left") as TextAsset;
				//Time.timeScale = 0;
				//mask = new GameObject[88];
				//sounds_l = new SoundNote[10000];
				start_time = Time.time;      
				music = GetComponent<AudioSource>();
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
				Debug.Log("start"); 
				print("第3页");
				isMusicOk = true;
				break;
						
			}
            case 3:
                print("第4页");
                break;
            //如果只设置的了4项，而代码中有第五个，是永远调用不到的
            //需要对应在 Dropdown组件中的 Options属性 中增加选择项即可
            case 4:
                print("第5页");
                break;
        }
    }
}
