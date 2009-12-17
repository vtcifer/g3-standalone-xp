using System;
using System.Collections;
using GeniePlugin.Interfaces;
using System.Xml;
using System.Text.RegularExpressions;

namespace Standalone_EXPTracker
{
    public class Class1 : IPlugin
    {
        string _VERSION = "1.1.0";

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
            public int sortTB = 0;                  //Used for sorting As, Top to Bottom (Based on ALL skills visible)
        }

        //Class Sortskill
        //Used for sorting the skills for display in the Experience window
        //Used in an array list for sorting, which is fed from a hashtable
        public class Sortskill
        {
            public string name = "";    //Name of skill
            public int sortLR = 0;      //Ordered value based on Reading sort (Left to Right)
            public int sortTB = 0;      //Ordered value based on top to bottom, THEN left to right 
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
            //User asking for help with commands 
            //Note as more plugins do this this could get VERY long.
            //Also not sure how this would work with multiple plugins doing this as well
            if (Text == "/?")
            {
                _host.SendText("#echo");
                _host.SendText(@"#echo Standalone EXPTracker (Ver:"+ _VERSION +") Usage:");
                _host.SendText(@"#echo /trackreset");
                _host.SendText(@"#echo """"    """" Used to reset tracking");
                return "";
            }
            //Reset all tracking
            //IE:
            //      TDPs
            //      Ranks gained
            if (Text == "/trackreset")
            {
                //Reset TDP tracking
                _TDP = 0;
                _startTDP = 0;

                //Reset "Tracking Since"
                _startTime = DateTime.Now;

                //Reset Skill Tracking info
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
            try
            {
                if (_host != null)
                {
                    if (_parsing == true)
                    {
                        if (Text.StartsWith("EXP HELP for more information"))
                        {
                            _parsing = false;
                            if (_sleeping == true && _host.get_Variable("TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "0")
                            {
                                _host.set_Variable("ExpTracker.Sleeping", "1");
                                _host.SendText("#var save");
                            }
                            else if (_sleeping == false && _host.get_Variable("TrackSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "1")
                            {
                                _host.set_Variable("ExpTracker.Sleeping", "0");
                                _host.SendText("#var save");
                            }
                            ShowExperience();
                        }
                        else if (Text.Contains("%"))
                        {
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
                        else if (Text.StartsWith("Time Development Points:"))
                        {
                            _TDP = Convert.ToInt32(Text.Substring(24, Text.IndexOf("Favors") - 24).Trim());
                            if (_startTDP == 0)
                                _startTDP = _TDP;
                        }
                        else if (Text.StartsWith("You are relaxed and your mind has entered a state of rest.") )
                            _sleeping = true;
                    }
                    else if (Text.StartsWith("Circle: "))
                    {
                        _parsing = true;
                        _sleeping = false;
                    }

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

                    if (Text.StartsWith("Overall state of mind:"))
                        ParseMindState(Text.Substring(Text.IndexOf(":") + 1).Trim());
                    else if (_parsing && _host.get_Variable("ExpTracker.GagExp") == "1")
                        Text = "";
                }
            }
            catch (Exception ex)
            {
            }
            return Text;
        }

        //Required for Plugin - 
        //Parameters:
        //              string Text:  That "xml" text comes from the game
        public void ParseXML(string XML)
        {
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

            if (_host.get_Variable("ExpTracker.SortType") == "1")
                form.comboSort.Text = "Left to Right";
            else if (_host.get_Variable("ExpTracker.SortType") == "2")
                form.comboSort.Text = "Top to Bottom";
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
            switch (_mindState)
            {
                case "clear":
                    mindState = 0;
                    break;
                case "fluid":
                    mindState = 1;
                    break;
                case "murky":
                    mindState = 2;
                    break;
                case "very murky":
                    mindState = 3;
                    break;
                case "thick":
                    mindState = 4;
                    break;
                case "very thick":
                    mindState = 5;
                    break;
                case "dense":
                    mindState = 6;
                    break;
                case "very dense":
                    mindState = 7;
                    break;
                case "stagnant":
                    mindState = 8;
                    break;
                case "very stagnant":
                    mindState = 9;
                    break;
                case "frozen":
                    mindState = 10;
                    break;
                case "very frozen":
                    mindState = 11;
                    break;
                default:
                    break;
            }
            _host.set_Variable("MindState", mindState.ToString());
        }

        private int GetLearningRateInt(string skillRate)
        {
            switch (skillRate)
            {
                case "clear":
                    return 0;
                case "dabbling":
                    return 1;
                case "perusing":
                    return 2;
                case "learning":
                    return 3;
                case "thoughtful":
                    return 4;
                case "thinking":
                    return 5;
                case "considering":
                    return 6;
                case "pondering":
                    return 7;
                case "ruminating":
                    return 8;
                case "concentrating":
                    return 9;
                case "attentive":
                    return 10;
                case "deliberative":
                    return 11;
                case "interested":
                    return 12;
                case "examining":
                    return 13;
                case "understanding":
                    return 14;
                case "absorbing":
                    return 15;
                case "intrigued":
                    return 16;
                case "scrutinizing":
                    return 17;
                case "analyzing":
                    return 18;
                case "studious":
                    return 19;
                case "focused":
                    return 20;
                case "very focused":
                    return 21;
                case "engaged":
                    return 22;
                case "very engaged":
                    return 23;
                case "cogitating":
                    return 24;
                case "fascinated":
                    return 25;
                case "captivated":
                    return 26;
                case "engrossed":
                    return 27;
                case "riveted":
                    return 28;
                case "very riveted":
                    return 29;
                case "rapt":
                    return 30;
                case "very rapt":
                    return 31;
                case "enthralled":
                    return 32;
                case "nearly locked":
                    return 33;
                case "mind lock":
                    return 34;
                default:
                    return 0;
            }

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
            int SortTB = 0;
            switch (name)
            {
                case "Bone Armor":
                    SortLR = 7;
                    SortTB = 37;
                    break;
                case "Cloth Armor":
                    SortLR = 6;
                    SortTB = 3;
                    break;
                case "Heavy Chain":
                    SortLR = 3;
                    SortTB = 35;
                    break;
                case "Heavy Plate":
                    SortLR = 5;
                    SortTB = 36;
                    break;
                case "Leather Armor":
                    SortLR = 1;
                    SortTB = 34;
                    break;
                case "Light Chain":
                    SortLR = 2;
                    SortTB = 1;
                    break;
                case "Light Plate":
                    SortLR = 4;
                    SortTB = 2;
                    break;
                case "Shield Usage":
                    SortLR = 0;
                    SortTB = 0;
                    break;
                case "Brawling":
                    SortLR = 31;
                    SortTB = 49;
                    break;
                case "Composite Bow":
                    SortLR = 22;
                    SortTB = 11;
                    break;
                case "Halberds":
                    SortLR = 28;
                    SortTB = 14;
                    break;
                case "Heavy Blunt":
                    SortLR = 16;
                    SortTB = 8;
                    break;
                case "Heavy Crossbow":
                    SortLR = 24;
                    SortTB = 12;
                    break;
                case "Heavy Edged":
                    SortLR = 12;
                    SortTB = 6;
                    break;
                case "Heavy Thrown":
                    SortLR = 30;
                    SortTB = 15;
                    break;
                case "Light Blunt":
                    SortLR = 14;
                    SortTB = 07;
                    break;
                case "Light Crossbow":
                    SortLR = 23;
                    SortTB = 45;
                    break;
                case "Light Edged":
                    SortLR = 10;
                    SortTB = 05;
                    break;
                case "Light Thrown":
                    SortLR = 29;
                    SortTB = 48;
                    break;
                case "Long Bow":
                    SortLR = 21;
                    SortTB = 44;
                    break;
                case "Medium Blunt":
                    SortLR = 15;
                    SortTB = 41;
                    break;
                case "Medium Edged":
                    SortLR = 11;
                    SortTB = 39;
                    break;
                case "Multi Opponent":
                    SortLR = 9;
                    SortTB = 38;
                    break;
                case "Offhand Weapon":
                    SortLR = 32;
                    SortTB = 16;
                    break;
                case "Parry Ability":
                    SortLR = 8;
                    SortTB = 4;
                    break;
                case "Pikes":
                    SortLR = 27;
                    SortTB = 47;
                    break;
                case "Quarter Staff":
                    SortLR = 26;
                    SortTB = 13;
                    break;
                case "Short Bow":
                    SortLR = 20;
                    SortTB = 10;
                    break;
                case "Short Staff":
                    SortLR = 25;
                    SortTB = 46;
                    break;
                case "Slings":
                    SortLR = 18;
                    SortTB = 9;
                    break;
                case "Staff Sling":
                    SortLR = 19;
                    SortTB = 43;
                    break;
                case "Twohanded Blunt":
                    SortLR = 17;
                    SortTB = 42;
                    break;
                case "Twohanded Edged":
                    SortLR = 13;
                    SortTB = 40;
                    break;
                case "Harness Ability":
                    SortLR = 34;
                    SortTB = 17;
                    if (_host.get_Variable("ExpTracker.Debug") == "1")
                        _host.SendText("#echo >Debug Name: " + name + " XX LR:" + SortLR + " XX TB:" + SortTB);
                    break;
                case "Arcana":
                    SortLR = 36;
                    SortTB = 18;
                    break;
                case "Power Perceive":
                    SortLR = 35;
                    SortTB = 51;
                    if (_host.get_Variable("ExpTracker.Debug") == "1")
                        _host.SendText("#echo >Debug Name: " + name + " XX LR:" + SortLR + " XX TB:" + SortTB);
                    break;
                case "Lunar Magic":
                case "Life Magic": 
                case "Holy Magic":
                case "Elemental Magic":
                case "Inner Magic":
                case "Arcane Magic":
                    SortLR = 33;
                    SortTB = 50;
                    if (_host.get_Variable("ExpTracker.Debug") == "1")
                        _host.SendText("#echo >Debug Name: " + name + " XX LR:" + SortLR + " XX TB:" + SortTB);
                    break;
                case "Targeted Magic":
                    SortLR = 37;
                    SortTB = 52;
                    break;
                case "Animal Lore":
                    SortLR = 59;
                    SortTB = 63;
                    break;
                case "Appraisal":
                    SortLR = 56;
                    SortTB = 28;
                    break;
                case "Astrology":
                    SortLR = 64;
                    SortTB = 32;
                    break;
                case "Mechanical Lore":
                    SortLR = 54;
                    SortTB = 27;
                    break;
                case "Percussions":
                    SortLR = 60;
                    SortTB = 30;
                    break;
                case "Scholarship":
                    SortLR = 53;
                    SortTB = 60;
                    break;
                case "Strings":
                    SortLR = 61;
                    SortTB = 64;
                    break;
                case "Teaching":
                    SortLR = 57;
                    SortTB = 62;
                    break;
                case "Winds":
                    SortLR = 62;
                    SortTB = 31;
                    break;
                case "Vocals":
                    SortLR = 63;
                    SortTB = 65;
                    break;
                case "Trading":
                    SortLR = 58;
                    SortTB = 29;
                    break;
                case "Empathy":
                    SortLR = 65;
                    SortTB = 66;
                    break;
                case "Thanatology":
                    SortLR = 66;
                    SortTB = 33;
                    break;
                case "Climbing":
                    SortLR = 39;
                    SortTB = 53;
                    break;
                case "Disarm Traps":
                    SortLR = 44;
                    SortTB = 22;
                    break;
                case "Escaping":
                    SortLR = 49;
                    SortTB = 58;
                    break;
                case "Evasion":
                    SortLR = 38;
                    SortTB = 19;
                    break;
                case "First Aid":
                    SortLR = 47;
                    SortTB = 57;
                    break;
                case "Foraging":
                    SortLR = 48;
                    SortTB = 24;
                    break;
                case "Hiding":
                    SortLR = 42;
                    SortTB = 21;
                    break;
                case "Lockpicking":
                    SortLR = 43;
                    SortTB = 55;
                    break;
                case "Perception":
                    SortLR = 40;
                    SortTB = 20;
                    break;
                case "Skinning":
                    SortLR = 51;
                    SortTB = 59;
                    break;
                case "Stalking":
                    SortLR = 45;
                    SortTB = 56;
                    break;
                case "Stealing":
                    SortLR = 46;
                    SortTB = 23;
                    break;
                case "Swimming":
                    SortLR = 52;
                    SortTB = 26;
                    break;
                case "Backstab":
                    SortLR = 50;
                    SortTB = 25;
                    break;
                case "Scouting":
                    SortLR = 41;
                    SortTB = 54;
                    break;
                default:
                    SortLR = 100;
                    SortTB = 100;
                    break;
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
                skill.sortTB = SortTB;
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
                    sortTB = SortTB
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
        public class MyComparerTB : IComparer
        {
            public int Compare(object x, object y)
            {
                return Comparer.Default.Compare(((Sortskill)x).sortTB, ((Sortskill)y).sortTB);
            }
        }
        private void ShowExperience()
        {
            if (_host != null)
            {
                if (_host.get_Variable("ExpTracker.Window") == "1")
                {
                    ArrayList sortList = new ArrayList();
                    foreach (DictionaryEntry sk in _skillList)
                    {
                        Skill skill = (Skill)sk.Value;
                        if (skill.learningRate != "clear")
                        {
                            Sortskill sortSkill = new Sortskill
                            {
                                name = sk.Key.ToString(),
                                sortLR = ((Skill)sk.Value).sortLR,
                                sortTB = ((Skill)sk.Value).sortTB
                            };
                            sortList.Add(sortSkill);
                        }
                    }
                    if (_host.get_Variable("ExpTracker.SortType") == "0")
                    {
                        sortList.Sort(new MyComparer());
                        if (_host.get_Variable("ExpTracker.Debug") == "1")
                            _host.SendText("#echo >Debug Sort Alpha");
                    }
                    else if (_host.get_Variable("ExpTracker.SortType") == "1")
                    {
                        sortList.Sort(new MyComparerLR());
                        if (_host.get_Variable("ExpTracker.Debug") == "1")
                            _host.SendText("#echo >Debug Sort L to R");
                    }
                    else if (_host.get_Variable("ExpTracker.SortType") == "2")
                    {
                        sortList.Sort(new MyComparerTB());
                        if (_host.get_Variable("ExpTracker.Debug") == "1")
                            _host.SendText("#echo >Debug Sort T to B");
                    }
                    if (sortList.Count >= 0)
                    {
                        _host.SendText("#echo >Experience @suspend@");


                        foreach (Sortskill item in sortList)
                        {
                            Skill skill = (Skill)_skillList[item.name];


                            string output = String.Format("{0,15:G}:{1,9} ", item.name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, " ") + "%");

                            if (_host.get_Variable("ExpTracker.LearningRate") == "1")
                                output += String.Format("{0,-13}", skill.learningRate);

                            if (_host.get_Variable("ExpTracker.LearningRateNumber") == "1")
                                output += String.Format("{0,8}", "(" + skill.iLearningRate + "/34)");

                            double rankGain = skill.rank - skill.startRank;

                            if (_host.get_Variable("ExpTracker.ShowRankGain") == "1")
                                output += " +" + String.Format("{0:0.00}", rankGain);


                            output = @" """ + output + @"""";

                            /*
                            if (_host.get_Variable("ExpTracker.ShowWall") == "1")
                                if (skill.wall == 0)
                                    format = "{0,15:G}:{1,9} {2,-14:G}{3}{4}";
                                else
                                    format = "{0,15:G}:{1,9} {2,-13:G}{3}{4}";
                            else
                                format = "{0,15:G}:{1,9} {2,-13:G}{3}{4}";
                            */

                            //double rankGain = skill.rank - skill.startRank;

                            _host.SendText("#echo >Experience " + (skill.rankGained == true ? _host.get_Variable("ExpTracker.Color.RankGained") : (skill.learned == true ? _host.get_Variable("ExpTracker.Color.Learned") : _host.get_Variable("ExpTracker.Color.Normal"))) + output);

                            //_host.SendText("#echo >Experience " + (skill.rankGained == true ? _host.get_Variable("ExpTracker.Color.RankGained") : (skill.learned == true ? _host.get_Variable("ExpTracker.Color.Learned") : _host.get_Variable("ExpTracker.Color.Normal"))) + @" """  + String.Format(format, item.name, (skill.rank > 99.99 ? "" : " ") + String.Format("{0:0.00}", skill.rank).Replace(".", " ") + "%", skill.learningRate, (_host.get_Variable("ExpTracker.ShowWall") == "1" ? " (" + skill.wall + ")" : ""), (rankGain > 0 && _host.get_Variable("ExpTracker.ShowRankGain") == "1" ? " +" + String.Format("{0:0.00}", rankGain) : "")) + @"""");
                        }

                        string tdp = String.Empty;
                        string asleep = String.Empty;
                        if (_startTDP < _TDP)
                            tdp = ";#echo >Experience TDP gained: " + (_TDP - _startTDP);
                        if (_host.get_Variable("ExpTracker.TrackSleep") == "1" && _host.get_Variable("ExpTracker.EchoSleep") == "1" && _host.get_Variable("ExpTracker.Sleeping") == "1")
                            if (_host.get_Variable("ExpTracker.Echo") == "")
                                asleep = ";#echo >Experience YOU ARE ASLEEP!";
                            else
                                asleep = ";#echo >Experience " + _host.get_Variable("ExpTracker.Echo");

                        TimeSpan oTimeSpan = DateTime.Now - _startTime;
                        _host.SendText("#echo >Experience Overall state of mind: " + _mindState + asleep + tdp + ";#echo >Experience Last updated: $time;#echo >Experience Tracking for: " + FormatTimeSpan(oTimeSpan) + ";#echo >Experience @resume@");
                    }
                }
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            string txt = "";

            if (ts.Days > 0)
            {
                if (txt.Length > 0)
                    txt += ", ";
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
            return txt;
        }
        #endregion

    }
}
