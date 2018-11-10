using System;
using System.Collections.Generic;
using Pololu.UsbWrapper;

namespace Pololu.SimpleMotorControllerG2
{
    /// <summary>
    /// This variable represents a native USB connection to a Simple Motor Controller.
    /// It contains functions taht allow you to easily send any of the native USB commands
    /// that the device supports.
    /// </summary>

    public class Smc : UsbDevice
    {
        /// <summary>
        /// This is the GUID used in Windows to identify the native USB interfaces of all
        /// of the Simple Motor Controllers.  It comes from smc.inf.
        /// </summary>
        public static Guid deviceInterfaceGuid = new Guid("fc726873-3627-47fb-b297-4f81ca88f7a9");

        /// <summary>
        /// This is the USB Vendor ID (Pololu Corporation).
        /// </summary>
        public static UInt16 vendorID = 0x1ffb;

        /// <summary>
        /// These are the USB product IDs of the Simple Motor Controllers.
        /// </summary>
        public static UInt16[] productIDs = new UInt16[] {
            0xA3, 0xA5, 0xA7, 0xA9
        };

        /// <summary>
        /// These are the USB product IDs for the bootloaders of the Simple Motor Controllers.
        /// </summary>
        public static UInt16[] bootloaderProductIDs = new UInt16[] {
            0xA2, 0xA4, 0xA6, 0xA8
        };

        /// <summary>
        /// The channels that are available on this device.
        /// </summary>
        public static SmcChannel[] channels = new SmcChannel[] { SmcChannel.Rc1, SmcChannel.Rc2, SmcChannel.Analog1, SmcChannel.Analog2 };

        /// <summary>
        /// The generic name of this type of device.
        /// </summary>
        public static String name
        {
            get
            {
                return "Simple Motor Controller";
            }
        }

        /// <summary>
        /// The plural generic name of this type of device.
        /// </summary>
        public static String namePlural
        {
            get
            {
                return "Simple Motor Controllers";
            }
        }

        /// <summary>
        /// Link to user's guide.
        /// </summary>
        public static string documentationUrl
        {
            get
            {
                return "https://www.pololu.com/docs/0J77";
            }
        }

        /// <summary>
        /// This function defines how we will display lists of these devices/bootloaders, both
        /// in SmcCmd and in the Simple Motor Control Center and in the Firmware Upgrade form.
        /// </summary>
        public static void setDeviceListItemText(DeviceListItem dli)
        {
            dli.text = productIdToShortModelString(dli.productId) + " #" + dli.serialNumber;
        }

        /// <summary>
        /// Coverts a USB product ID to a short string describing the device.
        /// </summary>
        public static String productIdToShortModelString(UInt16 productId)
        {
            // Warning: This is used for settings files, so don't change it lightly.
            switch (productId)
            {
                case 0xA2: return "18v15 bootloader";
                case 0xA3: return "18v15";
                case 0xA4: return "24v12 bootloader";
                case 0xA5: return "24v12";
                case 0xA6: return "18v25 bootloader";
                case 0xA7: return "18v25";
                case 0xA8: return "24v19 bootloader";
                case 0xA9: return "24v19";
                default: return "Unknown (" + productId + ")";
            }
        }

        /// <summary>
        /// The inverse of productIdToShortModelString.
        /// </summary>
        public static UInt16 shortModelStringToProductId(String s)
        {
            // Warning: This is used for settings files, so don't change it lightly.
            switch (s)
            {
                case "18v15": return 0xA3;
                case "24v12": return 0xA5;
                case "18v25": return 0xA7;
                case "24v19": return 0xA9;
                default: return 0;
            }
        }

        /// <summary>
        /// Converts a USB product ID to the official, long string describing
        /// the device.
        /// </summary>
        public static String productIdToLongModelString(UInt16 productId)
        {
            // These should be the same as the USB product string descriptors.
            switch (productId)
            {
                case 0xA3: return "Pololu High-Power Simple Motor Controller G2 18v15";
                case 0xA5: return "Pololu High-Power Simple Motor Controller G2 24v12";
                case 0xA7: return "Pololu High-Power Simple Motor Controller G2 18v25";
                case 0xA9: return "Pololu High-Power Simple Motor Controller G2 24v19";
                default: return "Unknown (" + productId + ")";
            }
        }

        /// <summary>
        /// The USB product ID.
        /// </summary>
        public readonly UInt16 productId;

        /// <summary>
        /// Opens (connects to) the selected device, constructing an object that
        /// we can use to communicate with the device over its native USB interface.
        /// </summary>
        public Smc(DeviceListItem deviceListItem) : base(deviceListItem)
        {
            this.productId = getProductID();
        }

        UInt16 getRsenseMilliohms()
        {
            switch (productId)
            {
                case 0xA3: return 2;
                case 0xA5: return 3;
                case 0xA7: return 1;
                case 0xA9: return 2;
                default: return 0;
            }
        }

        /// <summary>
        /// Gets a list of devices that are currently connected to USB.
        /// </summary>
        public static List<DeviceListItem> getConnectedDevices()
        {
            List<DeviceListItem> devices;
            try
            {
                try
                {
                    devices = UsbDevice.getDeviceList(Smc.deviceInterfaceGuid);
                }
                catch (NotImplementedException)
                {
                    // use vendor and product instead
                    devices = UsbDevice.getDeviceList(Smc.vendorID, Smc.productIDs);
                }
            }
            catch (Exception e)
            {
                throw new Exception("There was an error getting the list of connected devices.", e);
            }

            foreach (DeviceListItem dli in devices)
            {
                Smc.setDeviceListItemText(dli);
            }

            return devices;
        }

        /// <summary>
        /// Gets a list of bootloaders that are currently connected to USB.
        /// </summary>
        public static List<DeviceListItem> getConnectedBootloaders()
        {
            Guid bootloaderGuid = new Guid("82959cfa-7a2d-431f-a9a1-500b55d90950");

            List<DeviceListItem> bootloaders;
            try
            {
                List<UInt16> bootloaderProductIDs = new List<UInt16>(Smc.bootloaderProductIDs);
                List<DeviceListItem> allBootloaders = UsbDevice.getDeviceList(bootloaderGuid);
                bootloaders = new List<DeviceListItem>();
                foreach (DeviceListItem dli in allBootloaders)
                {
                    if (bootloaderProductIDs.Contains(dli.productId)){ bootloaders.Add(dli); }
                }
            }
            catch (NotImplementedException)
            {
                // use vendor and product instead
                bootloaders = UsbDevice.getDeviceList(0x1FFB, Smc.bootloaderProductIDs);
            }

            foreach (DeviceListItem dli in bootloaders)
            {
                Smc.setDeviceListItemText(dli);
            }

            return bootloaders;
        }

        UInt16 privateFirmwareVersion = 0;
        Byte privateFirmwareVersionMajor;
        Byte privateFirmwareVersionMinor;

        /// <summary>
        /// Gets the minor version number of the firmware (the number after the decimal point).
        /// </summary>
        public Byte getFirmwareVersionMinor()
        {
            getFirmwareVersion();
            return privateFirmwareVersionMinor;
        }

        /// <summary>
        /// Gets the major version number of the firmware (the number before the decimal point).
        /// </summary>
        public Byte getFirmwareVersionMajor()
        {
            getFirmwareVersion();
            return privateFirmwareVersionMajor;
        }

        /// <summary>
        /// Returns a string representing the firmware version, like "1.00".
        /// </summary>
        public String getFirmwareVersionString()
        {
            return getFirmwareVersionMajor().ToString() + "." + getFirmwareVersionMinor().ToString("D2");
        }

        /// <summary>
        /// Gets the firmware version as a number (e.g. 0x106 for version 1.06).
        /// </summary>
        public UInt16 getFirmwareVersion()
        {
            if (privateFirmwareVersion != 0) { return privateFirmwareVersion; }

            Byte[] buffer = new Byte[14];

            // Note: It would be nice to get this info from our WinUSB layer instead of having to do
            // an extra USB request here.

            try
            {
                // Get the device descriptor.
                controlTransfer(0x80, 6, 0x0100, 0x0000, buffer);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error getting the firmware version from the device.", exception);
            }

            privateFirmwareVersionMinor = (Byte)((buffer[12] & 0xF) + (buffer[12] >> 4 & 0xF) * 10);
            privateFirmwareVersionMajor = (Byte)((buffer[13] & 0xF) + (buffer[13] >> 4 & 0xF) * 10);
            privateFirmwareVersion = (UInt16)(buffer[12] + buffer[13] * 256);
            return privateFirmwareVersion;
        }

        /// <summary>
        /// Causes the device to disconnect and enter bootloader mode so you can
        /// upgrade its firmware.  After calling this, you should call Dispose()
        /// and delete any references you have to this object because it will not
        /// be usable anymore.
        /// </summary>
        public void startBootloader()
        {
            try
            {
                controlTransfer(0x40, (byte)SmcRequest.StartBootloader, 0, 0);
            }
            catch (Exception e)
            {
                throw new Exception("There was an error entering bootloader mode.", e);
            }
        }

        /// <summary>
        /// Resets the device to its default, factory settings.
        /// This takes about 26 ms.
        /// </summary>
        public void resetSettings()
        {
            try
            {
                controlTransfer(0x40, (byte)SmcRequest.ResetSettings, 0, 0);
            }
            catch (Exception e)
            {
                throw new Exception("There was an error resetting the device to its default settings.", e);
            }
        }

        /// <summary>
        /// Fixes certain things about a setttings object so that it doesn't make the device
        /// do something invalid.
        /// For each thing that gets fixed, a warning is added to the warnings list that is passed in.
        /// </summary>
        /// <param name="newSettings">The settings to fix.</param>
        /// <param name="warnings">A list of warnings.  This function will add items to the list.</param>
        /// <param name="productId">The product ID of the device these settings will be used for.</param>
        /// <param name="firmwareVersion">The firmware version of the device these settings will be used for.
        /// If unknown, this argument should be 0.</param>
        public static void fixSettings(SmcSettings newSettings, List<string> warnings, UInt16 productId, UInt16 firmwareVersion)
        {
            if (productId == 0)
            {
                throw new Exception("Internal error: an invalid product ID was passed to fixSettings.");
            }

            if (newSettings.productId != productId)
            {
                throw new Exception(
                    "These settings are for a different device.  " +
                    "The settings are for the " + Smc.productIdToShortModelString(newSettings.productId) + ", " +
                    "not the " + Smc.productIdToShortModelString(productId) + ".");
            }

            // TODO: change the messages here to use present tense and future tense (like the Tic and Jrk).

            if (newSettings.overTempCompleteShutoffThreshold < newSettings.overTempNormalOperationThreshold)
            {
                warnings.Add(
                    "The over-temperature complete shutoff threshold was " +
                    "lower than the over-temperature normal operation threshold.  " +
                    "Both settings will be set to " +
                    Smc.temperatureToString(newSettings.overTempCompleteShutoffThreshold) +
                    " so the motor will shut off at that temperature.");
                newSettings.overTempNormalOperationThreshold = newSettings.overTempCompleteShutoffThreshold;
            }

            if (newSettings.lowVinStartupMv < newSettings.lowVinShutoffMv)
            {
                if (newSettings.lowVinShutoffMv + 500 > UInt16.MaxValue)
                {
                    newSettings.lowVinStartupMv = UInt16.MaxValue;
                }
                else
                {
                    newSettings.lowVinStartupMv = (UInt16)(newSettings.lowVinShutoffMv + 500);
                }
                warnings.Add("The Low VIN Startup voltage was lower than the Low VIN Shutoff voltage (" + newSettings.lowVinShutoffMv/(decimal)1000 + " V).  " +
                    "The Low VIN Startup voltage will be changed to " + newSettings.lowVinStartupMv/(decimal)1000 + " V.");
            }

            if (newSettings.highVinShutoffMv < newSettings.lowVinStartupMv)
            {
                newSettings.highVinShutoffMv = (new SmcSettings(productId).highVinShutoffMv);
                warnings.Add("The High VIN Shutoff voltage was lower than the Low VIN Startup voltage (" + newSettings.lowVinStartupMv / (decimal)1000 + " V).  " +
                    "The High VIN Shutoff voltage will be changed to " + newSettings.highVinShutoffMv / (decimal)1000 + " V.");
            }

            if (newSettings.vinScaleCalibration > 1500)
            {
                newSettings.vinScaleCalibration = 1500;
                warnings.Add("The VIN scale calibration was too high.  It will be changed to 1500.");
            }

            if (newSettings.currentScaleCalibration > 20000)
            {
                newSettings.currentScaleCalibration = 20000;
                warnings.Add("The current scale calibration was too high.  It will be changed to 20000.");
            }

            // Prevent the channel scaling values from being out of order (it's okay if they are equal)
            foreach(SmcChannel channel in Smc.channels)
            {
                SmcChannelSettings cs = newSettings.getChannelSettings(channel);
                if (cs.errorMin > cs.inputMin ||
                    cs.inputMin > cs.inputNeutralMin ||
                    cs.inputNeutralMin > cs.inputNeutralMax ||
                    cs.inputNeutralMax > cs.inputMax ||
                    cs.inputMax > cs.errorMax)
                {
                    warnings.Add("The scaling values for " + channel.name() + " are out of order.  They will be reset to their default settings.");

                    SmcChannelSettings defaults = SmcChannelSettings.defaults(channel);
                    cs.errorMin = defaults.errorMin;
                    cs.inputMin = defaults.inputMin;
                    cs.inputNeutralMin = defaults.inputNeutralMin;
                    cs.inputNeutralMax = defaults.inputNeutralMax;
                    cs.inputMax = defaults.inputMax;
                    cs.errorMax = defaults.errorMax;
                }
            }

            fixMotorLimits(newSettings.forwardLimits, "forward", warnings);
            fixMotorLimits(newSettings.reverseLimits, "reverse", warnings);
        }

        private static void fixMotorLimits(SmcMotorLimits ml, string directionName, List<String> warnings)
        {
            if (ml.maxSpeed > 0 && ml.startingSpeed > ml.maxSpeed)
            {
                warnings.Add("The " + directionName + " max speed was non-zero and smaller than the " + directionName + " starting speed.  " +
                    "The " + directionName + " max speed will be changed to 0 and the motor will not be " +
                    "able to drive in the " + directionName + " direction.");
                ml.maxSpeed = 0;
            }

            if (ml.maxSpeed > 3200)
            {
                warnings.Add("The " + directionName + " max speed was greater than 3200 (100%) so it will be changed to 3200.");
                ml.maxSpeed = 3200;
            }

            if (ml.maxAcceleration > 3200)
            {
                warnings.Add("The " + directionName + " max acceleration was greater than 3200 so it will be changed to 3200.");
                ml.maxAcceleration = 3200;
            }

            if (ml.maxDeceleration > 3200)
            {
                warnings.Add("The " + directionName + " max deceleration was greater than 3200 so it will be changed to 3200.");
                ml.maxDeceleration = 3200;
            }
        }

        /// <summary>
        /// Reads the current settings from the device.
        /// </summary>
        public unsafe SmcSettings getSmcSettings()
        {
            SmcSettingsStruct settingsStruct = new SmcSettingsStruct();
            UInt16 settingsSize = (UInt16)sizeof(SmcSettingsStruct);
            try
            {
                controlTransfer(0xC0, (byte)SmcRequest.GetSettings, 0, 0, &settingsStruct, settingsSize);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error reading settings from the device.", exception);
            }

            return new SmcSettings(productId, settingsStruct);
        }

        /// <summary>
        /// Temporarily sets a motor limit.  This change will last until the
        /// next time the device resets, or until another setMotorLimit command
        /// is issued which changes the limit.  This function will not violate
        /// the hard motor limits that are stored in flash (with setSmcSettings).
        /// </summary>
        public SmcSetMotorLimitProblem setMotorLimit(SmcMotorLimit limit, UInt16 value)
        {
            Byte[] buffer = new Byte[1]{ 0 };

            // This should be the same as the code in actionSetMotorLimit in SmcCmd.
            if ((byte)limit >= 12)
            {
                throw new ArgumentException("Invalid limit ID.", "limit");
            }

            if ((limit & (SmcMotorLimit)3) == SmcMotorLimit.BrakeDuration)
            {
                // Divide by 4 because the user will specify it in ms but the device expects
                // it in units of 4ms for this command.  Round up to err on the side of making
                // it safer (that's the +3).
                value = (UInt16)((value + 3) / 4);
            }
            else if (value > 3200)
            {
                throw new ArgumentOutOfRangeException("value", "Maximum value allowed for MaxAcceleration, MaxDeceleration, or MaxSpeed is 3200.");
            }

            try
            {
                UInt32 lengthTransferred = controlTransfer(0xC0, (byte)SmcRequest.SetMotorLimit, value, (byte)limit, buffer);
                if (lengthTransferred != 1)
                {
                    throw new Exception("Expected to receive 1 byte but received " + lengthTransferred + " bytes.");
                }
                return (SmcSetMotorLimitProblem)buffer[0];
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error clearing the safe start violation.", exception);
            }
        }

        /// <summary>
        /// Clears the Safe Start Violation bit in the SmcVariables.errorStatus.
        /// This function has no effect unless the input mode of the device is SerialUsb.
        /// </summary>
        public void exitSafeStart()
        {
            try
            {
                controlTransfer(0x40, (byte)SmcRequest.ExitSafeStart, 0, 0);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error clearing the safe start violation.", exception);
            }
        }

        /// <summary>
        /// Sends a new set of settings to the device, to be written to flash.
        /// This takes about 26 ms.
        /// </summary>
        public unsafe void setSmcSettings(SmcSettings settings)
        {
            SmcSettingsStruct settingsStruct = settings.convertToStruct();
            try
            {
                controlTransfer(0x40, (byte)SmcRequest.SetSettings, 0, 0, &settingsStruct, (UInt16)sizeof(SmcSettingsStruct));
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error writing settings to the device.", exception);
            }
        }

        /// <summary>
        /// Gets the current state of the device.
        /// </summary>
        public unsafe SmcVariables getSmcVariables()
        {
            SmcVariables vars = new SmcVariables();
            try
            {
                // Note: In later versions of this software, this function should have
                // arguments that allow you to specify which of these flags to set.
                // For now, just set all of them, meaning that this request will clear
                // error occurred flags and clear the current chopping occurrence count.
                UInt16 flags = 3;
                controlTransfer(0xC0, (byte)SmcRequest.GetVariables, flags, 0, &vars, (UInt16)sizeof(SmcVariables));
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error reading variables from the device.", exception);
            }
            return vars;
        }

        SmcResetFlags? cachedResetFlags;

        /// <summary>
        /// Returns the cause of the device's last reset.
        /// </summary>
        public SmcResetFlags getResetFlags()
        {
            // We use caching because this value only changes when the device resets.
            // The Smc object represents a single USB connection and will have to be
            // discarded whenever the device resets, so there is no point in a single
            // Smc object fetching the reset flags twice from the device.
            if (cachedResetFlags.HasValue)
            {
                return cachedResetFlags.Value;
            }

            Byte[] buffer = new Byte[1];
            try
            {
                controlTransfer(0xC0, (Byte)SmcRequest.GetResetFlags, 0, 0, buffer);
                cachedResetFlags = (SmcResetFlags)buffer[0];
                return cachedResetFlags.Value;
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error getting the reset flags from the device.", exception);
            }
        }

        /// <summary>
        /// Returns the cause of the device's last reset.
        /// Provides the same information as getResetFlags, but in a human-friendly
        /// string instead of an enum.
        /// </summary>
        public String getResetCause()
        {
            switch (getResetFlags())
            {
                case SmcResetFlags.Power: return "Power-on reset";
                case SmcResetFlags.ResetPin: return "Reset (RST) pin driven low";
                case SmcResetFlags.Software: return "Software reset (bootloader)";
                case SmcResetFlags.Watchdog: return "Watchdog reset.  Report to Pololu.com.";
                default: return "Unknown code: " + ((byte)getResetFlags()).ToString("X2") + ".  Report to Pololu.com.";
            }
        }

        /// <summary>
        /// Creates a string for representing a temperature reading to the user.
        /// Warning: this string will contain non-ascii characters.
        /// </summary>
        public static String temperatureToString(UInt16 temperature)
        {
            if (temperature == 0)
            {
                return "\u22640 0 \u00B0C"; // less-than-or-equal-to zero degrees C
            }
            if (temperature == 3000)
            {
                return "Error";
            }
            return (temperature / 10M).ToString("F1") + " \u00B0C";
        }

        /// <summary>
        /// Creates an ASCII-only string for representing a temperature reading to the user.
        /// </summary>
        public static String temperatureToAsciiString(UInt16 temperature)
        {
            if (temperature == 0)
            {
                return "<= 0 C";
            }
            if (temperature == 3000)
            {
                return "Error";
            }
            return (temperature / 10M).ToString("F1") + " C";
        }

        /// <summary>
        /// Sets the state of the USB kill switch.
        /// True means activate the switch (stop the motor).
        /// False means deactive the switch (allow the motor to start).
        /// The state of the USB kill switch is available in the UsbKill bit of SmcVariables.limitStatus.
        /// </summary>
        public void setUsbKill(Boolean active)
        {
            try
            {
                controlTransfer(0x40, (byte)SmcRequest.UsbKill, (UInt16)(active ? 1 : 0), 0);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error setting the USB kill switch.", exception);
            }
        }

        /// <summary>
        /// Sets the state of the motor.  This only works if you are in Serial/USB input mode.
        /// </summary>
        /// <param name="speed">
        ///   If direction is Forward or Reverse, speed is a number from 0 to 3200.
        ///   If direction is Brake, speed is a number from 0 to 32.
        /// </param>
        /// <param name="direction">
        ///   The direction to drive the motor in: Forward, Reverse, or Brake.
        /// </param>
        private void setSpeed(UInt16 speed, SmcDirection direction)
        {
            // NOTE: This should be the same as the logic in the firmware (cmdSetSpeed).
            if (0 != (direction & SmcDirection.Brake))
            {
                if (speed > 32)
                {
                    throw new ArgumentOutOfRangeException("speed", "When braking, speed parameter must be between 0 and 32.");
                }
            }
            else
            {
                if (speed > 3200)
                {
                    throw new ArgumentOutOfRangeException("speed", "Speed parameter must be between 0 and 3200.");
                }
            }

            try
            {
                controlTransfer(0x40, (Byte)SmcRequest.SetSpeed, speed, (Byte)direction);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error setting the speed.", exception);
            }
        }

        /// <summary>
        /// Sets the state of the motor.  This overload only lets you drive the motor
        /// in forward or reverse.
        /// This function only works if you are in Serial/USB input mode.
        /// </summary>
        /// <param name="speed">
        /// A number between -3200 and 3200, where -3200 to -1 is
        /// in reverse direction and 0 to 3200 is forward direction.
        /// </param>
        public void setSpeed(Int16 speed)
        {
            if (speed > 3200 || speed < -3200)
            {
                throw new ArgumentOutOfRangeException("speed", "Speed parameter must be between -3200 and 3200.");
            }

            if (speed < 0)
            {
                setSpeed((UInt16)(-speed), SmcDirection.Reverse);
            }
            else
            {
                setSpeed((UInt16)speed, SmcDirection.Forward);
            }
        }

        /// <summary>
        /// Makes the motor immediately start coasting, ignoring deceleration limits.
        /// This function only works if you are in Serial/USB input mode.
        /// </summary>
        public void coast()
        {
            setSpeed(0, SmcDirection.Brake);
        }

        /// <summary>
        /// Makes the motor immediately start braking, ignoring deceleration limits.
        /// This function only works if you are in Serial/USB input mode.
        /// </summary>
        public void brake()
        {
            setSpeed(32, SmcDirection.Brake);
        }

        /// <summary>
        /// Stops the motor.  Has the same effect as the "Stop Motor" button in the control center.
        /// Equivalent to setUsbKill(true).
        /// </summary>
        public void stop()
        {
            setUsbKill(true);
        }

        /// <summary>
        /// Allows the motor start.  Has the same effect as the "Resume" button in the control center.
        /// Equivalent to setUsbKill(false) followed by exitSafeStart().
        /// </summary>
        public void resume()
        {
            setUsbKill(false);
            exitSafeStart();
        }

        /// <summary>
        /// Causes the motor to brake.
        /// </summary>
        /// <param name="brakeAmount">
        ///   A number from 0 to 32 specifying the amount of braking.
        ///   0 is nearly-full coasting.
        ///   32 is full braking.
        /// </param>
        public void setBrake(Byte brakeAmount)
        {
            setSpeed(brakeAmount, SmcDirection.Brake);
        }

        /// <summary>
        /// Sets the current limit.
        /// </summary>
        /// <param name="currentLimit">A raw current limit number between 0 and 3200.</param>
        public void setCurrentLimit(UInt16 currentLimit)
        {
            try
            {
                controlTransfer(0x40, (Byte)SmcRequest.SetCurrentLimit, currentLimit, 0);
            }
            catch (Exception exception)
            {
                throw new Exception("There was an error setting the current limit.", exception);
            }
        }

        const Int32 instructionFrequency = 72000000;

        /// <summary>
        /// Converts a baud rate from the value used in the baud rate
        /// register to bps.
        /// </summary>
        public static UInt32 convertBaudRegisterToBps(UInt16 baudRegister)
        {
            return (UInt32)(instructionFrequency / baudRegister);
        }

        /// <summary>
        /// Converts a baud rate from bps to a value suitable to be used
        /// in the baud rate register.
        /// </summary>
        public static UInt16 convertBpsToBaudRegister(UInt32 bps)
        {
            return (UInt16)((instructionFrequency + bps/2) / bps);
        }

        /// <summary>
        /// Converts a signed speed in to a percentage string (with a minus sign if needed).
        /// </summary>
        /// <param name="speed">A speed value from -3200 to 3200.</param>
        /// <returns>A string like "34.00 %".</returns>
        public static String speedToPercentString(Int16 speed)
        {
            return (speed / 32M).ToString("F2") + " %";
        }

        /// <summary>
        /// Converts an acceleration or deceleration limit to a user-friendly string.
        /// 0 is shown as "N/A".
        /// </summary>
        public static String accelDecelToString(UInt16 value)
        {
            if (value == 0)
            {
                return "N/A";
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Converts an RC period from SmcVariables to a user-friendly string.
        /// </summary>
        public static String rcPeriodToString(UInt16 period)
        {
            if (period >= 16)
            {
                // Display with millisecond precision because the tenths digit will always be 0.
                return (period / 10).ToString();
            }
            else if (period > 0)
            {
                // Display tenths of a millisecond precision.
                return (period / 10M).ToString("F1");
            }
            else
            {
                return "N/A";
            }
        }

        /// <summary>
        /// Given an instantaneous current sense or current limit voltage, calculates the
        /// instantaneous current through the sense resistor.
        /// </summary>
        /// <param name="voltage">A voltage in units of 3V3/2^16</param>
        /// <param name="offsetCalibration">The currentOffsetCalibration setting.</param>
        /// <param name="scaleCalibration">The currentScaleCalibration setting.</param>
        /// <param name="rsense">Sense resistor resistance in milliohms.</param>
        /// <returns>The corresponding current in units of mA/3200.</returns>
        static UInt32 calculateRsenseCurrentFromVoltage(UInt32 voltage,
            UInt16 offsetCalibration, UInt16 scaleCalibration,
            UInt16 rsense)
        {
            if (rsense == 0) { return 0; }
            if (offsetCalibration > voltage) { return 0; }
            return (UInt32)((voltage - offsetCalibration) * scaleCalibration / rsense);
        }

        /// <summary>
        /// Calculates the current limit in milliamps.
        /// </summary>
        /// <param name="currentLimitPwmValue">The currentLimit setting.</param>
        /// <param name="offsetCalibration">The currentOffsetCalibration setting.</param>
        /// <param name="scaleCalibration">The currentScaleCalibration setting.</param>
        /// <returns>The current limit in milliamps.</returns>
        public UInt32 calculateCurrentLimit(UInt16 currentLimitPwmValue,
            UInt16 offsetCalibration, UInt16 scaleCalibration)
        {
            return calculateRsenseCurrentFromVoltage(
                (UInt32)(currentLimitPwmValue * 0x10000 / 3200),
                offsetCalibration,
                scaleCalibration,
                getRsenseMilliohms()) / 3200;
        }

        public UInt16 findCurrentLimitPwmValue(UInt32 currentLimitMa,
            UInt16 offsetCalibration, UInt16 scaleCalibration)
        {
            UInt16 bestPwmValue = 0;
            UInt32 bestCurrentLimit = 0;
            UInt16 pwmValue = 0;
            while (pwmValue <= 3200)
            {
                UInt32 currentLimit = calculateCurrentLimit(pwmValue, offsetCalibration, scaleCalibration);
                if (currentLimit > bestCurrentLimit && currentLimit <= currentLimitMa)
                {
                    bestPwmValue = pwmValue;
                    bestCurrentLimit = currentLimit;
                }
                if (currentLimit > currentLimitMa)
                {
                    break;
                }
                pwmValue++;
            }
            return bestPwmValue;
        }
    }
}
