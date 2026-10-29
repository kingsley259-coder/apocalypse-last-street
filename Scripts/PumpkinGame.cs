using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class PumpkinGame : MonoBehaviour
{
    public enum Page
    {
        Opening,MainMenu,Stats,Credits,HowToPlay,Playing,GameOver,Controls,Difficulty,Pause,Settings
    }
    public static PumpkinGame Instance;
    static Page? nextPage;
    public Page page=Page.Opening;
    public bool IsPlaying=>page==Page.Playing&&!ended;
    public ApocalypseCampaign Campaign=>GetComponent<ApocalypseCampaign>();
    public PumpkinPlayer player;
    public ZombieEnemy malePrefab,femalePrefab;
    public SpawnManager spawnManager;
    public Vector3 arenaCenter;
    public Vector2 arenaHalfSize=new Vector2(5,28);
    public int wave=1,kills;
    public bool ended,won;
    public readonly List<ZombieEnemy> enemies=new List<ZombieEnemy>();
    public AudioClip ambience;
    public Texture2D menuBackground;
    public Font titleFont,uiFont;
    float elapsed;
    bool resultSaved,runStarted;
    int bindingAction=-1;
    string bindingMessage="Select a key to change it.";
    Page returnPage=Page.MainMenu;
    GUIStyle hero,title,label,small,body,button;
    void Awake()
    {
        Instance=this;
        page=nextPage??Page.Opening;
        nextPage=null;
        ApplyPause();
    }
    void Start()
    {
        enemies.Clear();
        enemies.AddRange(FindObjectsByType<ZombieEnemy>(FindObjectsSortMode.None));
        if(ambience)
        {
            var a=gameObject.AddComponent<AudioSource>();
            a.clip=ambience;
            a.loop=true;
            a.Play();
            var audio=GetComponent<GameAudio>();
            if(audio)audio.music=a;
        }
        if(page==Page.Playing)StartGame();
    }
    void OnDestroy()
    {
        if(Instance==this)
        {
            Instance=null;
            Time.timeScale=1;
        }
    }
    void Update()
    {
        var keyboard=Keyboard.current;
        bool capturing=bindingAction>=0;
        if(page==Page.Controls&&capturing&&keyboard!=null)
        {
            foreach(var key in keyboard.allKeys)if(key.wasPressedThisFrame)
            {
                if(key.keyCode==Key.Escape)
                {
                    bindingAction=-1;
                    bindingMessage="Cancelled.";
                    break;
                }
                if(PlayerControls.Set(bindingAction,key.keyCode))
                {
                    bindingAction=-1;
                    bindingMessage="Key saved.";
                }
                else bindingMessage="Key already assigned. Choose another.";
                break;
            }
        }
        if(keyboard!=null)
        {
            if(keyboard.escapeKey.wasPressedThisFrame&&!capturing)
            {
                if(page==Page.Playing)PauseGame();
                else if(page==Page.Pause)StartGame();
                else if(page==Page.Controls||page==Page.Settings||page==Page.HowToPlay)Back();
            }
            if(page==Page.Opening&&keyboard.enterKey.wasPressedThisFrame)ShowDifficulty();
        }
        if(!IsPlaying)return;
        elapsed+=Time.deltaTime;
        enemies.RemoveAll(z=>!z||z.dead);
        if(Campaign&&!Campaign.BossPhase&&player.transform.position.z>=Campaign.CheckpointZ)AdvanceCheckpoint();
    }
    public void SelectDifficulty(int mode)
    {
        runStarted=resultSaved=false;
        elapsed=0;
        kills=0;
        ended=won=false;
        page=Page.Playing;
        ApplyPause();
        Campaign.NewRun((ApocalypseCampaign.Difficulty)Mathf.Clamp(mode,0,2));
        CountStart();
    }
    public void ShowDifficulty()
    {
        page=Page.Difficulty;
        ApplyPause();
    }
    public void AdvanceCheckpoint()
    {
        if(Campaign)Campaign.Advance();
    }
    public void AddPoints(int points)
    {
        if(Campaign)Campaign.points+=points;
    }
    public void OpenMainMenu()
    {
        bindingAction=-1;
        page=Page.MainMenu;
        ApplyPause();
    }
    public void PauseGame()
    {
        page=Page.Pause;
        ApplyPause();
    }
    public void ShowStats()
    {
        returnPage=runStarted&&!ended?Page.Pause:Page.MainMenu;
        page=Page.Stats;
        ApplyPause();
    }
    public void ShowCredits()
    {
        returnPage=Page.MainMenu;
        page=Page.Credits;
        ApplyPause();
    }
    public void ShowControls()
    {
        returnPage=runStarted&&!ended?Page.Pause:Page.MainMenu;
        bindingAction=-1;
        page=Page.Controls;
        ApplyPause();
    }
    public void ShowSettings()
    {
        returnPage=runStarted&&!ended?Page.Pause:Page.MainMenu;
        page=Page.Settings;
        ApplyPause();
    }
    public void ShowHowToPlay()
    {
        returnPage=runStarted&&!ended?Page.Pause:Page.MainMenu;
        page=Page.HowToPlay;
        ApplyPause();
    }
    void Back()
    {
        bindingAction=-1;
        page=returnPage;
        ApplyPause();
    }
    public void StartGame()
    {
        if(ended)
        {
            RestartGame();
            return;
        }
        page=Page.Playing;
        ApplyPause();
        if(Campaign)Campaign.EnsureStarted();
        CountStart();
    }
    void CountStart()
    {
        if(runStarted)return;
        runStarted=true;
        PlayerPrefs.SetInt("HH.Runs",PlayerPrefs.GetInt("HH.Runs")+1);
        PlayerPrefs.Save();
    }
    void ApplyPause()
    {
        Time.timeScale=IsPlaying?1:0;
        Cursor.lockState=CursorLockMode.None;
        Cursor.visible=true;
    }
    public void RestartGame()
    {
        Reload(Page.Playing);
    }
    public void ReturnToMainMenu()
    {
        Reload(Page.MainMenu);
    }
    void Reload(Page destination)
    {
        nextPage=destination;
        Time.timeScale=1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void WinGame()
    {
        EndGame(true);
    }
    void EndGame(bool victory)
    {
        if(ended)return;
        ended=true;
        won=victory;
        page=Page.GameOver;
        SaveResult();
        ApplyPause();
    }
    void SaveResult()
    {
        if(resultSaved)return;
        resultSaved=true;
        PlayerPrefs.SetInt("HH.TotalKills",PlayerPrefs.GetInt("HH.TotalKills")+kills);
        PlayerPrefs.SetInt("HH.BestKills",Mathf.Max(PlayerPrefs.GetInt("HH.BestKills"),kills));
        PlayerPrefs.SetInt("HH.BestWave",Mathf.Max(PlayerPrefs.GetInt("HH.BestWave"),wave));
        PlayerPrefs.SetInt("AP.BestPoints",Mathf.Max(PlayerPrefs.GetInt("AP.BestPoints"),Campaign?Campaign.points:0));
        PlayerPrefs.SetFloat("HH.BestTime",Mathf.Max(PlayerPrefs.GetFloat("HH.BestTime"),elapsed));
        if(won)PlayerPrefs.SetInt("HH.Wins",PlayerPrefs.GetInt("HH.Wins")+1);
        PlayerPrefs.Save();
    }
    public void SpawnWave(int count)
    {
        if(Campaign&&Campaign.BossPhase)return;
        if(!spawnManager)spawnManager=GetComponent<SpawnManager>();
        if(spawnManager)spawnManager.SpawnWave(count);
    }
    public void DamagePlayer(float value)
    {
        if(!IsPlaying)return;
        player.health=Mathf.Max(0,player.health-value);
        player.hurtUntil=Time.time+.25f;
        if(player.health<=0)EndGame(false);
    }
    void Styles()
    {
        if(hero!=null)return;
        hero=new GUIStyle(GUI.skin.label)
        {
            font=titleFont,fontSize=82,normal=
            {
                textColor=new Color(1,.58f,.2f)
            }
        }
        ;
        title=new GUIStyle(hero)
        {
            fontSize=34
        }
        ;
        label=new GUIStyle(GUI.skin.label)
        {
            font=uiFont,fontSize=23,normal=
            {
                textColor=Color.white
            }
        }
        ;
        small=new GUIStyle(label)
        {
            fontSize=18
        }
        ;
        body=new GUIStyle(label)
        {
            fontSize=20,wordWrap=true
        }
        ;
        button=new GUIStyle(GUI.skin.button)
        {
            font=uiFont,fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter
        }
        ;
        button.normal.background=button.hover.background=button.active.background=Texture2D.whiteTexture;
        button.normal.textColor=button.hover.textColor=button.active.textColor=Color.white;
    }
    void OnGUI()
    {
        Styles();
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
        float w=Screen.width/scale,h=Screen.height/scale;
        if(!IsPlaying)
        {
            Menu(w,h);
            return;
        }
        Panel(new Rect(20,20,350,135),new Color(.02f,.025f,.035f,.9f));
        GUI.Label(new Rect(35,24,325,40),"APOCALYPSE",title);
        GUI.Label(new Rect(35,61,310,26),"PLAYER  "+Mathf.CeilToInt(player.health)+" / 100",small);
        Panel(new Rect(35,94,310,13),new Color(.18f,.07f,.07f));
        Panel(new Rect(35,94,310*Mathf.Clamp01(player.health/100),13),new Color(.92f,.22f,.13f));
        GUI.Label(new Rect(35,115,320,25),wave==4?"FINAL ROUND  /  BOSS MONSTER":"LEVEL "+wave+" / 3   /   "+(Campaign?Campaign.difficulty.ToString():"Medium"),small);
        Panel(new Rect(w-250,20,230,115),new Color(.02f,.025f,.035f,.9f));
        GUI.Label(new Rect(w-235,34,210,30),"POINTS  "+(Campaign?Campaign.points:0),label);
        GUI.Label(new Rect(w-235,75,210,28),"KILLS "+kills+"  /  "+Mathf.FloorToInt(elapsed)+"s",small);
        if(Button(new Rect(w-180,146,160,42),"Pause"))PauseGame();
        Panel(new Rect(20,h-64,w-40,42),new Color(.02f,.025f,.035f,.9f));
        GUI.Label(new Rect(35,h-58,w-70,32),PlayerControls.Hud,small);
        Panel(new Rect(20,h-78,160,5),new Color(.15f,.2f,.2f));
        Panel(new Rect(20,h-78,160*player.stamina/100,5),new Color(.2f,.85f,.7f));
    }
    void Menu(float w,float h)
    {
        if(menuBackground)
        {
            GUI.color=Color.white;
            GUI.DrawTexture(new Rect(0,0,w,h),menuBackground,ScaleMode.ScaleAndCrop);
        }
        else Panel(new Rect(0,0,w,h),new Color(.025f,.035f,.05f));
        Panel(new Rect(36,30,560,h-60),new Color(.015f,.022f,.03f,.88f));
        Panel(new Rect(36,30,5,h-60),new Color(1,.4f,.08f));
        float x=80;
        GUI.Label(new Rect(x,55,490,100),"APOCALYPSE",hero);
        GUI.Label(new Rect(x+3,143,490,40),"L A S T   S T R E E T",title);
        if(page==Page.Opening)
        {
            GUI.Label(new Rect(x,220,440,75),"Three streets. One final stand.\nKeep moving. Prepare for the boss.",body);
            if(Button(new Rect(x,330,440,58),"Play Game"))ShowDifficulty();
            if(Campaign&&Campaign.HasCheckpoint&&Button(new Rect(x,408,440,50),"Continue checkpoint"))StartGame();
            if(Button(new Rect(x,486,440,50),"Main Menu"))OpenMainMenu();
        }
        else if(page==Page.Difficulty)
        {
            GUI.Label(new Rect(x,205,440,35),"CHOOSE YOUR DIFFICULTY",label);
            if(Button(new Rect(x,268,440,56),"Easy  /  fewer, slower enemies"))SelectDifficulty(0);
            if(Button(new Rect(x,342,440,56),"Medium  /  balanced survival"))SelectDifficulty(1);
            if(Button(new Rect(x,416,440,56),"Hard  /  stronger pressure"))SelectDifficulty(2);
            if(Button(new Rect(x,510,440,44),"Back"))OpenMainMenu();
        }
        else if(page==Page.MainMenu)
        {
            if(Button(new Rect(x,220,440,46),runStarted?"Resume":Campaign&&Campaign.HasCheckpoint?"Continue checkpoint":"Start game"))
            {
                if(runStarted||(Campaign&&Campaign.HasCheckpoint))StartGame();
                else ShowDifficulty();
            }
            if(Button(new Rect(x,276,440,46),"New game"))ShowDifficulty();
            if(Button(new Rect(x,332,440,46),"Stats"))ShowStats();
            if(Button(new Rect(x,388,440,46),"How to Play"))ShowHowToPlay();
            if(Button(new Rect(x,444,440,46),"Controls"))ShowControls();
            if(Button(new Rect(x,500,440,46),"Settings"))ShowSettings();
            if(Button(new Rect(x,556,440,46),"Credits"))ShowCredits();
        }
        else if(page==Page.Pause)
        {
            GUI.Label(new Rect(x,205,440,35),"PAUSE",title);
            if(Button(new Rect(x,270,440,50),"Resume"))StartGame();
            if(Button(new Rect(x,336,440,50),"Settings"))ShowSettings();
            if(Button(new Rect(x,402,440,50),"Controls"))ShowControls();
            if(Button(new Rect(x,468,440,50),"How to Play"))ShowHowToPlay();
            if(Button(new Rect(x,534,440,50),"Main Menu"))ReturnToMainMenu();
        }
        else if(page==Page.Settings)
        {
            GUI.Label(new Rect(x,200,440,35),"AUDIO & ACCESSIBILITY",title);
            Volume(x,258,"Master","Master",.8f);
            Volume(x,342,"Music","Music",.4f);
            Volume(x,426,"Effects","FX",.65f);
            bool reduced=PlayerPrefs.GetInt("AP.ReducedFlashes",0)==1;
            bool value=GUI.Toggle(new Rect(x,506,440,38),reduced,"  Reduced muzzle flashes",label);
            if(value!=reduced)
            {
                PlayerPrefs.SetInt("AP.ReducedFlashes",value?1:0);
                PlayerPrefs.Save();
            }
            if(Button(new Rect(x,580,440,44),"Back"))Back();
        }
        else if(page==Page.Controls)
        {
            GUI.Label(new Rect(x,198,440,32),"CONTROLS",title);
            GUI.Label(new Rect(x,235,440,44),bindingMessage,small);
            for(int i=0;i<PlayerControls.Actions.Length;i++)
            {
                if(Button(new Rect(x,280+i*33,440,28),PlayerControls.Actions[i]+"   :   "+(bindingAction==i?"Press a key...":PlayerControls.BindingLabel(i))))
                {
                    bindingAction=i;
                    bindingMessage="Press a key. Escape cancels.";
                }
            }
            if(Button(new Rect(x,560,440,35),"Reset defaults"))
            {
                PlayerControls.Reset();
                bindingAction=-1;
                bindingMessage="Default keys restored.";
            }
            if(Button(new Rect(x,610,440,38),"Back"))Back();
        }
        else if(page==Page.GameOver)
        {
            GUI.Label(new Rect(x,210,440,60),won?"STREET CLEARED":"GAME OVER",title);
            GUI.Label(new Rect(x,290,440,120),"Points: "+(Campaign?Campaign.points:0)+"\n"+(won?"All three levels and the boss defeated.":"Checkpoint saved at level "+wave+"."),body);
            if(Button(new Rect(x,440,440,52),won?"New game":"Continue checkpoint"))
            {
                if(won)ShowDifficulty();
                else RestartGame();
            }
            if(Button(new Rect(x,514,440,48),"Main Menu"))ReturnToMainMenu();
        }
        else
        {
            string name=page==Page.Stats?"YOUR RECORD":page==Page.Credits?"CREDITS":"HOW TO PLAY";
            GUI.Label(new Rect(x,202,440,40),name,title);
            string text;
            if(page==Page.Stats)text="Best points: "+PlayerPrefs.GetInt("AP.BestPoints")+"\nVictories: "+PlayerPrefs.GetInt("HH.Wins")+"\nRuns: "+PlayerPrefs.GetInt("HH.Runs")+"\nTotal kills: "+PlayerPrefs.GetInt("HH.TotalKills");
            else if(page==Page.Credits)text="Kingsley Jermaine\nCity: mlkjaffer / CC BY 4.0\nAK-47: YieldingMist206 / CC BY 4.0\nCharacters & motion: Mixamo\nMonster: Khornes\nExplosions: Mirza Beig\nMusic: congusbongus / CC0\nFonts: Bebas Neue & Rajdhani / OFL\nOriginal menu artwork generated for this game.";
            else text="Complete levels 1, 2 and 3, then defeat the boss alone. Checkpoints save your level and inventory.\n\n"+PlayerControls.Instructions+"\n\nRed missile circles and rolling pumpkins are lethal. Save laser cells and airstrikes for the boss. Green heals; cyan gives laser ammo; purple gives an airstrike.";
            GUI.Label(new Rect(x,258,440,325),text,new GUIStyle(body)
            {
                fontSize=page==Page.HowToPlay?18:20
            }
            );
            if(Button(new Rect(x,610,440,38),"Back"))Back();
        }
        GUI.color=new Color(.75f,.75f,.75f);
        GUI.Label(new Rect(w-320,h-36,300,24),"APOCALYPSE  /  LAST STREET",small);
        GUI.color=Color.white;
    }
    void Volume(float x,float y,string labelText,string key,float fallback)
    {
        float value=PlayerPrefs.GetFloat("AP."+key,fallback);
        GUI.Label(new Rect(x,y,440,30),labelText+"  "+Mathf.RoundToInt(value*100)+"%",label);
        float next=GUI.HorizontalSlider(new Rect(x,y+40,430,24),value,0,1);
        if(Mathf.Abs(value-next)>.001f)GameAudio.Set(key,next);
    }
    bool Button(Rect rect,string text)
    {
        GUI.backgroundColor=rect.Contains(Event.current.mousePosition)?new Color(1,.5f,.16f):new Color(.7f,.23f,.055f);
        GUI.contentColor=Color.white;
        bool pressed=GUI.Button(rect,text,button);
        GUI.backgroundColor=Color.white;
        return pressed;
    }
    void Panel(Rect rect,Color color)
    {
        GUI.color=color;
        GUI.DrawTexture(rect,Texture2D.whiteTexture);
        GUI.color=Color.white;
    }
}
