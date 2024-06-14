﻿using MelonLoader;
using UnityEngine;
using Riptide;
//using ImGuiNET;
using UniverseLib.Input;
using UniverseLib.UI;
using UniverseLib;
using UnityEngine.UI;
using UniverseLib.UI.Models;
using Riptide.Utils;
using UnityEngine.SocialPlatforms;
using System.Runtime.InteropServices;
using static MelonLoader.MelonLogger;
using System;
using System.Configuration;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TestMod
{
    
    public static class BuildInfo
    {
        public const string Name = "mutiplayer mod"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "karson mutiplayer mod"; // Description for the Mod.  (Set as null if none)
        public const string Author = "8bitdev"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class TestMod : MelonMod
    {
        private static bool EnableLog = false;
        private static string LevelName;
        private IntPtr context;
        public static string txt_ip = "127.0.0.1";
        public static string txt_port = "7777";
        public GameObject player2; // player2 is a model not real player 2
        private GameObject LocalPlayer;
        public static Server server;
        public static Client client;
        public static int ServerMode = -1;
        public static Vector3 p2_loc = Vector3.zero;
        public static UIBase UiBase { get; private set; }

        void MyModEntryPoint()
        {
            float startupDelay = 1f;
            UniverseLib.Config.UniverseLibConfig config;
            config = new UniverseLib.Config.UniverseLibConfig
            {
                Disable_EventSystem_Override = false, // or null
                Force_Unlock_Mouse = false, // or null
                Unhollowed_Modules_Folder = System.IO.Path.Combine("Some", "Path", "To", "Modules") // or null
            };



            UniverseLib.Universe.Init(startupDelay, OnInitialized, LogHandler, config);
        }

        
        void OnInitialized()
        {
            UiBase = UniversalUI.RegisterUI("my.unique.ID", UiUpdate);
            UIBase myUIBase = UniversalUI.RegisterUI("me.mymod", UiUpdate);
            MyPanel myPanel = new MyPanel(myUIBase);
        }

        void UiUpdate()
        {
            // Called once per frame when your UI is being displayed.

        }
        void LogHandler(string message, UnityEngine.LogType type)
        {
            MelonLogger.Msg(message);
            
        }


        [MessageHandler(2)]
        private static void HandleSomeMessageFromServer(Message message)
        {
            float[] floats = message.GetFloats();
            p2_loc = new Vector3(floats[0], floats[1], floats[2]);


            //            MelonLogger.Msg($"vec: {p2_loc}");
        }



        [MessageHandler(1)]
        private static void HandleSomeMessageFromServer(ushort id, Message message)
        {

            float[] floats = message.GetFloats();
            p2_loc = new Vector3(floats[0], floats[1], floats[2]);
            

//            MelonLogger.Msg($"vec: {p2_loc}");
        }

        //        public GameClient gamecl;
        public override void OnInitializeMelon() {
            RiptideLogger.Initialize(MelonLogger.Msg, MelonLogger.Msg, MelonLogger.Warning, MelonLogger.Error, false);
            MyModEntryPoint();
        }

        
        public override void OnSceneWasLoaded(int buildindex, string sceneName) { }
       

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            LevelName = sceneName;
            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
            LocalPlayer = GameObject.Find("Player");
            player2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player2.transform.localScale = new Vector3(1.1f, 1.7f, 1.1f);


        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            MelonLogger.Msg("OnSceneWasUnloaded: " + buildIndex.ToString() + " | " + sceneName);
        }

        public override void OnUpdate() // Runs once per frame.
        {
            player2.transform.position = p2_loc;
            if (LocalPlayer != null && ServerMode == 1)
            {
                Message message = Message.Create(MessageSendMode.Unreliable, 1);
                Vector3 lp_loc = LocalPlayer.transform.position;
                message.AddFloats(new float[] { lp_loc.x, lp_loc.y, lp_loc.z });
                client.Send(message);
            }
            if (LocalPlayer != null && ServerMode == 0)
            {
                Message message = Message.Create(MessageSendMode.Unreliable, 2);
                Vector3 lp_loc = LocalPlayer.transform.position;
                message.AddFloats(new float[] { lp_loc.x, lp_loc.y, lp_loc.z });
                message.AddString(LevelName);
                server.SendToAll(message);
            }
            //            MelonLogger.Msg("OnUpdate");

        }

        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
            if(ServerMode == 0)
            {
                server.Update();
            }
            
            if (ServerMode == 1)
            {
                client.Update();
            }
            //            MelonLogger.Msg("OnFixedUpdate");
        }

        public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
        {
//            MelonLogger.Msg("OnLateUpdate");
        }

        public override void OnGUI() {  }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            if(ServerMode == 0)
            {
                server.Stop();
            }
            if (ServerMode == 1)
            {
                client.Disconnect();
            }
        }

        public override void OnPreferencesSaved() // Runs when Melon Preferences get saved.
        {
            MelonLogger.Msg("OnPreferencesSaved");
        }

        public override void OnPreferencesLoaded() // Runs when Melon Preferences get loaded.
        {
            MelonLogger.Msg("OnPreferencesLoaded");
        }

        public static void SetupServer()
        {
            MelonLogger.Msg("trying to setup Server...");
            server = new Server();
            
            server.Start(ushort.Parse(txt_port), 10, 0, true);
            ServerMode = 0;

        }

        public static void SetupClient()
        {
            MelonLogger.Msg("trying to setup Client...");
            client = new Client();
            client.Connect($"{txt_ip}:{txt_port}", 5, 0, null, true);
            ServerMode = 1;
        }
        
    }


}

public class MyPanel : UniverseLib.UI.Panels.PanelBase
{
    public MyPanel(UIBase owner) : base(owner) { }

    public override string Name => "Karson MutiPlayer Mod";
    public override int MinWidth => 200;
    public override int MinHeight => 200;
    public override Vector2 DefaultAnchorMin => new Vector2(0.25f, 0.25f);
    public override Vector2 DefaultAnchorMax => new Vector2(0.75f, 0.75f);
    public override bool CanDragAndResize => true;

    protected override void ConstructPanelContent()
    {
     
        // Creates Start Serrver Button
        ButtonRef StartServerButton = UIFactory.CreateButton(ContentRoot, "StartServerButton", "Start Server");
        UIFactory.SetLayoutElement(StartServerButton.Component.gameObject, minWidth: 200, minHeight: 50);
        StartServerButton.OnClick += OnStartServerClick;

        // Creates Connect To Server Button
        ButtonRef ConnectToServerButton = UIFactory.CreateButton(ContentRoot, "ConnectToServerButton", "Connect To Server");
        UIFactory.SetLayoutElement(ConnectToServerButton.Component.gameObject, minWidth: 200, minHeight: 50);
        ConnectToServerButton.OnClick += OnConectToServerButtonClick;

        // Create a IP text box 
        InputFieldRef IPFieldRef = UIFactory.CreateInputField(ContentRoot, "IPField", "IP");
        UIFactory.SetLayoutElement(IPFieldRef.Component.gameObject, minWidth: 200, minHeight: 30);
        IPFieldRef.Text = TestMod.TestMod.txt_ip;
        IPFieldRef.OnValueChanged += OnIPFieldValueChanged;

        // Create a Port text box 
        InputFieldRef PortFieldRef = UIFactory.CreateInputField(ContentRoot, "IPField", "IP");
        UIFactory.SetLayoutElement(PortFieldRef.Component.gameObject, minWidth: 200, minHeight: 30);
        PortFieldRef.Text = TestMod.TestMod.txt_port;
        PortFieldRef.OnValueChanged += OnPortFieldValueChanged;
    }


    private void OnStartServerClick()
    {
        TestMod.TestMod.SetupServer();
    }

    private void OnCheckboxValueChanged(bool value)
    {
        
    }

    private void OnConectToServerButtonClick()
    {
        TestMod.TestMod.SetupClient();
    }
    private void OnIPFieldValueChanged(string value) 
    { 
   
        MelonLogger.Msg($"Input Field Value Changed: {value}");
        TestMod.TestMod.txt_ip = value;
    }

    private void OnPortFieldValueChanged(string value)
    {

        MelonLogger.Msg($"Input Field Value Changed: {value}");
        TestMod.TestMod.txt_port = value;
    }

}