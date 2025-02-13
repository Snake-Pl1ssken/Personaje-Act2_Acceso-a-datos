using SFML.Graphics;
using SFML.Audio;
using SFML.Window;
using SFML.System;

using System.IO;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace MansionExplorer
{

    internal static class Program
    {
        // Constants

        const string connectionString = "Server=127.0.0.1; Port=3307; Database=mansion; Uid=root; Pwd=;";
        const int initialRoom = 1;

        const int characterStartX = 100;
        const int characterStartY = 230;
        const int characterOffsetX = 400;

        const int characterNameOffsetX = -10;
        const int characterNameOffsetY = 240;
        const int characterNameTextOffsetX = 35;
        const int characterNameTextOffsetY = 15;

        const string storyTitle = "Mansion explorer";

        // Background

        static Dictionary<string, Texture> backgroundTextures;

        static Sprite backgroundSprite;

        // Characters

        static Dictionary<string, Texture> characterTextures;
        static Sprite characterSprite;

        // Music

        static Dictionary<string, Music> musics;


        // Dialog

        static Texture exploreTexture;
        static Sprite exploreBaseSprite;

        static Font font;

        static Text exploreRoomTitleNText;
        static Text exploreRoomTitleEText;
        static Text exploreRoomTitleOText;
        static Text exploreRoomTitleSText;

        static Text exploreRoomTitleText;
        static Text exploreRoomDescriptionText1;
        static Text exploreRoomDescriptionText2;
        static Text exploreRoomDescriptionText3;


        static bool goNPressed;
        static bool goEPressed;
        static bool goOPressed;
        static bool goSPressed;

        static Texture characterNameBaseTexture;
        static Sprite characterNameBaseSprite;
        static Text characterNameText;

        static bool test;

        // Characters

        struct CharacterProperties
        {
            public string name;
            public string image;
        }

        static List<CharacterProperties> characters;

        // Room

        static int room;
        static string roomName;
        static string roomDescription;
        static string roomBackground;

        static int roomN;
        static bool hasRoomN;
        static string roomNName;

        static int roomS;
        static bool hasRoomS;
        static string roomSName;

        static int roomE;
        static bool hasRoomE;
        static string roomEName;

        static int roomW;
        static bool hasRoomW;
        static string roomWName;

        // Database

        static DbConnection dbConnection;

        static void Main()
        { 
            // Window initialization

            var mode = new VideoMode((uint)1280, (uint)720);
            var window = new RenderWindow(mode, storyTitle);
            window.KeyPressed += OnKeyPressed;
            window.MouseButtonPressed += OnMousePressed;

            // Background initialization

            string[] files = Directory.GetFiles("backgrounds\\");

            backgroundTextures = new Dictionary<string, Texture>();

            for (int i = 0; i < files.Length; i++)
            {
                string name = GetFileName(files[i]);
                backgroundTextures[name] = new Texture(files[i]);
            }

            Texture texture = backgroundTextures.First().Value;
            backgroundSprite = new Sprite();
            backgroundSprite.Texture = texture;
            backgroundSprite.Position = new Vector2f(0, 0);

            // Characters initialization

            characters = new List<CharacterProperties>(50);

            files = Directory.GetFiles("characters\\");

            characterTextures = new Dictionary<string, Texture>();

            for (int i = 0; i < files.Length; i++)
            {
                string name = GetFileName(files[i]);
                characterTextures[name] = new Texture(files[i]);
            }

            texture = characterTextures.First().Value;
            characterSprite = new Sprite();
            characterSprite.Texture = texture;
            characterSprite.Position = new Vector2f(characterStartX, characterStartY);

            // Music initialization

            files = Directory.GetFiles("musics\\");

            musics = new Dictionary<string, Music>();

            for (int i = 0; i < files.Length; i++)
            {
                string name = GetFileName(files[i]);
                musics[name] = new Music(files[i]);

                Console.WriteLine("Loaded music " + name);
            }

            musics["normal"].Play();

            // Explore UI initialization

            exploreTexture = new Texture("ui\\explore.png");

            exploreBaseSprite = new Sprite();
            exploreBaseSprite.Texture = exploreTexture;
            exploreBaseSprite.Position = new Vector2f(0, 550);

            font = new Font("fonts\\default.ttf");

            exploreRoomDescriptionText1 = new Text("Descripcion de la habitacion", font);
            exploreRoomDescriptionText1 .Position = new Vector2f(689, 612);
            exploreRoomDescriptionText1.CharacterSize = 20;

            exploreRoomDescriptionText2 = new Text("sigue la descripcion", font);
            exploreRoomDescriptionText2 .Position = new Vector2f(689, 640);
            exploreRoomDescriptionText2.CharacterSize = 20;

            exploreRoomDescriptionText3 = new Text("mas descripcion aun", font);
            exploreRoomDescriptionText3 .Position = new Vector2f(689, 667);
            exploreRoomDescriptionText3.CharacterSize = 20;

            exploreRoomTitleText = new Text("Titulo habitacion", font);
            exploreRoomTitleText.Position = new Vector2f(737, 575);
            exploreRoomTitleText.CharacterSize = 20;

            exploreRoomTitleNText = new Text("Titulo habitacion N", font);
            exploreRoomTitleNText.Position = new Vector2f(268, 570);
            exploreRoomTitleNText.CharacterSize = 20;

            exploreRoomTitleEText = new Text("Titulo habitacion E", font);
            exploreRoomTitleEText.Position = new Vector2f(432, 622);
            exploreRoomTitleEText.CharacterSize = 20;

            exploreRoomTitleOText = new Text("Titulo habitacion O", font);
            exploreRoomTitleOText.Position = new Vector2f(164, 622);
            exploreRoomTitleOText.CharacterSize = 20;

            exploreRoomTitleSText = new Text("Titulo habitacion S", font);
            exploreRoomTitleSText.Position = new Vector2f(275, 678);
            exploreRoomTitleSText.CharacterSize = 20;

            // Characters UI initialization

            characterNameBaseTexture = new Texture("ui\\characterNameBase.png");

            characterNameBaseSprite = new Sprite();
            characterNameBaseSprite.Texture = characterNameBaseTexture;
            characterNameBaseSprite.Position = new Vector2f(characterStartX + characterNameOffsetX, characterStartY + characterNameOffsetY);

            characterNameText = new Text("", font);

            // Iniciar reloj

            Clock refreshRoomClock = new Clock();
            refreshRoomClock.Restart();

            // Iniciar DB

            dbConnection = new MySqlConnection(connectionString);
            dbConnection.Open();


            // Poner habitacion inicial

            SetRoom(initialRoom);

            Console.WriteLine("Iniciada habitacion");

            // Start the game loop
            while (window.IsOpen)
            {
                // Process events
                window.DispatchEvents();

                // Game logic goes here

                if(refreshRoomClock.ElapsedTime.AsSeconds() > 3.0f)
                {
                    if(CheckRoom(room)) { SetRoom(room); }
                    else { SetRoom(initialRoom); }

                    refreshRoomClock.Restart();
                }

                if(goNPressed)
                {
                    // Cambiar a habitacion N

                    if(hasRoomN) { SetRoom(roomN); refreshRoomClock.Restart(); }
                }
                else if(goEPressed)
                {
                    // Cambiar a habitacion E

                    if(hasRoomE) { SetRoom(roomE); refreshRoomClock.Restart(); }
                }
                else if(goOPressed)
                {
                    // Cambiar a habitacion O
                    if(hasRoomW) { SetRoom(roomW); refreshRoomClock.Restart(); }
                }
                else if(goSPressed)
                {
                    // COMPLETAR: Cambiar a habitacion S
                    Console.WriteLine("Ir a S");

                    if(hasRoomS) { SetRoom(roomS); refreshRoomClock.Restart(); }
                }
                else if(test)
                {
                    SetRoom(room + 1);
                }

                // Draw

                window.Draw(backgroundSprite);


                for(int i = 0; i < characters.Count; i++)
                {
                    int posX = characterStartX + i * characterOffsetX;
                    int posY = characterStartY;

                    characterSprite.Position = new Vector2f(posX, posY);
                    characterSprite.Texture = characterTextures[characters[i].image];
                    characterNameBaseSprite.Position = new Vector2f(posX + characterNameOffsetX,
                                                                    posY +  characterNameOffsetY);
                    characterNameText.Position = new Vector2f(posX + characterNameOffsetX + characterNameTextOffsetX , 
                                                              posY + characterNameOffsetY + characterNameTextOffsetY );
                    characterNameText.DisplayedString = characters[i].name;

                    window.Draw(characterSprite);
                    window.Draw(characterNameBaseSprite);
                    window.Draw(characterNameText);
                }

                window.Draw(exploreBaseSprite);
                window.Draw(exploreRoomTitleText);
                window.Draw(exploreRoomDescriptionText1);
                window.Draw(exploreRoomDescriptionText2);
                window.Draw(exploreRoomDescriptionText3);

                window.Draw(exploreRoomTitleNText);
                window.Draw(exploreRoomTitleEText);
                window.Draw(exploreRoomTitleOText);
                window.Draw(exploreRoomTitleSText);

                // Finally, display the rendered frame on screen
                window.Display();

                goNPressed = false;
                goEPressed = false;
                goOPressed = false;
                goSPressed = false;

                test = false;

            }
        }

        static string GetFileName(string fileName)
        {
            int pos1 = fileName.LastIndexOf('\\');
            int pos2 = fileName.LastIndexOf('.');

            return fileName.Substring(pos1 + 1, pos2 - pos1 - 1);
        }

        static bool CheckRoom(int _room)
        {
            bool exists = true;

            DbCommand command = dbConnection.CreateCommand();
            command.CommandText = "SELECT * FROM rooms WHERE id = " + _room + ";";
            DbDataReader reader = command.ExecuteReader();
            exists = reader.Read();
            reader.Close();

            return exists;           
        }

        static void SetRoom(int _room)
        {
            string nameCharacter;

            // Get room data

            DbCommand command = dbConnection.CreateCommand();
            command.CommandText = "SELECT * FROM rooms WHERE id = " + _room + ";";
            DbDataReader reader = command.ExecuteReader();
            reader.Read();

            roomName = reader.GetString(1);
            roomDescription = reader.GetString(2);
            roomBackground = reader.GetString(3);

            hasRoomN = !reader.IsDBNull(4);
            if(hasRoomN) { roomN = reader.GetInt32(4); }

            hasRoomS = !reader.IsDBNull(5);
            if(hasRoomS) { roomS = reader.GetInt32(5); }

            hasRoomE = !reader.IsDBNull(6);
            if(hasRoomE) { roomE = reader.GetInt32(6); }

            hasRoomW = !reader.IsDBNull(7);
            if(hasRoomW) { roomW = reader.GetInt32(7); }

            reader.Close();

            roomNName = "";
            roomSName = "";
            roomEName = "";
            roomWName = "";

            if(hasRoomN)
            {
                command.CommandText = "SELECT name FROM rooms WHERE id = " + roomN + ";";
                reader = command.ExecuteReader();
                reader.Read();
                roomNName = reader.GetString(0);
                reader.Close();
            }

            if(hasRoomS)
            {
                command.CommandText = "SELECT name FROM rooms WHERE id = " + roomS + ";";
                reader = command.ExecuteReader();
                reader.Read();
                roomSName = reader.GetString(0);
                reader.Close();
            }
            
            if(hasRoomE)
            {
                command.CommandText = "SELECT name FROM rooms WHERE id = " + roomE + ";";
                reader = command.ExecuteReader();
                reader.Read();
                roomEName = reader.GetString(0);
                reader.Close();
            }

            if(hasRoomW)
            {
                command.CommandText = "SELECT name FROM rooms WHERE id = " + roomW + ";";
                reader = command.ExecuteReader();
                reader.Read();
                roomWName = reader.GetString(0);
                reader.Close();
            }

            Console.WriteLine("Room: " + _room);
            Console.WriteLine("  Title: " + roomName);
            Console.WriteLine("  Description: " + roomDescription);
            Console.WriteLine("  Background: " + roomBackground);
            Console.WriteLine("  NeighbourN: " + (hasRoomN ? roomNName : "none"));
            Console.WriteLine("  NeighbourS: " + (hasRoomS ? roomSName : "none"));
            Console.WriteLine("  NeighbourE: " + (hasRoomE ? roomEName : "none"));
            Console.WriteLine("  NeighbourW: " + (hasRoomW ? roomWName : "none"));

            // Apply data to UI

            exploreRoomTitleText.DisplayedString = roomName;

            string[] descriptionLines = roomDescription.Split("|");
            exploreRoomDescriptionText1.DisplayedString = descriptionLines[0];
            exploreRoomDescriptionText2.DisplayedString = descriptionLines.Length > 1 ? descriptionLines[1] : "";
            exploreRoomDescriptionText3.DisplayedString = descriptionLines.Length > 2 ? descriptionLines[2] : "";

            exploreRoomTitleNText.DisplayedString = roomNName;
            exploreRoomTitleSText.DisplayedString = roomSName;
            exploreRoomTitleEText.DisplayedString = roomEName;
            exploreRoomTitleOText.DisplayedString = roomWName;

            backgroundSprite.Texture = backgroundTextures[roomBackground];

            // Vaciar la lista de personajes antes de cargar los nuevos
            characters.Clear();

            // Obtener los personajes de la habitación actual
            command = dbConnection.CreateCommand();
            command.CommandText = "SELECT name, background FROM characters WHERE room = " + _room + ";";
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                CharacterProperties character;
                character.name = reader.GetString(0); // Nombre del personaje
                character.image = reader.GetString(1); // Nombre del archivo de imagen del personaje

                characters.Add(character);
            }

            reader.Close();


        }


        static void OnMousePressed(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)sender;

            if (e.Button == Mouse.Button.Left)
            {
                if (exploreBaseSprite.GetGlobalBounds().Contains(e.X, e.Y))
                {
                    if(Mouse.GetPosition(window).Y < 602)
                    {
                        goNPressed = true;
                    }
                    else if(Mouse.GetPosition(window).Y > 655)
                    {
                        goSPressed = true;
                    }
                    else if(Mouse.GetPosition(window).X < 386)
                    {
                        goOPressed = true;
                    }
                    else
                    {
                        goEPressed = true;
                    }
                }
            }
        }

        static void OnKeyPressed(object sender, KeyEventArgs e)
        {
            var window = (Window)sender;
            if (e.Code == Keyboard.Key.Escape)
            {
                window.Close();
            }
            else if (e.Code == Keyboard.Key.N)
            {
                goNPressed = true;
            }
            else if (e.Code == Keyboard.Key.S)
            {
                goSPressed = true;
            }
            else if (e.Code == Keyboard.Key.E)
            {
                goEPressed= true;
            }
            else if (e.Code == Keyboard.Key.O)
            {
                goOPressed = true;
            }
            else if (e.Code == Keyboard.Key.T)
            {
                test = true;
            }
        }

    }


}
