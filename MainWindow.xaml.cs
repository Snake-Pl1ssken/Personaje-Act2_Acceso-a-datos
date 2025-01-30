using System.Data.Common;
using System.Windows;
using MySql.Data.MySqlClient;

namespace MansionMapEditor
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection connection;

        bool initializingComponent;

        public MainWindow()
        {
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

            try
            {
                DbCommand command = connection.CreateCommand();

                command.CommandText = "DROP TABLE IF EXISTS characters;";

                command.ExecuteNonQuery();

                command.CommandText = "DROP TABLE IF EXISTS rooms;";
            
                command.ExecuteNonQuery();

                





                command.CommandText = "CREATE TABLE rooms\n" +
                                      "(\n" +
                                      "id INT PRIMARY KEY,\n" +
                                      "name VARCHAR(80),\n" +
                                      "description VARCHAR(200),\n" +
                                      "background VARCHAR(80),\n" +
                                      "neighbourN INT,\n" +
                                      "neighbourS INT,\n" +
                                      "neighbourE INT,\n" +
                                      "neighbourW INT,\n" +
                                      "FOREIGN KEY(neighbourN) REFERENCES rooms(id),\n" +
                                      "FOREIGN KEY(neighbourS) REFERENCES rooms(id),\n" +
                                      "FOREIGN KEY(neighbourE) REFERENCES rooms(id),\n" +
                                      "FOREIGN KEY(neighbourW) REFERENCES rooms(id)\n" +
                                      ");";
                command.ExecuteNonQuery();


                command.CommandText = "CREATE TABLE characters\n" +
                                      "(\n" +
                                      "id INT PRIMARY KEY,\n" +
                                      "name VARCHAR(80),\n" +
                                      "background VARCHAR(80),\n" +
                                      "room INT,\n" +
                                      "FOREIGN KEY(room) REFERENCES rooms(id)\n" +
                                      ");";

                command.ExecuteNonQuery();
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

            try
            {
                DbCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM rooms WHERE id = " + RoomIdText.Text + ";";

               //ShowMessage(command.CommandText);

                DbDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    RoomNameText.Text = reader.GetString(1);
                    RoomDescriptionText.Text = reader.GetString(2);
                    RoomBackgroundText.Text = reader.GetString(3);
                    RoomNeighbourNText.Text = reader.IsDBNull(4) ? "" : "" + reader.GetInt32(4);
                    RoomNeighbourSText.Text = reader.IsDBNull(5) ? "" : "" + reader.GetInt32(5);
                    RoomNeighbourEText.Text = reader.IsDBNull(6) ? "" : "" + reader.GetInt32(6);
                    RoomNeighbourWText.Text = reader.IsDBNull(7) ? "" : "" + reader.GetInt32(7);
                }

                reader.Close();
            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RoomAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Añadir una nueva habitación con los valores que el diseñador tenga
            // puestos en los campos

            try
            {
                DbCommand command = connection.CreateCommand();

                command.CommandText = "INSERT INTO rooms VALUES (" + RoomIdText.Text + "," +
                                                                     Quote(RoomNameText.Text) + "," +
                                                                     Quote(RoomDescriptionText.Text) + "," +
                                                                     Quote(RoomBackgroundText.Text) + "," +
                                                                     (RoomNeighbourNText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourNText.Text) + "," +
                                                                     (RoomNeighbourSText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourSText.Text) + "," +
                                                                     (RoomNeighbourEText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourEText.Text) + "," +
                                                                     (RoomNeighbourWText.Text.Trim().Length == 0 ? "NULL" : RoomNeighbourWText.Text) + ");";

               //ShowMessage(command.CommandText);

               command.ExecuteNonQuery();

               UpdateRoomList();

            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
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
            try
            {
                MySqlCommand command = connection.CreateCommand();

                int id = Int32.Parse(CharacterIdText.Text);
                int characterRoomDestiny = Int32.Parse(CharacterRoomText.Text);

                command.CommandText = "INSERT INTO characters (" +
                                        "ID," +
                                        "Name," +
                                        "Background," +
                                        "room)";

                command.CommandText += " VALUES (" +
                         id + "," +
                        "'" + CharacterName.Text + "'" + "," +
                        "'" + CharacterImageText.Text + "'" + "," +
                         + characterRoomDestiny + ");";

                MessageBox.Show(command.CommandText);
                command.ExecuteNonQuery();

            }
            catch(Exception ex)
            {
                MessageBox.Show("No line added" + ex.Message);
            }
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
                command.CommandText += "SELECT * FROM habitaciones;";

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
                    roomdestinyID = Convert.ToString(roomID);

                    RoomListText.Text += roomID + " " + roomName + " " + roomBG + " " + roomdestinyID + " \n";
                    RoomListText.Text += " \n";
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