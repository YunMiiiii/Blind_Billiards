using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//����1 : ���� ���� ������ �ְ� ���� ����Ǹ� ���� �ѱ��.
//�Ӽ�1 : ���� ��
//����1 : ���� ���� ������ �ѱ��.

//����2 : �Ͽ� �ش��ϴ� ���� ��ȯ�Ѵ�.
//�Ӽ�2 : �÷��̾� �� ����Ʈ
//����2 : �Ͽ� �ش��ϴ� ���� ��ȯ�Ѵ�.

//�ַ��÷��� ������ �� ���� ����Ǹ� ���� �� ����� MyPlayer�� �����Ѵ�.

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    //�Ӽ�1 : ���� ��
    public int currentTurn
    {
        get
        {
            return _currentTurn;
        }
        private set
        {
            _currentTurn = value;
        }
    }
    int _currentTurn;

    //�Ӽ�2 : �÷��̾� �� ����Ʈ
    [SerializeField] List<GameObject> ballList;

    //����1 : ���� ���� ������ �ѱ��.
    public void EndTurn()
    {
        currentTurn++;
        GameObject.Find("Variable Joystick").GetComponent<BallLineRender>().ResetBallStatus();
        UIManager.Instance.UpdateTurn(currentTurn);
        if(TCP_BallCore.networkMode == NetworkMode.None)
        {
            GameManager.Instance.SoloPlaySet(currentTurn);
        }
    }

    //����2 : �Ͽ� �ش��ϴ� ���� ��ȯ�Ѵ�.
    public GameObject GetTurnBall()
    {
        if(ballList == null)
        {
            return null;
        }
        return ballList[currentTurn];
    }
    public GameObject GetTurnBall(int turn)
    {
        if(ballList == null)
        {
            return null;
        }
        return ballList[turn];
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void GetListFromGameManager()
    {
        ballList = GameManager.Instance.gamePlayers;
    }
}