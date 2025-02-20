using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.Emit;
using System.Windows;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations.Schema;

namespace MansionMapEditor
{
    public class MansionContext : DbContext
    {
        const string connectionString = "Server=127.0.0.1; Port=3307; Database=mansionef; Uid=root; Pwd=;";

        public DbSet<Room> rooms { get; set; }
        public DbSet<Character> characters { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySQL(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>().HasOne(e => e.neighbourN).WithMany();
            modelBuilder.Entity<Room>().HasOne(e => e.neighbourE).WithMany();
        }

    }
    public class Room
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string background { get; set; }

        public ICollection<Character> characters { get; } = new List<Character>();

        [ForeignKey("neighbourNId")]
        public Room? neighbourN { get; set; }

        [ForeignKey("neighbourSId")]
        public Room? neighbourS { get; set; }

        [ForeignKey("neighbourEId")]
        public Room? neighbourE { get; set; }

        [ForeignKey("neighbourWId")]
        public Room? neighbourW { get; set; }

    }

    public class Character
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }

        public Room room { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection connection;

        bool initializingComponent;


        public MainWindow()
        {
            MansionContext mansion = new MansionContext();
        
            mansion.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

            initializingComponent = true;

            InitializeComponent();

            initializingComponent = false;

            ShowRoomsOrCharacters(true);

            ServerText.Text = "localhost";
            PortText.Text = "3307";
            DBText.Text = "mansion";
            UserText.Text = "root";
            //PassText.Text = "root";
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Crea la conexión con la base de datos utilizando los valores
            // de los campos como server o user para construir el connection string

            string connectionString = "Server=" + ServerText.Text + "; Port=" + PortText.Text + "; Database=" +
                                    DBText.Text + "; Uid=" + UserText.Text + "; Pwd=" + PassText.Text + ";";

            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                ShowMessage("Connected");

            }
            catch(Exception ex)
            {
                ShowError(ex);
            }

        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la conexión con la base de datos

            try
            {
                connection.Close();

                ShowMessage("Disconnected");
            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            // Volver a crear las tablas de la base de datos, eliminando las
            // tablas anteriores
            MansionContext mansion = new MansionContext();
            try
            {
                DbCommand command = connection.CreateCommand();

                mansion.Database.EnsureDeleted();
                mansion.Database.EnsureCreated();

                mansion.SaveChanges();
            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RoomFindButton_Click(object sender, RoutedEventArgs e)
        {
            // Actualizar los campos con los valores de la habitación que
            // corresponda al id que el diseñador tenga puesto
            MansionContext mansion = new MansionContext();

            Console.WriteLine("id? ");
            int id = Int32.Parse(Console.ReadLine());

            Room? r = mansion.rooms.Find(id);

            if (r != null)
            {
                Console.WriteLine("Habitacion " + r.id + ": " + r.name);
            }
            else
            {
                Console.WriteLine("No encontrada");
            }
        }

        private void RoomAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Añadir una nueva habitación con los valores que el diseñador tenga
            // puestos en los campos
            MansionContext mansion = new MansionContext();
            //try
            //{
                var room = new Room();
                room.id = Int32.Parse(RoomIdText.Text);
                room.name = RoomNameText.Text;
                room.description = RoomDescriptionText.Text;
                room.background = RoomBackgroundText.Text;

                Room? r;
                int test = Int32.Parse(RoomNeighbourSText.Text);
                r = mansion.rooms.Find((test != null) ? test : "");
                room.neighbourS = r;

                int test2 = Int32.Parse(RoomNeighbourWText.Text);
                r = mansion.rooms.Find((test2 != null) ? test2 : "");
                room.neighbourW = r;

                int test3 = Int32.Parse(RoomNeighbourEText.Text);
                r = mansion.rooms.Find((test3 != null) ? test : "");
                room.neighbourE = r;

                int test4 = Int32.Parse(RoomNeighbourNText.Text);
                r = mansion.rooms.Find((test4 != null) ? test : "");
                room.neighbourN = r;

                mansion.rooms.Add(room);

                mansion.SaveChanges();

            //if else en este caso

            //}
            //catch(Exception ex)
            //{
            //    ShowError(ex);
            //}
        }

        private void RoomModifyButton_Click(object sender, RoutedEventArgs e)
        {
            // Actualizar la habitación con el id que tenga puesto el diseñador
            // con que tenga puesto en el resto de campos

            try
            {
                DbCommand command = connection.CreateCommand();

                command.CommandText = "UPDATE rooms SET name = " + Quote(RoomNameText.Text) + "," +
                                                        "description = " + Quote(RoomDescriptionText.Text) + "," + 
                                                        "background = " + Quote(RoomBackgroundText.Text) + "," + 
                                                        "neighbourN = " + (RoomNeighbourNText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourNText.Text) + "," +
                                                        "neighbourS = " + (RoomNeighbourSText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourSText.Text) + "," +
                                                        "neighbourE = " + (RoomNeighbourEText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourEText.Text) + "," +
                                                        "neighbourW = " + (RoomNeighbourWText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourWText.Text) + " " +
                                                    "WHERE id = " + RoomIdText.Text + ";";

               //ShowMessage(command.CommandText);

               command.ExecuteNonQuery();

                UpdateRoomList();

            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RoomDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Eliminar la habitación con el id que tenga puesto el diseñador

            try
            {
               DbCommand command = connection.CreateCommand();

               command.CommandText = "DELETE FROM rooms WHERE id = " + RoomIdText.Text + ";";

               command.ExecuteNonQuery();

               UpdateRoomList();

            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RoomListUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Buscar todas las habitaciones y añadirlas a la lista.

            try
            {
                UpdateRoomList();
            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        void UpdateRoomList()
        {
            string text = "";

            DbCommand command = connection.CreateCommand();


            command.CommandText = "SELECT * FROM rooms;";

            DbDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                text += "" + reader.GetInt32(0);
                text += ", " + reader.GetString(1);
                text += ", " + reader.GetString(2);
                text += ", " + reader.GetString(3);
                text += ", " + (reader.IsDBNull(4) ? "" : "" + reader.GetInt32(4));
                text += ", " + (reader.IsDBNull(5) ? "" : "" + reader.GetInt32(5));
                text += ", " + (reader.IsDBNull(6) ? "" : "" + reader.GetInt32(6));
                text += ", " + (reader.IsDBNull(7) ? "" : "" + reader.GetInt32(7));
                text += "\n";
            }

            RoomListText.Text = text;

            //ShowMessage(RoomListText.Text);

            reader.Close();
        }

        private void EditRoomsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(initializingComponent) { return; }

            ShowRoomsOrCharacters(true);
        }

        private void EditCharactersRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(initializingComponent) { return; }

            ShowRoomsOrCharacters(false);
        }

        void ShowRoomsOrCharacters(bool rooms)
        {
            RoomsCanvas.Visibility = Visibility.Collapsed;
            CharactersCanvas.Visibility = Visibility.Collapsed;

            if(rooms)
            {
                RoomsCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                CharactersCanvas.Visibility = Visibility.Visible;
            }
        }

        private void CharacterFindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlCommand command = connection.CreateCommand();
                int id = Int32.Parse(CharacterIdText.Text);
                command.CommandText += "SELECT * FROM characters WHERE id =" + id + ";";

                MessageBox.Show(command.CommandText);
                MySqlDataReader reader = command.ExecuteReader();

                int RoomId, RoomDestinyID;


                if (reader.Read())
                {
                    RoomId = reader.GetInt32(0);
                    CharacterIdText.Text = Convert.ToString(RoomId);

                    CharacterName.Text = reader.GetString(1);
                    CharacterImageText.Text = reader.GetString(2);

                    RoomDestinyID = reader.GetInt32(3);
                    CharacterRoomText.Text = Convert.ToString(RoomDestinyID);

                    Console.WriteLine(RoomNameText.Text);
                }
                reader.Close();

            }
            catch
            {
                MessageBox.Show("No room found");
            }
        }

        private void CharacterAddButton_Click(object sender, RoutedEventArgs e)
        {
            //var character1 = new Character();
            //character1.id = 1;
            //character1.name = "Basilio";
            //character1.image = "";
            //character1.room = room1;

            //MansionContext mansion = new MansionContext();
            //try
            //{
            //    var character1 = new Character();
            //    character1.id = Int32.Parse(CharacterIdText.Text);
            //    character1.name = CharacterName.Text;
            //    character1.image = CharacterImageText.Text;

            //    Room? c;
            //    c = mansion.rooms.Find(Int32.Parse(CharacterRoomText.Text));
            //    character1.room = c;


            //    mansion.rooms.Add(character1);

            //}
            //catch (Exception ex)
            //{
            //    ShowError(ex);
            //}
        }

        private void CharacterModifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlCommand command = connection.CreateCommand();
                int id = Int32.Parse(CharacterIdText.Text);
                command.CommandText += "UPDATE characters SET Name='" + CharacterName.Text
                                    + "', Background='" + CharacterImageText.Text
                                    + "', room='" + CharacterRoomText.Text
                                    + "' WHERE id =" + id + ";";

                MessageBox.Show(command.CommandText);
                command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("No Modify room");
            }
        }

        private void CharacterDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlCommand command = connection.CreateCommand();
                int id = Int32.Parse(CharacterIdText.Text);
                command.CommandText += " DELETE FROM characters WHERE id =" + id + ";";
                MessageBox.Show(command.CommandText);
                command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                MessageBox.Show("No character deleted" + ex);
            }
        }

        private void CharacterListUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText += "SELECT * FROM characters;";

                MessageBox.Show(command.CommandText);
                MySqlDataReader reader = command.ExecuteReader();

                int RoomId, roomDestinyID;
                string roomID, roomName, roomBG, roomdestinyID;
                RoomListText.Text = "";
                while (reader.Read())
                {
                    RoomId = reader.GetInt32(0);
                    roomID = Convert.ToString(RoomId);

                    roomName = reader.GetString(1);
                    roomBG = reader.GetString(2);

                    roomDestinyID = reader.GetInt32(3);
                    roomdestinyID = Convert.ToString(roomDestinyID);

                    CharacterListText.Text += roomID + " " + roomName + " " + roomBG + " " + roomdestinyID + " \n";
                    CharacterListText.Text += " \n";
                }
                reader.Close();

            }
            catch
            {
                MessageBox.Show("No room found");
            }
        }

        private void ShowError(Exception e)
        {
            MessageBox.Show("Error: " + e.Message);
        }

        private void ShowMessage(string text)
        {
            MessageBox.Show(text);
        }

        private string Quote(string s)
        {
            return "'" + s + "'";
        }
    }
}