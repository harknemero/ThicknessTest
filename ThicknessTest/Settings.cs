﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thickness_Test_Settings
{
    public class Settings
    {
        private double intervalLengthMM;
        private int numOfIntervals;
        private int numOfRows;
        private int zaberOrigin;
        private int dirFromOrigin;
        private double targetThickness;
        private double acceptableRange;
        private double errorRange;
        private bool isLengthInMillimeters;
        private int sampleSize;
        private int maxNumOfRows; // arbitrary value for user input handling only.

        // This constructor loads some default settings.
        // Should only run in the event that no settings files are found.
        public Settings()
        {
            intervalLengthMM = 76.2;
            numOfIntervals = 10;
            numOfRows = 13;
            zaberOrigin = 26083;
            dirFromOrigin = 1;
            targetThickness = 12.25;
            acceptableRange = 0.5;
            errorRange = 2;
            isLengthInMillimeters = true;
            sampleSize = 100;
            maxNumOfRows = 500; // arbitrary value for user input handling only. Increase if needed.
        }

        // Called from the Profiles Class during initialization from file
        public Settings(String[] s)
        {
            int i = 1; // index 0 is profile name and is ignored here.
            intervalLengthMM = Convert.ToDouble(s[i]);
            i++;
            numOfIntervals = Convert.ToInt32(s[i]);
            i++;
            numOfRows = Convert.ToInt32(s[i]);
            i++;
            zaberOrigin = Convert.ToInt32(s[i]);
            i++;
            dirFromOrigin = Convert.ToInt32(s[i]);
            i++;
            targetThickness = Convert.ToDouble(s[i]);
            i++;
            acceptableRange = Convert.ToDouble(s[i]);
            i++;
            errorRange = Convert.ToDouble(s[i]);
            i++;
            isLengthInMillimeters = Convert.ToBoolean(s[i]);
            i++;
            sampleSize = Convert.ToInt32(s[i]);
            i++;
            maxNumOfRows = 500; // arbitrary value for user input handling only. Increase if needed.
        }

        public Settings(Settings s)
        {
            intervalLengthMM = s.IntervalLengthMM;
            numOfIntervals = s.NumOfIntervals;
            numOfRows = s.NumOfRows;
            zaberOrigin = s.ZaberOrigin;
            dirFromOrigin = s.DirFromOrigin;
            targetThickness = s.TargetThickness;
            acceptableRange = s.AcceptableRange;
            errorRange = s.ErrorRange;
            isLengthInMillimeters = s.IsLengthInMillimeters;
            sampleSize = s.SampleSize;
            maxNumOfRows = s.MaxNumOfRows;
        }

        public String toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("" + intervalLengthMM);
            sb.Append("," + numOfIntervals);
            sb.Append("," + numOfRows);
            sb.Append("," + zaberOrigin);
            sb.Append("," + dirFromOrigin);
            sb.Append("," + targetThickness);
            sb.Append("," + acceptableRange);
            sb.Append("," + errorRange);
            sb.Append("," + isLengthInMillimeters);
            sb.Append("," + sampleSize);

            return sb.ToString();
        }

        public double IntervalLengthMM { get => intervalLengthMM; set => intervalLengthMM = value; }
        public int NumOfIntervals { get => numOfIntervals; set => numOfIntervals = value; }
        public int NumOfRows { get => numOfRows; set => numOfRows = value; }
        public int ZaberOrigin { get => zaberOrigin; set => zaberOrigin = value; }
        public int DirFromOrigin { get => dirFromOrigin; set => dirFromOrigin = value; }
        public double TargetThickness { get => targetThickness; set => targetThickness = value; }
        public double AcceptableRange { get => acceptableRange; set => acceptableRange = value; }
        public double ErrorRange { get => errorRange; set => errorRange = value; }
        public bool IsLengthInMillimeters { get => isLengthInMillimeters; set => isLengthInMillimeters = value; }
        public int SampleSize { get => sampleSize; set => sampleSize = value; }
        public int MaxNumOfRows { get => maxNumOfRows; set => maxNumOfRows = value; }
    }

    public class Profiles
    {
        private SortedDictionary<String, Settings> profiles;
        private string controlledProfilesFilePath;        
        private string defaultProfile;
        private string defaultControlledProfile;
        private string defaultSaveLocation;
        private bool controlled;
        private static string internalSettingsFileName = "Internal_Settings.txt";
        private static string internalProfilesFilePath = "Thickness_Test_System_610_00051_Settings.txt";
        private static string commentsAndInstructions =
            "//, This file contains settings profiles for the ClearShield thickness test system.\n" +
            "//, The contents of this file should not be altered except by authorized personel.\n" +
            "//, If no startup profile is set then a blank line should follow the instructions.\n" +
            "//, Each line following the startup profile line will contain the settings for a single profile.\n" +
            "//, \n" +
            "//, The settings values for each profile are listed in the following order:\n" +
            "//, Profile Name -!Must not contain any commas!-\n" +
            "//, Interval Length in Millimeters\n" +
            "//, Intervals Per Run\n" +
            "//, Number of Runs\n" +
            "//, Zaber Origin (the start location for the test, or zero on the Y-axis)\n" +
            "//, Direction from Origin (-1 = toward home, 1 = away from home)\n" +
            "//, Target Thickness\n" +
            "//, Acceptable Range\n" +
            "//, Error Range\n" +
            "//, Length is in Millimeters (This value must either be True or False)\n" +
            "//, Number of Samples (The keyence will return an average of this many thickness samples)\n" +
            "//, \n" +
            "//, Each value must be separated by a comma.\n";

        public string DefaultSaveLocation { get => defaultSaveLocation; set => defaultSaveLocation = value; }

        public Profiles()
        {
            controlledProfilesFilePath = "";
            controlled = false;
            profiles = new SortedDictionary<String, Settings>();
            defaultProfile = "";
            defaultControlledProfile = "";
            defaultSaveLocation = "P:\\Turner MedTech\\ClearShield\\Work Order Data";

            loadInternalSettings();
            try
            {
                loadProfiles();
            }
            catch
            {

            }
        }

        public void loadInternalSettings()
        {
            string line;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(internalSettingsFileName);

                line = reader.ReadLine();
                defaultProfile = line;
                line = reader.ReadLine();
                defaultControlledProfile = line;
                line = reader.ReadLine();
                controlled = Convert.ToBoolean(line);
                line = reader.ReadLine();
                controlledProfilesFilePath = line;
                line = reader.ReadLine();
                defaultSaveLocation = line;
            }
            catch
            {
                reader.Close();
                saveInternalSettings();
                return;
            }

            reader.Close();
        }

        public void saveInternalSettings()
        {
            StreamWriter writer = new StreamWriter(internalSettingsFileName);
            writer.WriteLine(defaultProfile);
            writer.WriteLine(defaultControlledProfile);
            writer.WriteLine(controlled);
            writer.WriteLine(controlledProfilesFilePath);            
            writer.WriteLine(defaultSaveLocation);

            writer.Close();
        }

        public void loadProfiles()
        {
            string filePath;
            if (controlled)
            {
                filePath = controlledProfilesFilePath;
            }
            else
            {
                filePath = internalProfilesFilePath;
            }
            StreamReader reader;
            try
            {
                reader = new StreamReader(filePath);
            }
            catch
            {
                if (controlled)
                {
                    controlled = false;
                    loadProfiles();
                    throw new FileNotFoundException();
                }
                else if (filePath != "" && filePath != null)
                {
                    saveProfiles();
                }
                else
                {
                    throw new System.ArgumentException("FilePath is Empty String or Null.");
                }
            }
            reader = new StreamReader(filePath);
            String line = reader.ReadLine();
            // Ignore comments and instructions in settings file.
            if (line != null && line != "")
            {
                while (line.Split(',')[0] == "//")
                {
                    line = reader.ReadLine();
                }
            }
            // Read and parse each line into the profiles object.
            profiles.Clear();
            profiles = new SortedDictionary<String, Settings>();
            while (line != null && line != "")
            {
                string[] values = line.Split(',');
                if (values.Length > 8 && values[0] != "//")
                {
                    Settings settings = new Settings(values);
                    String key = values[0];
                    try
                    {
                        profiles.Add(key, settings);
                    }
                    catch
                    {
                        profiles[key] = settings;
                    }
                }
                line = reader.ReadLine();
            }
            reader.Close();
        }

        public void saveProfiles()
        {
            if (!controlled)
            {
                StreamWriter writer = new StreamWriter(internalProfilesFilePath);
                StringBuilder sb = new StringBuilder();
                sb.Append(commentsAndInstructions);
                foreach (KeyValuePair<String, Settings> profile in profiles)
                {
                    sb.Append(profile.Key + ",");
                    sb.Append(profile.Value.toString() + "\n");
                }
                // If this isn't written line by line, then the '\n' characters are present but invisible in the txt file.
                // The functionality is unaffected, but the readability is not acceptable.
                string[] lines = sb.ToString().Split('\n');
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }

                writer.Close();
            }
        }

        public void addProfile(Settings settings, String key)
        {
            try
            {
                profiles.Add(key, settings);
            }
            catch
            {
                profiles[key] = settings;
            }
            saveProfiles();
        }

        public Settings getProfile(String key)
        {
            loadProfiles();
            Settings newSet = new Settings();
            if (key == "")
            {
                return new Settings();
            }
            try
            {
                newSet = new Settings(profiles[key]);
            }
            catch (KeyNotFoundException)
            {
                if (controlled)
                {
                    defaultControlledProfile = "";
                }
                else
                {
                    defaultProfile = "";
                }
            }
            return newSet;
        }

        public String[] getKeys()
        {
            String[] keys = new String[profiles.Count];
            int count = 0;
            foreach(KeyValuePair<String, Settings> profile in profiles)
            {
                keys[count] = profile.Key;
                count++;
            }

            return keys;
        }

        public void removeProfile(String key)
        {
            bool deleted = profiles.Remove(key);
            if (!deleted)
            {
                throw new System.ArgumentException("Profile Not Deleted");
            }
            setDefaultProfileName("");
            saveInternalSettings();
        }

        public bool isControlled()
        {
            return controlled;
        }

        public void setControlled(bool isControlled)
        {
            controlled = isControlled;
            saveInternalSettings();
            try
            {
                loadProfiles();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string getControlledProfilesFilePath()
        {
            return controlledProfilesFilePath;
        }

        public void setControlledProfilesFilePath(string path)
        {
            controlledProfilesFilePath = path;
            saveInternalSettings();
            loadProfiles();
        }

        public string getDefaultProfileName()
        {
            if(controlled){
                return defaultControlledProfile;
            }
            else
            {
                return defaultProfile;
            }            
        }

        public void setDefaultProfileName(String name)
        {
            if (name == null)
            {
                name = "";
            }
            if (controlled)
            {
                defaultControlledProfile = name;
            }
            else
            {
                defaultProfile = name;
            }
            saveInternalSettings();
        }
    }
    
}
