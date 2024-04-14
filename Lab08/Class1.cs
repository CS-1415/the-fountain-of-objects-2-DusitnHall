namespace Lab08;

public class DungeonDriver
{
    Dungeon dungeon;

    bool Continue { get => dungeon.FountainActive && dungeon.Rooms[X, Y].IsEnterance; }

    int X { get => dungeon.player.Position.X; }
    int Y { get => dungeon.player.Position.Y; }

    public DungeonDriver() {
        Console.Clear();
        Console.WriteLine("Do you want easy, medium, or difficult?");
        string? response = Console.ReadLine();

        switch (response) {
            case "easy":
                dungeon = new Dungeon(4, 4);
                break;
            case "medium":
                dungeon = new Dungeon(6, 6);
                break;
            case "difficult":
                dungeon = new Dungeon(8, 8);
                break;
            default:
                dungeon = new Dungeon();
                break;
        }   

        dungeon.Display();
        Console.ReadLine();
    }

    public void Start() {
        while (!Continue) {
            Console.Clear();
            Console.WriteLine($"You are in the room at Column: {X} Row: {Y}");
            if (dungeon.Sense(X, Y)) {

            Console.WriteLine("------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("What do you want to do?");

            Action(Console.ReadLine());

            } else {
                return;
            }
        }

        Console.WriteLine("You escaped successfuly congragulations!");
    }

    void Action(string? response) {
        if (response == null)
            return;
        switch (response) {
            case "move north":
                dungeon.Move(X, Y - 1);
                break;
            case "move south":
                dungeon.Move(X, Y + 1);
                break;
            case "move east":
                dungeon.Move(X + 1, Y);
                break;
            case "move west":
                dungeon.Move(X - 1, Y);
                break;
            case "enable fountain":
                if (dungeon.Rooms[X, Y].ContainsFountain) {
                    dungeon.FountainActive = true;
                    Console.WriteLine("YOu hear waters rush from the Fountain of Objects. You have enabled the fountain");
                }
                break;
            default:
                
                break;
        }
    }
}
public class Dungeon 
{
    public Room[,] Rooms;

    public Player player;

    public (int X, int Y) Enterance = (0, 0);

    int Width;
    int Height;

    public bool FountainActive = false;

    public Dungeon(int width = 4, int height = 4) {
        Random random = new Random();

        Width = width;
        Height = height;

        Rooms = new Room[width, height];
        for (int x = 0; x < width; x++) {
            int randomColumn = random.Next(width);

            for (int y = 0; y < height; y++) {
                if (y == height - 1 && x == randomColumn) 
                {
                    Rooms[x, y] = new Room(true);
                } else {
                    Rooms[x, y] = new Room();
                }
            }
        }

        Enterance.X = random.Next(width);
        Rooms[Enterance.X, 0].IsEnterance = true;

        player = new Player(Enterance.X, 0);
    }

    public bool Sense(int x, int y) {
        Console.ForegroundColor = ConsoleColor.Yellow;

        List<string> information = new List<string>();

        (int, int)[] changeBy = new (int, int)[4] {(0, -1), (0, 1), (-1, 0), (1, 0)};

        foreach ((int x, int y) item in changeBy) {
            if (ValidRoom(x + item.x, y + item.y)) {
                Room CheckingRoom = Rooms[x + item.x, y + item.y];

                if (CheckingRoom.Monster == null) {
                    //nothing
                } else if (CheckingRoom.Monster.MonsterType == "Amarok") {
                    if (!information.Contains("You can smell the rotten stench of an Amarok in a nearby room.")) {
                        information.Add("You can smell the rotten stench of an Amarok in a nearby room.");
                    }
                } else if (CheckingRoom.Monster.MonsterType == "Maelstrom") {
                    if (!information.Contains("You hear the growling and groaning of a Maelstrom from a neighboring room")) {
                        information.Add("You hear the growling and groaning of a Maelstrom from a neighboring room");
                    }
                } else {
                    //nothing
                }
            }
        }

        Room CurrentRoom = Rooms[x, y];

        if (CurrentRoom.ContainsFountain) {
            information.Add("You hear a dripping sound coming from inside the room.");
        } else if (CurrentRoom.IsEnterance) {
            information.Add("You see light coming in through the cavern enterance.");
        } else if (CurrentRoom.Monster != null) {
            FightSequence fightSequence = new FightSequence(CurrentRoom, player);

            if (!fightSequence.Start()) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You lose");

                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        if (Rooms[x, y].IsEnterance && FountainActive) {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("You Win!");
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        } else {
            foreach (string item in information)
                Console.WriteLine(item);
            return true;
        }
    }
    public void Move(int x, int y) {
        if (ValidRoom(x, y)) {
            player.Position = (x, y);
        } else {
            Console.WriteLine("You can't move in that direction");
            Console.ReadLine();
        }
    }
    public bool ValidRoom(int x, int y) {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public void Display() {
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                switch (Rooms[x, y].Monster) {
                    case null:
                        Console.Write("[ ]");
                        break;
                    case Maelstrom:
                        Console.Write("[@]");
                        break;
                    case Amarok:
                        Console.Write("[5]");
                        break;
                }
                Console.Write(" ");
            }
            Console.WriteLine("");
        }
    }
}
public class Room
{
    public Monster? Monster = null;

    public bool ContainsFountain;
    public bool IsEnterance = false;

    public Room(bool FountainRoom = false) {
        ContainsFountain = FountainRoom;

        Random random = new Random();

        if (random.Next(10) < 2.5 && !ContainsFountain) {
            Monster = random.Next(3) switch {
                0 => new Amarok(),
                1 => new Maelstrom(),
                2 => new Amarok(),
                _ => null
            };
        }
    }
}

public class Player 
{
    public Weapon PlayerWeapon = new Weapon("Sword");

    public List<Item> Inventory = new List<Item>();

    public (int X, int Y) Position;

    public bool Alive { get => Health > 0;}
    public int MaxHealth = 30;
    public int Health;

    public int Armor = 6;

    public Player(int x = 0, int y = 0) {
        Position.X = x;
        Position.Y = y;

        Health = MaxHealth;

        Inventory.Add(new Potion(10));
    }

    public bool TakeDamage(int damage) {
        Health -= damage;

        return Alive;
    }
}

public abstract class Monster
{
    public int Health;
    public List<Item> Inventory = new List<Item>();

    public int Armor { get; protected set; } = 5;

    public string MonsterType = "Monster";
    public string AbilityName = "None";

    public Action<Player>? ExtraAction = null;

    public Weapon MonsterWeapon;

    public Monster() {
        MonsterWeapon = new Weapon();
        Random random = new Random();

        if(random.Next(10) < 6) {
            Inventory.Add(new Potion(random.Next(5, 15)));
        }
    }

    public bool DealDamage(int damage) {
        Health -= damage;

        return Health <= 0;
    }
}
public class Amarok : Monster {
    public Amarok() {
        MonsterWeapon = new Weapon("Axe", 6);

        MonsterType = "Amarok";
        Armor = 4;
        Health = 14;
    }
}
public class Maelstrom : Monster {
    public Maelstrom() {
        MonsterWeapon = new Weapon("Rock", 5);

        ExtraAction = MovePlayer;
        MonsterType = "Maelstrom";
        AbilityName = "Wind Attack";
        Armor = 6;
        Health = 8;
    }

    void MovePlayer(Player player) {
        Console.WriteLine("You got moved!");
    }
}

public class FightSequence
{
    Monster _attackingMonster;
    Player _player;

    Room Room;

    Weapon PlayerWeapon { get => _player.PlayerWeapon; }
    Weapon MonsterWeapon { get => _attackingMonster.MonsterWeapon;}

    public FightSequence(Room room, Player player) {
        Room = room;
        _attackingMonster = Room.Monster!;
        _player = player;
    }

    public bool Start() {
        Random random = new Random();
        Console.WriteLine($"You stumble into a {_attackingMonster.MonsterType} and it engages you in battle");

        while (0 == 0) {
            Console.WriteLine($"Your health is at {_player.Health}");
            Console.ReadLine();

            if (!PlayerOptions())
                return true;

            //Monster turn
            bool UseAbility = random.Next(3) == 2 && _attackingMonster.MonsterType == "Maelstrom";

            if (UseAbility) {
                Console.WriteLine($"The {_attackingMonster.MonsterType} attacks with its {_attackingMonster.AbilityName}");
            } else {
                Console.WriteLine($"The {_attackingMonster.MonsterType} attacks with its {MonsterWeapon.WeaponName}");
            }
            Console.ReadLine();

            if (random.Next(20) > _player.Armor)
            {
                int damage = random.Next(MonsterWeapon.WeaponMin, MonsterWeapon.WeaponMax);
                        
                if (!UseAbility) {  
                    Console.WriteLine("The attack lands");           
                    Console.WriteLine($"The {_attackingMonster.MonsterType} deals {damage} damage");

                    if (_player.TakeDamage(damage)) {
                        Console.WriteLine();
                    } 
                    else {
                        Console.WriteLine("You fall to the ground defeated");
                        return false;
                    }
                } else {
                    _attackingMonster.ExtraAction!(_player);
                    break;
                }
            }
            else {
                Console.WriteLine("The attack misses...");
            }
        }
        Console.Clear();

        return true;
        
    }

    bool PlayerOptions() {
        bool repeat = true;
        while(repeat) {
            Console.WriteLine("What action do you want to make");

            Console.WriteLine("1. Use Weapon");
            Console.WriteLine("2. View Inventory");
            
            string response = Console.ReadLine()!;

            switch (response) {
                case "1":
                    if (PlayerAttack())
                        return false;
                    repeat = false;
                    break;
                case "2":
                    repeat = InventoryDisplay();
                    break;
                default:
                    Console.WriteLine("Invalid Response");
                    break;
            }
        }
        return true;
    }

    bool PlayerAttack() {
        Random random = new Random();

        //Player turn
        Console.WriteLine($"You attack the {_attackingMonster.MonsterType} with your {PlayerWeapon.WeaponName}");
        Console.ReadLine();

        if (random.Next(20) > _attackingMonster.Armor) 
        {
            Console.WriteLine("The attack lands");

            int damage = random.Next(PlayerWeapon.WeaponMin, PlayerWeapon.WeaponMax);
            Console.WriteLine($"You deal {damage} damage");
            Console.ReadLine();

            if (_attackingMonster.DealDamage(damage)) {
                Console.WriteLine($"The {_attackingMonster.MonsterType} falls to the ground defeated");

                Console.ReadLine();

                if (_attackingMonster.Inventory.Count > 0) {

                    Console.Write("The monster drops");
                    foreach (Item thing in _attackingMonster.Inventory) {
                        Console.Write($"{thing.Name}, ");
                    }
                    Console.WriteLine("");
                    Console.WriteLine("You pick up the monsters items");

                    _player.Inventory.AddRange(_attackingMonster.Inventory);
                }

                if (MonsterWeapon.WeaponMax > PlayerWeapon.WeaponMax) {
                    Console.WriteLine($"You pick up the {_attackingMonster.MonsterType}'s {MonsterWeapon.WeaponName}");

                    _player.PlayerWeapon = MonsterWeapon;
                }           

                Console.ReadLine();
                Console.Clear();

                Room.Monster = null;

                return true;
            }

        } else {
            Console.WriteLine("The attack misses...");
            return false;
        }
        return false;
    }

    bool InventoryDisplay() {
        if (_player.Inventory.Count <= 0) {
            Console.WriteLine("\nYour Inventory is empty.\n");
            return true;
        }
        Console.WriteLine("Which item do you want to use?");

        for (int i = 0; i < _player.Inventory.Count; i ++) {
            Console.WriteLine($"{i + 1}. {_player.Inventory[i].Name}");
        }

        Console.WriteLine($"{_player.Inventory.Count + 1}. None");

        bool validResponse = int.TryParse(Console.ReadLine(), out int result);

        if (result > _player.Inventory.Count) {
            return true;
        }

        if (validResponse) {
            Console.WriteLine($"You are healed {_player.Inventory[result - 1].Power} hp.");
            _player.Health += _player.Inventory[result - 1].Power;

            _player.Inventory.RemoveAt(result - 1);
        }
        
        return !validResponse;
    }
}

public class Weapon {
    public string WeaponName;
    public int WeaponMax;
    public int WeaponMin { get => WeaponMax - (WeaponMax / 2); }

    public Weapon(string weaponName = "GenericWeapon", int weaponPower = 4) {
        WeaponName = weaponName;
        WeaponMax = weaponPower;
    }
}

public abstract class Item {
    public string Name = "DefaultItem";
    public int Power = 0;

    public Item(int power = 5) {
        Power = power;
    }
}

public class Potion : Item
{
    public Potion(int power = 10) {
        Name = "Potion";
        Power = power;
    }
}