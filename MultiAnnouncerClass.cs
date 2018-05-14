using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fougerite;
using UnityEngine;
using System.IO;
using System.Collections;

namespace MultiAnnouncer
{
    public class MultiAnnouncerClass : Fougerite.Module
    {
        public override string Name { get { return "MultiAnnouncer"; } }
        public override string Author { get { return "salva/juli"; } }
        public override string Description { get { return "MultiAnnouncer"; } }
        public override Version Version { get { return new Version("1.0"); } }
        public IniParser Settings;
        public string rutatxt = "";
        public List<string> msgs = new List<string>();
        public int IntervalSeconds = 1;
        public bool chat = true;
        public bool popup = true;
        public string popupicon = "X";
        public int popupduration = 10;
        public override void Initialize()
        {
            Hooks.OnServerLoaded += OnServerLoaded;
            rutatxt = Path.Combine(ModuleFolder, "RandomMessages.txt");
        }
        public override void DeInitialize()
        {
            Hooks.OnServerLoaded -= OnServerLoaded;
        }
        public void OnServerLoaded()
        {
            ReloadConfig();
            Timer1(IntervalSeconds * 1000, null).Start();
        }
        public TimedEvent Timer1(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += CallBack;
            return timedEvent;
        }
        public void CallBack(TimedEvent e)
        {
            e.Kill();
            Timer1(IntervalSeconds * 1000, null).Start();

            ReloadConfig();

            System.Random rnd = new System.Random();
            int Totalmsgs = msgs.Count();
            int selected = rnd.Next(0, Totalmsgs);

            if (chat)
            {
                Server.GetServer().BroadcastFrom(Name, msgs[selected]);
            }
            if (popup)
            {
                foreach (Fougerite.Player pl in Server.GetServer().Players.Where(pl => pl.IsOnline))
                {
                    pl.Notice(popupicon, msgs[selected], popupduration);
                }
            }
        }
        public void ReloadConfig()
        {
            Loom.QueueOnMainThread(() =>
            {
                if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
                {
                    File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                    Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));

                    Settings.AddSetting("Timer", "IntervalBetweenMessages_seconds", "60");
                    Settings.AddSetting("ChatMessages", "ShowInChat", "true");
                    Settings.AddSetting("PopupMessages", "ShowInPopup", "true");
                    Settings.AddSetting("PopupMessages", "PopupIcon", "X");
                    Settings.AddSetting("PopupMessages", "DurationPopup_secs", "10");

                    Settings.Save();
                    Logger.LogError(Name + "... New Settings File Created!");
                    ReloadConfig();
                    return;
                }
                else
                {
                    Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                    if (Settings.ContainsSetting("Timer", "IntervalBetweenMessages_seconds") &&
                        Settings.ContainsSetting("ChatMessages", "ShowInChat") &&
                        Settings.ContainsSetting("PopupMessages", "ShowInPopup") &&
                        Settings.ContainsSetting("PopupMessages", "PopupIcon") &&
                        Settings.ContainsSetting("PopupMessages", "DurationPopup_secs"))
                    {
                        try
                        {
                            IntervalSeconds = int.Parse(Settings.GetSetting("Timer", "IntervalBetweenMessages_seconds"));
                            chat = Settings.GetBoolSetting("ChatMessages", "ShowInChat");
                            popup = Settings.GetBoolSetting("PopupMessages", "ShowInPopup");
                            popupicon = Settings.GetSetting("PopupMessages", "PopupIcon");
                            popupduration = int.Parse(Settings.GetSetting("PopupMessages", "DurationPopup_secs"));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(Name + "... Detected a problem in the configuration");
                            Logger.LogError("ERROR -->" + ex.Message);
                            File.Delete(Path.Combine(ModuleFolder, "Settings.ini"));
                            Logger.LogError(Name + "... Deleted the old configuration file");
                            ReloadConfig();
                            return;
                        }
                    }
                    else
                    {
                        Logger.LogError(Name + "... Detected a problem in the configuration (lost key)");
                        File.Delete(Path.Combine(ModuleFolder, "Settings.ini"));
                        Logger.LogError(Name + "... Deleted the old configuration file");
                        ReloadConfig();
                        return;
                    }
                }

                if (!File.Exists(rutatxt))
                {
                    File.Create(rutatxt).Dispose();
                    StreamWriter WF = File.AppendText(rutatxt);
                    WF.WriteLine("[color red] Hello... this is message 1");
                    WF.WriteLine("[color blue] Hello... this is message 2");
                    WF.WriteLine("[color green] Hello... this is message 3");
                    WF.WriteLine("[color orange] Hello... this is message 4");
                    WF.WriteLine("[color yellow] Hello... this is message 5");
                    WF.WriteLine("Add more lines if you want ;)");
                    WF.Close();
                    Logger.LogError(Name + "... File not found (RandomMessages.txt) ,creating new one");
                    ReloadConfig();
                    return;
                }
                else
                {
                    if (File.ReadAllText(rutatxt).Length == 0)
                    {
                        Logger.LogError(Name + "... The File (RandomMessages.txt) dont contain any line or is empty...");
                        Logger.LogError(Name + "... You need to add at least one line so that the plugin works correctly");
                        msgs.Clear();
                        msgs.Add("The File (RandomMessages.txt) dont contain any line or is empty...");
                    }
                    else
                    {
                        msgs.Clear();
                        try
                        {
                            foreach (string str in File.ReadAllLines(rutatxt))
                            {
                                //if (str == string.Empty)
                                if (str.Length == 0)
                                {
                                    continue;
                                }
                                msgs.Add(str);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(Name + "... The File (RandomMessages.txt) could not be read for any reason");
                        }
                    }
                }
                //Logger.Log(File.ReadAllText(rutatxt).Length.ToString());//comprobar si el archivo txt esta vacio
                return;
            });
        }
    }
}
