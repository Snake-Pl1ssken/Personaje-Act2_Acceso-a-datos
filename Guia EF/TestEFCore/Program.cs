using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Principal;

namespace TestEFCore
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
        public string name{ get; set; }
        public string image { get; set; }

        public Room room { get; set; }
    }

    internal class Program
    {


        static void Main(string[] args)
        {
            MansionContext mansion = new MansionContext();

            mansion.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

            int opcion = -1;

            while(opcion != 0)
            {

                Console.WriteLine("1.- Iniciar la base de datos");
                Console.WriteLine("2.- Listar personajes");
                Console.WriteLine("3.- Listar habitaciones");
                Console.WriteLine("4.- Buscar un personaje");
                Console.WriteLine("5.- Buscar una habitación");
                Console.WriteLine("6.- Buscar personajes en una habitacion");
                Console.WriteLine("7.- Mostrar vecinas de una habitación");
                Console.WriteLine("0.- Salir");

                opcion = Int32.Parse(Console.ReadLine());

                if(opcion == 1)
                {
                    mansion.Database.EnsureDeleted();
                    mansion.Database.EnsureCreated();

                    foreach(Room r in mansion.rooms)
                    {
                        Console.WriteLine(r.id + ":" + r.name);
                    }

                    foreach(Character c in mansion.characters)
                    {
                        Console.WriteLine(c.id + ":" + c.name);
                        Console.WriteLine(c.room.name);
                    }

                    var room1 = new Room();
                    room1.id = 1;
                    room1.name = "Vestíbulo";
                    room1.description = "Es la entrada de la mansión";
                    room1.background = "vestibulo";

                    mansion.rooms.Add(room1);

                    var room2 = new Room();
                    room2.id = 2;
                    room2.name = "Comedor";
                    room2.description = "Es donde se come";
                    room2.background = "comedor";

                    mansion.rooms.Add(room2);

                    var room3 = new Room();
                    room3.id = 3;
                    room3.name = "Cocina";
                    room3.description = "Es donde se prepara la comida";
                    room3.background = "cocina";

                    mansion.rooms.Add(room3);

                    mansion.SaveChanges();

                    // Comedor esta al norte del vestibulo
                    room2.neighbourS = room1;

                    // Vestibulo esta al sur del comedor
                    room1.neighbourN = room2;

                    // Comedor esta al oeste de la cocina
                    room2.neighbourE = room3;

                    // Cocina esta al este del comedor
                    room3.neighbourW = room2;


                    mansion.SaveChanges();

                    var character1 = new Character();
                    character1.id = 1;
                    character1.name = "Basilio";
                    character1.image = "";
                    character1.room = room1;

                    mansion.characters.Add(character1);

                    var character2 = new Character();
                    character2.id = 2;
                    character2.name = "Juan";
                    character2.image = "";
                    character2.room = room2;

                    mansion.characters.Add(character2);

                    mansion.SaveChanges();
                }
                else if(opcion == 2)
                {
                    foreach(Character c in mansion.characters)
                    {
                        Console.WriteLine("Personaje " + c.id + ": " + c.name);
                    }
                }
                else if(opcion == 3)
                {
                    foreach(Room r in mansion.rooms)
                    {
                        Console.WriteLine("Habitacion " + r.id + ": " + r.name);
                    }
                }
                else if(opcion == 4)
                {
                    Console.WriteLine("id? ");
                    int id = Int32.Parse(Console.ReadLine());

                    Character? c = mansion.characters.Find(id);

                    if(c != null)
                    {
                        Console.WriteLine("Personaje " + c.id + ": " + c.name);
                    }
                    else
                    {
                        Console.WriteLine("No encontrado");
                    }
                }
                else if(opcion == 5)
                {
                    Console.WriteLine("id? ");
                    int id = Int32.Parse(Console.ReadLine());

                    Room? r = mansion.rooms.Find(id);

                    if(r != null)
                    {
                        Console.WriteLine("Habitacion " + r.id + ": " + r.name);
                    }
                    else
                    {
                        Console.WriteLine("No encontrada");
                    }
                }
                else if(opcion == 6)
                {
                    Console.WriteLine("id de la habitación? ");
                    int id = Int32.Parse(Console.ReadLine());

                    Room? r = mansion.rooms.Find(id);

                    if(r != null)
                    {
                        bool hasCharacters = false;

                        foreach(Character c in r.characters)
                        {
                            Console.WriteLine("Personaje " + c.id + ": " + c.name);

                            hasCharacters = true;
                        }

                        if(!hasCharacters)
                        {
                            Console.WriteLine("La habitación no tiene personajes");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No encontrada");
                    }
                }
                else if(opcion == 7)
                {
                    // FIX: Por alguna razón, las vecinas no se cargan hasta que se han consultado por segunda vez
                    // Parece que poner más de una relación uno a uno cuando se refieren a la misma tabla confunden al ORM
                    // https://stackoverflow.com/questions/53240371/entity-framework-core-self-referencing-fk-column-value-being-replaced-with-nul

                    foreach (Room r2 in mansion.rooms)
                    {
                        Console.WriteLine("Habitacion: " + r2.name + ", N:" + r2.neighbourN?.name + ", S:" + r2.neighbourS?.name + ", E:" + r2.neighbourE?.name + ", W:" + r2.neighbourW?.name);
                    }

                    Console.WriteLine("id de la habitación? ");
                    int id = Int32.Parse(Console.ReadLine());

                    Room? r = mansion.rooms.Find(id);

                    if(r != null)
                    {
                        Console.WriteLine("Habitacion: " + r.name);

                        if(r.neighbourN != null) { Console.WriteLine("Vecina N: " + r.neighbourN.name); }
                        else { Console.WriteLine("Vecina N: No tiene"); }

                        if(r.neighbourS != null) { Console.WriteLine("Vecina S: " + r.neighbourS.name); }
                        else { Console.WriteLine("Vecina S: No tiene"); }

                        if(r.neighbourW != null) { Console.WriteLine("Vecina W: " + r.neighbourW.name); }
                        else { Console.WriteLine("Vecina W: No tiene"); }

                        if(r.neighbourE != null) { Console.WriteLine("Vecina E: " + r.neighbourE.name); }
                        else { Console.WriteLine("Vecina E: No tiene"); }
                    }
                    else
                    {
                        Console.WriteLine("No encontrada");
                    }
                }
            }
        }
    }
}