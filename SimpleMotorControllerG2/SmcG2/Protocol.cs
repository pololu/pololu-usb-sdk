using System;
using System.Runtime.InteropServices;

// Disable the warning from Mono about how the RESERVED0 fields are unused.
#pragma warning disable 169

namespace Pololu.SimpleMotorControllerG2
{
    ///<summary>
    /// These are the values to put in to bRequest when making a setup packet
    /// for a control transfer to the Smc.  See the comments and code in Smc.cs
    /// for more information about what these requests do and the format of the
    /// setup packet.
    ///</summary>
    internal enum SmcRequest : byte
    {
        GetSettings = 0x81,
        SetSettings = 0x82,
        GetVariables = 0x83,
        ResetSettings = 0x84,
        GetResetFlags = 0x85,
        SetSpeed = 0x90,
        ExitSafeStart = 0x91,
        SetMotorLimit = 0x92,
        UsbKill = 0x93,
        SetCurrentLimit = 0x94,
        StartBootloader = 0xFF
    }

    public enum SmcBoolSettings1 : uint
    {
        NeverSleep = (1 << 0),
        UartResponseDelay = (1 << 1),
        UseFixedBaudRate = (1 << 2),
        DisableSafeStart = (1 << 3),
        EnableI2C = (1 << 4),
        IgnoreErrLineHigh = (1 << 5),
        TempLimitGradual = (1 << 6),
        IgnorePotDisconnect = (1 << 7),
        MotorInvert = (1 << 8),
        CoastWhenOff = (1 << 9),
        CrcForCommands = (1 << 10),
        CrcForResponses = (1 << 11),
    };

    /// <summary>Represents limits on the motor's behavior.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SmcMotorLimitsStruct
    {
        /// <summary>
        /// Max Speed is a number between 0 and 3200 that specifies the maximum speed at
        /// which the motor controller will ever drive the motor.
        /// </summary>
        public UInt16 maxSpeed;

        /// <summary>
        /// Max Acceleration is a number between 0 and 3200 that specifies how much the
        /// magnitude (absolute value) of the motor speed is allowed to increase every
        /// speed update period.  0 means no limit.
        /// </summary>
        public UInt16 maxAcceleration;

        /// <summary>
        /// Max Deceleration is a number between 0 and 3200 that specifies how much the
        /// magnitude (absolute value) of the motor speed is allowed to decrease every
        /// speed update period.  0 means no limit.
        /// </summary>
        public UInt16 maxDeceleration;

        /// <summary>
        /// Brake duration is the time, in milliseconds, that the motor controller will
        /// spend braking the motor (Current Speed = 0) before allowing the Current Speed
        /// to change signs.
        /// </summary>
        public UInt16 brakeDuration;

        /// <summary>
        /// Minimum non-zero speed (1-3200).  In RC or Analog mode, a scaled value of 1 maps to this speed.
        /// 0 means no effect.
        /// </summary>
        public UInt16 startingSpeed;

        private UInt16 RESERVED0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SmcChannelSettingsStruct
    {
        /// <summary>
        /// Used to invert the direction of scaling.
        /// If true, then a higher rawValue corresponds to lower scaledValue.
        /// </summary>
        public Boolean invert;

        /// <summary>
        /// 0 = Linear, 1 = Quadratic, 2 = Cubic, etc.
        /// </summary>
        public Byte scalingDegree;

        /// <summary>
        /// This is used to allow a channel be a limit switch if it is not controlling the motor.
        /// </summary>
        public SmcChannelAlternateUse alternateUse;

        /// <summary>
        /// Determines if the analog input is floating, pulled up, or pulled down.
        /// Does not apply to RC channels.
        /// </summary>
        public SmcPinMode pinMode;

        /// <summary>
        /// rawValues greater than this generate an error.
        /// </summary>
        public UInt16 errorMin;

        /// <summary>
        /// rawValues less than this generate an error.
        /// </summary>
        public UInt16 errorMax;

        /// <summary>
        /// rawValues greater than or equal to this get mapped to a
        /// speed of -reverseLimits.maxSpeed (or forwardLimits.maxSpeed
        /// if invert==true).
        /// </summary>
        public UInt16 inputMin;

        /// <summary>
        /// rawValues less than or equal to this get mapped to a
        /// speed of forwardLimits.maxSpeed (or -reverseLimits.maxSpeed
        /// if invert==true).
        /// </summary>
        public UInt16 inputMax;

        /// <summary>
        /// rawValues between inputNeutralMin and inputNeutralMax get mapped
        /// to a speed of zero.
        /// </summary>
        public UInt16 inputNeutralMin;

        /// <summary>
        /// rawValues between inputNeutralMin and inputNeutralMax get mapped
        /// to a speed of zero.
        /// </summary>
        public UInt16 inputNeutralMax;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SmcSettingsStruct
    {
        // [Add-new-settings-here]

        public SmcBoolSettings1 b1;

        public SmcInputMode inputMode;
        public SmcMixingMode mixingMode;
        public SmcSerialMode serialMode;
        public Byte serialDeviceNumber;

        public UInt16 fixedBaudRateRegister;
        public UInt16 speedUpdatePeriod;

        public UInt16 commandTimeout;
        public UInt16 rcTimeout;

        public UInt16 overTempCompleteShutoffThreshold;
        public UInt16 overTempNormalOperationThreshold;

        public Byte pwmPeriodFactor;
        public Byte consecGoodPulses;
        public Byte RESERVED0;
        public Byte RESERVED1;

        public UInt16 minPulsePeriod;
        public UInt16 maxPulsePeriod;

        public UInt16 vinScaleCalibration;
        public UInt16 lowVinShutoffTimeout;

        public UInt16 lowVinShutoffMv;
	    public UInt16 lowVinStartupMv;

        public UInt16 highVinShutoffMv;
        public UInt16 currentLimit;

        public UInt16 currentOffsetCalibration;
        public UInt16 currentScaleCalibration;

        public SmcChannelSettingsStruct rc1;
        public SmcChannelSettingsStruct rc2;
        public SmcChannelSettingsStruct analog1;
        public SmcChannelSettingsStruct analog2;

        public SmcMotorLimitsStruct forwardLimits;
        public SmcMotorLimitsStruct reverseLimits;
    }

    /// <summary>
    /// Represents the current state of an input channel.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SmcChannelVariables
    {
        /// <summary>
        /// The raw value of the channel as read from the pin.
        /// This is mainly useful for the control input setup wizard.
        /// 0xFFFF if disconnected but not affected by absolute max/min limits.
        /// Units of quarter-microseconds if an RC channel.
        /// 12-bit ADC reading (0-4095) if analog.
        /// 0xFFFF if input is disconnected.
        /// </summary>
        public UInt16 unlimitedRawValue;

        /// <summary>
        /// This is just like unlimitedRawValue except that it will be 0xFFFF
        /// if the absolute max/min limits are violated.
        /// </summary>
        public UInt16 rawValue;

        /// <summary>
        /// The result of scaling the rawValue.  This value
        /// depends on all the scaling settings in the channel's
        /// SmcChannelSettings struct.
        /// </summary>
        public Int16 scaledValue;

        private UInt16 RESERVED0;
    }

    /// <summary>
    /// Represents the current state of the device, including all input channels
    /// and motor limits.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SmcVariables
    {
        /// <summary>
        /// The errors that are currently happening.
        /// </summary>
        public SmcError errorStatus;

        /// <summary>
        /// The errors that occurred since the last time this register was cleared.
        /// </summary>
        public SmcError errorOccurred;

        /// <summary>
        /// The serial errors that occurred since the last time this register was
        /// cleared.
        /// </summary>
        public SmcSerialError serialErrorOccurred;

        /// <summary>
        /// Status bits for anything that is limiting the motor speed that isn't
        /// an error (but it could be caused by an error, such as StartedState).
        /// </summary>
        public SmcLimitStatus limitStatus;

        /// <summary>
        /// The current state of RC Channel 1.
        /// </summary>
        public SmcChannelVariables rc1;

        /// <summary>
        /// The current state of RC Channel 2.
        /// </summary>
        public SmcChannelVariables rc2;

        /// <summary>
        /// The current state of Analog Channel 1.
        /// </summary>
        public SmcChannelVariables analog1;

        /// <summary>
        /// The current state of Analog Channel 2.
        /// </summary>
        public SmcChannelVariables analog2;

        /// <summary>
        /// Target speed of motor from -3200 to 3200.
        /// </summary>
        public Int16 targetSpeed;

        /// <summary>
        /// Current speed of motor from -3200 to 3200.
        /// Can be non-zero even if power is off.
        /// </summary>
        public Int16 speed;

        /// <summary>
        /// Current braking amount, from 0 to 32.
        /// This value is only relevant when speed==0,
        /// otherwise it will be 0xFF.
        /// </summary>
        public UInt16 brakeAmount;

        /// <summary>
        /// The voltage on the VIN line in units of millivolts.
        /// </summary>
        public UInt16 vinMv;

        /// <summary>
        /// The reading from temperature sensor A, in units of one
        /// tenth of a degree Celsius.
        /// Temperatures below 0 degrees are reported as 0.
        /// Temperatures above 200 degrees are reported as 2000.
        /// 3000 is an error code.
        /// </summary>
        public UInt16 temperatureA;

        /// <summary>
        /// The reading from temperature sensor B, in units of one
        /// tenth of a degree Celsius.
        /// Temperatures below 0 degrees are reported as 0.
        /// Temperatures below 0 degrees are reported as 0.
        /// Temperatures above 200 degrees are reported as 2000.
        /// 3000 is an error code.
        /// </summary>
        public UInt16 temperatureB;

        /// <summary>
        /// The measured period of the RC pulses, in units of 0.1 ms.
        /// </summary>
        public UInt16 rcPeriod;

        /// <summary>
        /// Value from the device's baud-rate register.
        /// </summary>
        public UInt16 baudRateRegister;

        /// <summary>
        /// The time that the device has been running.  Units: 1 ms.
        /// </summary>
        public UInt32 timeMs;

        /// <summary>
        /// The currently-used motor limits for the forward direction.
        /// By default, these are equal to the hard limits in SmcSettingsStruct,
        /// but they can be temporarily changed by USB or Serial commands.
        /// </summary>
        public SmcMotorLimitsStruct forwardLimits;

        /// <summary>
        /// The currently-used motor limits for the reverse direction.
        /// By default, these are equal to the hard limits in SmcSettingsStruct,
        /// but they can be temporarily changed by USB or Serial commands.
        /// </summary>
        public SmcMotorLimitsStruct reverseLimits;

        /// <summary>
        /// The motor current limit, as a number from 0 to 3200.
        /// </summary>
        public UInt16 currentLimit;

        /// <summary>
        /// The voltage measured on the current sense line.
        /// 0xFFFF represents 3.3 V while 0 represents 0.
        /// </summary>
        public UInt16 rawCurrent;

        /// <summary>
        /// Measured motor current, in milliamps.
        /// </summary>
        public UInt16 current;

        /// <summary>
        /// The number of consecutive times that current chopping has been detected.
        /// (It is checked every 10 ms.)
        /// </summary>
        public UInt16 currentLimitingConsecutiveCount;

        /// <summary>
        /// The number of times current chopping has been detected since this variable
        /// was last cleared.  (It gets cleared when you read it over serial or USB.)
        /// </summary>
        public UInt16 currentLimitingOccurrenceCount;
    }

    /// <summary>
    /// The Input Mode setting for the device.  This specifies how the motor speed is determined.
    /// </summary>
    public enum SmcInputMode : byte
    {
        /// <summary>
        /// Motor speed is set via serial and USB commands.
        /// </summary>
        SerialUsb = 0,

        /// <summary>
        /// The voltage on A1/A2 determines the motor speed.
        /// </summary>
        Analog = 1,
        
        /// <summary>
        /// The width of pulses received on RC1 and RC2 determines the motor speed.
        /// </summary>
        RC = 2,
    }

    /// <summary>
    /// The Mixing Mode setting for the device.  This specifies which channels
    /// are used in calculating the motor speed and how the calculation is to
    /// be done.  The Mixing Mode only applies if the Input Mode is Analog or RC.
    /// </summary>
    public enum SmcMixingMode : byte
    {
        /// <summary>
        /// The motor speed is determined entirely by RC/Analog Channel 1.
        /// </summary>
        None = 0,

        /// <summary>
        /// The motor speed is determined by adding RC/Analog Channels 1 and 2.
        /// </summary>
        Left = 1,

        /// <summary>
        /// The motor speed is determined by subtracting RC/Analog Channel 2 from Channel 1.
        /// </summary>
        Right = 2,
    }

    /// <summary>
    /// Specifies what kinds of serial commands will be accepted.
    /// </summary>
    public enum SmcSerialMode : byte
    {
        /// <summary>
        /// Commands and responses support the Pololu, Compact, or Mini SSC binary protocols,
        /// specified in the user's guide.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// Commands and responses obey the ASCII command protocol,
        /// specified in the user's guide.
        /// </summary>
        Ascii = 1,
    }

    /// <summary>
    /// Specifies a motor direction: Forward, Reverse or Brake (for variable braking).
    /// </summary>
    public enum SmcDirection : byte
    {
        /// <summary>
        /// Forward.
        /// </summary>
        Forward = 0,

        /// <summary>
        /// Reverse (opposite of Forward).
        /// </summary>
        Reverse = 1,

        /// <summary>
        /// Brake.  Used for variable braking commands.
        /// </summary>
        Brake = 2,
    }

    /// <summary>
    /// Specifies the alternate use setting for a channel. This setting is
    /// only relevant if the channel is not configured to control the motor
    /// speed (see SmcInputMode and SmcMixingMode).
    /// </summary>
    public enum SmcChannelAlternateUse : byte
    {
        /// <summary>
        /// None: This channel is not used for anything special but its value
        /// can be read using Serial or USB commands.
        /// </summary>
        None = 0,

        /// <summary>
        /// Forward Limit Switch: if this channel is active (Scaled Value >= 1600),
        /// then the motor is not allowed to move forward.
        /// </summary>
        LimitForward = 1,

        /// <summary>
        /// Reverse Limit Switch: if this channel is active (Scaled Value >= 1600),
        /// then the motor is not allowed to move in reverse.
        /// </summary>
        LimitReverse = 2,

        /// <summary>
        /// Kill Switch: if this channel is active (Scale Value >= 1600), then the
        /// motor is not allowed to move.
        /// </summary>
        KillSwitch = 3
    }

    /// <summary>
    /// Specifies the pin mode for an Analog input channel.
    /// This setting is not relevant for RC input cahnnels.
    /// </summary>
    public enum SmcPinMode : byte
    {
        /// <summary>
        /// Floating: no pull-up or pull-down resistors enabled.
        /// </summary>
        Floating = 0,

        /// <summary>
        /// Weak pull-up resistor (to 3.3 V) enabled.
        /// </summary>
        PullUp = 1,

        /// <summary>
        /// Weak pull-down resistor (to 0 V) enabled.
        /// </summary>
        PullDown = 2
    }

    /// <summary>
    /// Defines the bits in the LimitStatus register, which tells us
    /// what things are currently limiting the motor speed.
    /// </summary>
    public enum SmcLimitStatus : ushort
    {
        /// <summary>
        /// Motor is not allowed to run due to an error or safe-start violation.
        /// </summary>
        StartedState = (1<<0),

        /// <summary>
        /// Temperature is actively reducing Target Speed.
        /// </summary>
        Temperature = (1<<1),

        /// <summary>
        /// Max speed limit is actively reducing Target Speed.
        /// Only happens when Input Mode is Serial/USB.
        /// </summary>
        MaxSpeed = (1<<2),

        /// <summary>
        /// Starting speed limit is actively reducing Target Speed to 0.
        /// Only happens when Input Mode is Serial/USB.
        /// </summary>
        StartingSpeed = (1<<3),
        
        /// <summary>
        /// Motor speed is not equal to target speed because of acceleration,
        /// deceleration, or brake duration limits.
        /// </summary>
        Acceleration = (1<<4),

        /// <summary>
        /// RC Channel 1 is configured as a limit/kill switch and it is active (Scaled Value >= 1600).
        /// </summary>
        Rc1 = (1<<5),

        /// <summary>
        /// RC Channel 2 is configured as a limit/kill switch and it is active (Scaled Value >= 1600).
        /// </summary>
        Rc2 = (1<<6),

        /// <summary>
        /// Analog Channel 1 is configured as a limit/kill switch and it is active (Scaled Value >= 1600).
        /// </summary>
        Analog1 = (1<<7),

        /// <summary>
        /// Analog Channel 2 is configured as a limit/kill switch and it is active (Scaled Value >= 1600).
        /// </summary>
        Analog2 = (1<<8),

        /// <summary>
        /// USB kill switch is active.
        /// </summary>
        UsbKill = (1<<9)
    }

    /// <summary>
    /// The PWM mode to use.  This feature is not exposed to users because
    /// we found the DriveBrake mode to be much better than DriveCode mode.
    /// </summary>
    public enum SmcPwmMode : byte
    {
        /// <summary>
        /// PWM between driving and braking (both low-side MOSFETs on).
        /// </summary>
        DriveBrake = 0,

        /// <summary>
        /// PWM between driving and coasting.
        /// </summary>
        DriveCoast = 1,
    }

    /// <summary>
    /// Defines the different errors that the device has.
    /// </summary>
    public enum SmcError : ushort
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Safe start violation.
        /// </summary>
        SafeStart = (1<<0),

        /// <summary>
        /// This error occurs whenever any required RC or Analog channel is invalid. 
        /// </summary>
        ChannelInvalid = (1<<1),

        /// <summary>
        /// This error occurs whenever the Input Mode is Serial/USB and something goes
        /// wrong with the serial communication, either on the RX/TX lines or on the
        /// USB virtual COM port.
        /// </summary>
        Serial = (1<<2),

        /// <summary>
        /// This error occurs if Input Mode is Serial/USB and the (configurable) time
        /// period has elapsed with no valid serial or USB commands being received by
        /// the controller.  See commandTimeout in SmcSettings.
        /// </summary>
        /// <see cref="SmcSettings.commandTimeout"/>
        CommandTimeout = (1<<3),

        /// <summary>
        /// This error occurs when a limit or kill switch channel stops the motor.
        /// </summary>
        LimitSwitch = (1<<4),

        /// <summary>
        /// This error occurs whenever your power supply's voltage is too low or it is disconnected.
        /// </summary>
        VinLow = (1<<5),

        /// <summary>
        /// This error occurs whenever your power supply's voltage is too high.
        /// </summary>
        VinHigh = (1<<6),

        /// <summary>
        /// This error occurs whenever the reading from the temperature sensor is too high.
        /// </summary>
        Temperature = (1<<7),

        /// <summary>
        /// This error occurs whenever the motor driver chip reports an under-voltage or
        /// over-temperature error (by driving its fault line low).
        /// </summary>
        MotorDriverError = (1<<8),

        /// <summary>
        /// This error occurs whenever there are no other errors but the voltage on the
        /// ERR line is high (2.3-5 V).
        /// </summary>
        ErrLineHigh = (1<<9)
    }

    /// <summary>
    /// Defines the different serial errors that are recorded.
    /// </summary>
    public enum SmcSerialError : ushort
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// This is not used.
        /// </summary>
        Parity = (1<<0),
        
        /// <summary>
        /// This is error occurs when a de-synchronization or excessive
        /// noise on the RX line is detected.
        /// </summary>
        Frame = (1<<1),

        /// <summary>
        /// This error occurs when noise is detected on the RX line.
        /// </summary>
        Noise = (1<<2),

        /// <summary>
        /// This error occurs when the buffer for storing bytes received
        /// on the RX line is full and data was lost as a result.
        /// </summary>
        RxOverrun = (1<<3),

        /// <summary>
        /// This error occurs if the serial bytes received on RX or the
        /// virtual COM port do not obey the protocol specified in this guide.
        /// </summary>
        Format = (1<<5),

        /// <summary>
        /// This error occurs if you have enabled cyclic redundancy check (CRC)
        /// for serial commands, but the CRC byte received was invalid.
        /// </summary>
        Crc = (1<<6),
    }

    /// <summary>
    /// Specifies what motor limits to set.
    /// You can use the OR (|) operator to apply the ForwardOnly or ReverseOnly
    /// modifiers to MaxSpeed, MaxAcceleration, MaxDeceleration, or BrakeDuration.
    /// Without any modifies, this value specifies woth the forward and reverse limits.
    /// </summary>
    public enum SmcMotorLimit : byte
    {
        /// <summary>
        /// Max Speed is a number between 0 and 3200 that specifies the maximum speed at
        /// which the motor controller will ever drive the motor.
        /// </summary>
        MaxSpeed = 0,

        /// <summary>
        /// Max Acceleration is a number between 0 and 3200 that specifies how much the
        /// magnitude (absolute value) of the motor speed is allowed to increase every
        /// speed update period.  0 means no limit.
        /// </summary>
        MaxAcceleration = 1,

        /// <summary>
        /// Max Deceleration is a number between 0 and 3200 that specifies how much the
        /// magnitude (absolute value) of the motor speed is allowed to decrease every
        /// speed update period.  0 means no limit.
        /// </summary>
        MaxDeceleration = 2,

        /// <summary>
        /// Brake duration is the time, in milliseconds, that the motor controller will
        /// spend braking the motor (Current Speed = 0) before allowing the Current Speed
        /// to change signs.
        /// </summary>
        BrakeDuration = 3,

        /// <summary>
        /// A modifier that specifies that only the forward limit should be set.
        /// </summary>
        ForwardOnly = 4,

        /// <summary>
        /// A modified that specifies that only the reverse limit should be set.
        /// </summary>
        ReverseOnly = 8,
    }

    /// <summary>
    /// Specifies the return code from a Set Motor Limit command.
    /// </summary>
    public enum SmcSetMotorLimitProblem : byte
    {
        /// <summary>
        /// There were no problems with the set motor limit command.
        /// </summary>
        None = 0,

        /// <summary>
        /// The value you were trying to set was more dangerous than the hard limit for the Forward
        /// direction, so the hard limit was used instead.  This may be the desired behavior.
        /// </summary>
        ForwardConflict = 1,

        /// <summary>
        /// The value you were trying to set was more dangerous than the hard limit for the Reverse
        /// direction, so the hard limit was used instead.  This may be the desired behavior.
        /// </summary>
        ReverseConflict = 2,
    }

    /// <summary>
    /// Specifies the causes of the device's last reset.
    /// </summary>
    public enum SmcResetFlags : byte
    {
        /// <summary>
        /// The device was reset because the voltage on the Reset pin went low.
        /// </summary>
        ResetPin = 0x04,

        /// <summary>
        /// The device was reset because power was turned off/on.
        /// </summary>
        Power = 0x0C,

        /// <summary>
        /// The device was reset by software running on the device.
        /// This happens at the end of a firmware upgrade when the bootloader
        /// starts the new firmware.
        /// </summary>
        Software = 0x14,

        /// <summary>
        /// The device was reset by the watchdog timer.  This indicates a
        /// problem with the firmware which should be reported to Pololu.com.
        /// </summary>
        Watchdog = 0x24,
    }
}
