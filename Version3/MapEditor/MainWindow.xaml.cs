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

            //ServerText.Text = "localhost";
            //PortText.Text = "3307";
            //DBText.Text = "mansion";
            //UserText.Text = "root";
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
            MansionContext mansion = new MansionContext();

            int id = Int32.Parse(RoomIdText.Text);

            Room? r = mansion.rooms.Find(id);

            if (r != null)
            {
                RoomNameText.Text = r.name;
                RoomDescriptionText.Text = r.description;
                RoomBackgroundText.Text = r.background;
                RoomNeighbourEText.Text = Convert.ToString(r.neighbourE);
                RoomNeighbourNText.Text = Convert.ToString(r.neighbourN);
                RoomNeighbourSText.Text = Convert.ToString(r.neighbourS);
                RoomNeighbourWText.Text = Convert.ToString(r.neighbourW);
                Console.WriteLine("Habitacion " + r.id + ": " + r.name);
            }
            else
            {
                Console.WriteLine("Error 303: Not Found");
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
                string test = RoomNeighbourSText.Text;

                if (test == "")
                {
                    room.neighbourS = null;
                }
                else
                { 
                    r = mansion.rooms.Find(Int32.Parse(test));
                    room.neighbourS = r;                
                }
                
                string test2 = RoomNeighbourWText.Text;
                if (test == "")
                {
                    room.neighbourW = null;
                }
                else
                {
                    r = mansion.rooms.Find(Int32.Parse(test2));
                    room.neighbourW = r;
                }

                string test3 = RoomNeighbourEText.Text;
                if (test == "")
                {
                    room.neighbourE = null;
                }
                else
                {
                    r = mansion.rooms.Find(Int32.Parse(test3));
                    room.neighbourE = r;
                }


                string test4 = RoomNeighbourNText.Text;
                if (test == "")
                {
                    room.neighbourN = null;
                }
                else
                {
                    r = mansion.rooms.Find(Int32.Parse(test4));
                    room.neighbourN = r;
                }

                mansion.rooms.Add(room);

                mansion.SaveChanges();
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
            //find, cambiar lo que quieres y guardar
        }

        private void RoomDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MansionContext mansion = new MansionContext();
            int id = Int32.Parse(RoomIdText.Text);
            Room? r = mansion.rooms.Find(id);
            if (r != null)
            {
                mansion.rooms.Remove(r);
                mansion.SaveChanges();
            }
            else
            {
                Console.WriteLine("Error403:RoomNotFound");
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
            MansionContext mansion = new MansionContext();
            foreach (Room c in mansion.rooms)
            {
                Console.WriteLine("Personaje " + c.id + ": " + c.name);

                RoomListText.Text += c.id + " " + c.name + " " + c.description + " " + c.background + " " +
                                     c.neighbourN + " " + c.neighbourE + " " + c.neighbourW + " " + c.neighbourS + "\n";
            }
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
            MansionContext mansion = new MansionContext();

            int id = Int32.Parse(CharacterIdText.Text);

            Character? r = mansion.characters.Find(id);

            if (r != null)
            {
                CharacterIdText.Text = Convert.ToString(r.id);
                CharacterName.Text = r.name;
                CharacterImageText.Text = r.image;
                CharacterRoomText.Text = Convert.ToString(r.room);

                Console.WriteLine("Habitacion " + r.id + ": " + r.name);
            }
            else
            {
                Console.WriteLine("Error 303: Not Found");
            }
        }

        private void CharacterAddButton_Click(object sender, RoutedEventArgs e)
        {
            MansionContext mansion = new MansionContext();
            var character = new Character();
            character.id = Int32.Parse(CharacterIdText.Text);
            character.name = CharacterName.Text;
            character.image = CharacterImageText.Text;

            Room? r;
            string test = CharacterRoomText.Text;

            if (test == "")
            {
                character.room = null;
            }
            else
            {
                r = mansion.rooms.Find(Int32.Parse(test));
                character.room = r;
            }

            mansion.characters.Add(character);

            mansion.SaveChanges();
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
            MansionContext mansion = new MansionContext();
            int id = Int32.Parse(CharacterIdText.Text);
            Character? r = mansion.characters.Find(id);
            if (r != null)
            {
                mansion.characters.Remove(r);
                mansion.SaveChanges();
            }
            else
            {
                Console.WriteLine("Error403:RoomNotFound");
            }
        }

        private void CharacterListUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MansionContext mansion = new MansionContext();
            foreach (Character c in mansion.characters)
            {
                Console.WriteLine("Personaje " + c.id + ": " + c.name);

                RoomListText.Text += c.id + " " + c.name + " " + c.image + " " + c.room + "\n";
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