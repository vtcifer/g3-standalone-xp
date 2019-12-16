using System;
using System.Collections;
using GeniePlugin.Interfaces;
using System.Xml;
using System.Text.RegularExpressions;


namespace EXPTracker
{
    public class EXPTracker : IPlugin
    {
        //Constant variable for the Properties of the plugin
        //At the top for easy changes.
        readonly string _NAME = "EXPTracker";
        readonly string _VERSION = "3.3.25";
        readonly string _AUTHOR = "VTCifer";
        readonly string _DESCRIPTION = "Parses the XML output of skills in DragonRealms to create skill named global variables and emmulate the experience window of the StormFront Front End.";

        public IHost _host;                             //Required for plugin
        public System.Windows.Forms.Form _parent;       //Required for plugin

        #region EXPTracker Members

        private const int MAX_SKILL = 69;                       //Total of 69 skills in game
        private const int MAX_LEARNRATE = 35;                   //Max Learning rates: 0 - 34

        private DateTime _startTime;                    //Used for TDP/Rank tracking to know how long tracking since
        private Hashtable _skillList = new Hashtable(); //Used for storing/sorting skills for display in Exp Win
        private int _TDP = 0;                           //Used for TDP tracking, this is current TDPs
        private int _startTDP = 0;                      //Used for TDP tracking, this is set when first checking
        private int pulseSeed = -1;                     //Used for adjusting what the 'start' pulse was
        private int _RestedEXPStored = -1;              //Used for tracking rested experience.
        private int _RestedExpUsable = -1;              //Used for tracking rested experience.
        private bool _trackingTDP = false;              //Used for TDP tracking, this it to know when the plugin has gathered TDP info
        private bool _updateExp = false;                //Used for know when next prompt is shown, to update EXPWindow
        private bool _parsing = false;                  //Used for ParseText, to know if EXP command output is returned
        private int _sleeping = -1;                     //Used for tracking if you are sleeping or not
        private bool _report = false;                   //Used for ParseText, to know if you are running a report
        private bool _ExpBrief = false;
        private bool _enabled = true;                   //Used for "Pausing" the tracker, so no new data is input.  Also used to disable from
                                                        //Plugins Window
        private string EchoLearned = "";
        private string EchoPulsed = "";

        //The following hashtables are used as alternatives to switch statments using string data which can have rather bad run time.  
        private Hashtable MasterSkill;
        private class ItemMasterSkill
        {
            public int SortLR;
            public string ShortName;
        }

        private Hashtable MasterLearnRate;

        //Class Skill
        //Used for storing all skill related info
        //Used in a hashtable whose key is the name of the skill
        private class Skill
        {
            public double rank = 0;                 //Rank of the skill
            public string learningRate = "clear";   //Text Learning rate
            public int iLearningRate = 0;           //Numerical Learning rate
            public double startRank = -1;           //When Rank tracking enabled, this is starting rank calculated for
            public bool rankGained = false;         //Used for displaying text in a specific color when rank gained - currently broken from DR XML
            public bool learned = false;            //Used for displaying text in a specific color when bits are added to pool
            public int sortLR = 0;                  //Used for sorting As reading, Left to Right
            public string shortname = "";           //Used for short name display instead of long name
            public string output = "";              //Used for output of skill info -> Exp window and tracking
            public string parseoutput = "";         //Used for output using #parse of skill info -> only used in tracking
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

        #region IPlugin Properties

        //Required for Plugin - Called when Genie needs the name of the plugin (On menu)
        //Return Value:
        //              string: Text that is the name of the Plugin
        public string Name
        {
            get { return _NAME; }
        }

        //Required for Plugin - Called when Genie needs the plugin version (error text
        //                      or the plugins window)
        //Return Value:
        //              string: Text that is the version of the plugin
        public string Version
        {
            get { return _VERSION; }
        }

        //Required for Plugin - Called when Genie needs the plugin Author (plugins window)
        //Return Value:
        //              string: Text that is the Author of the plugin
        public string Author
        {
            get { return _AUTHOR; }
        }

        //Required for Plugin - Called when Genie needs the plugin Description (plugins window)
        //Return Value:
        //              string: Text that is the description of the plugin
        //                      This can only be up to 200 Characters long, else it will appear
        //                      "truncated"
        public string Description
        {
            get { return _DESCRIPTION; }
        }

        //Required for Plugin - Called when Genie needs disable/enable the plugin (Plugins window,
        //                      and from the CLI), or when Genie needs to know the status of the 
        //                      plugin (???)
        //Get:
        //      Not Known what it is used for
        //Set:
        //      Used by Plugins Window + CLI
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
            _skillList = new Hashtable(MAX_SKILL);
            
            //Set Genie Variables if not already set
            init_variables();
           
            //create AND popluate the master hashtables
            //This in theory should front load processing time for when Genie loads
            //and speed up the time it takes to parse, and generate EXP window data.
            
            //
            MasterSkill = new Hashtable(MAX_SKILL);
            //Armor skills - 7
            MasterSkill.Add("Shield Usage", new ItemMasterSkill { SortLR = 0, ShortName = "Shield" });
            MasterSkill.Add("Light Armor", new ItemMasterSkill { SortLR = 1, ShortName = "Lt Armor" });
            MasterSkill.Add("Chain Armor", new ItemMasterSkill { SortLR = 2, ShortName = "Chain" });
            MasterSkill.Add("Brigandine", new ItemMasterSkill { SortLR = 3, ShortName = "Brigan" });
            MasterSkill.Add("Plate Armor", new ItemMasterSkill { SortLR = 4, ShortName = "Plate" });
            MasterSkill.Add("Defending", new ItemMasterSkill { SortLR = 5, ShortName = "Defend" });
            MasterSkill.Add("Conviction", new ItemMasterSkill { SortLR = 6, ShortName = "Convict" });
            
            //Weapon Skills - 19
            MasterSkill.Add("Parry Ability", new ItemMasterSkill { SortLR = 100, ShortName = "Parry" });
            MasterSkill.Add("Small Edged", new ItemMasterSkill { SortLR = 101, ShortName = "SE" });
            MasterSkill.Add("Large Edged", new ItemMasterSkill { SortLR = 102, ShortName = "LE" });
            MasterSkill.Add("Twohanded Edged", new ItemMasterSkill { SortLR = 103, ShortName = "2HE" });
            MasterSkill.Add("Small Blunt", new ItemMasterSkill { SortLR = 104, ShortName = "SB" });
            MasterSkill.Add("Large Blunt", new ItemMasterSkill { SortLR = 105, ShortName = "LB" });
            MasterSkill.Add("Twohanded Blunt", new ItemMasterSkill { SortLR = 106, ShortName = "2HB" });
            MasterSkill.Add("Slings", new ItemMasterSkill { SortLR = 107, ShortName = "Sling" });
            MasterSkill.Add("Bow", new ItemMasterSkill { SortLR = 108, ShortName = "Bow" });
            MasterSkill.Add("Crossbow", new ItemMasterSkill { SortLR = 109, ShortName = "XBow" });
            MasterSkill.Add("Staves", new ItemMasterSkill { SortLR = 110, ShortName = "Staves" });
            MasterSkill.Add("Polearms", new ItemMasterSkill { SortLR = 111, ShortName = "Polearm" });
            MasterSkill.Add("Light Thrown", new ItemMasterSkill { SortLR = 112, ShortName = "LT" });
            MasterSkill.Add("Heavy Thrown", new ItemMasterSkill { SortLR = 113, ShortName = "HT" });
            MasterSkill.Add("Brawling", new ItemMasterSkill { SortLR = 114, ShortName = "Brawl" });
            MasterSkill.Add("Offhand Weapon", new ItemMasterSkill { SortLR = 115, ShortName = "Offhand" });
            MasterSkill.Add("Melee Mastery", new ItemMasterSkill { SortLR = 116, ShortName = "Melee" });
            MasterSkill.Add("Missile Mastery", new ItemMasterSkill { SortLR = 117, ShortName = "Missile" });
            MasterSkill.Add("Expertise", new ItemMasterSkill { SortLR = 118, ShortName = "Expert" });
     
            //Magic Skills - 18
            MasterSkill.Add("Arcane Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Elemental Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Holy Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Inner Fire", new ItemMasterSkill { SortLR = 200, ShortName = "IF" });
            MasterSkill.Add("Inner Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Life Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Lunar Magic", new ItemMasterSkill { SortLR = 200, ShortName = "Magic" });
            MasterSkill.Add("Attunement", new ItemMasterSkill { SortLR = 201, ShortName = "Attune" });
            MasterSkill.Add("Arcana", new ItemMasterSkill { SortLR = 202, ShortName = "Arcana" });
            MasterSkill.Add("Targeted Magic", new ItemMasterSkill { SortLR = 203, ShortName = "TM" });
            MasterSkill.Add("Augmentation", new ItemMasterSkill { SortLR = 204, ShortName = "Augment" });
            MasterSkill.Add("Debilitation", new ItemMasterSkill { SortLR = 205, ShortName = "Debilit" });
            MasterSkill.Add("Utility", new ItemMasterSkill { SortLR = 206, ShortName = "Utility" });
            MasterSkill.Add("Warding", new ItemMasterSkill { SortLR = 207, ShortName = "Warding" });
            MasterSkill.Add("Sorcery", new ItemMasterSkill { SortLR = 208, ShortName = "Sorcery" });
            MasterSkill.Add("Astrology", new ItemMasterSkill { SortLR = 209, ShortName = "Astro" });
            MasterSkill.Add("Summoning", new ItemMasterSkill { SortLR = 209, ShortName = "Summon" });
            MasterSkill.Add("Theurgy", new ItemMasterSkill { SortLR = 209, ShortName = "Theurgy" });
 
            //Survival Skills - 12
            MasterSkill.Add("Evasion", new ItemMasterSkill { SortLR = 300, ShortName = "Evade" });
            MasterSkill.Add("Athletics", new ItemMasterSkill { SortLR = 301, ShortName = "Athletic" });
            MasterSkill.Add("Perception", new ItemMasterSkill { SortLR = 302, ShortName = "Percep" });
            MasterSkill.Add("Stealth", new ItemMasterSkill { SortLR = 303, ShortName = "Stealth" });
            MasterSkill.Add("Locksmithing", new ItemMasterSkill { SortLR = 304, ShortName = "Locks" });
            MasterSkill.Add("Thievery", new ItemMasterSkill { SortLR = 305, ShortName = "Thievery" });
            MasterSkill.Add("First Aid", new ItemMasterSkill { SortLR = 306, ShortName = "FA" });
            MasterSkill.Add("Outdoorsmanship", new ItemMasterSkill { SortLR = 307, ShortName = "Outdoor" });
            MasterSkill.Add("Skinning", new ItemMasterSkill { SortLR = 308, ShortName = "Skin" });
            MasterSkill.Add("Backstab", new ItemMasterSkill { SortLR = 309, ShortName = "BS" });
            MasterSkill.Add("Scouting", new ItemMasterSkill { SortLR = 309, ShortName = "Scout" });
            MasterSkill.Add("Thanatology", new ItemMasterSkill { SortLR = 309, ShortName = "Than" });
            
            //Lore Skills - 13
            MasterSkill.Add("Forging", new ItemMasterSkill { SortLR = 401, ShortName = "Forging" });
            MasterSkill.Add("Engineering", new ItemMasterSkill { SortLR = 402, ShortName = "Engineer" });
            MasterSkill.Add("Outfitting", new ItemMasterSkill { SortLR = 403, ShortName = "Outfit" });
            MasterSkill.Add("Alchemy", new ItemMasterSkill { SortLR = 404, ShortName = "Alchemy" });
            MasterSkill.Add("Enchanting", new ItemMasterSkill { SortLR = 405, ShortName = "Enchant" });
            MasterSkill.Add("Scholarship", new ItemMasterSkill { SortLR = 406, ShortName = "Scholar" });
            MasterSkill.Add("Mechanical Lore", new ItemMasterSkill { SortLR = 407, ShortName = "Mech" });
            MasterSkill.Add("Appraisal", new ItemMasterSkill { SortLR = 408, ShortName = "App" });
            MasterSkill.Add("Performance", new ItemMasterSkill { SortLR = 409, ShortName = "Perform" });
            MasterSkill.Add("Tactics", new ItemMasterSkill { SortLR = 410, ShortName = "Tactics" });
            MasterSkill.Add("Bardic Lore", new ItemMasterSkill { SortLR = 410, ShortName = "BardLore" });
            MasterSkill.Add("Empathy", new ItemMasterSkill { SortLR = 410, ShortName = "Empathy" });
            MasterSkill.Add("Trading", new ItemMasterSkill { SortLR = 410, ShortName = "Trading" });

            MasterLearnRate = new Hashtable(MAX_LEARNRATE);
            MasterLearnRate.Add("clear", 0);
            MasterLearnRate.Add("dabbling", 1);
            MasterLearnRate.Add("perusing", 2);
            MasterLearnRate.Add("learning", 3);
            MasterLearnRate.Add("thoughtful", 4);
            MasterLearnRate.Add("thinking", 5);
            MasterLearnRate.Add("considering", 6);
            MasterLearnRate.Add("pondering", 7);
            MasterLearnRate.Add("ruminating", 8);
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

        //Required for Plugin - Called when user enters text in the command box
        //Parameters:
        //              string Text:  The text the user entered in the command box
        //Return Value:
        //              string: Text that will be sent to the game
        public string ParseInput(string Text)
        {
            if (Text.StartsWith("/track ") || Text.StartsWith("/trackr") || Text.Equals("/track"))
            {
                //Reset all tracking, to current value for skills/TDPS
                if (Text == "/trackreset" || Text == "/track reset")
                {
                    ResetTracking();
                    return "";
                }
                //Resets all tracking to 0, as if Genie just launched
                else if (Text == "/track clear")
                {
                    ClearTracking();
                    return "";
                }
                else if (Text == "/track tdp reset")
                {
                    ResetTDP();
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
                else if (Text == "/track report")
                {
                    _report = true;
                    return "exp all";
                }

                //User asking for help with commands, or invalid command entered
                else
                {
                    _host.SendText("#echo");
                    _host.SendText(@"#echo Standlone EXPTracker (Ver:" + _VERSION + ") Usage:");
                    _host.SendText(@"#echo /track reset");
                    _host.SendText(@"#echo """"    """" Used to reset tracking");
                    _host.SendText(@"#echo /track tdp reset");
                    _host.SendText(@"#echo """"    """" Used to reset tdp tracking only");
                    _host.SendText(@"#echo /track clear");
                    _host.SendText(@"#echo """"    """" Used to reset as if you just started Genie");
                    _host.SendText(@"#echo /track pause");
                    _host.SendText(@"#echo """"    """" Used to pause Exp Tracker from tracking ANY Exp Changes");
                    _host.SendText(@"#echo /track resume");
                    _host.SendText(@"#echo """"    """" Used to resume Exp Tracker");
                    _host.SendText(@"#echo /track report");
                    _host.SendText(@"#echo """"    """" Produce a report ");

                    return "";
                }
            }
            //means no special arguments, send command on to game
            return Text;
        }

        //Required for Plugin - 
        //Parameters:
        //              string Text:    The DIRECT text comes from the game (non-"xml")
        //              string Window:  The Window the Text was received from
        //Return Value:
        //              string: Text that will be sent to the main window
        public string ParseText(string Text, string Window)
        {
            //check to see if tracker is paused or not.  If paused, just return the text back to Genie
            if (!_enabled)
                return Text;

            //Try/Catch used incase exception thrown, keeps plugin from being unloaded on bad data.
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
                            if (_sleeping == 1 && _host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "1")
                            {
                                _host.SendText("#var ExpTracker.Sleeping 1");
                                _host.SendText("#var save");
                            }
                            else if (_sleeping == 2 && _host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "2")
                            {
                                _host.SendText("#var ExpTracker.Sleeping 2");
                                _host.SendText("#var save");
                            }
                            else if (_sleeping == 0 && _host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "0")
                            {
                                _host.SendText("#var ExpTracker.Sleeping 0");
                                _host.SendText("#var save");
                            }

                            //once parsing is done, update the Experience Window
                            ShowExperience();
                        }
                        //If the line contains a % sign, it's a line with Experience info on it
                        else if (Text.Contains("%"))
                        {
                            //Format of EXP strings, 2 per line for most, possibility for 1 for line
                            //  Power Perceive:    142 71% examining     (13/34)        Swimming:     93 61% scrutinizing  (17/34)
                            //          Arcana:    194 23% thoughtful     (4/34)

                            //Get index of the first %, and grab 15 characters after that, since that is the length of
                            //the learning rate text.  Send it to PArseExperience (assuming, and clear learned/ranked)
                            int i = Text.IndexOf("%");
                            string part = Text.Substring(0, i + 15).Trim();
                            ParseExperience(part, 0);
                            //Clear the just processed info from the string, and check for another set of experience data
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
                            if (_trackingTDP == false)
                            {
                                _startTDP = _TDP;
                                _trackingTDP = true;
                            }
                        }
                        //Get details of rested exp.  Stored, Usable, reset
                        else if (Text.StartsWith("Rested EXP Stored: "))
                        {
                            //get details of rested exp
                            //Rested EXP Stored: 5:50 hours Usable This Cycle: 5:27 hours Cycle Refreshes: 20:36 hours
                            //Rested EXP Stored: 1:23 hour Usable This Cycle: 59 minutes Cycle Refreshes: 13:47 hours
                            //Rested EXP Stored: 24 minutes  Usable This Cycle: 1 minute  Cycle Refreshes: 12:43 hours

                        }
                        //string for sleeping
                        else if (Text.StartsWith("You are relaxed and your mind has entered a light state of rest."))
                            _sleeping = 1;
                        else if (Text.StartsWith("You are relaxed and your mind has entered a deep state of rest."))
                            _sleeping = 2;

                    }
                    //Signals the start of the Experience command response
                    else if (Text.StartsWith("Circle: "))
                    {
                        _parsing = true;
                        //Assume not sleeping, since there is no string when you're not.
                        _sleeping = 0;
                    }
                    //Report has been finished generated (String is last line of exp output)
                    if (_report == true && Text.StartsWith("EXP HELP for more information"))
                    {
                        _report = false;
                        DisplayReport();
                    }

                    //Following response strings are for when you sleeping/awake using sleep/awake commands
                    if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "1" && Text.StartsWith("You relax and allow your mind to enter a state of rest") )
                    {
                        _host.SendText("#var ExpTracker.Sleeping 1");
                        _host.SendText("#var save");
                        _updateExp = true;
                    }
                    else if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "2" && Text.StartsWith("You draw deeper into rest, trying to destress"))
                    {
                        _host.SendText("#var ExpTracker.Sleeping 2");
                        _host.SendText("#var save");
                        _updateExp = true;

                    }
                    else if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "0" && (Text.StartsWith("You awaken from your reverie and begin to take in the world around you") || Text.StartsWith("You stir yourself from the depths of relaxation and prepare for another day") || Text.StartsWith("But you are not sleeping!")))
                    {
                        _host.SendText("#var ExpTracker.Sleeping 0");
                        _host.SendText("#var save");
                        _updateExp = true;
                    }

                    if (_parsing && ((_host.get_Variable("ExpTracker.GagExp") == "1") || (_report)) )
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
            //check to see if tracker is paused or not.  If paused, just return the text back to Genie
            if (_enabled == false)
                return;
            
            //trigger output of updates with XML prompt
            if (XML.Contains("prompt"))
            {
                //If it was detected that the exp brief toggle is used, turn it off and clear any garbage data in the plugin
                if (_ExpBrief)
                {
                    _host.SendText("#echo white,red The ExpBrief Toggle is not supported.");
                    _host.SendText("#echo white,red Turning it off and clearing tracking data:");
                    ClearTracking();
                    _host.SendText("FLAG BRIEFEXP OFF");
                    _host.SendText("EXP");
                    _ExpBrief = false;
                }
                
                if (EchoLearned != "" && _host.get_Variable("ExpTracker.EchoExp") == "1")
                {
                    EchoLearned = EchoLearned.Substring(0, EchoLearned.Length - 2);
                    _host.SendText("#echo " + _host.get_Variable("ExpTracker.Color.EchoGained") + " Learned: " + EchoLearned);
                    _host.SendText("#parse Learned: " + EchoLearned);
                    _host.SendText("#log Learned: " + EchoLearned);
                }
                if (EchoLearned != "")
                    EchoLearned = "";

                if (EchoPulsed != "" && _host.get_Variable("ExpTracker.EchoExp") == "1")
                {
                    EchoPulsed = EchoPulsed.Substring(0, EchoPulsed.Length - 2);
                    _host.SendText("#echo " + _host.get_Variable("ExpTracker.Color.EchoPulsed") + " Pulsed: " + EchoPulsed);
                    
                }
                if (EchoPulsed != "")
                    EchoPulsed = "";

                //However, only ouptut the update, if there is an update
                //for the exp window.
                if (_updateExp == true)
                {
                    ShowExperience();
                    _updateExp = false;
                }
            }

            //XML data for Learning skills
            //<component id='exp Light Chain'><preset id='whisper'>     Light Chain:  282 22% mind lock    </preset></component>

            //XML data for pulsed skills
            //<component id='exp Hiding'>          Hiding:  398 33% rapt         </component>

            //XML data for ranked skills (always does it 2x):
            //<component id='exp Utility'><b>         Utility:  160 10% learning     </b></component>
            //<component id='exp Utility'>         Utility:  160 10% learning     </component>

            //XML Data for plused to clear skills
            //<component id='exp Climbing'></component>

            //XML Data for skill when the unsupported ExpBrief Toggle is used:
            //<component id='exp Foraging'><d cmd='skill Foraging'> Forage</d>:  402 65%  [ 9/34]</component>


            //Only care about exp window updates
            if (XML.StartsWith("<component id='exp "))
            {
                //If xml xml output has a bracket, using unsupported Exp Brief toggle, track that to turn off)
                if ( XML.Contains("[") )
                {
                    _ExpBrief = true;
                }

                //setup a new XML doc for easier iterating through nodes
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<doc>" + XML + "</doc>");

                //create a list of the nodes
                XmlNodeList xnl = doc.ChildNodes.Item(0).ChildNodes;
                for (int i = 0; i < xnl.Count; i++)
                {
                    XmlNode xn = xnl.Item(i);

                    switch (xn.Name)
                    {
                        case "component":
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
                                        ParseExperience(xn2.InnerText.Trim(), 2, true);
                                    else
                                        ParseExperience(xn2.InnerText.Trim(), 1, true);
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
                                    ParseExperience(xn.InnerText.Trim(), 0, true);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //Required for Plugin - This method is called when clicking on the plugin 
        //name from the Menu item Plugins
        public void Show()
        {
            OpenSettingsWindow(_host.ParentForm);
        }

        //Required for Plugin - This method is called when a global variable in genie
        //                      is changed
        //Parameters:
        //              string Text:  The variable name in Genie that changed
        public void VariableChanged(string Variable)
        {
            if (Variable.StartsWith("ExpTracker."))
            {
                if ((Variable == "ExpTracker.Sleeping") || (Variable == "ExpTracker.NextPulse")|| (Variable == "ExpTracker.LearningSkills"))
                    return;
                if (Variable == "ExpTracker.Window")
                {
                    if (_host.get_Variable("ExpTracker.Window") == "1")
                    {
                        _host.SendText("#window show Experience");
                    }
                    else if (_host.get_Variable("ExpTracker.Window") == "0")
                    {
                        _host.SendText("#window hide Experience");
                    }
                }
                else if (Variable == "ExpTracker.CountMinMindstate")
                {
                    set_min_mindstae();
                    return;
                }

                if (_host.get_Variable(Variable) == "")
                    init_variables(true, Variable);
            }
/*          Removed this due to needing a way to figure out the 'seed' for when game started. 
            if (Variable == "gametime")
            {

                uint pulseOffset = Convert.ToUInt32(_host.get_Variable("gametime")) % 200;

                if (pulseOffset > 192 || pulseOffset <= 12)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Missile_Mastery|Primary_Magic|Attunement|Arcana|Targeted_Magic|Augmentation")
                        _host.set_Variable("ExpTracker.NextPulse", "Missile_Mastery|Primary_Magic|Attunement|Arcana|Targeted_Magic|Augmentation");
                }
                else if (pulseOffset > 172)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Staves|Polearms|Light_Thrown|Heavy_Thrown|Brawling|Offhand_Weapon|Melee_Mastery")
                        _host.set_Variable("ExpTracker.NextPulse", "Staves|Polearms|Light_Thrown|Heavy_Thrown|Brawling|Offhand_Weapon|Melee_Mastery");
                }
                else if (pulseOffset > 152)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Small_Blunt|Large_Blunt|Twohanded_Blunt|Slings|Bow|Crossbow")
                        _host.set_Variable("ExpTracker.NextPulse", "Small_Blunt|Large_Blunt|Twohanded_Blunt|Slings|Bow|Crossbow");
                }
                else if (pulseOffset > 132)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Parry_Ability|Small_Edged|Large_Edged|Twohanded_Edged")
                        _host.set_Variable("ExpTracker.NextPulse", "Parry_Ability|Small_Edged|Large_Edged|Twohanded_Edged");
                }
                else if (pulseOffset > 112)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Shield_Usage|Light_Armor|Chain_Armor|Brigandine|Plate_Armor|Defending")
                        _host.set_Variable("ExpTracker.NextPulse", "Shield_Usage|Light_Armor|Chain_Armor|Brigandine|Plate_Armor|Defending");
                }
                else if (pulseOffset > 92)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Performance|Tactics|Astrology|Empathy|Thanatology|Expertise|Summoning|Theurgy|Conviction")
                        _host.set_Variable("ExpTracker.NextPulse", "Performance|Tactics|Astrology|Empathy|Thanatology|Expertise|Summoning|Theurgy|Conviction");
                }
                else if (pulseOffset > 72)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Forging|Engineering|Outfitting|Alchemy|Enchanting|Scholarship|Mechanical_Lore|Appraisal|Bardic_Lore|Trading")
                        _host.set_Variable("ExpTracker.NextPulse", "Forging|Engineering|Outfitting|Alchemy|Enchanting|Scholarship|Mechanical_Lore|Appraisal|Bardic_Lore|Trading");
                }
                else if (pulseOffset > 52)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Skinning|Backstab")
                        _host.set_Variable("ExpTracker.NextPulse", "Skinning|Backstab");
                }
                else if (pulseOffset > 32)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Stealth|Locksmithing|Thievery|First_Aid|Outdoorsmanship")
                        _host.set_Variable("ExpTracker.NextPulse", "Stealth|Locksmithing|Thievery|First_Aid|Outdoorsmanship");
                }
                else if (pulseOffset > 12)
                {
                    if (_host.get_Variable("ExpTracker.NextPulse") != "Debilitation|Utility|Warding|Sorcery|Evasion|Athletics|Perception|Scouting")
                        _host.set_Variable("ExpTracker.NextPulse", "Debilitation|Utility|Warding|Sorcery|Evasion|Athletics|Perception|Scouting");
                }
            }
*/
        }

        public void ParentClosing()
        {
        }


        #endregion

        #region Custom Parse/Display methods

        //Opens the settings window.  Called when a user clicks on the menu item for 
        //this plugin (via above call)
        //
        //Parameters:
        //              Form Parent:  The parent form of the plugin.  Genie in this case
        public void OpenSettingsWindow(System.Windows.Forms.Form parent)
        {
            frmEXPTracker form = new frmEXPTracker(ref _host);

            //All the following code reads the ExpTracker Global variables
            //and sets the form to reflect the current setup.

            //Done in reverse order

            form.txtEchoGain.Text = _host.get_Variable("ExpTracker.Color.EchoGained");
            form.txtEchoPulse.Text = _host.get_Variable("ExpTracker.Color.EchoPulsed");
            if (_host.get_Variable("ExpTracker.EchoExp") == "1")
                form.cbEchoExp.Checked = true;
            else
                form.cbEchoExp.Checked = false;

            form.txtLearned.Text = _host.get_Variable("ExpTracker.Color.Learned");
            form.txtRankGained.Text = _host.get_Variable("ExpTracker.Color.RankGained");
            form.txtNormal.Text = _host.get_Variable("ExpTracker.Color.Normal");

            if (_host.get_Variable("ExpTracker.ReportType") == "1")
                form.comboReportSort.Text = "Left to Right";
            else if (_host.get_Variable("ExpTracker.ReportType") == "2")
                form.comboReportSort.Text = "Learning Rate";
            else if (_host.get_Variable("ExpTracker.ReportType") == "3")
                form.comboReportSort.Text = "Learning Rate Reverse";
            else
                form.comboReportSort.Text = "A to Z";

            if (_host.get_Variable("ExpTracker.SortType") == "1")
                form.comboExpSort.Text = "Left to Right";
            else if (_host.get_Variable("ExpTracker.SortType") == "2")
                form.comboExpSort.Text = "Learning Rate";
            else if (_host.get_Variable("ExpTracker.SortType") == "3")
                form.comboExpSort.Text = "Learning Rate Reverse";
            else
                form.comboExpSort.Text = "A to Z";

            if (_host.get_Variable("ExpTracker.Persistent") == "1")
                form.cbPersistent.Checked = true;
            else
                form.cbPersistent.Checked = false;

            form.updownMinMindstate.Value = get_min_mindstate();
            if (_host.get_Variable("ExpTracker.CountSkills") == "1")
                form.cbCountSkills.Checked = true;
            else
                form.cbCountSkills.Checked = false;
            

            if (_host.get_Variable("ExpTracker.ShortNames") == "1")
                form.cbShort.Checked = true;
            else
                form.cbShort.Checked = false;

            if (_host.get_Variable("ExpTracker.GagExp") == "1")
                form.cbGagExp.Checked = true;
            else
                form.cbGagExp.Checked = false;

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
            form.Text = "Experience Tracker v" + _VERSION;
            if (parent != null)
                form.MdiParent = parent;

            form.Show();
        }

        private int GetLearningRateInt(string skillRate)
        {
            if (MasterLearnRate.Contains(skillRate))
                return (int)MasterLearnRate[skillRate];
            else
                return -1;
        }

        //Helper function - called to extract data from experience line
        //Parameters:
        //              string line:    the string containing the exp data (from xml or game output)
        //              int type:       If skill pulsed (0), learned (1) or ranked (2)
        private void ParseExperience(string line, int type, bool IsXML = false)
        {
            //Ensure using decimal point instead of decimal comma or other
            if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }


            //Get skill name, which always precedes the ':'
            int i = line.IndexOf(":");
            if (i == -1) return;
            string name = line.Substring(0, i).Trim();

            // Skip lines with broke names - Conny
            if (name.Contains("(")) return;

            //get learnirng rate, which always is after '%'
            //if no %, nothing to do
            int j = line.IndexOf("%");
            if (j == -1) return;
            string learningRate = "";
            if (line.Contains("("))     //parsed text will contain a (nn/34) after the learning rate
                learningRate = line.Substring(j + 1, line.IndexOf("(") - j - 1).Trim();
            else                        //parsed Xml will not contain a (nn/34), and thus will just have whitespace after the learning rate
                learningRate = line.Substring(j + 1).Trim();
            //rank data will be between the ":" and the "%"
            string rank = line.Substring(i + 1, j - i - 1).Trim();


            //as exp data from game doesn't use decimal point, but rank + %, change the space to decimal
            rank = rank.Replace(" ", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            //I have no idea what this code does...so it's been dummied out for now
            //Leaving in, just in case something unexpected breaks
            /*
            int k = rank.IndexOf(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (k > -1)
            {
                if (rank.Substring(k + 1).Length == 3)
                {
                    rank = rank.Substring(0, k + 1) + rank.Substring(k + 2);
                }
            }
            */
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
                if ((learningRate != skill.learningRate) || (dRank != skill.rank) || (skill.learned == false && type == 1))
                    _updateExp = true;

                skill.learningRate = learningRate;
                int oldLearningRate = skill.iLearningRate;
                skill.iLearningRate = GetLearningRateInt(learningRate);
                int diffLearningRate = skill.iLearningRate - oldLearningRate;
                if (IsXML)
                {
                    if (type == 1)
                    {
                        if (diffLearningRate > 0)
                            EchoLearned = EchoLearned + name + "(+" + diffLearningRate.ToString() + "), ";
                        else
                            EchoLearned = EchoLearned + name + "(" + diffLearningRate.ToString() + "), ";
                    }
                    else
                    {
                        if (!EchoPulsed.Contains(name))
                            EchoPulsed = EchoPulsed + name + "(" + diffLearningRate.ToString() + "), ";
                    }
                }
                skill.rank = dRank;
                if (skill.startRank == -1)
                    skill.startRank = dRank;
                skill.learned = false;
                skill.rankGained = false;
                if (type == 1)
                    skill.learned = true;
                if (type == 2)
                    skill.rankGained = true;
                skill.sortLR = SortLR;
                skill.shortname = ShortName;

                //Next section builds the output string.  This will need to be recalucated for all skills if the 
                //output options are ever changed.

                //Outputs name of skill (short or normal) & ranks
                if (_host.get_Variable("ExpTracker.ShortNames") == "1")
                    skill.output = String.Format("{0,8:G}:{1,9}", skill.shortname, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                else
                    skill.output = String.Format("{0,15:G}:{1,9}", name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                //Always use long name for prase output
                skill.parseoutput = String.Format("{0,15:G}:{1,9}", name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                //Outputs Learning Rate NAME if set to
                if (_host.get_Variable("ExpTracker.LearningRate") == "1")
                    skill.output += String.Format(" {0,-13}", skill.learningRate);
                //Outputs Learning Rate NUMBER if set to
                if (_host.get_Variable("ExpTracker.LearningRateNumber") == "1")
                    skill.output += String.Format("{0,8}", "(" + skill.iLearningRate + "/34)");
                //Outputs Skill gain if set to 
                if (_host.get_Variable("ExpTracker.ShowRankGain") == "1")
                {
                    //Calculate Skill gain
                    double rankGain = skill.rank - skill.startRank;
                    if (rankGain >= 0) skill.output += " +" + String.Format("{0:0.00}", rankGain);
                    else skill.output += " " + String.Format("{0:0.00}", rankGain);
                }
                skill.parseoutput = skill.output;
                //Used for #echo >Window Options "   Text "
                //to preserve white space
                skill.output = " \"" + skill.output + "\"";

                //set color of text if ranked, or learned, or normal
                if (skill.rankGained == true)
                    skill.output = _host.get_Variable("ExpTracker.Color.RankGained") + skill.output;
                else if (skill.learned == true)
                    skill.output = _host.get_Variable("ExpTracker.Color.Learned") + skill.output;
                else
                    skill.output = _host.get_Variable("ExpTracker.Color.Normal") + skill.output;

                _skillList[name] = skill;
            }
            else
            {
                _updateExp = true;
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
                //Next section builds the output string.  This will need to be recalucated for all skills if the 
                //output options are ever changed.

                //Outputs name of skill (short or normal) & ranks
                if (_host.get_Variable("ExpTracker.ShortNames") == "1")
                    skill.output = String.Format("{0,7:G}:{1,9}", skill.shortname, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                else
                    skill.output = String.Format("{0,15:G}:{1,9}", name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                //Always use long name for prase output
                skill.parseoutput = String.Format("{0,15:G}:{1,9}", name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");
                //Outputs Learning Rate NAME if set to
                if (_host.get_Variable("ExpTracker.LearningRate") == "1")
                    skill.output += String.Format(" {0,-13}", skill.learningRate);
                //Outputs Learning Rate NUMBER if set to
                if (_host.get_Variable("ExpTracker.LearningRateNumber") == "1")
                    skill.output += String.Format("{0,8}", "(" + skill.iLearningRate + "/34)");
                //Outputs Skill gain if set to 
                if (_host.get_Variable("ExpTracker.ShowRankGain") == "1")
                {
                    //Calculate Skill gain
                    double rankGain = skill.rank - skill.startRank;
                    if (rankGain >= 0) skill.output += " +" + String.Format("{0:0.00}", rankGain);
                    else skill.output += " " + String.Format("{0:0.00}", rankGain);
                }
                skill.parseoutput = skill.output;
                //Used for #echo >Window Options "   Text "
                //to preserve white space
                skill.output = " \"" + skill.output + "\"";

                //set color of text if ranked, or learned, or normal
                if (skill.rankGained == true)
                    skill.output = _host.get_Variable("ExpTracker.Color.RankGained") + skill.output;
                else if (skill.learned == true)
                    skill.output = _host.get_Variable("ExpTracker.Color.Learned") + skill.output;
                else
                    skill.output = _host.get_Variable("ExpTracker.Color.Normal") + skill.output;

                _skillList.Add(name, skill);
            }

            if (_host.get_Variable("ExpTracker.Persistent") == "1")
            {
                _host.SendText("#var {" + name.Replace(" ", "_") + ".LearningRate} {" + GetLearningRateInt(learningRate).ToString() + "}");
                _host.SendText("#var {" + name.Replace(" ", "_") + ".Ranks} {" + dRank.ToString() + "}");
            }
            else
            {
                _host.set_Variable(name.Replace(" ", "_") + ".LearningRate", GetLearningRateInt(learningRate).ToString());
                _host.set_Variable(name.Replace(" ", "_") + ".Ranks", dRank.ToString());
            }

        }

        private void ParseClear(string name)
        {
            _updateExp = true;
            if (name.EndsWith("Magic") && !name.StartsWith("Targeted"))
                name = "Primary Magic";

            Skill skill = new Skill();
            if (_skillList.ContainsKey(name))
            {
                skill = (Skill)_skillList[name];
                skill.learningRate = "clear";
                skill.iLearningRate = 0;
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
                    int SkillCount = 0;
                    int CountMinMindstate = get_min_mindstate();


                    //iterate through the list of all skills
                    foreach (DictionaryEntry sk in _skillList)
                    {
                        Skill skill = (Skill)sk.Value;
                        //for those that aren't at "clear" learning rate (0/34)
                        if (skill.iLearningRate > 0)
                        {
                            if (_host.get_Variable("ExpTracker.CountSkills") == "1" && skill.iLearningRate > CountMinMindstate)
                                SkillCount++;

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
                    //1: Reading sort (based on game exp output)
                    if (_host.get_Variable("ExpTracker.SortType") == "1")
                        sortList.Sort(new MyComparerLR());
                    //2: By Learning Rate (Low to High)
                    else if (_host.get_Variable("ExpTracker.SortType") == "2")
                        sortList.Sort(new MyComparerLearning());
                    //3: By Learning Rate Reverse (High to Low)
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

                        //only echo the items that are in the sortList (learning rate > 0)
                        foreach (Sortskill item in sortList)
                        {
                            //get the skill info from the hash table, then echo it
                            Skill skill = (Skill)_skillList[item.name];
                            _host.SendText("#echo >Experience " + skill.output);
                        }

                        if (_host.get_Variable("ExpTracker.CountSkills") == "1")
                        { 
                            _host.SendText("#echo >Experience Learning Skills: " + SkillCount.ToString());
                            _host.set_Variable("ExpTracker.LearningSkills", SkillCount.ToString());
                        }

                        //tdp and sleep are blank in case there is no info for them to output
                        string asleep = "";
                        string tdp = "";

                        if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.EchoSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") != "0")
                            if (_host.get_Variable("ExpTracker.Sleeping") == "1")
                                asleep = "#echo >Experience Lightly sleeping - no new exp.";
                            else if (_host.get_Variable("ExpTracker.Sleeping") == "2")
                                asleep = "#echo >Experience Deeply sleeping - not draining.";
                        if (asleep != "")
                            _host.SendText(asleep);

                        if (_trackingTDP)
                        {
                            tdp = "#echo >Experience TDPs: " + _TDP;

                            if (_startTDP < _TDP)
                                tdp = tdp + "  Gained: " + (_TDP - _startTDP);

                        }
                        if (tdp != "")
                            _host.SendText(tdp);
                        TimeSpan oTimeSpan = DateTime.Now - _startTime;
                        _host.SendText("#echo >Experience Last updated: $time;#echo >Experience Tracking for: " + FormatTimeSpan(oTimeSpan));
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
        
        private void DisplayReport()
        {
            //list for sorting
            ArrayList sortList = new ArrayList();

            //iterate through the list of all skills
            foreach (DictionaryEntry sk in _skillList)
            {
                Skill skill = (Skill)sk.Value;
                //add the skill + sort types to the list of items to be sorted
                if (skill.startRank - skill.rank != 0.0 || skill.iLearningRate > 0 )
                {
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
            if (_host.get_Variable("ExpTracker.ReportType") == "1")
                sortList.Sort(new MyComparerLR());
            //2: By Learning Rate
            else if (_host.get_Variable("ExpTracker.ReportType") == "2")
                sortList.Sort(new MyComparerLearning());
            //3: By Learning Rate Reverse
            else if (_host.get_Variable("ExpTracker.ReportType") == "3")
                sortList.Sort(new MyComparerLearningRev());
            //0/Default: Alphabetical
            else
                sortList.Sort(new MyComparer());
            
            foreach (Sortskill item in sortList)
            {
                //get the skill info from the hash table
                Skill skill = (Skill)_skillList[item.name];
                //_host.EchoText(item.name);
                if (skill.output == "")
                    continue;
                _host.SendText("#echo " + skill.output);
                _host.SendText("#parse " + skill.parseoutput);
            }
        }
        #endregion

        #region Helper functions - reset, clear, init_variables

        public void ResetTracking()
        {
            //Reset TDP tracking
            _TDP = 0;
            _startTDP = 0;
            _trackingTDP = false;

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
        }
        
        public void ClearTracking()
        {
            //Reset TDP tracking
            _TDP = 0;
            _startTDP = 0;
            _trackingTDP = false;

            //Reset "Tracking Since"
            _startTime = DateTime.Now;

            //Reset all skill tracking info to start values
            _skillList.Clear();
            _skillList = new Hashtable(67);

            //Alert User of Reset:
            _host.SendText("#echo");
            _host.SendText("#echo XP Tracker reset to intial values");
        }
        public void ResetTDP()
        {
            _TDP = 0;
            _startTDP = 0;
            _trackingTDP = false;

            _host.SendText("#echo");
            _host.SendText("#echo TDP tracking reset");
        }

        public void init_variables(bool _echoinit = false, string variable = "")
        {
            bool needsave = _echoinit;
            if (_echoinit == true)
                _host.EchoText("Missing variable (" + variable +") detected, attempting to reset missing variables to default.");

            if (_host.get_Variable("ExpTracker.Window") == "")
            {
                _host.SendText("#var ExpTracker.Window 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.ShowRankGain") == "")
            { 
                _host.SendText("#var ExpTracker.ShowRankGain 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.LearningRate") == "")
            { 
                _host.SendText("#var ExpTracker.LearningRate 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.LearningRateNumber") == "")
            {
                _host.SendText("#var ExpTracker.LearningRateNumber 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.TrackSleep") == "")
            {
                _host.SendText("#var ExpTracker.TrackSleep 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.EchoSleep") == "")
            {
                _host.SendText("#var ExpTracker.EchoSleep 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.GagExp") == "")
            {
                _host.SendText("#var ExpTracker.GagExp 0");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.ShortNames") == "")
            {
                _host.SendText("#var ExpTracker.ShortNames 0");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.Persistent") == "")
            {
                _host.SendText("#var ExpTracker.Persistent 0");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.CountSkills") == "")
            {
                _host.SendText("#var ExpTracker.CountSkills 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.CountMinMindstate") == "")
            {
                _host.SendText("#var ExpTracker.CountMinMindstate 0");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.SortType") == "")
            {
                _host.SendText("#var ExpTracker.SortType 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.ReportType") == "")
            {
                _host.SendText("#var ExpTracker.ReportType 1");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.Color.Normal") == "")
            {
                _host.SendText("#var ExpTracker.Color.Normal WhiteSmoke");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.Color.RankGained") == "")
            {
                _host.SendText("#var ExpTracker.Color.RankGained Red");
                needsave = true;
            }

            if (_host.get_Variable("ExpTracker.Color.Learned") == "")
            {
                _host.SendText("#var ExpTracker.Color.Learned Aqua");
                needsave = true;
            }

            if (needsave == true)
                _host.SendText("#var save");
        }

        public int get_min_mindstate()
        {
            int temp;
            try
            {
                temp = Convert.ToInt32(_host.get_Variable("ExpTracker.CountMinMindstate"));
                if (temp < 0)
                    temp = 0;
                else if (temp > 33)
                    temp = 33;
            }
            catch
            {
                temp = 0;
            }
            return temp;
        }

        public void set_min_mindstae()
        {
            int temp;
            try
            {
                temp = Convert.ToInt32(_host.get_Variable("ExpTracker.CountMinMindstate"));
                if (temp < 0)
                {
                    _host.EchoText("Variable ExpTracker.CountMinMindstate minimum value is 0.");
                    _host.SendText("#var ExpTracker.CountMinMindstate 0");
                }
                else if (temp > 33)
                {
                    _host.EchoText("Variable ExpTracker.CountMinMindstate maximum value is 33.");
                    _host.SendText("#var ExpTracker.CountMinMindstate 33");
                }
            }
            catch
            {
                _host.EchoText("Variable ExpTracker.CountMinMindstate must be an integer.");
                _host.EchoText("Setting value to default of 0.");
                _host.SendText("#var ExpTracker.CountMinMindstate 0");
            }


        }
        #endregion

    }
}
