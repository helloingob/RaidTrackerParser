-- Player definition

CREATE TABLE Player (
	Player_ID INTEGER NOT NULL,
	Name TEXT NOT NULL,
	Guild TEXT,
	Player_Class TEXT,
	Race TEXT,
	Lvl INTEGER,
	Left_Guild INTEGER,
	Twink INTEGER,
	PRIMARY KEY(Player_ID)
);

-- Raid definition

CREATE TABLE Raid (
	Raid_ID INTEGER NOT NULL,
	Name TEXT,
	Start_Date INTEGER,
	End_Date INTEGER,
	PRIMARY KEY(Raid_ID)
);

-- Item definition

CREATE TABLE Item (
	Item_ID INTEGER NOT NULL,
	WOW_ID INTEGER UNIQUE,
	Name TEXT,
	PRIMARY KEY(Item_ID)
);

CREATE INDEX Item_WOW_ID ON Item (WOW_ID);


-- Mob definition

CREATE TABLE Mob (
	Mob_ID INTEGER NOT NULL,
	Name TEXT,
	PRIMARY KEY(Mob_ID)
);


-- Loot definition

CREATE TABLE Loot (
	Loot_ID INTEGER NOT NULL,
	Date INTEGER,
	Item_ID INTEGER,
	Mob_ID INTEGER,
	Raid_ID INTEGER,
	Player_ID INTEGER,
	PRIMARY KEY(Loot_ID),
	CONSTRAINT Loot_Item_ID_Item_Item_ID FOREIGN KEY (Item_ID) REFERENCES Item (Item_ID),
	CONSTRAINT Loot_Mob_ID_Mob_Mob_ID FOREIGN KEY (Mob_ID) REFERENCES Mob (Mob_ID),
	CONSTRAINT Loot_Raid_Raid_ID_Raid_Raid_ID FOREIGN KEY (Raid_ID) REFERENCES Raid (Raid_ID),
	CONSTRAINT Loot_Player_ID_Player_Player_ID FOREIGN KEY (Player_ID) REFERENCES Player (Player_ID)
);


-- Attendance definition

CREATE TABLE Attendance (
	Attendance_ID INTEGER NOT NULL,
	Player_ID INTEGER NOT NULL,
	Raid_ID INTEGER NOT NULL,
	Start_Date INTEGER,
	End_Date INTEGER,
	PRIMARY KEY(Attendance_ID),
	CONSTRAINT Attendance_Player_ID_Player_Player_ID FOREIGN KEY (Player_ID) REFERENCES Player (Player_ID),
	CONSTRAINT Attendance_Raid_ID_Raid_Raid_ID FOREIGN KEY (Raid_ID) REFERENCES Raid (Raid_ID)
);