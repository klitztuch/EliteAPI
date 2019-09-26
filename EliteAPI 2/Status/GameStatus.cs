﻿namespace EliteAPI.Status
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class GameStatus
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; internal set; }

        [JsonProperty("event")]
        public string Event { get; internal set; }

        [JsonProperty("Flags")]
        public ShipStatusFlags Flags { get; internal set; }

        [JsonProperty("Pips")]
        public List<long> Pips { get; internal set; }

        [JsonProperty("FireGroup")]
        public long FireGroup { get; internal set; }

        [JsonProperty("GuiFocus")]
        public long GuiFocus { get; internal set; }

        [JsonProperty("Fuel")]
        public Fuel Fuel { get; internal set; }

        [JsonProperty("LegalState")]
        public string LegalState { get; internal set; }

        [JsonProperty("Cargo")]
        public long Cargo { get; internal set; }

        public bool Docked { get { return GetFlag(0); } }

        public bool Landed { get { return GetFlag(1); } }

        public bool Gear { get { return GetFlag(2); } }

        public bool Shields { get { return GetFlag(3); } }

        public bool Supercruise { get { return GetFlag(4); } }

        public bool FlightAssist { get { return !GetFlag(5); } }

        public bool Hardpoints { get { return GetFlag(6); } }

        public bool Winging { get { return GetFlag(7); } }

        public bool Lights { get { return GetFlag(8); } }

        public bool CargoScoop { get { return GetFlag(9); } }

        public bool SilentRunning { get { return GetFlag(10); } }

        public bool Scooping { get { return GetFlag(11); } }

        public bool SrvHandbreak { get { return GetFlag(12); } }

        public bool SrvTurrent { get { return GetFlag(13); } }

        public bool SrvNearShip { get { return GetFlag(14); } }

        public bool SrvDriveAssist { get { return GetFlag(15); } }

        public bool MassLocked { get { return GetFlag(16); } }

        public bool FsdCharging { get { return GetFlag(17); } }

        public bool FsdCooldown { get { return GetFlag(18); } }

        public bool LowFuel { get { return GetFlag(19); } }

        public bool Overheating { get { return GetFlag(20); } }

        public bool HasLatlong { get { return GetFlag(21); } }

        public bool InDanger { get { return GetFlag(22); } }

        public bool InInterdiction { get { return GetFlag(23); } }

        public bool InMothership { get { return GetFlag(24); } }

        public bool InFighter { get { return GetFlag(25); } }

        public bool InSRV { get { return GetFlag(26); } }

        public bool AnalysisMode { get { return GetFlag(27); } }

        public bool NightVision { get { return GetFlag(28); } }

        public string GameMode { get; internal set; }
        public bool InNoFireZone { get; internal set; }
        public double JumpRange { get; internal set; }
        public bool IsRunning { get { return (Flags != 0); } }
        public bool InMainMenu { get; internal set; }
        public string MusicTrack { get; internal set; }

        public bool GetFlag(int bit)
        {
            return Flags.HasFlag((ShipStatusFlags)(1 << bit));
        }
    }

    public partial class Fuel
    {
        [JsonProperty("FuelMain")]
        public double FuelMain { get; internal set; }

        [JsonProperty("FuelReservoir")]
        public double FuelReservoir { get; internal set; }

        public double MaxFuel { get; internal set; }
    }

    public partial class GameStatus
    {
        public static GameStatus Process(string json) => JsonConvert.DeserializeObject<GameStatus>(json, EliteAPI.Status.ShipStatusConverter.Settings);
        public static GameStatus FromFile(FileInfo file, EliteDangerousAPI api)
        {
            try
            {
                if (!File.Exists(file.FullName)) { api.Logger.Error("Could not find Status.json.", new Exception($"Could not find {file}.")); return new GameStatus(); }

                //Create a stream from the log file.
                FileStream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //Create a stream from the file stream.
                StreamReader streamReader = new StreamReader(fileStream);

                //Go through the stream.
                while (!streamReader.EndOfStream)
                {
                    try
                    {
                        //Process this string.
                        string json = streamReader.ReadLine();
                        GameStatus s = Process(json);
                        if(s.Fuel == null) { s.Fuel = new Fuel(); }
                        if(s.Pips == null) { s.Pips = new List<long>() { 0, 0, 0}; }
                        return s;
                    }
                    catch(Exception ex) { api.Logger.Warning("Could not update Status.json.", ex); }
                }

                return api.Status;

            }
            catch(Exception ex) { api.Logger.Warning("Could not update status.", ex);}

            return new GameStatus();
        }
    }

    

    public static class ShipStatusConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore, MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
