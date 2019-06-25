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
        dSpec
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
    public struct MoveData
        {
            bool check;         //True if requires condition to be active, false otherwise
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
    public struct PlayerModifiers
    {
        GameObject model;
        float size;
        float weight;
        float moveSpeed;
        float jumpHeight;
        float healthPerSec;     //Amount of healing/damage sustained per second

        public PlayerModifiers(bool _default)
        {
            model = null;
            size = 1;
            weight = 1;
            moveSpeed = 1;
            jumpHeight = 1;
            healthPerSec = 0;
        }
        public PlayerModifiers(GameObject _model, float _size, float _weight, float _moveSpeed, float _jumpHeight, float _HPS)
        {
            model = _model;
            size = _size;
            weight = _weight;
            moveSpeed = _moveSpeed;
            jumpHeight = _jumpHeight;
            healthPerSec = _HPS;
        }

        public override string ToString()
        {
            string s= 
                "";

            return s;
        }
    }
    //
    public struct Move
    {

        public string name;
        public SlotType type;       //Attackor block will change variable uses
        public Rarity rarity;
        public int duration;        //Number of frames if attack, startup frames if block
        public int landLag;         //Stun on land from aerial actions, 0 if grounded action
        public int numHitboxes;     //Number of hitboxes possible during animation
        public HBData[] data;       //Data for each hitbox
        public SpecialEffect specType;        //Non-damaging actions
        public int specStart;
        public int specEnd;
        public bool specCheck;      //Flag for hitbox associated with non-damaging action

        public Move(string _name, SlotType _type, Rarity _rarity, int _duration, int _landLag,
                    int _numHitboxes, HBData[] _data, SpecialEffect _specType, int _specStart, int _specEnd)
        {
            name = _name;
            type = _type;
            rarity = _rarity;
            duration = _duration;
            landLag = _landLag;
            numHitboxes = _numHitboxes;
            data = _data;
            specType = _specType;
            specStart = _specStart;
            specEnd = _specEnd;
            specCheck = false;          //Starts false by default
        }

        public override string ToString()
        {
            int i = 1;

            string s =
                "Name: " + name + '\n' +
                "Type: " + type + '\n' +
                "Rarity: " + rarity + '\n' +
                "Duration: " + duration + '\n' +
                "Landing Lag: " + landLag + '\n' +
                "# of Hitboxes: " + numHitboxes + '\n';
            foreach (HBData hb in data)
            {
                s += "   Hitbox " + i + '\n' + hb.ToString();
                i++;
            }
            s += "Special Effect? " + specType + '\n' +
                "Effect Start Frame: " + specStart + '\n' +
                "Effect End Frame: " + specEnd + '\n' +
                "Effect Triggers Hitbox? " + specCheck + '\n';

            return s;
        }
    }
    //
    public struct Ability
    {
        public string name;
        public SlotType type;       //Attackor block will change variable uses
        public Rarity rarity;
        public PlayerModifiers modifiers;       //Adjustments to core player attributes
        public int numHitboxes;     //Number of hitboxes associated with ability
        public HBData[] data;       //Data for each hitbox
        public SpecialEffect specType;          //Non-damaging actions
        public int specStart;
        public int specEnd;
        public bool specCheck;      //Flag for hitbox associated with non-damaging action

        public Ability(string _name, Rarity _rarity, PlayerModifiers _modifiers, int _numHitboxes, HBData[] _data, SpecialEffect _specType, int _specStart, int _specEnd)
        {
            name = _name;
            type = SlotType.ability;
            rarity = _rarity;
            modifiers = _modifiers;
            numHitboxes = _numHitboxes;
            data = _data;
            specType = _specType;
            specStart = _specStart;
            specEnd = _specEnd;
            specCheck = false;          //Starts false by default
        }

        public override string ToString()
        {
            int i = 1;

            string s =
                "Name: " + name + '\n' +
                "Type: " + type + '\n' +
                "Rarity: " + rarity + '\n' +
                "# of Hitboxes: " + numHitboxes + '\n';
            foreach (HBData hb in data)
            {
                s += "   Hitbox " + i + '\n' + hb.ToString();
                i++;
            }
            s += "Special Effect? " + specType + '\n' +
                "Effect Start Frame: " + specStart + '\n' +
                "Effect End Frame: " + specEnd + '\n' +
                "Effect Triggers Hitbox? " + specCheck + '\n';

            return s;
        }
    }

    //Sets of moves
    public static List<Move[]> MOVES_ = new List<Move[]>
    {
        //ALPHA
        new Move[]
        {
            new Move("Vanilla Punch", SlotType.jab, Rarity.common, 10, 0, 1, new HBData[]{
                        new HBData(2, 6, 4, 0, 5, 1, 0, 5, 1, false) },
                    SpecialEffect.none, 0, 0),

            new Move("Vanilla Fistpump", SlotType.uLight, Rarity.common, 22, 0, 1, new HBData[]{
                        new HBData(5, 11, 6, 0, 5, 3, 0, 8, 2, false) },
                    SpecialEffect.none, 0, 0)
        }
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

        return new Move("ERROR", SlotType.ability, Rarity.common, 0, 0, 0, new HBData[]{}, SpecialEffect.none, 0, 0);
    }

    //Sets of abilities
    public static List<Ability[]> ABILITIES_ = new List<Ability[]>
    {
        //ALPHA
        new Ability[]
        {
            new Ability("Vanilla", Rarity.common, new PlayerModifiers(false), 0, new HBData[]{ }, SpecialEffect.none, 0, 0)
        }
    };
    public static List<Ability[]> GetAllAbilities
    {
        get { return ABILITIES_; }
    }
    public static Ability[] GetSetAbilities(int setNumber)
    {
        return ABILITIES_[setNumber];
    }
    public static Ability GetAbility(int setNumber, int abilityNumber)
    {
        return ABILITIES_[setNumber][abilityNumber];
    }
    public static Ability GetAbility(string abilityName)
    {
        foreach (Ability[] _set in ABILITIES_)
        {
            foreach (Ability _ability in _set)
            {
                if (_ability.name == abilityName) return _ability;
            }
        }

        return new Ability("ERROR", Rarity.common, new PlayerModifiers(false), 0, new HBData[] { }, SpecialEffect.none, 0, 0);
    }

    public static ControlScheme ReadControls(string name)
    {
        if (File.Exists("Assets/Text/" + name + ".txt"))
        {
            ControlScheme _controls = new ControlScheme();
            StreamReader reader = new StreamReader("Assets/Text/" + name + ".txt");

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

    public static Dictionary<SlotType, Move> ReadMoves(string name)
    {
        if (File.Exists("Assets/Text/" + name + ".txt"))
        {
            Dictionary<SlotType, Move> _moves = new Dictionary<SlotType, Move>();
            StreamReader reader = new StreamReader("Assets/Text/" + name + ".txt");

            _moves.Add(SlotType.ability, GetMove(reader.ReadLine().Split(new char[] { ' ' }, 2, System.StringSplitOptions.None)[1]));
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
