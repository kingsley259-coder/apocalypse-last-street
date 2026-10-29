using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>Saved keyboard bindings shared by gameplay and the Controls menu.</summary>
public static class PlayerControls
{
    public static readonly string[] Actions=
    {
        "Forward","Left","Backward","Right","Sprint","Airstrike","Jump","Switch weapon"
    }
    ;
    // Permanent movement aliases; primary keys remain configurable.
    static readonly Key[] Arrows={Key.UpArrow,Key.LeftArrow,Key.DownArrow,Key.RightArrow};
    public static string BindingLabel(int action)=>action<4&&Get(action)!=Arrows[action]?Label(action)+" / "+Arrows[action].ToString().Replace("Arrow"," Arrow"):Label(action);
    static readonly Key[] Defaults=
    {
        Key.W,Key.A,Key.S,Key.D,Key.LeftShift,Key.F,Key.Space,Key.Q
    }
    ;
    public static Key Get(int action) => (Key)PlayerPrefs.GetInt("HH.Key."+Actions[action],(int)Defaults[action]);
    public static string Label(int action) => Get(action)==Key.LeftShift?"Left Shift":Get(action).ToString();
    public static bool Set(int action,Key key)
    {
        if(key==Key.None||key==Key.Escape)return false;
        int arrowAction=System.Array.IndexOf(Arrows,key);
        if(arrowAction>=0&&arrowAction!=action)return false;
        for(int i=0;i<Actions.Length;i++)if(i!=action&&Get(i)==key)return false;
        PlayerPrefs.SetInt("HH.Key."+Actions[action],(int)key);
        PlayerPrefs.Save();
        return true;
    }
    public static void Reset()
    {
        for(int i=0;i<Actions.Length;i++)PlayerPrefs.DeleteKey("HH.Key."+Actions[i]);
        PlayerPrefs.Save();
    }
    public static bool Held(Keyboard keyboard,int action)=>keyboard!=null&&(keyboard[Get(action)].isPressed||(action<4&&keyboard[Arrows[action]].isPressed));
    public static Vector3 Move(Keyboard keyboard)=>Vector3.ClampMagnitude(new Vector3((Held(keyboard,3)?1:0)-(Held(keyboard,1)?1:0),0,(Held(keyboard,0)?1:0)-(Held(keyboard,2)?1:0)),1);
    public static string Instructions => Label(0)+" / "+Label(2)+" - forward / backward    "+Label(1)+" / "+Label(3)+" - left / right\nArrow keys also move in all four directions.\n"+Label(4)+" - sprint. Left click - walk and shoot.\n"+Label(5)+" - aim saved airstrike; left click confirms; right click cancels.\n"+Label(6)+" - jump. "+Label(7)+" - switch rifle / laser. Esc pauses.\nBoss round: aim with the mouse. Camera angle stays fixed.";
    public static string Hud => Label(0)+"/"+Label(1)+"/"+Label(2)+"/"+Label(3)+" / ARROWS  MOVE    "+Label(4)+"  SPRINT    LEFT CLICK  SHOOT    "+Label(6)+"  JUMP    "+Label(7)+"  WEAPON";
}
