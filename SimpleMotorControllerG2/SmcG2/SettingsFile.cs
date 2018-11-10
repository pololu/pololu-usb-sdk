using System;
using System.Collections.Generic;
using System.IO;

namespace Pololu.SimpleMotorControllerG2
{
    /// <summary>
    /// A static class with methods for reading and writing configuration files.
    /// </summary>
    public static class SettingsFile
    {
        /// <summary>
        /// Parses a saved configuration file and returns a SmcSettings object.
        /// </summary>
        /// <param name="warnings">A list of warnings.  Whenever something goes
        /// wrong with the file loading, a warning will be added to this list.
        /// The warnings are not fatal; if the function returns it will return
        /// a valid SmcSettings object.
        /// </param>
        /// <param name="filename">The file to read from.</param>
        public static SmcSettings load(String filename, List<String> warnings)
        {
            using (FileStream settingsFile = File.Open(filename, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(settingsFile))
                {
                    return SettingsFile.load(sr, warnings);
                }
            }
        }

        /// <summary>
        /// Parses a saved configuration file and returns a SmcSettings object.
        /// </summary>
        /// <param name="warnings">A list of warnings.  Whenever something goes
        /// wrong with the file loading, a warning will be added to this list.
        /// The warnings are not fatal; if the function returns it will return
        /// a valid SmcSettings object.
        /// </param>
        /// <param name="sr">The file to read from.</param>
        public static SmcSettings load(StreamReader sr, List<String> warnings)
        {
            // For now, we just ignore the 'product' line in the settings file.
            // Note: In the next version of this software, we probably want to do things
            // differently here.  We would not have a productId argument.  We would get the
            // product from the settings file itself and use that to populate the default settings.
            // We would have a "fix_and_change_product" function that takes care of switching the
            // settings over to the new product (and firmware version) and fixing any issues in
            // it at the same time.

            var map = new Dictionary<string, string>();
            int lineNumber = 0;
            char[] separators = new[] { ':' }; 
            while (!sr.EndOfStream)
            {
                lineNumber++;
                string line = sr.ReadLine();
                if (line.Length == 0 || line.StartsWith("#")) { continue; }

                string[] parts = line.Split(separators);

                // Make sure we have the right number of parts and make sure the first part does not
                // contain invalid characters that also make our later error messages about it be
                // confusing.
                if (parts.Length != 2 || parts[0].Contains(" ") || parts[0].Contains("\""))
                {
                    string hint = "";
                    if (line.StartsWith("<!--"))
                    {
                        hint = "  Settings files saved from original Simple Motor Controller software are not supported.";
                    }
                    throw new Exception("Line " + lineNumber + " has an invalid format." + hint);
                }

                map[parts[0]] = parts[1].Trim();
            }

            UInt16 productId = Smc.shortModelStringToProductId(map["product"]);
            if (productId == 0)
            {
                throw new Exception("Invalid product name: \"" + map["product"] + "\".");
            }
            SmcSettings settings = new SmcSettings(productId);

            foreach (var item in map)
            {
                string value = item.Value;
                string key = item.Key;
                switch (key)
                {
                    case "product":
                        // Already processed.
                        break;
                    case "input_mode":
                        settings.inputMode = parseInputMode(value);
                        break;
                    case "mixing_mode":
                        settings.mixingMode = parseMixingMode(value);
                        break;
                    case "serial_mode":
                        settings.serialMode = parseSerialMode(value);
                        break;
                    case "enable_i2c":
                        settings.enableI2C = parseBool(key, value);
                        break;
                    case "serial_device_number":
                        settings.serialDeviceNumber = parseByte(key, value);
                        break;
                    case "crc_for_commands":
                        settings.crcForCommands = parseBool(key, value);
                        break;
                    case "crc_for_responses":
                        settings.crcForResponses = parseBool(key, value);
                        break;
                    case "uart_response_delay":
                        settings.uartResponseDelay = parseBool(key, value);
                        break;
                    case "use_fixed_baud_rate":
                        settings.useFixedBaudRate = parseBool(key, value);
                        break;
                    case "fixed_baud_rate":
                        settings.fixedBaudRateBps = parseU32(key, value);
                        break;
                    case "rc1_alternate_use":
                        settings.rc1.alternateUse = parseAlternateUse(key, value);
                        break;
                    case "rc1_invert":
                        settings.rc1.invert = parseBool(key, value);
                        break;
                    case "rc1_scaling_degree":
                        settings.rc1.scalingDegree = parseByte(key, value);
                        break;
                    case "rc1_error_min":
                        settings.rc1.errorMin = parseU16(key, value);
                        break;
                    case "rc1_input_min":
                        settings.rc1.inputMin = parseU16(key, value);
                        break;
                    case "rc1_input_neutral_min":
                        settings.rc1.inputNeutralMin = parseU16(key, value);
                        break;
                    case "rc1_input_neutral_max":
                        settings.rc1.inputNeutralMax = parseU16(key, value);
                        break;
                    case "rc1_input_max":
                        settings.rc1.inputMax = parseU16(key, value);
                        break;
                    case "rc1_error_max":
                        settings.rc1.errorMax = parseU16(key, value);
                        break;
                    case "rc2_alternate_use":
                        settings.rc2.alternateUse = parseAlternateUse(key, value);
                        break;
                    case "rc2_invert":
                        settings.rc2.invert = parseBool(key, value);
                        break;
                    case "rc2_scaling_degree":
                        settings.rc2.scalingDegree = parseByte(key, value);
                        break;
                    case "rc2_error_min":
                        settings.rc2.errorMin = parseU16(key, value);
                        break;
                    case "rc2_input_min":
                        settings.rc2.inputMin = parseU16(key, value);
                        break;
                    case "rc2_input_neutral_min":
                        settings.rc2.inputNeutralMin = parseU16(key, value);
                        break;
                    case "rc2_input_neutral_max":
                        settings.rc2.inputNeutralMax = parseU16(key, value);
                        break;
                    case "rc2_input_max":
                        settings.rc2.inputMax = parseU16(key, value);
                        break;
                    case "rc2_error_max":
                        settings.rc2.errorMax = parseU16(key, value);
                        break;
                    case "analog1_alternate_use":
                        settings.analog1.alternateUse = parseAlternateUse(key, value);
                        break;
                    case "analog1_pin_mode":
                        settings.analog1.pinMode = parsePinMode(key, value);
                        break;
                    case "analog1_invert":
                        settings.analog1.invert = parseBool(key, value);
                        break;
                    case "analog1_scaling_degree":
                        settings.analog1.scalingDegree = parseByte(key, value);
                        break;
                    case "analog1_error_min":
                        settings.analog1.errorMin = parseU16(key, value);
                        break;
                    case "analog1_input_min":
                        settings.analog1.inputMin = parseU16(key, value);
                        break;
                    case "analog1_input_neutral_min":
                        settings.analog1.inputNeutralMin = parseU16(key, value);
                        break;
                    case "analog1_input_neutral_max":
                        settings.analog1.inputNeutralMax = parseU16(key, value);
                        break;
                    case "analog1_input_max":
                        settings.analog1.inputMax = parseU16(key, value);
                        break;
                    case "analog1_error_max":
                        settings.analog1.errorMax = parseU16(key, value);
                        break;
                    case "analog2_alternate_use":
                        settings.analog2.alternateUse = parseAlternateUse(key, value);
                        break;
                    case "analog2_pin_mode":
                        settings.analog2.pinMode = parsePinMode(key, value);
                        break;
                    case "analog2_invert":
                        settings.analog2.invert = parseBool(key, value);
                        break;
                    case "analog2_scaling_degree":
                        settings.analog2.scalingDegree = parseByte(key, value);
                        break;
                    case "analog2_error_min":
                        settings.analog2.errorMin = parseU16(key, value);
                        break;
                    case "analog2_input_min":
                        settings.analog2.inputMin = parseU16(key, value);
                        break;
                    case "analog2_input_neutral_min":
                        settings.analog2.inputNeutralMin = parseU16(key, value);
                        break;
                    case "analog2_input_neutral_max":
                        settings.analog2.inputNeutralMax = parseU16(key, value);
                        break;
                    case "analog2_input_max":
                        settings.analog2.inputMax = parseU16(key, value);
                        break;
                    case "analog2_error_max":
                        settings.analog2.errorMax = parseU16(key, value);
                        break;
                    case "pwm_period_factor":
                        settings.pwmPeriodFactor = parseByte(key, value);
                        break;
                    case "motor_invert":
                        settings.motorInvert = parseBool(key, value);
                        break;
                    case "coast_when_off":
                        settings.coastWhenOff = parseBool(key, value);
                        break;
                    case "speed_update_period":
                        settings.speedUpdatePeriod = parseU16(key, value);
                        break;
                    case "forward_max_speed":
                        settings.forwardLimits.maxSpeed = parseU16(key, value);
                        break;
                    case "forward_max_acceleration":
                        settings.forwardLimits.maxAcceleration = parseU16(key, value);
                        break;
                    case "forward_max_deceleration":
                        settings.forwardLimits.maxDeceleration = parseU16(key, value);
                        break;
                    case "forward_brake_duration":
                        settings.forwardLimits.brakeDuration = parseU16(key, value);
                        break;
                    case "forward_starting_speed":
                        settings.forwardLimits.startingSpeed = parseU16(key, value);
                        break;
                    case "reverse_max_speed":
                        settings.reverseLimits.maxSpeed = parseU16(key, value);
                        break;
                    case "reverse_max_acceleration":
                        settings.reverseLimits.maxAcceleration = parseU16(key, value);
                        break;
                    case "reverse_max_deceleration":
                        settings.reverseLimits.maxDeceleration = parseU16(key, value);
                        break;
                    case "reverse_brake_duration":
                        settings.reverseLimits.brakeDuration = parseU16(key, value);
                        break;
                    case "reverse_starting_speed":
                        settings.reverseLimits.startingSpeed = parseU16(key, value);
                        break;
                    case "current_limit":
                        settings.currentLimit = parseU16(key, value);
                        break;
                    case "current_offset_calibration":
                        settings.currentOffsetCalibration = parseU16(key, value);
                        break;
                    case "current_scale_calibration":
                        settings.currentScaleCalibration = parseU16(key, value);
                        break;
                    case "min_pulse_period":
                        settings.minPulsePeriod = parseU16(key, value);
                        break;
                    case "max_pulse_period":
                        settings.maxPulsePeriod = parseU16(key, value);
                        break;
                    case "rc_timeout":
                        settings.rcTimeout = parseU16(key, value);
                        break;
                    case "consec_good_pulses":
                        settings.consecGoodPulses = parseByte(key, value);
                        break;
                    case "vin_scale_calibration":
                        settings.vinScaleCalibration = parseU16(key, value);
                        break;
                    case "temp_limit_gradual":
                        settings.tempLimitGradual = parseBool(key, value);
                        break;
                    case "over_temp_complete_shutoff_threshold":
                        settings.overTempCompleteShutoffThreshold = parseU16(key, value);
                        break;
                    case "over_temp_normal_operation_threshold":
                        settings.overTempNormalOperationThreshold = parseU16(key, value);
                        break;
                    case "low_vin_shutoff_timeout":
                        settings.lowVinShutoffTimeout = parseU16(key, value);
                        break;
                    case "low_vin_shutoff_mv":
                        settings.lowVinShutoffMv = parseU16(key, value);
                        break;
                    case "low_vin_startup_mv":
                        settings.lowVinStartupMv = parseU16(key, value);
                        break;
                    case "high_vin_shutoff_mv":
                        settings.highVinShutoffMv = parseU16(key, value);
                        break;
                    case "disable_safe_start":
                        settings.disableSafeStart = parseBool(key, value);
                        break;
                    case "ignore_pot_disconnect":
                        settings.ignorePotDisconnect = parseBool(key, value);
                        break;
                    case "ignore_err_line_high":
                        settings.ignoreErrLineHigh = parseBool(key, value);
                        break;
                    case "never_sleep":
                        settings.neverSleep = parseBool(key, value);
                        break;
                    case "command_timeout":
                        settings.commandTimeout = parseU16(key, value);
                        break;
                    default:
                        throw new Exception("Unrecognized key: \"" + key + "\".");
                }
            }

            // [Add-new-settings-here]

            // TODO: parse settings

            return settings;
        }

        private static SmcChannelAlternateUse parseAlternateUse(string key, string value)
        {
            switch (value)
            {
                case "none": return SmcChannelAlternateUse.None;
                case "limit_forward": return SmcChannelAlternateUse.LimitForward;
                case "limit_reverse": return SmcChannelAlternateUse.LimitReverse;
                case "kill_switch": return SmcChannelAlternateUse.KillSwitch;
            }
            throw new Exception("Invalid value for \"" + key + "\".");
        }

        private static SmcPinMode parsePinMode(string key, string value)
        {
            switch (value)
            {
                case "floating": return SmcPinMode.Floating;
                case "pull_down": return SmcPinMode.PullDown;
                case "pull_up": return SmcPinMode.PullUp;
            }
            throw new Exception("Invalid value for \"" + key + "\".");
        }

        private static UInt32 parseU32(string key, string value)
        {
            try
            {
                return UInt32.Parse(value);
            }
            catch
            {
                throw new Exception("Invalid value for \"" + key + "\".");
            }
        }

        private static Int16 parseS16(string key, string value)
        {
            try
            {
                return Int16.Parse(value);
            }
            catch
            {
                throw new Exception("Invalid value for \"" + key + "\".");
            }
        }

        private static UInt16 parseU16(string key, string value)
        {
            try
            {
                return UInt16.Parse(value);
            }
            catch
            {
                throw new Exception("Invalid value for \"" + key + "\".");
            }
        }

        private static Byte parseByte(string key, string value)
        {
            try
            {
                return Byte.Parse(value);
            }
            catch
            {
                throw new Exception("Invalid value for \"" + key + "\".");
            }
        }

        private static bool parseBool(string key, string value)
        {
            switch (value)
            {
                case "true": return true;
                case "false": return false;
            }
            throw new Exception("Invalid value for \"" + key + "\".");
        }

        private static SmcInputMode parseInputMode(string value)
        {
            switch (value)
            {
                case "serial": return SmcInputMode.SerialUsb;
                case "analog": return SmcInputMode.Analog;
                case "rc": return SmcInputMode.RC;
            }
            throw new Exception("Unrecognized input mode: \"" + value + "\".");
        }

        private static SmcMixingMode parseMixingMode(string value)
        {
            switch (value)
            {
                case "none": return SmcMixingMode.None;
                case "left": return SmcMixingMode.Left;
                case "right": return SmcMixingMode.Right;
            }
            throw new Exception("Unrecognized mixing mode: \"" + value + "\".");
        }

        private static SmcSerialMode parseSerialMode(string value)
        {
            switch (value)
            {
                case "ascii": return SmcSerialMode.Ascii;
                case "binary": return SmcSerialMode.Binary;
            }
            throw new Exception("Unrecognized serial mode: \"" + value + "\".");
        }

        /// <summary>
        /// Writes a SmcSettings object as text to the specified file.
        /// </summary>
        /// <param name="settings">The settings to read from.</param>
        /// <param name="filename">The file to write to.</param>
        /// <param name="device">The device that these settings came from.</param>
        public static void save(SmcSettings settings, string filename, Smc device)
        {
            using (FileStream settingsFile = File.Open(filename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(settingsFile))
                {
                    save(settings, sw, device);
                }
            }
        }

        /// <summary>
        /// Writes a SmcSettings object as text to the specified streamwriter.
        /// </summary>
        /// <param name="settings">The settings to read from.</param>
        /// <param name="sw">The file to write to.</param>
        /// <param name="device">The device that these settings came from.</param>
        public static void save(SmcSettings settings, StreamWriter sw, Smc device)
        {
            sw.WriteLine("# Pololu Simple Motor Controller G2 settings file.");
            sw.WriteLine("# " + Smc.documentationUrl);
            sw.WriteLine("product: " + Smc.productIdToShortModelString(device.productId));

            // [Add-new-settings-here]

            writeInputMode(sw, settings.inputMode);
            writeMixingMode(sw, settings.mixingMode);
            writeSerialMode(sw, settings.serialMode);
            writeBool(sw, "enable_i2c", settings.enableI2C);
            writeU32(sw, "serial_device_number", settings.serialDeviceNumber);
            writeBool(sw, "crc_for_commands", settings.crcForCommands);
            writeBool(sw, "crc_for_responses", settings.crcForResponses);
            writeBool(sw, "uart_response_delay", settings.uartResponseDelay);
            writeBool(sw, "use_fixed_baud_rate", settings.useFixedBaudRate);
            writeU32(sw, "fixed_baud_rate", settings.fixedBaudRateBps);

            // Channel settings
            writeChannelSettings(sw, "rc1", false, settings.rc1);
            writeChannelSettings(sw, "rc2", false, settings.rc2);
            writeChannelSettings(sw, "analog1", true, settings.analog1);
            writeChannelSettings(sw, "analog2", true, settings.analog2);

            // Motor settings
            writeU32(sw, "pwm_period_factor", settings.pwmPeriodFactor);
            writeBool(sw, "motor_invert", settings.motorInvert);
            writeBool(sw, "coast_when_off", settings.coastWhenOff);
            writeU32(sw, "speed_update_period", settings.speedUpdatePeriod);
            writeMotorLimits(sw, "forward", settings.forwardLimits);
            writeMotorLimits(sw, "reverse", settings.reverseLimits);

            writeU32(sw, "current_limit", settings.currentLimit);
            writeU32(sw, "current_offset_calibration", settings.currentOffsetCalibration);
            writeU32(sw, "current_scale_calibration", settings.currentScaleCalibration);
 
            // Advanced settings
            writeU32(sw, "min_pulse_period", settings.minPulsePeriod);
            writeU32(sw, "max_pulse_period", settings.maxPulsePeriod);
            writeU32(sw, "rc_timeout", settings.rcTimeout);
            writeU32(sw, "consec_good_pulses", settings.consecGoodPulses);
            writeS32(sw, "vin_scale_calibration", settings.vinScaleCalibration);

            writeBool(sw, "temp_limit_gradual", settings.tempLimitGradual);
            writeU32(sw, "over_temp_complete_shutoff_threshold", settings.overTempCompleteShutoffThreshold);
            writeU32(sw, "over_temp_normal_operation_threshold", settings.overTempNormalOperationThreshold);
            writeU32(sw, "low_vin_shutoff_timeout", settings.lowVinShutoffTimeout);
            writeU32(sw, "low_vin_shutoff_mv", settings.lowVinShutoffMv);
            writeU32(sw, "low_vin_startup_mv", settings.lowVinStartupMv);
            writeU32(sw, "high_vin_shutoff_mv", settings.highVinShutoffMv);

            writeBool(sw, "disable_safe_start", settings.disableSafeStart);
            writeBool(sw, "ignore_pot_disconnect", settings.ignorePotDisconnect);
            writeBool(sw, "ignore_err_line_high", settings.ignoreErrLineHigh);
            writeBool(sw, "never_sleep", settings.neverSleep);
            writeU32(sw, "command_timeout", settings.commandTimeout);
        }

        private static void writeChannelSettings(StreamWriter sw, string name, bool analog, SmcChannelSettings cs)
        {
            writeAlternateUse(sw, name + "_alternate_use", cs.alternateUse);
            if (analog)
            {
                writePinMode(sw, name + "_pin_mode", cs.pinMode);
            }
            writeBool(sw, name + "_invert", cs.invert);
            writeU32(sw, name + "_scaling_degree", cs.scalingDegree);
            writeU32(sw, name + "_error_min", cs.errorMin);
            writeU32(sw, name + "_input_min", cs.inputMin);
            writeU32(sw, name + "_input_neutral_min", cs.inputNeutralMin);
            writeU32(sw, name + "_input_neutral_max", cs.inputNeutralMax);
            writeU32(sw, name + "_input_max", cs.inputMax);
            writeU32(sw, name + "_error_max", cs.errorMax);
        }

        private static void writeMotorLimits(StreamWriter sw, string name, SmcMotorLimits ml)
        {
            writeU32(sw, name + "_max_speed", ml.maxSpeed);
            writeU32(sw, name + "_max_acceleration", ml.maxAcceleration);
            writeU32(sw, name + "_max_deceleration", ml.maxDeceleration);
            writeU32(sw, name + "_brake_duration", ml.brakeDuration);
            writeU32(sw, name + "_starting_speed", ml.startingSpeed);
        }

        private static void writeU32(StreamWriter sw, string name, UInt32 value)
        {
            sw.WriteLine(name + ": " + value);
        }

        private static void writeS32(StreamWriter sw, string name, Int32 value)
        {
            sw.WriteLine(name + ": " + value);
        }

        private static void writeBool(StreamWriter sw, string name, bool value)
        {
            sw.WriteLine(name + ": " + (value ? "true" : "false"));
        }

        private static void writeInputMode(StreamWriter sw, SmcInputMode mode)
        {
            string str = "";
            switch (mode)
            {
                case SmcInputMode.SerialUsb: str = "serial"; break;
                case SmcInputMode.RC: str = "rc"; break;
                case SmcInputMode.Analog: str = "analog"; break;
            }
            sw.WriteLine("input_mode: " + str);
        }

        private static void writeMixingMode(StreamWriter sw, SmcMixingMode mode)
        {
            string str = "";
            switch (mode)
            {
                case SmcMixingMode.None: str = "none"; break;
                case SmcMixingMode.Left: str = "left"; break;
                case SmcMixingMode.Right: str = "right"; break;
            }
            sw.WriteLine("mixing_mode: " + str);
        }

        private static void writeSerialMode(StreamWriter sw, SmcSerialMode mode)
        {
            string str = "";
            switch (mode)
            {
                case SmcSerialMode.Binary: str = "binary"; break;
                case SmcSerialMode.Ascii: str = "ascii"; break;
            }
            sw.WriteLine("serial_mode: " + str);
        }

        private static void writePinMode(StreamWriter sw, string name, SmcPinMode mode)
        {
            string str = "";
            switch (mode)
            {
                case SmcPinMode.Floating: str = "floating"; break;
                case SmcPinMode.PullDown: str = "pull_down"; break;
                case SmcPinMode.PullUp: str = "pull_up"; break;
            }
            sw.WriteLine(name + ": " + str);
        }

        private static void writeAlternateUse(StreamWriter sw, string name, SmcChannelAlternateUse use)
        {
            string str = "";
            switch (use)
            {
                case SmcChannelAlternateUse.None: str = "none"; break;
                case SmcChannelAlternateUse.LimitForward: str = "limit_forward"; break;
                case SmcChannelAlternateUse.LimitReverse: str = "limit_reverse"; break;
                case SmcChannelAlternateUse.KillSwitch: str = "kill_switch"; break;
            }
            sw.WriteLine(name + ": " + str);
        }
    }
}