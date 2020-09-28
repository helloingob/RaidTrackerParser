using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;
using RaidTrackerParser.Data;

namespace RaidTrackerParser
{
    public class SQLHandler
    {
        private SQLiteConnection connection;
        public const string SqliteConnectionString = "Data Source={0}; Version=3;";
        private const string OutputDatabaseFile = "RaidTrackerDatabase.sqlite";

        public SQLHandler()
        {
            connection = new SQLiteConnection(string.Format(SqliteConnectionString, OutputDatabaseFile));
        }

        public void GenerateDatabase()
        {
            if (File.Exists(OutputDatabaseFile))
            {
                File.Delete(OutputDatabaseFile);
                SQLiteConnection.CreateFile(OutputDatabaseFile);
            }

            using var command = new SQLiteCommand(File.ReadAllText("Schema.sql"), connection);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void Persist(RaidTO raid)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            WriteRaid(raid);
            foreach (var player in raid.Players)
            {
                WritePlayer(player);
                WriteAttendance(raid, player);
            }

            foreach (var loot in raid.Loot)
            {
                WriteMob(loot.Mob);
                WriteItem(loot.Item);
                WriteLoot(raid, loot);
            }
        }

        private void WriteAttendance(RaidTO raid, PlayerTO player)
        {
            if (GetAttendance(raid, player)) return;
            using var sqLiteCommand = new SQLiteCommand("INSERT INTO Attendance(Player_ID, Raid_ID, Start_Date, End_Date) VALUES(@PlayerID,@RaidID,@StartDate,@EndDate)", connection);

            sqLiteCommand.Parameters.AddWithValue("@PlayerID", player.ID);
            sqLiteCommand.Parameters.AddWithValue("@RaidID", raid.ID);
            sqLiteCommand.Parameters.AddWithValue("@StartDate", player.Joined.ToFileTimeUtc());
            sqLiteCommand.Parameters.AddWithValue("@EndDate", player.Left.ToFileTimeUtc());

            Console.WriteLine("Add new Attendance: " + player.Name + " in Raid " + raid.Name);

            sqLiteCommand.ExecuteNonQuery();
        }

        private bool GetAttendance(RaidTO raid, PlayerTO player)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT COUNT(*) FROM Attendance WHERE Player_ID = @PlayerID AND Raid_ID = @RaidID AND Start_Date = @StartDate AND End_Date = @EndDate", connection);
            sqLiteCommand.Parameters.AddWithValue("@PlayerID", player.ID);
            sqLiteCommand.Parameters.AddWithValue("@RaidID", raid.ID);
            sqLiteCommand.Parameters.AddWithValue("@StartDate", player.Joined.ToFileTimeUtc());
            sqLiteCommand.Parameters.AddWithValue("@EndDate", player.Left.ToFileTimeUtc());
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                return sqLiteDataReader.GetInt32(0) > 0;
            }

            return false;
        }


        private void WriteItem(ItemTO item)
        {
            var foundItem = GetItem(item.WowId);
            if (foundItem == null)
            {
                using var sqLiteCommand = new SQLiteCommand("INSERT INTO Item(WOW_ID, Name) VALUES(@WOWID,@Name)", connection);

                sqLiteCommand.Parameters.AddWithValue("@WOWID", item.WowId);
                sqLiteCommand.Parameters.AddWithValue("@Name", item.Name);

                Console.WriteLine("Add new Item: " + item.Name);

                sqLiteCommand.ExecuteNonQuery();

                sqLiteCommand.CommandText = "SELECT last_insert_rowid()";

                var lastRowId64 = (long) sqLiteCommand.ExecuteScalar();
                item.ID = (int) lastRowId64;
            }
            else
            {
                item.ID = foundItem.ID;
            }
        }

        private ItemTO GetItem(int WOWID)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT * FROM Item WHERE WOW_ID = @WOWID", connection);
            sqLiteCommand.Parameters.AddWithValue("@WOWID", WOWID);
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                var item = new ItemTO();
                item.WowId = WOWID;
                item.ID = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Item_ID"));
                item.Name = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Name"));
                return item;
            }

            return null;
        }

        private void WriteMob(MobTO mob)
        {
            var foundMob = GetMob(mob.Name);
            if (foundMob == null)
            {
                using var sqLiteCommand = new SQLiteCommand("INSERT INTO Mob(Name) VALUES(@Name)", connection);

                sqLiteCommand.Parameters.AddWithValue("@Name", mob.Name);

                Console.WriteLine("Add new Mob: " + mob.Name);

                sqLiteCommand.ExecuteNonQuery();

                sqLiteCommand.CommandText = "SELECT last_insert_rowid()";

                var lastRowId64 = (long) sqLiteCommand.ExecuteScalar();
                mob.ID = (int) lastRowId64;
            }
            else
            {
                mob.ID = foundMob.ID;
            }
        }

        private MobTO GetMob(string mobName)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT * FROM Mob WHERE Name = @Name", connection);
            sqLiteCommand.Parameters.AddWithValue("@Name", mobName);
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                var mob = new MobTO();
                mob.ID = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Mob_ID"));
                mob.Name = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Name"));
                return mob;
            }

            return null;
        }

        private void WriteRaid(RaidTO raid)
        {
            var foundRaid = GetRaid(raid);
            if (foundRaid == null)
            {
                using var sqLiteCommand = new SQLiteCommand("INSERT INTO Raid(Name, Start_Date, End_Date) VALUES(@Name,@StartDate,@EndDate)", connection);

                sqLiteCommand.Parameters.AddWithValue("@Name", raid.Name);
                sqLiteCommand.Parameters.AddWithValue("@StartDate", raid.StartDate.ToFileTimeUtc());
                sqLiteCommand.Parameters.AddWithValue("@EndDate", raid.EndDate.ToFileTimeUtc());

                Console.WriteLine("Add new Raid: " + raid.Name + " from " + raid.StartDate + " until " + raid.EndDate);

                sqLiteCommand.ExecuteNonQuery();

                sqLiteCommand.CommandText = "SELECT last_insert_rowid()";

                var lastRowId64 = (long) sqLiteCommand.ExecuteScalar();
                raid.ID = (int) lastRowId64;
            }
            else
            {
                raid.ID = foundRaid.ID;
            }
        }

        private RaidTO GetRaid(RaidTO raid)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT * FROM Raid WHERE Name = @Name AND Start_Date = @StartDate AND End_Date = @EndDate", connection);
            sqLiteCommand.Parameters.AddWithValue("@Name", raid.Name);
            sqLiteCommand.Parameters.AddWithValue("@StartDate", raid.StartDate.ToFileTimeUtc());
            sqLiteCommand.Parameters.AddWithValue("@EndDate", raid.EndDate.ToFileTimeUtc());
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                raid.ID = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Raid_ID"));
                return raid;
            }

            return null;
        }

        private void WriteLoot(RaidTO raid, LootTO loot)
        {
            if (GetLoot(raid, loot)) return;
            using var sqLiteCommand = new SQLiteCommand("INSERT INTO Loot(Date, Item_ID, Mob_ID, Raid_ID, Player_ID) VALUES(@Date,@ItemID,@MobID,@RaidID,@PlayerID)", connection);

            sqLiteCommand.Parameters.AddWithValue("@Date", loot.Date.ToFileTimeUtc());
            sqLiteCommand.Parameters.AddWithValue("@ItemID", loot.Item.ID);
            sqLiteCommand.Parameters.AddWithValue("@MobID", loot.Mob.ID);
            sqLiteCommand.Parameters.AddWithValue("@RaidID", raid.ID);
            sqLiteCommand.Parameters.AddWithValue("@PlayerID", loot.Player.ID);

            Console.WriteLine("Add new Loot: " + loot.Item.Name + " given to " + loot.Player.Name + " (" + loot.Player.Guild + ")");

            sqLiteCommand.ExecuteNonQuery();

            sqLiteCommand.CommandText = "SELECT last_insert_rowid()";

            var lastRowId64 = (long) sqLiteCommand.ExecuteScalar();
            loot.ID = (int) lastRowId64;
        }

        private bool GetLoot(RaidTO raid, LootTO loot)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT COUNT(*) FROM Loot WHERE Date = @Date AND Item_ID = @ItemID AND Player_ID = @Player_ID", connection);
            sqLiteCommand.Parameters.AddWithValue("@Date", loot.Date.ToFileTimeUtc());
            sqLiteCommand.Parameters.AddWithValue("@ItemID", loot.Item.ID);
            sqLiteCommand.Parameters.AddWithValue("@Player_ID", loot.Player.ID);
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                return sqLiteDataReader.GetInt32(0) > 0;
            }

            return false;
        }

        private void WritePlayer(PlayerTO player)
        {
            var foundPlayer = GetPlayer(player.Name);
            if (foundPlayer == null)
            {
                using var sqLiteCommand = new SQLiteCommand("INSERT INTO Player(Name, Guild, Player_Class, Race, Lvl) VALUES(@Name,@Guild,@PlayerClass,@Race,@Lvl)", connection);

                sqLiteCommand.Parameters.AddWithValue("@Name", player.Name);
                sqLiteCommand.Parameters.AddWithValue("@Guild", player.Guild);
                sqLiteCommand.Parameters.AddWithValue("@PlayerClass", player.PlayerClass);
                sqLiteCommand.Parameters.AddWithValue("@Race", player.Race);
                sqLiteCommand.Parameters.AddWithValue("@Lvl", player.Level);

                Console.WriteLine("Add new Player: " + player.Name + " from " + player.Guild);

                sqLiteCommand.ExecuteNonQuery();

                sqLiteCommand.CommandText = "SELECT last_insert_rowid()";

                var lastRowId64 = (long) sqLiteCommand.ExecuteScalar();
                player.ID = (int) lastRowId64;
            }
            else
            {
                player.ID = foundPlayer.ID;
                if (!string.Equals(player.Guild, foundPlayer.Guild))
                {
                    Console.WriteLine("Update player "+ player.Name +"'s guild: '" + foundPlayer.Guild + "' -> '" + player.Guild +"'");
                    UpdatePlayer(player);
                }
                if (!Equals(player.Level, foundPlayer.Level))
                {
                    Console.WriteLine("Update player "+ player.Name +"'s level: '" + foundPlayer.Level + "' -> '" + player.Level +"'");
                    UpdatePlayer(player);
                }
            }
        }

        private void UpdatePlayer(PlayerTO player)
        {
            using var sqLiteCommand = new SQLiteCommand("UPDATE Player SET Guild = @Guild, Lvl = @Level WHERE Name = @Name", connection);
            sqLiteCommand.Parameters.AddWithValue("@Guild", player.Guild);
            sqLiteCommand.Parameters.AddWithValue("@Level", player.Level);
            sqLiteCommand.Parameters.AddWithValue("@Name", player.Name);
            
            sqLiteCommand.ExecuteNonQuery();
        }

        private PlayerTO GetPlayer(string playerName)
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT * FROM Player WHERE Name = @Name", connection);
            sqLiteCommand.Parameters.AddWithValue("@Name", playerName);
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            while (sqLiteDataReader.Read())
            {
                var player = new PlayerTO();
                player.ID = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Player_ID"));
                player.Name = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Name"));
                if (!sqLiteDataReader.IsDBNull("Guild"))
                {
                    player.Guild = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Guild"));
                }

                player.PlayerClass = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Player_Class"));
                player.Race = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Race"));
                player.Level = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Lvl"));
                return player;
            }

            return null;
        }
        
        public void UpdateItemQuality()
        {
            foreach (var item in GetItemsWithoutQuality())
            {
                var quality = ItemQualityHandler.GetQuality(item);
                item.Quality = quality;
                UpdateItem(item);
                //DDOS prevention (:
                Thread.Sleep(1000);
            }
        }

        private IEnumerable<ItemTO> GetItemsWithoutQuality()
        {
            using var sqLiteCommand = new SQLiteCommand("SELECT * FROM Item WHERE Quality IS null", connection);
            using var sqLiteDataReader = sqLiteCommand.ExecuteReader();

            var items = new List<ItemTO>();

            while (sqLiteDataReader.Read())
            {
                var item = new ItemTO();
                item.WowId = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("WOW_ID"));
                item.ID = sqLiteDataReader.GetInt32(sqLiteDataReader.GetOrdinal("Item_ID"));
                item.Name = sqLiteDataReader.GetString(sqLiteDataReader.GetOrdinal("Name"));
                items.Add(item);
            }

            return items;
        }

        private void UpdateItem(ItemTO item)
        {
            using var sqLiteCommand = new SQLiteCommand("UPDATE Item SET Quality = @Quality WHERE WOW_ID = @WOWID", connection);
            sqLiteCommand.Parameters.AddWithValue("@Quality", item.Quality);
            sqLiteCommand.Parameters.AddWithValue("@WOWID", item.WowId);
            Console.WriteLine("Quality Updated: " + item.Name + " has quality " + item.Quality);
            sqLiteCommand.ExecuteNonQuery();
        }
    }
}