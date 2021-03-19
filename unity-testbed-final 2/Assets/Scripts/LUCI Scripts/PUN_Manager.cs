/* PUN_Manager Class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles photon networking for the task
 *              - Establishing connection to master and hurdle test room
 *              - Updating object transforms 
 *              - Maintaining Connection
 *              - Preventing task interruption from other users
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          TransformObject.cs
 *          ChatManager.cs
 *          
 *       Find in unity inspector, Managers -> PUN_Manager Settings
 *          
 *       In use room name: "hurdle test room"
 */ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityWebGLSpeech;
using System.IO;


public class PUN_Manager : MonoBehaviourPunCallbacks
{
    //Photon Relevant 
    public static PUN_Manager instance;
    private bool once = true;
    public bool isServer, isRobot;
    public Text debugText;
    bool stayDisconnected;
    PhotonView view;
    float send = 5;
    bool init = false;
    string robotCommand;
    public Canvas GameEndCanvas, gameCanvas;
    public string roomName;
    public float walkSpeed = 1, turnSpeed = 5;
    bool isOtherPlayer;
    public InputField sentMessageText;
    public ChatManager chat;
    Vector3 linearCommand, angularCommand;
    bool connectedToMaster;
    public InputField[] chatBox;
    public RoomOptions roomOptions;
    bool wasInRoom;


    //Dictionary of objects' transforms
    Dictionary<string, TransformObject> moveables;
    List<Vector3> lastPos;
    List<Quaternion> lastRot;

    float wait = 3;
    //int index = 0;

    //Current Player Index (server = 1, player = 2+) 
    int currentPlayerID = 0;

    //Red On-Screen Message
    public Text roomFullText;


    //Environment Transforms  
    public Transform plasmaEmitter;
    public Transform bypass;
    public Transform deltaCalibrator;
    public Transform cryoCalibrator;
    public Transform megabandModule;
    public Transform ultrabandModule;
    public Transform quantumOptimizer;
    public Transform hyperbandModule;
    public Transform adaptiveCapacitor;
    public Transform advancedOptimizer;
    public Transform organicEmitter;
    public Transform temporalEmitter;
    public Transform teslaCapacitor;
    public Transform mechanoCalibrator;
    public Transform sonicOptimizer;
    public Transform electroCapacitor;
    public Transform galvanicSynthesizer;
    public Transform optimizedSynthesizer;
    public Transform modularSynthesizer;
    public Transform doorSecondaryTop;
    public Transform doorSecondaryBottom;
    public Transform doorLockerA;
    public Transform doorLockerB;
    public Transform doorLockerC;
    public Transform lockerSwitch;
    public Transform lockerSlider;
    public Transform crateCoverA;
    public Transform crateCoverB;
    public Transform crateCoverC;
    public Transform doorPrimaryLeft;
    public Transform doorPrimaryRight;
    public Transform blueSim;
    public Transform brownSim;
    public Transform redSim;
    public Transform greySim;
    public Transform rootBone;

    public Transform redObject;
    public Transform blueObject;
    public Transform greenObject; 





    /* 
     * Called on first frame load
     *  - If client, do not recieve transform updates (because sending) 
     *  - Make ObjectTransforms dictionary
     */
    void Start()
    {
        //Attempt connection to hurdle test room 
        Application.runInBackground = true;
        instance = this;
        roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectUsingSettings();
        view = transform.GetComponent<PhotonView>();
        once = true;
        PhotonNetwork.KeepAliveInBackground = 6000;

        if (!isServer)
            turnTransformsOff(); 

        makeTransformDictionary(); 

        foreach (TransformObject to in moveables.Values)
        {
            lastPos.Add(to.transform.position);
            lastRot.Add(to.transform.rotation);
        }
    }



    /* 
     * Called on each frame update
     *  - If in room, and is client, and transforms have changed, send update
     *  - If not in room, attempt reconnection 
     */
    void Update()
    {
        //If client is in hurdle test room, send updates
        if (PhotonNetwork.InRoom)
        {
            send -= Time.deltaTime;
            wasInRoom = true;

            //If this is the client, send updates on object transform values if they have changed 
            if (!isServer)
                sendTransformUpdatesOnChange();

            //If this is the server, 
            else
            {
                if (send <= 0)
                    send = 3;
            }
        }
        //If not in room, create and join if server or join if client
        else
        {
            getRoomConnection(); 
        }
    }


    /* 
     * Called when hurdle test room is joined
     *  - Disables "Please Wait" on screen message
     *  - Double checks that hurdle test room is not pre-occupied
     *      - For the case that MaxPlayers cap fails
     */
    public override void OnJoinedRoom()
    {
        if (roomFullText != null)
            roomFullText.enabled = false;

        currentPlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("Joined Player ID: " + currentPlayerID);
        if (currentPlayerID > 2)
        {
            Debug.Log("Waiting for other player to finish");
            PhotonNetwork.LeaveRoom();
        }

        base.OnJoinedRoom();
    }


    /* 
     * Called on disconnect from master
     *  - Log disconnection cause
     */
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);
        base.OnDisconnected(cause);
    }


    /* 
     * Called on connection to master 
     *  - Log connection
     *  - If this is the server, create or join the room
     */
    public override void OnConnectedToMaster()
    {
        Debug.Log("connected to master");
        connectedToMaster = true;
        if (isServer)
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        else
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        base.OnConnectedToMaster();
    }


    /* 
     * Expected call on attempt to join full room ( > MaxPlayers ) 
     *  - Logs fail cause 
     *  - Display message to on screen 
     *  - Freeze player ( time scale = 0 ) 
     */
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode + " : Message :  " + message);
        if (!isServer)
        {
            if (roomFullText != null)
            {
                roomFullText.enabled = true;
                roomFullText.text = "Please Return Hit \n " +
                                    "Currently Testing Other Subject"; 
            }
            Time.timeScale = 0;
        }
        base.OnJoinRoomFailed(returnCode, message);
    }


    /* 
     * Called when sending chat
     *  - If client, send with participant tag 
     *  - If server, send with commander tag
     */
    public void SendChatMessage(string message)
    {
        if (!isServer)
            view.RPC("Message", RpcTarget.Others, message, global::Message.MessageType.P);
        else
            view.RPC("Message", RpcTarget.Others, message, global::Message.MessageType.C);
    }

    //Start region photon callbacks
    #region Photon Callbacks


    /* 
     * Called when players enter room 
     *  - Log player photon nickname
     */
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            isOtherPlayer = true;
        }
    }



    /* 
     * RPC Call for Robot root bone position 
     */
    [PunRPC]
    void RootBonePositionRPC(Vector3 position)
    {
        rootBone.GetComponent<TransformObject>().toPosition = position;
    }


    /* 
     * RPC Call for Robot root bone rotation 
     */
    [PunRPC]
    void RootBoneRotationRPC(Quaternion rotation)
    {
        rootBone.GetComponent<TransformObject>().toRotation = rotation;
    }


    /* 
     * RPC Call for disconnection
     *  - If message is disconnect, disconnect and stay disconnected
     */
    [PunRPC]
    void Message(string message, global::Message.MessageType type)
    {
        if (!isServer && message.Contains(":disconnect:"))
        {
            PhotonNetwork.Disconnect();
            stayDisconnected = true;
        }
        chat.SendMessageToChat(message, type);
    }

    /* 
     * RPC Call for objects' positions
     */
    [PunRPC]
    void TransformPosition(string message)
    {
        string[] split = message.Split(':');

        string[] sArray = split[1].Split(',');
        Debug.Log("sending position");
        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        moveables[split[0]].toPosition = result;
    }

    /* 
     * RPC Call for objects' rotations 
     */
    [PunRPC]
    void TransformRotation(string message)
    {
        string[] split = message.Split(':');

        string[] sArray = split[1].Split(',');

        // store as a Vector3
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));
        ;

        moveables[split[0]].toRotation = result;
    }

    //End photon callback region
    #endregion


    /***                                        ***/
    /***    Helper functions defined bellow     ***/
    /***                                        ***/

    /* 
     * Called at start if this is server
     *  - Turns off all objects' TransformObject scripts (updater script) 
     */
    void turnTransformsOff()
    {
        plasmaEmitter.GetComponent<TransformObject>().enabled = false;
        bypass.GetComponent<TransformObject>().enabled = false;
        deltaCalibrator.GetComponent<TransformObject>().enabled = false;
        cryoCalibrator.GetComponent<TransformObject>().enabled = false;
        megabandModule.GetComponent<TransformObject>().enabled = false;
        ultrabandModule.GetComponent<TransformObject>().enabled = false;
        quantumOptimizer.GetComponent<TransformObject>().enabled = false;
        hyperbandModule.GetComponent<TransformObject>().enabled = false;
        adaptiveCapacitor.GetComponent<TransformObject>().enabled = false;
        advancedOptimizer.GetComponent<TransformObject>().enabled = false;
        organicEmitter.GetComponent<TransformObject>().enabled = false;
        temporalEmitter.GetComponent<TransformObject>().enabled = false;
        teslaCapacitor.GetComponent<TransformObject>().enabled = false;
        mechanoCalibrator.GetComponent<TransformObject>().enabled = false;
        sonicOptimizer.GetComponent<TransformObject>().enabled = false;
        electroCapacitor.GetComponent<TransformObject>().enabled = false;
        galvanicSynthesizer.GetComponent<TransformObject>().enabled = false;
        optimizedSynthesizer.GetComponent<TransformObject>().enabled = false;
        modularSynthesizer.GetComponent<TransformObject>().enabled = false;
        doorSecondaryTop.GetComponent<TransformObject>().enabled = false;
        doorSecondaryBottom.GetComponent<TransformObject>().enabled = false;
        doorLockerA.GetComponent<TransformObject>().enabled = false;
        doorLockerB.GetComponent<TransformObject>().enabled = false;
        doorLockerC.GetComponent<TransformObject>().enabled = false;
        lockerSwitch.GetComponent<TransformObject>().enabled = false;
        lockerSlider.GetComponent<TransformObject>().enabled = false;
        crateCoverA.GetComponent<TransformObject>().enabled = false;
        crateCoverB.GetComponent<TransformObject>().enabled = false;
        crateCoverC.GetComponent<TransformObject>().enabled = false;
        doorPrimaryLeft.GetComponent<TransformObject>().enabled = false;
        doorPrimaryRight.GetComponent<TransformObject>().enabled = false;
        blueSim.GetComponent<TransformObject>().enabled = false;
        brownSim.GetComponent<TransformObject>().enabled = false;
        redSim.GetComponent<TransformObject>().enabled = false;
        greySim.GetComponent<TransformObject>().enabled = false;
        redObject.GetComponent<TransformObject>().enabled = false;
        blueObject.GetComponent<TransformObject>().enabled = false;
        greenObject.GetComponent<TransformObject>().enabled = false; 
        rootBone.GetComponent<TransformObject>().enabled = false;
    }

    /* 
     * Makes dictionary of objects' TransformObjects 
     */
    void makeTransformDictionary()
    {
        moveables = new Dictionary<string, TransformObject>();
        lastPos = new List<Vector3>();
        lastRot = new List<Quaternion>();
        moveables.Add(plasmaEmitter.name, plasmaEmitter.GetComponent<TransformObject>());
        moveables.Add(bypass.name, bypass.GetComponent<TransformObject>());
        moveables.Add(deltaCalibrator.name, deltaCalibrator.GetComponent<TransformObject>());
        moveables.Add(cryoCalibrator.name, cryoCalibrator.GetComponent<TransformObject>());
        moveables.Add(megabandModule.name, megabandModule.GetComponent<TransformObject>());
        moveables.Add(ultrabandModule.name, ultrabandModule.GetComponent<TransformObject>());
        moveables.Add(quantumOptimizer.name, quantumOptimizer.GetComponent<TransformObject>());
        moveables.Add(hyperbandModule.name, hyperbandModule.GetComponent<TransformObject>());
        moveables.Add(adaptiveCapacitor.name, adaptiveCapacitor.GetComponent<TransformObject>());
        moveables.Add(advancedOptimizer.name, advancedOptimizer.GetComponent<TransformObject>());
        moveables.Add(organicEmitter.name, organicEmitter.GetComponent<TransformObject>());
        moveables.Add(temporalEmitter.name, temporalEmitter.GetComponent<TransformObject>());
        moveables.Add(teslaCapacitor.name, teslaCapacitor.GetComponent<TransformObject>());
        moveables.Add(mechanoCalibrator.name, mechanoCalibrator.GetComponent<TransformObject>());
        moveables.Add(sonicOptimizer.name, sonicOptimizer.GetComponent<TransformObject>());
        moveables.Add(electroCapacitor.name, electroCapacitor.GetComponent<TransformObject>());
        moveables.Add(galvanicSynthesizer.name, galvanicSynthesizer.GetComponent<TransformObject>());
        moveables.Add(optimizedSynthesizer.name, optimizedSynthesizer.GetComponent<TransformObject>());
        moveables.Add(modularSynthesizer.name, modularSynthesizer.GetComponent<TransformObject>());
        moveables.Add(doorSecondaryTop.name, doorSecondaryTop.GetComponent<TransformObject>());
        moveables.Add(doorSecondaryBottom.name, doorSecondaryBottom.GetComponent<TransformObject>());
        moveables.Add(doorLockerA.name, doorLockerA.GetComponent<TransformObject>());
        moveables.Add(doorLockerB.name, doorLockerB.GetComponent<TransformObject>());
        moveables.Add(doorLockerC.name, doorLockerC.GetComponent<TransformObject>());
        moveables.Add(lockerSwitch.name, lockerSwitch.GetComponent<TransformObject>());
        moveables.Add(lockerSlider.name, lockerSlider.GetComponent<TransformObject>());
        moveables.Add(crateCoverA.name, crateCoverA.GetComponent<TransformObject>());
        moveables.Add(crateCoverB.name, crateCoverB.GetComponent<TransformObject>());
        moveables.Add(crateCoverC.name, crateCoverC.GetComponent<TransformObject>());
        moveables.Add(doorPrimaryLeft.name, doorPrimaryLeft.GetComponent<TransformObject>());
        moveables.Add(doorPrimaryRight.name, doorPrimaryRight.GetComponent<TransformObject>());
        moveables.Add(blueSim.name, blueSim.GetComponent<TransformObject>());
        moveables.Add(brownSim.name, brownSim.GetComponent<TransformObject>());
        moveables.Add(redSim.name, redSim.GetComponent<TransformObject>());
        moveables.Add(greySim.name, greySim.GetComponent<TransformObject>());
        moveables.Add(rootBone.name, rootBone.GetComponent<TransformObject>());
        moveables.Add(redObject.name, redObject.GetComponent<TransformObject>());
        moveables.Add(greenObject.name, greenObject.GetComponent<TransformObject>());
        moveables.Add(blueObject.name, blueObject.GetComponent<TransformObject>());

    }

    /* 
     * Called on frame update 
     *  - Updates transforms if they have changed
     */
    void sendTransformUpdatesOnChange()
    {
        int i = 0;

        foreach (KeyValuePair<string, TransformObject> kvp in moveables)
        {
            if (kvp.Value.transform.position != lastPos[i])
            {
                lastPos[i] = kvp.Value.transform.position;
                string positionString = kvp.Value.transform.position.x.ToString("0.##") + "," +
                                        kvp.Value.transform.position.y.ToString("0.##") + "," +
                                        kvp.Value.transform.position.z.ToString("0.##");
                send = 1;
                view.RPC("TransformPosition", RpcTarget.Others, kvp.Key + ":" + positionString);
            }

            if (kvp.Value.transform.rotation != lastRot[i])
            {
                lastRot[i] = kvp.Value.transform.rotation;

                string rotationString = kvp.Value.transform.rotation.x.ToString("0.##") + "," +
                                        kvp.Value.transform.rotation.y.ToString("0.##") + "," +
                                        kvp.Value.transform.rotation.z.ToString("0.##") + "," +
                                        kvp.Value.transform.rotation.w.ToString("0.##");
                view.RPC("TransformRotation", RpcTarget.Others, kvp.Key + ":" + rotationString);
            }
            i++;
        }
    }


    /* 
     * Called on updated frame if not connected to room 
     *  - If disconnected from master, attempt reconnection
     *  - If connected to master, create or join hurdle test room
     */
    void getRoomConnection()
    {
        if (!stayDisconnected && connectedToMaster)
        {
            wait -= Time.deltaTime;
            if (wait <= 0)
            {


                if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
                {
                    PhotonNetwork.ConnectUsingSettings();
                    wait = 2;
                }
                else
                {
                    if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToGameServer || PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                    {
                        if (isServer)
                            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
                        else
                            PhotonNetwork.JoinRoom(roomName);

                        wait = 2;
                    }
                }
            }
        }
    }
}