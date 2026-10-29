using UnityEngine;
public class CheckpointGuide : MonoBehaviour
{
    public Transform gate;
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(gate&&g&&g.Campaign)
        {
            gate.gameObject.SetActive(!g.Campaign.BossPhase);
            gate.position=new Vector3(0,0,g.Campaign.CheckpointZ);
        }
    }
    void OnGUI()
    {
        GUI.matrix=Matrix4x4.identity;
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying||!g.Campaign)return;
        GUI.color=Color.cyan;
        GUI.Label(new Rect(Screen.width/2-220,20,440,28),g.Campaign&&g.Campaign.BossPhase?"DEFEAT THE BOSS MONSTER":"CHECKPOINT  "+Mathf.Max(0,Mathf.CeilToInt(g.Campaign.CheckpointZ-g.player.transform.position.z))+"m",new GUIStyle(GUI.skin.label)
        {
            alignment=TextAnchor.MiddleCenter,fontSize=18,fontStyle=FontStyle.Bold
        }
        );
        GUI.color=Color.white;
    }
}
