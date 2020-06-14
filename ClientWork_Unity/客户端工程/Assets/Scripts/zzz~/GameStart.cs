using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    private string m_IpStr = "127.0.0.1";
    private int m_port = 8011;
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Instance.Connect(m_IpStr, m_port);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Instance.MsgUpdate();
    }
}
