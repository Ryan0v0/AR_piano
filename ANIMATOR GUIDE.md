ANIMATOR GUIDE

![image-20200319154203719](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154203719.png)

##### 1. 首先不要忘了改**脚本执行顺序**。

我会整个项目打包回去的（会去掉后面jx等姓名标记，以及github日常大文件挂机我传qq）。

##### 2.大家拿到项目怎么用呢》fpArms_skin》找到animator controller

<img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154329283.png" alt="image-20200319154329283"  />![image-20200319154503633](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154503633.png)

首先分了6个layer（顺序无所谓）

然后有很多parameter-控制这个状态机的转换。脚本skeletonAnimator上控制这些parameters 的数值以控制整个动画状态机。

##### 3.以Base Layer为例--它控制手腕的动作。

<img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154755029.png" alt="image-20200319154755029" />

###### 1）waist，waist0 是两个一模一样的手腕（手立起来的位置）的动画。

![image-20200319154945176](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154945176.png)

右键选中transition（变蓝），设定参数。

<img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319155048711.png" alt="image-20200319155048711" style="zoom:80%;" />

目前Transition Duration暂时设为0，之后可微调使得动画转换更自然。

Can Transition To self 不勾选。即不能抓换到自身状态，不勾选会一直循环（因为条件没有变）

<u>***以上两条要求适用于现在所有的Transition***</u>

最重要的是设置Condition：

Finger 变量一直跟踪弹奏的手指。这里，只要NotEqual 1，即不是一指弹奏，就是立手位，调用waist 动画。waist 变量 为了使得两个相同动画不停调用，每到一新音，waist反转，（waist ==false 调用 waist动画，反转为true 调用waist 0 动画）形成手腕的微震动。*只是一个小技巧。

###### 2）反观 Finger==1 则调用Waist for 1（自己点开Transition,即蓝线看参数）。手腕适当旋转。

<u>以上两种手腕状态都有个条件，就是mode = 0</u><u>，一般手位时适用，那么其它手位呢？</u>

###### 3）cross 和weave是穿跨指，本身是附着在一指 和 三指弹奏动作上的。（尚未拆成两部分时，这样直接调用就可以。当然可能你们收到的版本是已经拆了的。）这里就要涉及到**mode 参数**。

<img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319155944748.png" alt="image-20200319155944748" style="zoom: 80%;" /><img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319154755029.png" alt="image-20200319154755029" style="zoom:67%;" />

mode 是手指模式变量，目前只有几个模式（可以后期扩展），mode=0，就是一般正立手位弹奏；mode = 1是穿指弹奏，2是穿指后恢复的下一音；mode = 3是跨指弹奏，4是跨指后恢复后一音。可以看出来，调用weave 和cross 只需要mode一个参数，因为其它模式需要mode 等于0，已经排除歧义。mode ==1或3时会自动找到唯一符合的状态转换。

##### 4.以一指Finger 1 layer 看所有手指layer。

<img src="C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319161122999.png" alt="image-20200319161122999" style="zoom: 67%;" />

###### 1）mode == 0 立手位。up and down

![image-20200319161229965](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319161229965.png)

***仍是Transition Duration暂时设为0，Can Transition To self 不勾选 该要求适用于现在所有的Transition。**

上面是Finger 1_down 的Conditon 也就是音开始手指**往下摁**的动作。

关键变量叫up1，一共有up1~up5。

控制它们的代码如下，注释很明显，就是音开始，设为false，音结束设为true。

同时该代码还<u>更新mode。</u>

```c#
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
```

true 就会调用，Finger1_up, 即手指向上抬得动作，它的transition Conditon 如下：

![image-20200319161655751](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319161655751.png)

###### 2）mode ！=0 其它手位

这个最明显就是cross F1，既然Finger1所有其它状态mode！=1，那么只需要一个mode == 1的条件满足即可。

![image-20200319162012520](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319162012520.png)

##### 5. mode 的选择和不同模式下对translate（位移函数的影响）

###### 1）mode 的选择（早期代码比较 sh*t）

解释就是 如果 1 指接 3 指，且音上行（一指弹的音比三指高）调用穿指（mode == 1）。

反之，3指接1指，且下行，调用跨指（mode == 3）。同理判断它们后面的音分别为m2，m4。（mode 简称m）mode 状态会存入sound4hand_r, sound4hand_l【加入SoundNote 类】。

穿跨指动画正在扩展，以适用多种情况。

```c#
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
```

###### 2）mode 对Translate 影响。

***首先不会改变中存入sound4hand_r, sound4hand_l的distance域。因为它们都对于前一项有高度依赖性。**我们只能在取出初始distance后，根据mode对它进行加工。

```c#
    private void UpdateTranslate()
    {
        int mode;
        float distance;
        //trans_order is the key order (work for singular key at a time while order work for double or more keys at the same time)
        if (trans_order < notesNum_r && Time.time >= sounds_r4hands[trans_order].start) //start time minus translate time;
        {
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
```

##### 6. 如何建立一层Animator （需要的看）or 加动画

###### 1）加动画

必须 **不要勾选** legacy。拖进即可。

###### 2）加Layer，+号，取名。

![image-20200319163744132](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319163744132.png)

点开小齿轮。设定如图。主要是要加一个Mask，mask就是我们的手avatar的一部分。加了mask后，一个layer 只能控制手的一部分骨骼，如手腕，手指等。

###### 3）新建mask，以左手手腕为例

在project 中 create》avatar mask， 命名lefthand。

![image-20200319164100070](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319164100070.png)

在inspector中，打开transform，use skeleton from 唯一的fpArm_skinAvatar。import skeleton，expand all全展开，toggle all 全部取消。

![image-20200319164200274](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319164200274.png)

然后勾选一项，LeftHand

![image-20200319164422266](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319164422266.png)

就完成我们这个mask 了。

###### 4）新建AnimatorController

这时候我只建立右手的AnimatorController，我还需要左手的，create》

AnimatorController

![image-20200319164744625](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319164744625.png)

右边右手，左边左手~

![image-20200319165204249](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319165204249.png)

右键Layer，选上即可。

##### 7.啰嗦一下，改男女皮肤

![image-20200319165327617](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319165327617.png)

![image-20200319165358543](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319165358543.png)

Skinned 勾想要的哪一个，另一个皮肤的Skinned 不勾即可。

##### 8.注意事项

###### 1）帧数的设计

![image-20200319191609193](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319191609193.png)

###### 2）动画的设计示范

全长动画（抬起放下）

![image-20200319191724160](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319191724160.png)

抬起

![image-20200319191746100](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319191746100.png)

放下

![image-20200319191759482](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319191759482.png)

错误案例-冗余 不是0.04s倍数的长度 各层级动作不规整

![image-20200319191930890](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319191930890.png)

3）Animator 的理解&优势

·不需要拆解动画，
比如base layer 只控制手腕一个关节骨骼
我要调桐雨的动画，我就调用整个（但只有涵盖手腕的部分展示）

![image-20200319192056933](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319192056933.png)

桐雨的动画同时涉及到Finger 1，那么在Finger 1 中我同样调用这个cross F1（这一层展示一指部分的动画）

![image-20200319192115042](C:\Users\15460\AppData\Roaming\Typora\typora-user-images\image-20200319192115042.png)

所以如果出现多指演奏，多层会控制各个手指独立弹奏。我们接下来的主要挑战是手型协调。

