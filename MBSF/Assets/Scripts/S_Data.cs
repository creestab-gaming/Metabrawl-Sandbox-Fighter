using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class S_Data
{
    //Debug Flags
    public static bool HIT_DATA = false;

    //Input command supertypes
    public enum Action
    {
        Move,
        Light,
        Heavy,
        Special,
        Block,
        Grab,
        Jump,
        Alt,    //Action modification (clutch), "story mode" use (toggle combat inputs into interaction commands), debugging
        Pause
    };
    //Command subtypes
    public enum ActionType
    {
        jump,
        shield,
        dodge,
        airdodge,
        tech,
        techHop,
        fRoll,
        bRoll,
        getup,
        getupAtk,
        grab,
        pummel,
        fThrow,
        bThrow,
        uThrow,
        dThrow,
        jab,
        uLight,
        fLight,
        dLight,
        uStrong,
        fStrong,
        dStrong,
        nAir,
        uAir,
        fAir,
        dAir,
        bAir,
        nSpec,
        uSpec,
        fSpec,
        dSpec,
        projectile      //While not technically an action, this is required for sensor referencing
    }
    //Customizable subtypes and static modifiers
    public enum SlotType
    {
        enchantment,    //Static modifier w/ an associated element
        block,
        getupAtk,
        jab,
        uLight,
        fLight,
        dLight,
        uStrong,
        fStrong,
        dStrong,
        nAir,
        uAir,
        fAir,
        dAir,
        bAir,
        nSpec,
        uSpec,
        fSpec,
        dSpec
    }
    //
    public enum Element
    {
        None,
        Fire,
        Water,
        Earth
    }
    //
    public enum Rarity
    {
        common,
        uncommon,
        rare
    }
    //
    public enum ColliderState
    {
        none,   //Inactive hitbox
        hit,    //Active hitbox
        hurt    //Hurtbox (player model collider)
    }

    //
    public struct PlayerState
    {
        public int number;
        public GameObject model;
        public Color color;
        public int size;
        public int weight;
        public float runSpeed;
        public float gJumpSpeed;
        public float aJumpSpeed;
        public float fallSpeed;
        public int orientation; //1 for facing right, -1 for facing left.
        public int jumps;
        public int maxJumps;
        public float damage;    //Amount of damage currently sustained
        public float dps;       //Change in sustained damage per second
    }
    //Controller input mapping for action commands
    public struct ControlScheme
    {
        public int buffer;
        public string moveHorz;
        public string moveVert;
        public Action rStickUse;
        public string rHorz;
        public string rVert;
        public KeyCode left;
        public KeyCode right;
        public KeyCode up;
        public KeyCode down;
        public KeyCode light;
        public KeyCode heavy;
        public float lightToHeavy; //Threshold for light attack inputs to register as heavy. Values over 1 disable this.
        public KeyCode special;
        public KeyCode block;
        public KeyCode grab;
        public KeyCode jump;
        public KeyCode alt;
        public KeyCode pause;

        public override string ToString()
        {
            string s =
                "Input Buffer: " + buffer + '\n' +
                "Movement Axis (Horizonal): " + moveHorz + '\n' +
                "Movement Axis (Vertical): " + moveVert + '\n' +
                "Misc Axis (Horizontal): " + rHorz + '\n' +
                "Misc Axis (Vertical): " + rVert + '\n' +
                "Misc Axis Use: " + rStickUse + '\n' +
                "Binary Movement: " + '\n' + "   Left: " + left + "   Right: " + right + "   Up: " + up + "   Down: " + down + '\n' +
                "Light Attack: " + light + '\n' +
                "Heavy Attack: " + heavy + '\n' +
                "Light Input to Heavy Attack Threshold: " + lightToHeavy + '\n' +
                "Special: " + special + '\n' +
                "Block: " + block + '\n' +
                "Grab: " + grab + '\n' +
                "Jump: " + jump + '\n' +
                "Alter Input: " + alt + '\n' +
                "Pause: " + pause + '\n';

            return s;
        }
    }
    //
    public struct Move
    {
        public string name;
        public Rarity rarity;
        public SlotType slot;
        public Element element;
        public int duration;        //Number of frames if attack, startup frames if block
        public HBData[] hitbox;     //Individual hitbox data
        public int landLag;         //Stun on land from aerial actions, 0 if grounded action
        public int aStrength;       //superarmor strength. If hit with a move with more damage than armor, armor negated.
        public int[][] aFrames;     //superarmor frames, [0][0] if none
        public int[][] iFrames;     //invincibility frames, [0][0] if none
        public int[][] cFrames;     //check frames, [0][0] if none
        public bool checkCon;
        public int lastUsed;        //the frame # this move was last used successfully
        public int charge;          //a number from 0 to 100 for moves that use charge

        public Move(string _name, Rarity _rarity, SlotType _slot, Element _ele, int _dur,
                    HBData[] _hb, int _lLag, int _aStr, int[][] _aF, int[][] _iF, int[][] _cF)
        {
            name = _name;
            rarity = _rarity;
            slot = _slot;
            element = _ele;
            duration = _dur;
            hitbox = _hb;
            landLag = _lLag;
            aStrength = _aStr;
            aFrames = _aF;
            iFrames = _iF;
            cFrames = _cF;
            checkCon = false;           //Starts false by default
            lastUsed = 0;               //Starts at 0 by default
            charge = 0;                 //Starts at 0 by default
        }

        public override string ToString()
        {
            int i = 1;

            string s =
                "Name: " + name + '\n' +
                "Rarity: " + rarity + '\n' +
                "Type: " + slot + '\n' +
                "Element: " + element + '\n' +
                "Duration: " + duration + '\n' +
                "# of Hitboxes: " + hitbox.Length + '\n';
            foreach (HBData hb in hitbox)
            {
                s += "   Hitbox " + i + '\n' + hb.ToString();
                i++;
            }
            s += "Landing Lag: " + landLag + '\n' +
                "Superarmor Threshold: " + aStrength + '\n' +
                "Superarmor Frames: ";

            return s;
        }
    }

    //
    public struct HBData
        {
            bool check;         //True if requires checkCon satisfied to be active, false otherwise
            int start;
            int end;
            int damage;
            int priority;       //If 2 hitboxes from different players collide, the one with the lower priority will be disabled
            int hitstunBase;
            int hitstunGrowth;
            int knockbackAngle; //In relation to the collider vector w/ z-value held to 0
            int knockbackBase;
            int knockbackGrowth;

            public HBData(int _start, int _end, int _damage, int _priority, int _hsBase, int _hsGrowth, int _kbAngle, int _kbBase, int _kbGrowth, bool _check)
            {
                start = _start;
                end = _end;
                damage = _damage;
                priority = _priority;
                hitstunBase = _hsBase;
                hitstunGrowth = _hsGrowth;
                knockbackAngle = _kbAngle;
                knockbackBase = _kbBase;
                knockbackGrowth = _kbGrowth;
                check = _check;
            }

            public override string ToString()
            {
                string s =
                    "      Start Frame: " + start + '\n' +
                    "      End Frame: " + end + '\n' +
                    "      Damage: " + damage + '\n' +
                    "      Base Hitstun: " + hitstunBase + '\n' +
                    "      Hitstun Growth: " + hitstunGrowth + '\n' +
                    "      Knockback Angle: " + knockbackAngle + '\n' +
                    "      Base Knockback: " + knockbackBase + '\n' +
                    "      Knockback Growth: " + knockbackGrowth + '\n' +
                    "      Conditional? " + check + '\n';

                return s;
            }
        }
    //
    public struct Enchantment
    {
        public string name;
        public SlotType type;
        public Rarity rarity;
        public PlayerModifier modifiers;    //Adjustments to core player attributes
        public bool checkCon;               //Starts false by default

        public Enchantment(string _name, Rarity _rarity, PlayerModifier _modifiers)
        {
            name = _name;
            type = SlotType.enchantment;
            rarity = _rarity;
            modifiers = _modifiers;
            checkCon = false;
        }

        public override string ToString()
        {
            int i = 1;

            string s =
                "Name: " + name + '\n' +
                "Type: " + type + '\n' +
                "Rarity: " + rarity + '\n';

            return s;
        }
    }
    //
    public struct PlayerModifier
    {
        public int source;         //The player number of the modifiers source
        public int time;            //How many frames the modification is active for. Ticks down once per frame via itterating through an array of all modifiers on a player. If -1, lasts indefinitely.
        public GameObject model;
        public Color color;
        public int size;
        public int weight;
        public float runSpeed;
        public float gJumpSpeed;
        public float aJumpSpeed;
        public float fallSpeed;
        public int maxJumps;
        public float dps;           //Amount of healing/damage sustained per second

        public PlayerModifier(bool _default)
        {
            source = 0;
            time = -1;

            model = null;
            color = Color.gray;
            size = 0;
            weight = 0;
            runSpeed = 0;
            gJumpSpeed = 0;
            aJumpSpeed = 0;
            fallSpeed = 0;
            maxJumps = 0;
            dps = 0;
        }
        public PlayerModifier(int _source, int _time, GameObject _model, Color _color, int _size, int _weight, 
                                float _runSpeed, float _gJumpSpeed, float _aJumpSpeed, float _fallSpeed, int _maxJumps, float _dps)
        {
            source = _source;
            time = _time;

            model = _model;
            color = _color;
            size = _size;
            weight = _weight;
            runSpeed = _runSpeed;
            gJumpSpeed = _gJumpSpeed;
            aJumpSpeed = _aJumpSpeed;
            fallSpeed = _fallSpeed;
            maxJumps = _maxJumps;
            dps = _dps;
        }

        //Reduces the time left for the modifier by 1 frame
        public void Tick()
        {
            time--;
        }

        public override string ToString()
        {
            string s= 
                "";

            return s;
        }
    }

    //Sets of moves
    public static List<Move[]> MOVES_ = new List<Move[]>
    {
        
    };
    public static List<Move[]> GetAllMoves
    {
        get { return MOVES_; }
    }
    public static Move[] GetSetMoves (int setNumber)
    {
        return MOVES_[setNumber];
    }
    public static Move GetMove(int setNumber, int moveNumber)
    {
        return MOVES_[setNumber][moveNumber];
    }
    public static Move GetMove(string moveName)
    {
        foreach(Move[] _set in MOVES_)
        {
            foreach(Move _move in _set)
            {
                if (_move.name == moveName) return _move;
            }
        }

        return new Move("ERROR", Rarity.common, SlotType.enchantment, Element.None, 0, new HBData[]{}, 0, 0, new int[][]{}, new int[][] { }, new int[][] { });
    }

    //Sets of enchantments
    public static List<Enchantment[]> ENCHANTS_ = new List<Enchantment[]>
    {
        
    };
    public static List<Enchantment[]> GetAllEnchantments
    {
        get { return ENCHANTS_; }
    }
    public static Enchantment[] GetSetEnchantments(int setNumber)
    {
        return ENCHANTS_[setNumber];
    }
    public static Enchantment GetEnchantment(int setNumber, int enchantmentNumber)
    {
        return ENCHANTS_[setNumber][enchantmentNumber];
    }
    public static Enchantment GetEnchantment(string enchantmentName)
    {
        foreach (Enchantment[] _set in ENCHANTS_)
        {
            foreach (Enchantment _Enchantment in _set)
            {
                if (_Enchantment.name == enchantmentName) return _Enchantment;
            }
        }

        return new Enchantment("ERROR", Rarity.common, new PlayerModifier());
    }

    //List of all projectile object references for instantiation
    public static List<GameObject> PROJECTILES_ = new List<GameObject>
    {

    };

    //Complex processes for certain moves, returns wether a moves check condition is satisfied
    /*public static bool MoveEffect(Move move, ref S_Player user, ref S_Player[] opponent, ref S_Player[] ally)
    {
        if (move.name == )
        {

        }
        else if (move.name == )
        {

        }
        else return false;
    }*/

    public static ControlScheme ReadControls(string _name)
    {
        if (File.Exists("Assets/Text/" + _name + ".txt"))
        {
            ControlScheme _controls = new ControlScheme();
            StreamReader reader = new StreamReader("Assets/Text/" + _name + ".txt");

            _controls.buffer = int.Parse(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.moveHorz = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.moveVert = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.rStickUse = (Action)System.Enum.Parse(typeof(Action), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.rHorz = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.rVert = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.left = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.right = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.up = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.down = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.light = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.heavy = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.lightToHeavy = float.Parse(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.special = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.block = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.grab = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.jump = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.alt = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.pause = (KeyCode)System.Enum.Parse(typeof(KeyCode), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);

            return _controls;
        }
        else throw new System.NullReferenceException("File not found");
    }

    public static Dictionary<SlotType, Move> ReadMoves(string _name)
    {
        if (File.Exists("Assets/Text/" + _name + ".txt"))
        {
            Dictionary<SlotType, Move> _moves = new Dictionary<SlotType, Move>();
            StreamReader reader = new StreamReader("Assets/Text/" + _name + ".txt");

            _moves.Add(SlotType.enchantment, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.block, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.getupAtk, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.jab, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.uLight, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.fLight, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.dLight, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.uStrong, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.fStrong, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.dStrong, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.nAir, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.uAir, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.fAir, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.dAir, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.bAir, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.nSpec, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.uSpec, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.fSpec, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _moves.Add(SlotType.dSpec, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));

            return _moves;
        }
        else throw new System.NullReferenceException("File not found");
    }

    public static void CloseGame() { Application.Quit(); }
}
