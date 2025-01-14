﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//솔로 상태라면 배치된 게임 오브젝트를 찾아서 플레이어 리스트로 만든다.
//서버, 클라이언트 상태라면 서버에 연결된 플레이어를 추가한다.

public class GameManager : MonoBehaviour
{
    //싱글톤을 이용해서 쉽게 사용할 수 있도록 함
    public static GameManager Instance;

    public List<GameObject> gamePlayers;
    public List<BallEntryPlayerData> entryPlayerDataList = new List<BallEntryPlayerData>();

    //속성추가 : 공을 발사한 이후부터 공이 모두 멈추고 턴을 넘겨주는 사이에도 조작할 수 없도록 하기 위해서 bool 변수로 isNobodyMove 선언
    public bool isNobodyMove = true;

    //속성3 : MoveData List, 공을 발사했을 때의 시간
    public List<MoveData> ballMoveData;
    [HideInInspector] public float shootTime;

    //속성5 : 색상 리스트
    public List<Color> ballColors;

    [SerializeField] GameObject colorBallPrefab;
    public Transform[] spawnPoints;

    public GameObject gameObjects;

    public TMP_InputField maxPlayer;

    //###새로만든 UI에 있는 조이스틱으로 변경필요 코드
    public FixedJoystick joystick;

    public string myID = "Test";

    public GuestReplayer guestReplayer;

    public TMP_InputField inputID;

    public TMP_InputField chatInput;
    //public GameObject gameUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        //if(TCP_BallCore.networkMode == NetworkMode.None)
        //{
        //gamePlayers = GameObject.FindGameObjectsWithTag("Player");
        //}
    }

    private void Start()
    {
        //if(TCP_BallCore.networkMode == NetworkMode.None)
        //{
        //    for (int i = 0; i < gamePlayers.Count; i++)
        //    {
        //        if (i == 0)
        //        {
        //            gamePlayers[i].GetComponent<BallDoll>().showcaseColor = ballColors[i];
        //            gamePlayers[i].GetComponent<BallDoll>().Init(ballColors[i], BallShowMode.MyPlayer);
        //        }
        //        else
        //        {
        //            gamePlayers[i].GetComponent<BallDoll>().showcaseColor = ballColors[i];
        //            gamePlayers[i].GetComponent<BallDoll>().Init(ballColors[i], BallShowMode.OtherPlayer);
        //        }
        //    }
        //}

        myID += Random.Range(0.0f, 1.0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(chatInput.text != null)
            {
                UI_InGame.Chatting(myID, chatInput.text);
            }
            chatInput.text = null;
        }
    }

    public IEnumerator CheckMovement(float time = 0)
    {
        yield return new WaitForSeconds(time);
        foreach (var ball in gamePlayers)
        {
            isNobodyMove = true;
            if (ball.GetComponent<BallMove>().isMove == true)
            {
                isNobodyMove = false;
                break;
            }
        }
        if (isNobodyMove)
        {
            TurnManager.Instance.EndTurn();
        }
    }

    public void SoloPlaySet(int turn)
    {
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            BallDoll ballDollGO = gamePlayers[i].GetComponent<BallDoll>();
            if (i == turn)
            {
                ballDollGO.Init(ballDollGO.showcaseColor, BallShowMode.MyPlayer);
            }
            else
            {
                ballDollGO.Init(ballDollGO.showcaseColor, BallShowMode.OtherPlayer);
                ballDollGO.CollisionEvent();
            }
        }
    }

    public void MultiPlaySet()
    {
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            BallDoll ballDollGO = gamePlayers[i].GetComponent<BallDoll>();
            if (TCP_BallCore.networkMode != NetworkMode.None)
            {
                if (gamePlayers[i].name == myID)
                {
                    ballDollGO.Init(ballDollGO.showcaseColor, BallShowMode.MyPlayer);
                }
                else
                {
                    ballDollGO.Init(ballDollGO.showcaseColor, BallShowMode.OtherPlayer);
                }
            }
            else
            {
                Debug.Log("You Call MultiPlaySet at SoloPlay");
            }
        }
    }

    public void AddPlayerData(string playerID)
    {
        if (entryPlayerDataList != null)
        {
            //foreach (var playerData in entryPlayerDataList)
            //{
            //    if (playerData.id == playerID)
            //    {
            //        Debug.Log("이미 존재하는 플레이어를 추가하려고 했습니다.");
            //        return;
            //    }
            //}
        }

        BallEntryPlayerData data = new BallEntryPlayerData();
        int randomNum = Random.Range(0, ballColors.Count);

        data.color = ballColors[randomNum];
        data.id = playerID;
        data.score = 0;
        if (entryPlayerDataList == null)
        {
            data.index = 0;
        }
        else
        {
            data.index = entryPlayerDataList.Count;
        }

        ballColors.Remove(ballColors[randomNum]);

        Debug.Log(data.id + "를 플레이어로 추가헀습니다.");
        entryPlayerDataList.Add(data);
    }

    public void RemovePlayerData(string playerID)
    {
        if (entryPlayerDataList != null)
        {
            foreach (var playerData in entryPlayerDataList)
            {
                if (playerData.id == playerID)
                {
                    entryPlayerDataList.Remove(playerData);
                    return;
                }
            }
            Debug.Log("There no player data has ID : " + playerID);
        }
        else
        {
            Debug.Log("PlayerData is empty");
        }
    }

    public void RemoveRoomPlayer(List<string> playerID)
    {
        for(int i = 0; i < playerID.Count; i++)
        {
            foreach(var playerData in entryPlayerDataList)
            {
                if (playerData.id == playerID[i])
                {
                    entryPlayerDataList.Remove(playerData);
                    break;
                }
            }
        }
    }

    public void MakeBallByData()
    {
        //RemoveSameID();

        //셔플
        //for (int i = 0; i < entryPlayerDataList.Count; i++)
        //{
        //    int randomNum = Random.Range(0, entryPlayerDataList.Count);
        //    BallEntryPlayerData tempData = entryPlayerDataList[i];
        //    entryPlayerDataList[i] = entryPlayerDataList[randomNum];
        //    entryPlayerDataList[randomNum] = tempData;
        //}

        //셔플한 순서 index에 입력
        //for (int i = 0; i < entryPlayerDataList.Count; i++)
        //{
        //    entryPlayerDataList[i].index = i;
        //}

        foreach (var playerData in entryPlayerDataList)
        {
            GameObject ballGO = Instantiate(colorBallPrefab);
            ballGO.transform.position = spawnPoints[playerData.index].position;
            ballGO.GetComponent<BallDoll>().Init(playerData.color, BallShowMode.OtherPlayer);
            ballGO.GetComponent<BallDoll>().showcaseColor = playerData.color;
            gamePlayers.Add(ballGO);
            ballGO.name = playerData.id;
        }

        foreach(var ball in gamePlayers)
        {
            guestReplayer.balls.Add(ball.GetComponent<BallDoll>());
        }

        if (TCP_BallCore.networkMode == NetworkMode.None)
        {
            SoloPlaySet(0);
        }
        else
        {
            MultiPlaySet();
        }

        TurnManager.Instance.GetListFromGameManager();

        joystick.GetComponent<BallLineRender>().ResetBallStatus();
    }

    public void StartGameSolo(int playerNumber)
    {
        Debug.Log("StartGameSolo 시작");
        MakeLocalPlayer(playerNumber);
        Debug.Log("MakeLocalPlayer 완료");
        TurnManager.Instance.GetListFromGameManager();
        Debug.Log("TurnManager에게 플레이어 목록 전송 완료");
        MakeBallByData();
        Debug.Log("공 생성 완료");
    }

    public void StartGameFromRoom()
    {
        TurnManager.Instance.GetListFromGameManager();
        MakeBallByData();
        UI_InGame.MakeNew(entryPlayerDataList);
    }

    public void StartGameFromRoomClient()
    {
        TurnManager.Instance.GetListFromGameManager();
        MakeBallByData();
        UI_InGame.MakeNew(entryPlayerDataList);
    }

    public void MakeLocalPlayer(int _playerNumber)
    {
        for(int i = 0; i < _playerNumber; i++)
        {
            int randomID = Random.Range(0, 10000);
            AddPlayerData("LocalPlayer"+randomID);
        }
    }

    public bool CheckMyBall()
    {
        if (TurnManager.Instance.GetTurnBall().name == myID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveSameID()
    {
        for(int i = 0; i < entryPlayerDataList.Count-1; i++)
        {
            for(int j = i+1; j < entryPlayerDataList.Count; j++)
            {
                if (entryPlayerDataList[i].id == entryPlayerDataList[j].id)
                {
                    RemovePlayerData(entryPlayerDataList[i].id);
                }
            }
        }
    }

    public void GetAllPlayerFromServer(List<BallEntryPlayerData> datas)
    {
        entryPlayerDataList = datas;
        if(TCP_BallUI.gameState == GameState.Room)
        {
            UI_RoomManager.MakeNew(datas);
        }
    }

    public void GetLastPlayerFromServer(BallEntryPlayerData data)
    {
        if(TCP_BallCore.networkMode != NetworkMode.Server)
        {
            entryPlayerDataList.Add(data);
        }
        if (TCP_BallUI.gameState == GameState.Room)
        {
            UI_RoomManager.MakeNew(new List<BallEntryPlayerData>() { data });
        }
    }

    public void SetID()
    {
        myID = inputID.text;
    }

    public void SetID(string _id)
    {
        myID = _id;
    }

    public void AddMoveData(MoveData _moveData)
    {
        if(ballMoveData == null)
        {
            _moveData.index = 0;
        }
        else
        {
            _moveData.index = ballMoveData.Count;
        }
        ballMoveData.Add(_moveData);
    }

    public void SetSpawnPointAndStartOrder()
    {
        ShuffleSpawnPoint();
    }

    public void ShuffleSpawnPoint()
    {
        for(int i = 0; i < spawnPoints.Length; i++)
        {
            int randomNum = Random.Range(0, spawnPoints.Length);
            Transform tmp = spawnPoints[randomNum];
            spawnPoints[randomNum] = spawnPoints[i];
            spawnPoints[i] = tmp;
        }
    }

    //public void ShootBallInNetwork(Vector3 _shootDirection)
    //{
    //    Debug.Log("Test Ball Shoot");

    //    if (GameManager.Instance.isNobodyMove)
    //    {
    //        GameManager.Instance.shootTime = Time.time;
    //        Debug.Log(TurnManager.Instance.GetTurnBall().name);
    //        TCP_BallCore.ShootTheBall(_shootDirection * 50);
    //        //TurnManager.Instance.GetTurnBall().GetComponent<Rigidbody>().AddForce(clientDirection * power, ForceMode.Impulse);
    //        GameManager.Instance.isNobodyMove = false;

    //        foreach (var balls in GameManager.Instance.gamePlayers)
    //        {
    //            GameManager.Instance.AddMoveData(balls.GetComponent<BallHit>().moveData);
    //        }

    //        StartCoroutine(GameManager.Instance.CheckMovement(1));
    //    }
    //}

    public int GetIndexOfBall(string id)
    {
        foreach(var ball in entryPlayerDataList)
        {
            if(ball.id == id)
            {
                return ball.index;
            }
        }
        return -1;
    }

    public void ClearPlayerData()
    {
        entryPlayerDataList.Clear();
        if(entryPlayerDataList != null)
        {
            //Debug.LogError("플레이어 데이터 초기화에 실패했습니다.");
        }
    }

    public void ClearMoveData()
    {
        ballMoveData.Clear();
        if(ballMoveData != null)
        {
            //Debug.LogError("BallMove 데이터 초기화에 실패했습니다.");
        }
    }
}
