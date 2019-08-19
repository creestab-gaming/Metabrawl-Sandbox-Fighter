using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class S_Data
{
    //Debug Flags
    public static bool GENERAL_DEBUG = true;
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
        none,
        jump,
        block,
        floatDodge,
        fallDodge,
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
        model,          //The character model as well as modifiers like size and weight
        enchantment,    //Static modifier w/ an associated element
        move
    }
    //
    public enum Element
    {
        Null,
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
    public struct Player
    {
        public Model model;
        public Enchantment enchantment;
        public Dictionary<ActionType, Move> moveset;
    }
    //
    public struct PlayerState
    {
        public GameObject rig;  //Game asset representing player character
        public Color color;     //Color/skin of player character
        public Element element; //Player element from enchantment of modification, determines possible moves
        public int size;        //Scaling of player character and its hit/hurtboxes [1-10]
        public int weight;      //Character weight, influences jump heigh, fallspeed, knockback distance, ext [1-10]
        public float runSpeed;  //How fast the player moves on the ground [1-10]
        public float gJumpSpeed;//How fast/high the player jumps off the ground [1-10]
        public float aJumpSpeed;//How fast/high the player jumps in the air (double jump) [1-10]
        public float aDrift;     //Modifier for aerial drift [1-10]
        public float fallSpeed; //How fast the player falls in the air before weight is applied [1-10]
        public int orientation; //1 for facing right, -1 for facing left.
        public int jumps;       //How many jumps the player currently has
        public int maxJumps;    //The number of jumps restored when landed on stage.
        public float damage;    //Amount of damage currently sustained
        public float dps;       //Change in sustained damage per second
        public int stun;        //Number of frames of stun left, reduces by 1 every frame
        public bool holding;    //Is the player holding another player drom a grab
        public bool actable;    //Is the player able to perform normal moves
        public bool onStage;    //Is the player on stage
        public bool onWallLeft; //Is the player touching the left of the stage
        public bool onWallRight;//Is the player touching the right of the stage
        public bool airborne;   //Is the player in the air
        public bool fastfall;   //Is the player fastfalling

        public PlayerState(bool _default)
        {
            rig = GameObject.Find("M_Default");
            color = Color.grey;
            element = Element.None;
            size = 5;
            weight = 5;
            runSpeed = 5;
            gJumpSpeed = 5;
            aJumpSpeed = 5;
            aDrift = 5;
            fallSpeed = 5;
            orientation = 0;
            jumps = 0;
            maxJumps = 2;
            damage = 0;
            dps = 0;
            stun = 0;
            holding = false;
            actable = true;
            onStage = false;
            onWallLeft = false;
            onWallRight = false;
            airborne = true;
            fastfall = false;
        }

        public PlayerState(GameObject _rig, int _size, int _weight, float _runSpeed, float _gJumpSpeed, float _aJumpSpeed,
                float _aDrift, float _fallSpeed, int _maxJumps, float _damage, float _dps, int _stun, bool _actable, bool _fastfall)
        {
            rig = _rig;
            color = Color.grey;     //English spelling indicates null
            element = Element.None;
            size = _size;
            weight = _weight;
            runSpeed = _runSpeed;
            gJumpSpeed = _gJumpSpeed;
            aJumpSpeed = _aJumpSpeed;
            aDrift = _aDrift;
            fallSpeed = _fallSpeed;
            orientation = 0;
            maxJumps = _maxJumps;
            jumps = maxJumps;
            damage = _damage;
            dps = _dps;
            stun = _stun;
            holding = false;
            actable = _actable;
            onStage = false;
            onWallLeft = false;
            onWallRight = false;
            airborne = true;
            fastfall = _fastfall;
        }

        public PlayerState(GameObject _rig, Color _color, Element _element, int _size, int _weight, float _runSpeed,
                        float _gJumpSpeed, float _aJumpSpeed, float _aDrift, float _fallSpeed, int _orientation, int _maxJumps,
                        float _damage, float _dps, int _stun, bool _actable, bool _fastfall)
        {
            rig = _rig;
            color = _color;
            element = _element;
            size = _size;
            weight = _weight;
            runSpeed = _runSpeed;
            gJumpSpeed = _gJumpSpeed;
            aJumpSpeed = _aJumpSpeed;
            aDrift = _aDrift;
            fallSpeed = _fallSpeed;
            orientation = _orientation;
            maxJumps = _maxJumps;
            jumps = maxJumps;
            damage = _damage;
            dps = _dps;
            stun = _stun;
            holding = false;
            actable = _actable;
            onStage = false;
            onWallLeft = false;
            onWallRight = false;
            airborne = true;
            fastfall = _fastfall;
        }
    }
    //Controller input mapping for action commands
    public struct ControlScheme
    {
        public int buffer;
        public string mHorz;
        public string mVert;
        public Action cStickUse;
        public string cHorz;
        public string cVert;
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
                "Movement Axis (Horizonal): " + mHorz + '\n' +
                "Movement Axis (Vertical): " + mVert + '\n' +
                "Misc Axis (Horizontal): " + cHorz + '\n' +
                "Misc Axis (Vertical): " + cVert + '\n' +
                "Misc Axis Use: " + cStickUse + '\n' +
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
        public string code;
        public Rarity rarity;
        public ActionType slot;
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

        public Move(string _name, string _code, Rarity _rarity, ActionType _slot, Element _ele, int _dur,
                    HBData[] _hb, int _lLag, int _aStr, int[][] _aF, int[][] _iF, int[][] _cF)
        {
            name = _name;
            code = _code;
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
        public bool check;          //True if requires checkCon satisfied to be active, false otherwise
        public int sensor;          //The sensor associated with this hitbox
        public int start;           //Frame this hitbox becomes active
        public int end;             //Frame this hitbox becomes disabled
        public int damage;
        public int priority;        //If 2 hitboxes from different players collide, the one with the lower priority will be disabled
        public int hitstunBase;
        public int hitstunGrowth;
        public int knockbackAngle;  //In relation to the collider vector w/ z-value held to 0
        public int knockbackBase;
        public int knockbackGrowth;

        public HBData(int _start, int _sensor, int _end, int _damage, int _priority, int _hsBase, int _hsGrowth, int _kbAngle, int _kbBase, int _kbGrowth, bool _check)
        {
            start = _start;
            sensor = _sensor;
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
    public struct Model
    {
        public string name;
        public string code;
        public SlotType type;
        public Rarity rarity;
        public PlayerState modifiers;

        public Model(string _name, string _code, Rarity _rarity, PlayerState _modifiers)
        {
            name = _name;
            code = _code;
            type = SlotType.model;
            rarity = _rarity;
            modifiers = _modifiers;
        }
    }
    //
    public struct Enchantment
    {
        public string name;
        public string code;
        public SlotType type;
        public Element element;
        public Rarity rarity;
        public PlayerModifier modifiers;    //Adjustments to core player attributes
        public bool checkCon;               //Starts false by default

        public Enchantment(string _name, string _code, Element _element, Rarity _rarity, PlayerModifier _modifiers)
        {
            name = _name;
            code = _code;
            element = _element;
            type = SlotType.enchantment;
            rarity = _rarity;
            modifiers = _modifiers;
            checkCon = false;
        }

        public override string ToString()
        {
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
        public Element element;
        public int size;
        public int weight;
        public float runSpeed;
        public float gJumpSpeed;
        public float aJumpSpeed;
        public float aDrift;
        public float fallSpeed;
        public int maxJumps;
        public float dps;           //Amount of healing/damage sustained per second

        public PlayerModifier(bool _default)
        {
            source = 0;
            time = -1;

            model = null;
            color = Color.grey;     //English spelling indicates null
            element = Element.Null;
            size = 0;
            weight = 0;
            runSpeed = 0;
            gJumpSpeed = 0;
            aJumpSpeed = 0;
            aDrift = 0;
            fallSpeed = 0;
            maxJumps = 0;
            dps = 0;
        }

        public PlayerModifier(Element _element, int _size, int _weight,
                        float _runSpeed, float _gJumpSpeed, float _aJumpSpeed, float _aDrift, float _fallSpeed, int _maxJumps, float _dps)
        {
            source = 0;
            time = -1;

            model = null;
            color = Color.grey;
            element = _element;
            size = _size;
            weight = _weight;
            runSpeed = _runSpeed;
            gJumpSpeed = _gJumpSpeed;
            aJumpSpeed = _aJumpSpeed;
            aDrift = _aDrift;
            fallSpeed = _fallSpeed;
            maxJumps = _maxJumps;
            dps = _dps;
        }

        public PlayerModifier(int _source, int _time, GameObject _model, Color _color, Element _element, int _size, int _weight,
                                float _runSpeed, float _gJumpSpeed, float _aJumpSpeed, float _aDrift, float _fallSpeed, int _maxJumps, float _dps)
        {
            source = _source;
            time = _time;

            model = _model;
            color = _color;
            element = _element;
            size = _size;
            weight = _weight;
            runSpeed = _runSpeed;
            gJumpSpeed = _gJumpSpeed;
            aJumpSpeed = _aJumpSpeed;
            aDrift = _aDrift;
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
            string s =
                "";

            return s;
        }
    }

    //Sets of moves
    public static Move[][] MOVES_ = new Move[][]
    {
        //Null set/move array
        new Move[]{new Move("NULL","0000",Rarity.common,ActionType.none,Element.Null,0,null,0,0,null,null,null)},

        //Alpha
        new Move[]
        {
            new Move("Vanilla Punch","0100",Rarity.common,ActionType.jab,Element.None,11,new HBData[]{new HBData()},0,2,null,null,null)
        }
    };
    public static Move[][] GetAllMoves
    {
        get { return MOVES_; }
    }
    public static Move[] GetSetMoves(int setNumber)
    {
        return MOVES_[setNumber];
    }
    public static Move GetMove(string moveCode)
    {
        return MOVES_[System.Convert.ToInt32(moveCode.Substring(0, 2), 16)][System.Convert.ToInt32(moveCode.Substring(2), 16)];
    }
    public static Move GetMove(int setNumber, int moveNumber)
    {
        return MOVES_[setNumber][moveNumber];
    }
    public static Move SearchMove(string moveName)
    {
        for (int _set = 0; _set < MOVES_.Length; _set++)
        {
            for (int _move = 0; _move < MOVES_[_set].Length; _move++)
            {
                if (MODELS_[_set][_move].name == moveName) return MOVES_[_set][_move];
            }
        }

        //Not found, return null
        return MOVES_[0][0];
    }

    //Sets of models
    public static Model[][] MODELS_ = new Model[][]
    {
        //Null set/model array
        new Model[]{new Model("NULL", "0000", Rarity.common, new PlayerState(false))},
    
        //Alpha
        new Model[]
        {
            new Model("Default", "0100", Rarity.common, new PlayerState(GameObject.Find("M_Default"),5,5,5,5,5,5,5,2,0,0,0,true,false))
        }
    };
       
    public static Model[][] GetAllModels
    {
        get { return MODELS_; }
    }
    public static Model[] GetSetModels(int setNumber)
    {
        return MODELS_[setNumber];
    }
    public static Model GetModel(string modelCode)
    {
        return MODELS_[System.Convert.ToInt32(modelCode.Substring(0, 2), 16)][System.Convert.ToInt32(modelCode.Substring(2), 16)];
    }
    public static Model GetModel(int setNumber, int modelNumber)
    {
        return MODELS_[setNumber][modelNumber];
    }
    public static Model SearchModel(string modelName)
    {
        for (int _set = 0; _set < MODELS_.Length; _set++)
        {
            for (int _model = 0; _model < MODELS_[_set].Length; _model++)
            {
                if (MODELS_[_set][_model].name == modelName) return MODELS_[_set][_model];
            }
        }
        
        //Not found, return null
        return MODELS_[0][0];
    }

    //Sets of enchantments
    public static Enchantment[][] ENCHANTS_ = new Enchantment[][]
    {
        //Null set/enchant array
        new Enchantment[]{new Enchantment("NULL", "0000", Element.Null, Rarity.common, new PlayerModifier(false))},

        //Alpha
        new Enchantment[]
        {
            new Enchantment("Default", "0100", Element.None, Rarity.common, new PlayerModifier(Element.None,0,0,0,0,0,0,0,0,0)),
        }
    };
    public static Enchantment[][] GetAllEnchantments
    {
        get { return ENCHANTS_; }
    }
    public static Enchantment[] GetSetEnchantments(int setNumber)
    {
        return ENCHANTS_[setNumber];
    }
    public static Enchantment GetEnchantment(string enchantmentCode)
    {
        return ENCHANTS_[System.Convert.ToInt32(enchantmentCode.Substring(0, 2), 16)][System.Convert.ToInt32(enchantmentCode.Substring(2), 16)];
    }
    public static Enchantment GetEnchantment(int setNumber, int enchantmentNumber)
    {
        return ENCHANTS_[setNumber][enchantmentNumber];
    }
    public static Enchantment SearchEnchantment(string enchantmentName)
    {
        for (int _set = 0; _set < ENCHANTS_.Length; _set++)
        {
            for (int _ench = 0; _ench < ENCHANTS_[_set].Length; _ench++)
            {
                if (ENCHANTS_[_set][_ench].name == enchantmentName) return ENCHANTS_[_set][_ench];
            }
        }

        //Not found, return null
        return ENCHANTS_[0][0];
    }

    //List of all projectile object references for instantiation
    public static GameObject[] PROJECTILES_ = new GameObject[]
    {

    };
 
    //Complex processes for certain moves
    public static void MoveEffects(int _move)
    {
            switch(_move)
            {
                
            }
    }

    //Instantiates a character & moveset from file
    public static Player ReadCharacter(string _name)
    {
        if (File.Exists("Assets/Text/" + _name + ".txt"))
        {
            StreamReader reader = new StreamReader("Assets/Text/" + _name + ".txt");
            Player _char = new Player();
            _char.moveset = new Dictionary<ActionType, Move>();

            //Moveset
            _char.model = SearchModel(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _char.enchantment = SearchEnchantment(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _char.moveset.Add(ActionType.block, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.jab, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.uLight, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.fLight, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.dLight, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.uStrong, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.fStrong, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.dStrong, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.nAir, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.uAir, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.fAir, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.dAir, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.bAir, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.nSpec, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.uSpec, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.fSpec, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
            _char.moveset.Add(ActionType.dSpec, SearchMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));

            return _char;
        }
        else throw new System.NullReferenceException("File not found");
    }
    //Instantiates a control scheme from file
    public static ControlScheme ReadControls(string _name)
    {
        if (File.Exists("Assets/Text/" + _name + ".txt"))
        {
            ControlScheme _controls = new ControlScheme();
            StreamReader reader = new StreamReader("Assets/Text/" + _name + ".txt");

            _controls.buffer = int.Parse(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.mHorz = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.mVert = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.cStickUse = (Action)System.Enum.Parse(typeof(Action), reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]);
            _controls.cHorz = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
            _controls.cVert = reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1];
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

            if (GENERAL_DEBUG) { Debug.Log(_controls.cHorz.ToString()); Debug.Log(_controls.cVert.ToString()); }

            return _controls;
        }
        else throw new System.NullReferenceException("File not found");
    }

    public static void CloseGame() { Application.Quit(); }


}