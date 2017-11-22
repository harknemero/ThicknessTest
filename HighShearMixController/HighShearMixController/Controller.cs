﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighShearMixController
{
    class Controller
    {
        private ThermometerControl therm;
        private VFDriveControl drive;
        private int alarmLevel;
        private double predictedEqSpeed;
        private double currentSpeed;
        private double currentTemp;
        private bool manual;
        private bool automatic;
        private bool thermConn;
        private bool driveConn;
        private bool targetTempChanged;
        private bool manualSpeedChanged;

        private static int maxAlarmLevel = 2;

        public bool Manual { get => manual; set => manual = value; }
        public bool Automatic { get => automatic; set => automatic = value; }
        public bool ThermConn { get => thermConn; set => thermConn = value; }
        public bool DriveConn { get => driveConn; set => driveConn = value; }
        public static int MaxAlarmLevel { get => maxAlarmLevel;}
        public double CurrentTemp { get => currentTemp; set => currentTemp = value; }
        public bool ManualSpeedChanged { get => manualSpeedChanged; set => manualSpeedChanged = value; }
        public bool TargetTempChanged { get => targetTempChanged; set => targetTempChanged = value; }
        public double PredictedEqSpeed { get => predictedEqSpeed; set => predictedEqSpeed = value; }
        public double CurrentSpeed { get => currentSpeed; set => currentSpeed = value; }

        public Controller()
        {
            therm = new ThermometerControl();
            drive = new VFDriveControl();
            alarmLevel = 0;
            Manual = false;
            Automatic = false;
            manualSpeedChanged = true;
            targetTempChanged = true;
            //thermConn = therm.isConnected();
            //driveConn = drive.isConnected();
        }

        // Start Mixer
        public bool startDrive()
        {
            bool result = false;
            setSpeed();
            result = drive.start();

            return result;
        }

        // Stop Mixer
        public bool stopDrive()
        {
            bool result = false;
            result = drive.stop();
            ManualSpeedChanged = true;

            return result;
        }

        // Gets current temperature from Thermometer Controller
        public float getTemp()
        {
            float temp = therm.getTemp();

            return temp;
        }

        // Sets the speed of the VF Drive based on whether Auto or Manual is activated
        // If Auto is activated, then it sets the speed based on the predicted equilibrium speed as
        // as well as on the difference between the actual and target temperatures.
        public bool setSpeed()
        {
            bool result = false;
            if (!automatic && ManualSpeedChanged)
            {
                result = drive.setSpeed(Properties.Settings.Default.ManualSpeed);
                ManualSpeedChanged = false;
            }
            else if (automatic)
            {

                if(currentTemp < Properties.Settings.Default.TargetTemperature - 2)
                {
                    result = drive.setSpeed(Properties.Settings.Default.MaxSpeed);
                }
                else
                {
                    // Every degree difference between target and actual results in a 5% offset.
                    // The offset overcorrects for differences in order to get actual on target faster.
                    double offset = (1 + ((Properties.Settings.Default.TargetTemperature - currentTemp) / 20));

                    // If the actual temp is higher than the target temp, then this offset is multiplied
                    // by 2 + the difference in degrees.
                    // A difference of 1 degree = -15%     2 = -40%     3 = -75%
                    if(currentTemp > Properties.Settings.Default.TargetTemperature)
                    {
                        offset = 1 - (1 - offset) * (2 + currentTemp - Properties.Settings.Default.TargetTemperature);
                    }

                    if (PredictedEqSpeed * offset >= Properties.Settings.Default.MaxSpeed)
                    {
                        result = drive.setSpeed(Properties.Settings.Default.MaxSpeed);
                    }
                    else if(PredictedEqSpeed * offset <= Properties.Settings.Default.MinimumAutoSpeed)
                    {
                        result = drive.setSpeed(Properties.Settings.Default.MinimumAutoSpeed);
                    }
                    else
                    {
                        result = drive.setSpeed(PredictedEqSpeed * offset);
                    }
                }
            }

            return result;
        }

        public void getCurrentSpeed()
        {
            currentSpeed = drive.getCurrentSpeed();
        }

        public double getPredictedEqSpeed()
        {
            return PredictedEqSpeed;
        }

        private void calculateEqSpeed()
        {
            double eqSpeed = 40;

            PredictedEqSpeed = eqSpeed;
        }

        public void setAlarmLevel(int level)
        {
            alarmLevel = level;
            if(level < 0)
            {
                alarmLevel = 0;
            }
            else if (level > MaxAlarmLevel)
            {
                alarmLevel = MaxAlarmLevel;
            }
            
        }

        public int getAlarmLevel()
        {
            return alarmLevel;
        }

        public string getDriveWarning()
        {
            return drive.Warning;
        }

        public bool checkThermConn()
        {
            bool result = therm.isConnected();
            //result = true; // ********************************************* for testing
            ThermConn = result;

            return result; 
        }

        public bool checkDriveConn()
        {
            if(drive == null)
            {
                return drive.openDrive();
            }
            bool result = drive.isConnected();
            //result = true; // ********************************************* for testing
            DriveConn = result;

            return result; 
        }

        private void initializeDrive()
        {
            drive.initialize();
        }
    }
}
