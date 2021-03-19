/* Chat Manager 
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles chat functionality and data logging.
 *              
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          PUN_Manager.cs
 *       
 *       Find in unity inspector Scene -> Managers
 *          - To set which pads are recorded 
 *       Find in unity inspector Scene -> pad (multiple) 
 *          - To change the location and correct object of a pad
 *       
 *       LogData is Called on Button Press 
 *          - See unity inspector Spectator Scene -> Canvas -> Start/End Button
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class ChatManager : MonoBehaviour
{
    //Array of pads which are used to record location of objects
    //Note: In unity inspector, Spectator Scene -> Managers 
    public GameObject[] pads;

    public bool isServer, isRobot, gameOver, gameStarted;
    private float score, startTime, runTime;
    public GameObject chatPanel, textObject;
    public InputField chatBox;
    public Color playerMessage, info;
    private string content = "";
    private string path;

    public GameObject tutorialCollection;
    public GameObject mainCollection;

    [SerializeField]
    List<Message> messageList = new List<Message>();


    /* 
     * Called on first frame load
     *  - Reset startTime, score, and runTime
     *  - Find Log.txt, or create it if it cannot be found
     *  - Log initial information, namely date/time
     */
    void Start()
    {
        startTime = 0;
        score = 0;
        runTime = 0;
        gameOver = false;
        gameStarted = false; 
        path = Application.dataPath + "/Log.txt";

        if (isRobot) mainCollection.transform.position += Vector3.up * 50;
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "LUCI HuRDL Data Log \n \n");
        }
        string content = "Log Date: " + System.DateTime.Now + "\n\n";
        File.AppendAllText(path, content);
    }



    /* 
     * Called on each frame
     *  - If enter key is hit, and chat box is not empty, send message
     *  - Message type depends on isRobot
     *  - Set runtime to the time of the last message sent
     *  - Reset the chat box
     */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chatBox.text != "")
            {
                if (isRobot)
                {
                    if (chatBox.text == "/start")
                    {
                        if (gameStarted == false)
                        {
                            StartMainTask();
                            gameStarted = true; 
                        }
                    }
                    else
                    {
                        SendMessageToChat(chatBox.text, Message.MessageType.P);
                    }
                }
                else if (!isRobot)
                {
                    SendMessageToChat(chatBox.text, Message.MessageType.C);
                }
              
            }
            chatBox.text = "";
        }
    }

    /* Called when the on-screen button on the commander's view is pressed or 
     * when the participant types Start or start into the chat
     *  - In the first case,
     *      - The timer resets to 0 and chat messages begin logging
     *  - In the second case, 
     *      - The tutorial collection of objects are moved out of the way
     *      - The main task collection of objects are moved into place
     */ 
    public void StartMainTask()
    {
        if (!isRobot)
        {
            startTime = Time.time;
            gameStarted = true;
        }
        else if (isRobot)
        {
            mainCollection.transform.position -= Vector3.up * 50;
            tutorialCollection.transform.position += Vector3.up * 50;
            tutorialCollection.gameObject.active = false; 
        }
    }


    /* 
     * Called on last frame before this instance is destroyed (when the game ends)
     *  - Log data (score, runtime, objects, etc) if it is not yet logged
     */
    private void OnDestroy()
    {
        if (gameOver == false) LogData();
    }


    /* 
     * Called when sending a chat 
     *  - Create a new message
     *  - Format the message
     *  - Instantiate the message and add to list
     */
    public void SendMessageToChat(string text, Message.MessageType messageType)
    {

        Message newMessage = new Message();
        newMessage.text = " [Time " + (Time.time - startTime) + "]" + messageType + ": " + text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<Text>();

        newMessage.textObject.color = MessageTypeColor(messageType);



        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);

        if (gameStarted == true)
        {
            content = newMessage.text + "\n";
            File.AppendAllText(path, content);
        }
    }


    /* 
     * Determines the color of a message
     *  - If the message type is participant, return correct color 
     *  - If the message type is commander, return correct color 
     */
    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = info;
        Debug.Log("In message to color function");
        switch (messageType)
        {
            case Message.MessageType.P:
                color = playerMessage;
                break;
            case Message.MessageType.C:
                color = info;
                break;
        }
        return color;
    }


    /* 
     * Called when logging the data at the end of the task, aka when the on 
     * screen "End + Log Data" button is activated by the commander
     *  - Determine score
     *  - Log Scores 
     *  - Log Runtime 
     *  - Log Object Locations
     *      - Uses PadManager's current object to determine object locations 
     *      - See: PadManager.cs
     *      - See: Unity Inspector -> Spectator Scene -> Canvas -> Buttons
     */
    public void LogData()
    {
        for (int i = 0; i < pads.Length; i++)
        {
            if (pads[i].GetComponent<PadManager>().scoreIndicator == true)
                score++;
        }

        content = "\n Task Evaluation: \n" +
                      "    Correct Object Placements: " + score + " / 6\n" +
                      "    Score: " + score / 6f * 100f + "\n" +
                      "    Run Time: " + (runTime) + "\n" +
                      "    Object Locations: \n";

        File.AppendAllText(path, content);

        for (int i = 0; i < pads.Length; i++)
        {
            content = "      " + pads[i].GetComponent<PadManager>().Location + ": " + pads[i].GetComponent<PadManager>().currentObject + "\n";
            File.AppendAllText(path, content);
        }
        content = "\n\n\n";
        File.AppendAllText(path, content);

        gameOver = true;
        gameStarted = false;
    }


}



/* 
 * Message Class 
 *  - Message text
 *  - Message type 
 *  - Message color
 */
[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
    public MessageType messageType;

    public enum MessageType
    {
        P,
        C
    }
}