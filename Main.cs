using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Riptide;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using Riptide.Utils;
using System;
using UnityEngine.UIElements;
using UnityEngine.Networking;

namespace TestMod
{

    public static class BuildInfo
    {
        public const string Name = "mutiplayer mod"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "karlson mutiplayer mod"; // Description for the Mod.  (Set as null if none)
        public const string Author = "8bitdev"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = "https://github.com/devsssss/KarsonMutiplayerMod/releases"; // Download Link for the Mod.  (Set as null if none)
    }

    public class TestMod : MelonMod
    {
        //        private static bool EnableLog = false;
        private static string LevelName;
        //        private IntPtr context;
        public static string txt_ip = "127.0.0.1";
        public static string txt_port = "7777";
        public static GameObject player2_Model;
        public GameObject player2;
        private GameObject LocalPlayer;
        public static Server server;
        public static int Player2_Weapon_Id = -1;
        public static GameObject Player2_Weapon;
        public static Client client;
        public static int ServerMode = -1;
        public static Vector3 p2_loc = Vector3.zero;
        public static Vector3 p2_loc_old = Vector3.zero;
        private Mesh GrappleMesh;
        public GameObject Player2Gun;
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
            SCMessge(message);
        }



        [MessageHandler(1)]
        private static void HandleSomeMessageFromServer(ushort id, Message message)
        {
            SCMessge(message);
        }

        private static void SCMessge(Message message)
        {
            float[] floats = message.GetFloats();
            p2_loc = new Vector3(floats[0], floats[1], floats[2]);
            if (message.GetBool()) { player2_Model.transform.localScale = new Vector3(1, 0.5f, 1); }
            else { player2_Model.transform.localScale = new Vector3(1, 1.5f, 1); };
            GameObject P2_W = GameObject.Find(message.GetString());
            MelonLogger.Msg($"Player 2 Weapon {P2_W.name}");
            if (P2_W != null) 
            {
                Pickup P2_WS = P2_W.GetComponent<Pickup>();
                int P2_WC = WPGetWeaponId(P2_WS);
                if(!(P2_WC == Player2_Weapon_Id))
                {
                    Player2_Weapon = GameObject.Instantiate(P2_W, Vector3.zero, Quaternion.identity);
                }
            }
            else
            {
                GameObject.Destroy(Player2_Weapon);
            }     
            
        }
        public override void OnInitializeMelon() {
            RiptideLogger.Initialize(MelonLogger.Msg, MelonLogger.Msg, MelonLogger.Warning, MelonLogger.Error, false);
             MyModEntryPoint();
            
        }        
        public override void OnSceneWasLoaded(int buildindex, string sceneName) { }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            SpawnMeshFromResources();
            LevelName = sceneName;
            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
            LocalPlayer = GameObject.Find("Player");
            player2_Model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player2_Model.AddComponent<Rigidbody>();
            player2_Model.transform.localScale = new Vector3(1.1f, 1.7f, 1.1f);
            
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            MelonLogger.Msg("OnSceneWasUnloaded: " + buildIndex.ToString() + " | " + sceneName);
        }

        public static int LPGetWeaponId(DetectWeapons LP_DetectWeapons) // Get Weapon Id Turns The Gun That you are holding into a int class
        {

            if (!LP_DetectWeapons.HasGun()) { return -1; } // Has No Guns
            if (LP_DetectWeapons.GetWeaponScript().name == "Grappler") { return 0; } // Gun = Grappler
            RangedWeapon LC_RangedWeapon = (RangedWeapon)LP_DetectWeapons.GetWeaponScript();
            if (LC_RangedWeapon == null) { return -1; } // Unkown Gun
            if (LC_RangedWeapon.attackSpeed == 0.4f) { return 1; } // Gun = Pistel
            if (LC_RangedWeapon.attackSpeed == 0.15f) { return 2; } // Gun = Uzi
            if (LC_RangedWeapon.attackSpeed == 1f) // The Boomer And Shotgun use attackSpeed 1
            {
                if (LC_RangedWeapon.pushBackForce == 60) { return 3; } // Gun = Shotgun
                if (LC_RangedWeapon.pushBackForce == 50) { return 4; } // Gun = Boomer
            }
            return -1; // Unkown Gun
        }

        public static int WPGetWeaponId(Pickup LP_DetectWeapons) // Get Weapon Id Turns The Gun That you are holding into a int class
        {
            if (LP_DetectWeapons.name == "Grappler") { return 0; } // Gun = Grappler
            RangedWeapon LC_RangedWeapon = (RangedWeapon)LP_DetectWeapons;
            if (LC_RangedWeapon == null) { return -1; } // Unkown Gun
            if (LC_RangedWeapon.attackSpeed == 0.4f) { return 1; } // Gun = Pistel
            if (LC_RangedWeapon.attackSpeed == 0.15f) { return 2; } // Gun = Uzi
            if (LC_RangedWeapon.attackSpeed == 1f) // The Boomer And Shotgun use attackSpeed 1
            {
                if (LC_RangedWeapon.pushBackForce == 60) { return 3; } // Gun = Shotgun
                if (LC_RangedWeapon.pushBackForce == 50) { return 4; } // Gun = Boomer
            }
            return -1; // Unkown Gun
        }



        public override void OnUpdate() // Runs once per frame.
        {
            Time.timeScale = 1.0f;
            DetectWeapons LP_DetectWeapons = LocalPlayer.GetComponentInChildren<DetectWeapons>();
            if (!(p2_loc == p2_loc_old))
            {
                player2_Model.transform.localPosition = p2_loc;
            }
            p2_loc_old = p2_loc;
            // Packet Creates
            Message message = null;
            if (ServerMode == 0){ message = Message.Create(MessageSendMode.Unreliable, 2); }
            if (ServerMode == 1) { message = Message.Create(MessageSendMode.Unreliable, 1); }
            Vector3 lp_loc = LocalPlayer.transform.position;
            PlayerMovement LC_PM = LocalPlayer.GetComponent<PlayerMovement>();
            Vector3 lc_rot = LC_PM.playerCam.transform.rotation.eulerAngles;
            
            message.AddFloats(new float[] { lp_loc.x, lp_loc.y, lp_loc.z});
            
            message.AddBool(Input.GetKey(KeyCode.LeftControl));
            message.AddInt(LPGetWeaponId(LP_DetectWeapons));
                if (LP_DetectWeapons.HasGun())
            {
                message.AddString(LP_DetectWeapons.GetWeapon().name);
            }
            else
            {
                message.AddString("");
            }
            if (LocalPlayer != null && ServerMode == 1) { client.Send(message); }
            if (LocalPlayer != null && ServerMode == 0) { server.SendToAll(message); } 

        }
         
        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
            if (Input.GetKeyDown(KeyCode.K))
            {

            }                                                                                                                                                                                                                  
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

        public GameObject CreateWebPlayer()
        {
            GameObject WepPlayer = Object.Instantiate(LocalPlayer, new Vector3(0, 0, 0), Quaternion.identity);
            

            return WepPlayer;
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

    private InputFieldRef ModelIDTextBox;
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

        // Debug Stuff
        Text DebugText = UIFactory.CreateLabel(ContentRoot, "DebugText", "Debug Stuff");
        UIFactory.SetLayoutElement(DebugText.gameObject, minWidth: 200, minHeight: 25);

        // Creates Spawn Model Button
        ButtonRef SpawnModelButton = UIFactory.CreateButton(ContentRoot, "SpawnModelButton", "Spawn Model");
        UIFactory.SetLayoutElement(SpawnModelButton.Component.gameObject, minWidth: 200, minHeight: 50);
        ConnectToServerButton.OnClick += SpawnModelClick;

        // Create a Model Id
        ModelIDTextBox = UIFactory.CreateInputField(ContentRoot, "ModelID", "MODEL  Name");
        UIFactory.SetLayoutElement(ModelIDTextBox.Component.gameObject, minWidth: 200, minHeight: 30);
        IPFieldRef.OnValueChanged += OnIPFieldValueChanged;
    }

    private void SpawnModelClick()
    {
        GameObject meshPrefab = Resources.Load<GameObject>(ModelIDTextBox.Text);
        GameObject.Instantiate(meshPrefab, new Vector3(0, 10, 0), Quaternion.identity);
        

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