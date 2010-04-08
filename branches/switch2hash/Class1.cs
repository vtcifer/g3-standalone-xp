using System;
using System.Collections;
using GeniePlugin.Interfaces;
using System.Xml;
using System.Text.RegularExpressions;

namespace Standalone_EXPTracker
{
    public class Class1 : IPlugin
    {
        string _VERSION = "1.4.0";

        #region IPlugin Members
        public IHost _host;                             //Required for plugin
        public System.Windows.Forms.Form _parent;       //Required for plugin

        private DateTime _startTime;                    //Used for TDP/Rank tracking to know how long tracking since
        private Hashtable _skillList = new Hashtable(); //Used for storing/sorting skills for display in Exp Win
        private int _TDP;                               //Used for TDP tracking, this is current TDPs
        private int _startTDP = 0;                      //Used for TDP tracking, this is set when first checking
        private bool _updateExp = false;                //Used for know when next prompt is shown, to update EXPWindow
        private bool _parsing = false;                  //Used for ParseText, to know if EXP command output is returned
        private bool _sleeping = false;
        private string _mindState = "";                 //Used for converting mindstate parsed to an integer for GenieVariable
        
        //The following hashtables are used as alternatives to switch statments using string data
        //which can have rather bad run time
        private Hashtable MasterSkill;
        private class ItemMasterSkill
        {
            public int SortLR;
            public string ShortName;
        }
        
        private Hashtable MasterMindState;
        private Hashtable MasterLearnRate;


        // enabled appears unused at the moment
        private bool _enabled = true;

        //Class Skill
        //Used for storing all skill related info
        //Used in a hashtable whose key is the name of the skill
        private class Skill
        {
            public double rank = 0;                 //Rank of the skill
            public string learningRate = "clear";   //Text Learning rate
            public int iLearningRate = 0;           //Numerical Learning rate
            public double startRank = 0;            //When Rank tracking enabled, this is starting rank calculated for
            public bool rankGained = false;         //Used for displaying text in a specific color when rank gained
            public bool learned = false;            //Used for displaying text in a specific color when bits are added to pool
            public int sortLR = 0;                  //Used for sorting As reading, Left to Right
            public string shortname = "";           //Used for short name display instead of long name
        }

        //Class Sortskill
        //Used for sorting the skills for display in the Experience window
        //Used in an array list for sorting, which is fed from a hashtable
        public class Sortskill
        {
            public string name = "";        //Name of skill
            public string shortname = "";   //Shortened name of skill
            public int sortLR = 0;          //Ordered value based on Reading sort (Left to Right)
            public int sortLearning = 0;    //Ordered value based on top to bottom, THEN left to right 
        }
        #endregion
        #region IPlugin Methods

        //Required for Plugin - Called on first load
        //Parameters:
        //              IHost Host:  The host (instance of Genie) making the call
        public void Initialize(IHost Host)
        {
            //Set Decimal Seperator to a period (.) if not set that way
            if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            //Set _host variable to the Instance of Genie that started the plugin (so can call host API commands)
            _host = Host;
            //Set startTime to the time the plugin was called (used in how long tracking has been occuring)
            _startTime = DateTime.Now;
            //Create hash table for all skills
            _skillList = new Hashtable(67);

            //Set Genie Variables if not already set

            if (_host.get_Variable("ExpTracker.Window") == "")
                _host.set_Variable("ExpTracker.Window", "0");

            if (_host.get_Variable("ExpTracker.ShowRankGain") == "")
                _host.set_Variable("ExpTracker.ShowRankGain", "1");

            if (_host.get_Variable("ExpTracker.LearningRate") == "")
                _host.set_Variable("ExpTracker.LearningRate", "1");

            if (_host.get_Variable("ExpTracker.LearningRateNumber") == "")
                _host.set_Variable("ExpTracker.LearningRateNumber", "0");

            if (_host.get_Variable("ExpTracker.TrackSleep") == "")
                _host.set_Variable("ExpTracker.TrackSleep", "0");

            if (_host.get_Variable("ExpTracker.EchoSleep") == "")
                _host.set_Variable("ExpTracker.EchoSleep", "0");
            
            if (_host.get_Variable("ExpTracker.SortType") == "")
                _host.set_Variable("ExpTracker.SortType", "0");

            if (_host.get_Variable("ExpTracker.GagExp") == "")
                _host.set_Variable("ExpTracker.GagExp", "0");

            if (_host.get_Variable("ExpTracker.Color.Normal") == "")
                _host.set_Variable("ExpTracker.Color.Normal", "WhiteSmoke");

            if (_host.get_Variable("ExpTracker.Color.RankGained") == "")
                _host.set_Variable("ExpTracker.Color.RankGained", "WhiteSmoke");

            if (_host.get_Variable("ExpTracker.Color.Learned") == "")
                _host.set_Variable("ExpTracker.Color.Learned", "WhiteSmoke");

            if (_host.get_Variable("ExpTracker.ShortNames") == "")
                _host.set_Variable("ExpTracker.ShortNames", "0");

            //create AND popluate the master hashtables
            MasterSkill = new Hashtable(72);
    //Armor skills
            MasterSkill.Add("Shield Usage", new ItemMasterSkill {SortLR = 0, ShortName = "Shield"});
            MasterSkill.Add("Leather Armor", new ItemMasterSkill {SortLR = 1, ShortName = "Leather"});
            MasterSkill.Add("Light Chain", new ItemMasterSkill {SortLR = 2, ShortName = "LC"});
            MasterSkill.Add("Heavy Chain", new ItemMasterSkill {SortLR = 3, ShortName = "HC"});
            MasterSkill.Add("Light Plate", new ItemMasterSkill {SortLR = 4, ShortName = "LP"});
            MasterSkill.Add("Heavy Plate", new ItemMasterSkill {SortLR = 5, ShortName = "HP"});
            MasterSkill.Add("Cloth Armor", new ItemMasterSkill {SortLR = 6, ShortName = "Cloth"});
            MasterSkill.Add("Bone Armor", new ItemMasterSkill {SortLR = 7, ShortName = "Bone" });
    //Weapon Skills            
            MasterSkill.Add("Parry Ability", new ItemMasterSkill {SortLR = 100, ShortName = "Parry"});
            MasterSkill.Add("Multi Opponent", new ItemMasterSkill {SortLR = 101, ShortName = "MO"});
            MasterSkill.Add("Light Edged", new ItemMasterSkill {SortLR = 102, ShortName = "LE"});
            MasterSkill.Add("Medium Edged", new ItemMasterSkill {SortLR = 103, ShortName = "ME"});
            MasterSkill.Add("Heavy Edged", new ItemMasterSkill {SortLR = 104, ShortName = "HE"});
            MasterSkill.Add("Twohanded Edged", new ItemMasterSkill {SortLR = 105, ShortName = "2HE"});
            MasterSkill.Add("Light Blunt", new ItemMasterSkill {SortLR = 106, ShortName = "LB"});
            MasterSkill.Add("Medium Blunt", new ItemMasterSkill {SortLR = 107, ShortName = "MB"});
            MasterSkill.Add("Heavy Blunt", new ItemMasterSkill {SortLR = 108, ShortName = "HB"});
            MasterSkill.Add("Twohanded Blunt", new ItemMasterSkill {SortLR = 109, ShortName = "2HB"});
            MasterSkill.Add("Slings", new ItemMasterSkill {SortLR = 110, ShortName = "Sling"});
            MasterSkill.Add("Staff Sling", new ItemMasterSkill {SortLR = 111, ShortName = "S Sling"});
            MasterSkill.Add("Short Bow", new ItemMasterSkill {SortLR = 112, ShortName = "S Bow"});
            MasterSkill.Add("Long Bow", new ItemMasterSkill {SortLR = 113, ShortName = "L Bow"});
            MasterSkill.Add("Composite Bow", new ItemMasterSkill {SortLR = 114, ShortName = "C Bow"});
            MasterSkill.Add("Light Crossbow", new ItemMasterSkill {SortLR = 115, ShortName = "LX"});
            MasterSkill.Add("Heavy Crossbow", new ItemMasterSkill {SortLR = 116, ShortName = "HX"});
            MasterSkill.Add("Short Staff", new ItemMasterSkill {SortLR = 117, ShortName = "S Staff"});
            MasterSkill.Add("Quarter Staff", new ItemMasterSkill {SortLR = 118, ShortName = "Q Staff"});
            MasterSkill.Add("Pikes", new ItemMasterSkill {SortLR = 119, ShortName = "Pike"});
            MasterSkill.Add("Halberds", new ItemMasterSkill {SortLR = 120, ShortName = "Halberd"});
            MasterSkill.Add("Light Thrown", new ItemMasterSkill {SortLR = 121, ShortName = "LT"});
            MasterSkill.Add("Heavy Thrown", new ItemMasterSkill {SortLR = 122, ShortName = "HT"});
            MasterSkill.Add("Brawling", new ItemMasterSkill {SortLR = 123, ShortName = "Brawl"});
            MasterSkill.Add("Offhand Weapon", new ItemMasterSkill { SortLR = 124, ShortName = "Offhand"});
    //Magic Skills
            MasterSkill.Add("Lunar Magic", new ItemMasterSkill {SortLR = 200, ShortName = "Magic"});
            MasterSkill.Add("Holy Magic", new ItemMasterSkill {SortLR = 200, ShortName = "Magic"});
            MasterSkill.Add("Elemental Magic", new ItemMasterSkill {SortLR = 200, ShortName = "Magic"});
            MasterSkill.Add("Inner Magic", new ItemMasterSkill {SortLR = 200, ShortName = "Magic"});
            MasterSkill.Add("Arcane Magic", new ItemMasterSkill {SortLR = 200, ShortName = "Magic"});
            MasterSkill.Add("Harness Ability", new ItemMasterSkill {SortLR = 201, ShortName = "Harness"});
            MasterSkill.Add("Power Perceive", new ItemMasterSkill {SortLR = 202, ShortName = "PP"});
            MasterSkill.Add("Arcana", new ItemMasterSkill {SortLR = 203, ShortName = "Arcana"});
            MasterSkill.Add("Targeted Magic", new ItemMasterSkill {SortLR = 204, ShortName = "TM"});
    //Survival Skills
            MasterSkill.Add("Evasion", new ItemMasterSkill {SortLR = 300, ShortName = "Evade"});
            MasterSkill.Add("Climbing", new ItemMasterSkill {SortLR = 301, ShortName = "Climb"});
            MasterSkill.Add("Perception", new ItemMasterSkill {SortLR = 302, ShortName = "Percep"});
            MasterSkill.Add("Scouting", new ItemMasterSkill {SortLR = 303, ShortName = "Scout"});
            MasterSkill.Add("Hiding", new ItemMasterSkill {SortLR = 304, ShortName = "Hide"});
            MasterSkill.Add("Lockpicking", new ItemMasterSkill {SortLR = 305, ShortName = "Locks"});
            MasterSkill.Add("Disarm Traps", new ItemMasterSkill {SortLR = 306, ShortName = "Disarm"});
            MasterSkill.Add("Stalking", new ItemMasterSkill {SortLR = 307, ShortName = "Stalk"});
            MasterSkill.Add("Stealing", new ItemMasterSkill {SortLR = 308, ShortName = "Steal"});
            MasterSkill.Add("First Aid", new ItemMasterSkill {SortLR = 309, ShortName = "FA"});
            MasterSkill.Add("Foraging", new ItemMasterSkill {SortLR = 310, ShortName = "Forage"});
            MasterSkill.Add("Escaping", new ItemMasterSkill {SortLR = 311, ShortName = "Escape"});
            MasterSkill.Add("Backstab", new ItemMasterSkill {SortLR = 312, ShortName = "BS"});
            MasterSkill.Add("Skinning", new ItemMasterSkill {SortLR = 313, ShortName = "Skin"});
            MasterSkill.Add("Swimming", new ItemMasterSkill {SortLR = 314, ShortName = "Swim"});
    //Lore Skills
            MasterSkill.Add("Scholarship", new ItemMasterSkill {SortLR = 400, ShortName = "Scholar"});
            MasterSkill.Add("Mechanical Lore", new ItemMasterSkill {SortLR = 401, ShortName = "Mech"});
            MasterSkill.Add("Musical Theory", new ItemMasterSkill {SortLR = 402, ShortName = "Music"});
            MasterSkill.Add("Appraisal", new ItemMasterSkill {SortLR = 403, ShortName = "App"});
            MasterSkill.Add("Teaching", new ItemMasterSkill {SortLR = 404, ShortName = "Teach"});
            MasterSkill.Add("Trading", new ItemMasterSkill {SortLR = 405, ShortName = "Trade"});
            MasterSkill.Add("Animal Lore", new ItemMasterSkill {SortLR = 406, ShortName = "Animal"});
            MasterSkill.Add("Percussions", new ItemMasterSkill {SortLR = 407, ShortName = "Percuss"});
            MasterSkill.Add("Strings", new ItemMasterSkill {SortLR = 408, ShortName = "Strings"});
            MasterSkill.Add("Winds", new ItemMasterSkill {SortLR = 409, ShortName = "Winds"});
            MasterSkill.Add("Vocals", new ItemMasterSkill {SortLR = 410, ShortName = "Vocals"});
            MasterSkill.Add("Astrology", new ItemMasterSkill {SortLR = 411, ShortName = "Astro"});
            MasterSkill.Add("Empathy", new ItemMasterSkill {SortLR = 412, ShortName = "Empathy"});
            MasterSkill.Add("Thanatology", new ItemMasterSkill {SortLR = 413, ShortName = "Than"});


            MasterMindState = new Hashtable(12);
            MasterMindState.Add("clear", 0);
            MasterMindState.Add("fluid", 1);
            MasterMindState.Add("murky", 2);
            MasterMindState.Add("very murky", 3);
            MasterMindState.Add("thick", 4);
            MasterMindState.Add("very thick", 5);
            MasterMindState.Add("dense", 6);
            MasterMindState.Add("very dense", 7);
            MasterMindState.Add("stagnant", 8);
            MasterMindState.Add("very stagnant", 9);
            MasterMindState.Add("frozen", 10);
            MasterMindState.Add("very frozen", 11);

            MasterLearnRate = new Hashtable(35);
            MasterLearnRate.Add("clear", 0);
            MasterLearnRate.Add("dabbling", 1);
            MasterLearnRate.Add("perusing",  2);
            MasterLearnRate.Add("learning",  3);
            MasterLearnRate.Add("thoughtful",  4);
            MasterLearnRate.Add("thinking",  5);
            MasterLearnRate.Add("considering",  6);
            MasterLearnRate.Add("pondering",  7);
            MasterLearnRate.Add("ruminating",  8);
            MasterLearnRate.Add("concentrating", 9);
            MasterLearnRate.Add("attentive", 10);
            MasterLearnRate.Add("deliberative", 11);
            MasterLearnRate.Add("interested", 12);
            MasterLearnRate.Add("examining", 13);
            MasterLearnRate.Add("understanding", 14);
            MasterLearnRate.Add("absorbing", 15);
            MasterLearnRate.Add("intrigued", 16);
            MasterLearnRate.Add("scrutinizing", 17);
            MasterLearnRate.Add("analyzing", 18);
            MasterLearnRate.Add("studious", 19);
            MasterLearnRate.Add("focused", 20);
            MasterLearnRate.Add("very focused", 21);
            MasterLearnRate.Add("engaged", 22);
            MasterLearnRate.Add("very engaged", 23);
            MasterLearnRate.Add("cogitating", 24);
            MasterLearnRate.Add("fascinated", 25);
            MasterLearnRate.Add("captivated", 26);
            MasterLearnRate.Add("engrossed", 27);
            MasterLearnRate.Add("riveted", 28);
            MasterLearnRate.Add("very riveted", 29);
            MasterLearnRate.Add("rapt", 30);
            MasterLearnRate.Add("very rapt", 31);
            MasterLearnRate.Add("enthralled", 32);
            MasterLearnRate.Add("nearly locked", 33);
            MasterLearnRate.Add("mind lock", 34);
        }

        //Required for Plugin - Called when Genie needs the name of the plugin (On menu)
        //Return Value:
        //              string: Text that is the name of the Plugin
        public string Name
        {
            get { return "Standalone EXPTracker"; }
        }

        //Required for Plugin - Called when user enters text in the command box
        //Parameters:
        //              string Text:  The text the user entered in the command box
        //Return Value:
        //              string: Text that will be sent to the game
        public string ParseInput(string Text)
        {
            if (Text.StartsWith("/track"))
            {
                //Reset all tracking, to current value for skills/TDPS
                if (Text == "/trackreset" || Text == "/track reset") 
                {
                    //Reset TDP tracking
                    _TDP = 0;
                    _startTDP = 0;

                    //Reset "Tracking Since"
                    _startTime = DateTime.Now;

                    //Reset skill tracking to current values
                    IDictionaryEnumerator en = _skillList.GetEnumerator();
                    Hashtable ht = new Hashtable(67);
                    while (en.MoveNext())
                    {
                        Skill obj = (Skill)en.Value;
                        obj.startRank = obj.rank;
                        ht.Add(en.Key, obj);
                    }
                    _skillList.Clear();
                    _skillList = ht;

                    //Alert User Tracking is Reset
                    _host.SendText("#echo");
                    _host.SendText("#echo Rank tracking reset.");
                    return "";
                }
                //Resets all tracking to 0, as if Genie just launched
                else if (Text == "/track clear")
                {
                    //Reset TDP tracking
                    _TDP = 0;
                    _startTDP = 0;

                    //Reset "Tracking Since"
                    _startTime = DateTime.Now;

                    //Reset all skill tracking info to start values
                    _skillList.Clear();
                    _skillList = new Hashtable(67);

                    //Alert User of Reset:
                    _host.SendText("#echo");
                    _host.SendText("#echo XP Tracker reset to intial values");
                    return "";
                }
                //Pauses XP Tracker until /trackresume is seen
                else if (Text == "/track pause")
                {
                    _enabled = false;
                    _host.SendText("#echo");
                    _host.SendText("#echo XP Tracker paused.");
                    _host.SendText("#echo /track resume to un-pause");

                    return "";
                }
                //Resumes XP Tracker
                else if (Text == "/track resume")
                {
                    
                    _enabled = true;
                    _host.SendText("#echo");
                    _host.SendText("#echo XP Tracker resumed.");

                    return "";
                }
                //User asking for help with commands, or invalid command entered
                else
                {
                    _host.SendText("#echo");
                    _host.SendText(@"#echo Standlone EXPTracker (Ver:" + _VERSION + ") Usage:");
                    _host.SendText(@"#echo /track reset");
                    _host.SendText(@"#echo """"    """" Used to reset tracking");
                    _host.SendText(@"#echo /track clear");
                    _host.SendText(@"#echo """"    """" Used to reset as if you just started Genie");
                    _host.SendText(@"#echo /track pause");
                    _host.SendText(@"#echo """"    """" Used to pause Exp Tracker from tracking ANY Exp Changes");
                    _host.SendText(@"#echo /track clear");
                    _host.SendText(@"#echo """"    """" Used to resume Exp Tracker");
                    return "";
                }
            }
            //means no special arguments, send command on to game
            return Text;
        }

        //Required for Plugin - 
        //Parameters:
        //              string Text:  That DIRECT text comes from the game (non-"xml")
        //Return Value:
        //              string: Text that will be sent to the main window
        public string ParseText(string Text)
        {
            //check to see if tracker is paused or not.  If paused, just return the text back to Genie
            if (_enabled == false)
                 return Text;
            
            //Try/Catch used incase exception thrown, keeps plugin from being unloaded.
            try
            {
                if (_host != null)
                {
                    if (_parsing == true)
                    {
                        //Parsing of Plain text EXP is done at this point.
                        if (Text.StartsWith("EXP HELP for more information"))
                        {
                            _parsing = false;
                            //The following are set up to only modify the ExpTracker.Sleeping Variable IF it needs to be changed, 
                            //since it forces a variable save
                            if (_sleeping == true && _host.get_Variable("TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "1")
                            {
                                _host.set_Variable("ExpTracker.Sleeping", "1");
                                _host.SendText("#var save");
                            }
                            else if (_sleeping == false && _host.get_Variable("TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "0")
                            {
                                _host.set_Variable("ExpTracker.Sleeping", "0");
                                _host.SendText("#var save");
                            }
                            //once parsing is done, update the Experience Window
                            ShowExperience();
                        }
                        //If the line contains a % sign, it's a line with Experience info on it
                        else if (Text.Contains("%"))
                        {
                            //Format of EXP strings:
                            //  Power Perceive:    142 71% examining     (13/34)        Swimming:     93 61% scrutinizing  (17/34)
                            int i = Text.IndexOf("%");
                            string part = Text.Substring(0, i + 15).Trim();
                            ParseExperience(part, 0);
                            part = Text.Substring(i + 23).Trim();
                            if (part.Contains("%"))
                            {
                                i = part.Contains("(") ? part.IndexOf("(") : part.Length;
                                part = part.Substring(0, i);
                                ParseExperience(part, 0);
                            }
                        }
                        //Parse the number of TDPs
                        else if (Text.StartsWith("Time Development Points:"))
                        {
                            _TDP = Convert.ToInt32(Text.Substring(24, Text.IndexOf("Favors") - 24).Trim());
                            if (_startTDP == 0)
                                _startTDP = _TDP;
                        }
                        //string for sleeping
                        else if (Text.StartsWith("You are relaxed and your mind has entered a state of rest.") )
                            _sleeping = true;
                    }
                    //Signals the start of the Experience command response
                    else if (Text.StartsWith("Circle: "))
                    {
                        _parsing = true;
                        //Assume not sleeping, since there is no string when you're not.
                        _sleeping = false;
                    }
                    
                    //Following two the response strings for when you sleeping/awake
                    if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "0" && (Text.StartsWith("You relax and allow your mind to enter a state of rest.") || Text.StartsWith("You are already resting your mind!")) )
                    {
                        _host.set_Variable("ExpTracker.Sleeping", "1");
                        _host.SendText("#var save");
                        _updateExp = true;
                    }
                    else if ( _host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "1" && (Text.StartsWith("You awaken from your reverie and begin to take in") || Text.StartsWith("But you are not sleeping!")) )
                    {
                        _host.set_Variable("ExpTracker.Sleeping", "0");
                        _host.SendText("#var save");
                        _updateExp = true;
                    }

                    //gets mindstate, since there is no XML for that.
                    if (Text.StartsWith("Overall state of mind:"))
                        ParseMindState(Text.Substring(Text.IndexOf(":") + 1).Trim());
                    else if (_parsing && _host.get_Variable("ExpTracker.GagExp") == "1")
                        Text = "";
                }
            }
            catch (Exception ex)
            {
                _host.SendText("#echo >Debug \" " + ex.ToString() + "\"");
            }
            return Text;
        }

        //Required for Plugin - 
        //Parameters:
        //              string Text:  That "xml" text comes from the game
        public void ParseXML(string XML)
        {
            if (_enabled == false)
                 return;

            if (XML.Contains("prompt"))
            {

                if (_updateExp == true)
                {
                    ShowExperience();
                    _updateExp = false;
                }
            }

            if (XML.Contains("component") && XML.Contains("exp "))
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml("<doc>" + XML + "</doc>");

                XmlNodeList xnl = doc.ChildNodes.Item(0).ChildNodes;
                for (int i = 0; i < xnl.Count; i++)
                {
                    XmlNode xn = xnl.Item(i);

                    switch (xn.Name)
                    {
                        case "component":
                            _updateExp = true;
                            if (XML.Contains("whisper") || XML.Contains("<b>"))
                            {
                                XmlNode xn2 = xn.ChildNodes.Item(0);
                                if (xn2.InnerText == "")
                                {
                                    ParseClear(xn2.Attributes.GetNamedItem("id").Value.Substring(4));
                                }
                                else
                                {
                                    if (XML.Contains("<b>"))
                                        ParseExperience(xn2.InnerText.Trim(), 2);
                                    else
                                        ParseExperience(xn2.InnerText.Trim(), 1);
                                }
                            }
                            else
                            {
                                if (xn.InnerText == "")
                                {
                                    ParseClear(xn.Attributes.GetNamedItem("id").Value.Substring(4));
                                }
                                else
                                {
                                    ParseExperience(xn.InnerText.Trim(), 0);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //Required for Plugin - Opens the settings window for the plugin
        public void Show()
        {
            OpenSettingsWindow(_host.ParentForm);
        }

        public void VariableChanged(string Variable)
        {

        }

        public string Version
        {
            get { return _VERSION; }
        }

        public void ParentClosing()
        {
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }

        }

        public void OpenSettingsWindow(System.Windows.Forms.Form parent)
        {
            Form1 form = new Form1(ref _host);

            form.txtNormal.Text = _host.get_Variable("ExpTracker.Color.Normal");
            form.txtRankGained.Text = _host.get_Variable("ExpTracker.Color.RankGained");
            form.txtLearned.Text = _host.get_Variable("ExpTracker.Color.Learned");

            if (_host.get_Variable("ExpTracker.GagExp") == "1")
                form.cbGagExp.Checked = true;
            else
                form.cbGagExp.Checked = false;

            if (_host.get_Variable("ExpTracker.ShortNames") == "1")
                form.cbShort.Checked = true;
            else
                form.cbShort.Checked = false;

            if (_host.get_Variable("ExpTracker.SortType") == "1")
                form.comboSort.Text = "Left to Right";
            else if (_host.get_Variable("ExpTracker.SortType") == "2")
                form.comboSort.Text = "Learning Rate";
            else if (_host.get_Variable("ExpTracker.SortType") == "3")
                form.comboSort.Text = "Learning Rate Reverse";
            else
                form.comboSort.Text = "A to Z";
            
            form.txtEcho.Text = _host.get_Variable("ExpTracker.Echo");

            if (_host.get_Variable("ExpTracker.EchoSleep") == "1")
                form.cbEchoSleep.Checked = true;
            else
                form.cbEchoSleep.Checked = false;

            if (_host.get_Variable("ExpTracker.TrackSleep") == "1")
                form.cbTrackSleep.Checked = true;
            else
                form.cbTrackSleep.Checked = false;

            if (_host.get_Variable("ExpTracker.LearningRateNumber") == "1")
                form.cbLearningRateNumber.Checked = true;
            else
                form.cbLearningRateNumber.Checked = false;

            if (_host.get_Variable("ExpTracker.LearningRate") == "1")
                form.cbLearningRate.Checked = true;
            else
                form.cbLearningRate.Checked = false;

            if (_host.get_Variable("ExpTracker.ShowRankGain") == "1")
                form.cbRankGain.Checked = true;
            else
                form.cbRankGain.Checked = false;

            if (_host.get_Variable("ExpTracker.Window") == "1")
                form.cbEnable.Checked = true;
            else
                form.cbEnable.Checked = false;

            if (parent != null)
                form.MdiParent = parent;

            form.Show();
        }

        #endregion

        #region Custom Parse/Display methods
        private void ParseMindState(string text)
        {
            int mindState = 0;
            _mindState = text;
            if ( MasterMindState.Contains(_mindState) )
                mindState = (int) MasterMindState[_mindState];
            
            _host.set_Variable("MindState", mindState.ToString());
        }

        private int GetLearningRateInt(string skillRate)
        {
            if ( MasterLearnRate.Contains(skillRate) )
                return (int)MasterLearnRate[skillRate];
            else
                return -1;
        }

        private void ParseExperience(string line, int type)
        {
            if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            if (line.StartsWith("Mind State"))
            {
                ParseMindState(line.Substring(12).Trim().ToLower());
                return;
            }

            string name = "";

            int i = line.IndexOf(":");
            if (i == -1) return;
            name = line.Substring(0, i).Trim();

            // Skip lines with broke names - Conny
            if (name.Contains("(")) return;

            int j = line.IndexOf("%");
            if (j == -1) return;
            string learningRate = "";
            if (line.Contains("("))
                learningRate = line.Substring(j + 1, line.IndexOf("(") - j - 1).Trim();
            else
                learningRate = line.Substring(j + 1).Trim();

            string rank = line.Substring(i + 1, j - i - 1).Trim();



            rank = rank.Replace(" ", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            int k = rank.IndexOf(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (k > -1)
            {
                if (rank.Substring(k + 1).Length == 3)
                {
                    rank = rank.Substring(0, k + 1) + rank.Substring(k + 2);
                }
            }

            double dRank = Double.Parse(rank);

            int SortLR = 0;
            string ShortName = "";
            if (MasterSkill.Contains(name))
            {
                SortLR = ((ItemMasterSkill)MasterSkill[name]).SortLR;
                ShortName = ((ItemMasterSkill)MasterSkill[name]).ShortName;
                if (ShortName == "Magic")
                    name = "Primary Magic";
            }
            else
            {
                SortLR = 500;
                ShortName = "ERR!";
            }

            Skill skill;

            if (_skillList.ContainsKey(name))
            {
                skill = (Skill)_skillList[name];
                skill.learningRate = learningRate;
                skill.iLearningRate = GetLearningRateInt(learningRate);
                skill.rank = dRank;
                if (skill.startRank == 0)
                    skill.startRank = dRank;
                skill.learned = false;
                skill.rankGained = false;
                if (type == 1)
                    skill.learned = true;
                if (type == 2)
                    skill.rankGained = true;
                skill.sortLR = SortLR;
                skill.shortname = ShortName;
                _skillList[name] = skill;
            }
            else
            {
                skill = new Skill
                {
                    learningRate = learningRate,
                    iLearningRate = GetLearningRateInt(learningRate),
                    rank = dRank,
                    startRank = dRank,
                    sortLR = SortLR,
                    shortname = ShortName
                };
                if (type == 1)
                    skill.learned = true;
                if (type == 2)
                    skill.rankGained = true;
                _skillList.Add(name, skill);
            }

            _host.set_Variable(name.Replace(" ", "_") + ".LearningRate", GetLearningRateInt(learningRate).ToString());
            _host.set_Variable(name.Replace(" ", "_") + ".Ranks", dRank.ToString());

        }

        private void ParseClear(string name)
        {
            if( name.EndsWith("Magic") && !name.StartsWith("Targeted") )
                name = "Primary Magic";

            Skill skill = new Skill();
            if (_skillList.ContainsKey(name))
            {
                skill = (Skill)_skillList[name];
                skill.learningRate = "clear";
                _skillList[name] = skill;
            }
            else
                _skillList.Add(name, skill);

            _host.set_Variable(name.Replace(" ", "_") + ".LearningRate", "0");
        }

        public class MyComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return Comparer.Default.Compare(((Sortskill)x).name.ToString(), ((Sortskill)y).name.ToString());
            }
        }
        public class MyComparerLR : IComparer
        {
            public int Compare(object x, object y)
            {
                return Comparer.Default.Compare(((Sortskill)x).sortLR, ((Sortskill)y).sortLR);
            }
        }
        public class MyComparerLearning : IComparer
        {
            public int Compare(object x, object y)
            {
                return Comparer.Default.Compare(((Sortskill)y).sortLearning, ((Sortskill)x).sortLearning);
            }
        }
        public class MyComparerLearningRev : IComparer
        {
            public int Compare(object x, object y)
            {
                return Comparer.Default.Compare(((Sortskill)x).sortLearning, ((Sortskill)y).sortLearning);
            }
        }
        private void ShowExperience()
        {
            if (_host != null)
            {
                //If ExpTracker Window is enabled
                if (_host.get_Variable("ExpTracker.Window") == "1")
                {
                    //list for sorting
                    ArrayList sortList = new ArrayList();

                    //iterate through the list of all skills
                    foreach (DictionaryEntry sk in _skillList)
                    {
                        Skill skill = (Skill)sk.Value;
                        //for those that aren't at "clear" learning rate (0/34 )
                        if (skill.learningRate != "clear")
                        {
                            //add the skill + sort types to the list of items to be sorted
                            Sortskill sortSkill = new Sortskill
                            {
                                name = sk.Key.ToString(),
                                sortLR = ((Skill)sk.Value).sortLR,
                                sortLearning = ((Skill)sk.Value).iLearningRate
                            };
                            sortList.Add(sortSkill);
                        }
                    }
                    //Sort based on type of sort.  
                    //1: Reading sort 
                    if (_host.get_Variable("ExpTracker.SortType") == "1")
                        sortList.Sort(new MyComparerLR());
                    //2: By Learning Rate
                    else if (_host.get_Variable("ExpTracker.SortType") == "2")
                        sortList.Sort(new MyComparerLearning());
                    //3: By Learning Rate Reverse
                    else if (_host.get_Variable("ExpTracker.SortType") == "3")
                        sortList.Sort(new MyComparerLearningRev());
                    //0/Default: Alphabetical
                    else
                        sortList.Sort(new MyComparer());

                    //>=0 since if >0, when the last skill pulses to clear, 
                    //Exp window won'te update
                    if (sortList.Count >= 0)
                    {
                        //suspsend so it behaves as an update, not as a scrolling window
                        _host.SendText("#echo >Experience @suspend@");

                        //only echo the items that are in the sortList (non-clear learning ratE)
                        foreach (Sortskill item in sortList)
                        {
                            string output = "";
                           
                            //get the skill info from the hash table
                            Skill skill = (Skill)_skillList[item.name];
                           
                            //Outputs name of skill (short or normal) & ranks
                            if( _host.get_Variable("ExpTracker.ShortNames") == "1")
                                output = String.Format("{0,7:G}:{1,9}", skill.shortname, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                            else
                                output = String.Format("{0,15:G}:{1,9}", item.name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");

                            //Outputs Learning Rate NAME if set to
                            if (_host.get_Variable("ExpTracker.LearningRate") == "1")
                                output += String.Format(" {0,-13}", skill.learningRate);
                            
                            //Outputs Learning Rate NUMBER if set to
                            if (_host.get_Variable("ExpTracker.LearningRateNumber") == "1")
                                output += String.Format("{0,8}", "(" + skill.iLearningRate + "/34)");

                            //Calculate Skill gain
                            double rankGain = skill.rank - skill.startRank;

                            //Outputs Skill gain if set to 
                            if (_host.get_Variable("ExpTracker.ShowRankGain") == "1")
                                output += " +" + String.Format("{0:0.00}", rankGain);

                            //Used for #echo >Window Options "   Text "
                            //to preserve white space
                            output = " \"" + output + "\"";

                            //set color of text if ranked, or learned, or normal
                            if (skill.rankGained == true)
                                output = _host.get_Variable("ExpTracker.Color.RankGained") + output;
                            else if (skill.learned == true)
                                output = _host.get_Variable("ExpTracker.Color.Learned") + output;
                            else
                                output = _host.get_Variable("ExpTracker.Color.Normal") + output;

                            _host.SendText("#echo >Experience " + output);
                        }

                        //tdp and sleep are blank in case there is no info for them to output
                        string tdp = "";
                        string asleep = "";
                        
                        if (_startTDP < _TDP)
                            tdp = ";#echo >Experience TDP gained: " + (_TDP - _startTDP);
                        if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.EchoSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "1")
                            if (_host.get_Variable("ExpTracker.Echo") == "")
                                asleep = ";#echo >Experience YOU ARE ASLEEP!";
                            else
                                asleep = ";#echo >Experience " + _host.get_Variable("ExpTracker.Echo");
                        
                        TimeSpan oTimeSpan = DateTime.Now - _startTime;
                        
                        //Short or long version of mindstate
                        string mindstatetext="";
                        if (_host.get_Variable("ExpTracker.ShortNames") == "1")
                            mindstatetext = "Mindstate: ";
                        else
                            mindstatetext = "Overall state of mind: ";
                        
                        _host.SendText("#echo >Experience "+ mindstatetext + _mindState + asleep + tdp + ";#echo >Experience Last updated: $time;#echo >Experience Tracking for: " + FormatTimeSpan(oTimeSpan));
                        //resume so the updates actually show
                        _host.SendText("#echo >Experience @resume@");
                    }
                }
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            string txt = "";

            if (_host.get_Variable("ExpTracker.ShortNames") == "1")
            {
                //Format: [D.]HH:MM:SS
                if (ts.Days > 0)
                    txt += ts.Days.ToString() + ".";
                txt += ts.Hours.ToString() + ":";
                if (ts.Minutes < 10)
                    txt += "0";
                txt += ts.Minutes.ToString() + ":";
                if (ts.Seconds < 10)
                    txt += "0";
                txt += ts.Seconds.ToString();
            }
            else
            {
                //Format [D days, ][ hh hours, mm minutes][ss seconds]
                if (ts.Days > 0)
                {
                    txt += ts.Days.ToString() + " days";
                    ts = ts.Subtract(new TimeSpan(ts.Days, 0, 0, 0));
                }
                if (ts.Hours > 0)
                {
                    if (txt.Length > 0)
                        txt += ", ";
                    txt += ts.Hours.ToString() + " hours";
                    ts = ts.Subtract(new TimeSpan(0, ts.Hours, 0, 0));
                }
                if (ts.Minutes > 0)
                {
                    if (txt.Length > 0)
                        txt += ", ";
                    txt += ts.Minutes.ToString() + " minutes";
                    ts = ts.Subtract(new TimeSpan(0, 0, ts.Minutes, 0));
                }
                if (txt.Length == 0) // Show seconds
                {
                    if (ts.Seconds > 0)
                        txt = ts.Seconds.ToString() + " seconds";
                }
            }

            return txt;
        }
        #endregion

    }
}
