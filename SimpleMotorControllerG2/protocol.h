#ifndef _PROTOCOL_H
#define _PROTOCOL_H

#include <stdint.h>

// Native USB commands.  Note: these are NOT serial command bytes.
enum HpmcRequest
{
  REQUEST_GET_SETTINGS = 0x81,
  REQUEST_SET_SETTINGS = 0x82,
  REQUEST_GET_VARIABLES = 0x83,
  REQUEST_RESET_SETTINGS = 0x84,
  REQUEST_GET_RESET_FLAGS = 0x85,
  REQUEST_SET_SPEED = 0x90,
  REQUEST_EXIT_SAFE_START = 0x91,
  REQUEST_SET_MOTOR_LIMIT = 0x92,
  REQUEST_SET_USB_KILL = 0x93,
  REQUEST_SET_CURRENT_LIMIT = 0x94,
  REQUEST_GET_STALL_ERROR = 0xB0,
  REQUEST_START_BOOTLOADER = 0xFF,
};

// NOTE: the order and types of these struct members must not change!
struct __attribute__((packed, aligned(4))) HpmcMotorLimits
{
  uint16_t maxSpeed;        // Absolute maximum speed; maps to maximum/minimum in RC/analog mode (0-3200).
  uint16_t maxAcceleration; // Maximum amount that speed magnitude can increase each update period (0-3200).
  uint16_t maxDeceleration; // Maximum amount that speed magnitude can decrease each update period (0-3200).
  uint16_t brakeDuration;   // Brake time required before switching to driving direction.  Units: 1 ms.
  uint16_t startingSpeed;   // Minimum non-zero speed (RAM value cannot be changed by serial/USB); maps to scaledVal==1 in RC/analog mode (0-3200).  0 means no effect.
  uint16_t RESERVED0;
};

struct __attribute__((packed, aligned(4))) HpmcChannelSettings
{
  uint8_t invert;           // 0 or 1.  Used to invert the scaling (higher rawValue => lower scaledValue).
  uint8_t scalingDegree;    // 0 = linear, 1 = quadratic, 2 = cubic, etc.
  uint8_t alternateUse;     // determines if this is channel acts as a limit/kill switch
  uint8_t pinMode;          // determines if analog input is floating, pulled-up, or pulled-down (doesn't affect RC inputs)

  uint16_t errorMin;        // Raw values greater than this generate an error
  uint16_t errorMax;        // Raw values greater than this generate an error

  uint16_t inputMin;        // The rawValue that gets mapped to a speed of -reverseLimits.maxSpeed (or forwardLimits.maxSpeed if invert==1)
  uint16_t inputMax;        // The rawValue that gets mapped to a speed of forwardLimits.maxSpeed (or -reverseLimits.maxSpeed if invert==1)

  uint16_t inputNeutralMin; // rawValues from inputNeutralMin to inputNeutralMax map to speed 0.
  uint16_t inputNeutralMax;
};

struct __attribute__((packed, aligned(4))) HpmcSettings
{
  struct
  {
    unsigned neverSleep : 1;
    unsigned uartResponseDelay : 1;
    unsigned useFixedBaudRate : 1;
    unsigned disableSafeStart : 1;
    unsigned enableI2C : 1;
    unsigned ignoreErrLineHigh : 1;   // When set, ignore inputs state of ERR line
    unsigned tempLimitGradual : 1;  // true: gradual speed limit starting at overTempMin.  false: abrupt limit past overTempMax asserted until temp falls below overTempMin
    unsigned ignorePotDisconnect : 1;  // Check for pot disconnect toggling POTPWR pin.  Boolean. False: check.  True: don't check (POTPWR always high)
    unsigned motorInvert : 1;   // Invert Motor Direction.  Boolean.  False: 3200=OUTA>OUTB.  True:  3200=OUTA<OUTB.
    unsigned coastWhenOff : 1;  // Brake amount while input is in deadband (0-32), or there is an error, or motor is driving at speed zero.
    unsigned crcForCommands : 1;   // CRC for serial/I2C commands
    unsigned crcForResponses : 1;  // CRC for serial/I2C responses
    unsigned reservedBits : 20;
  };

  uint8_t inputMode;            // See INPUT_MODE_* macros.
  uint8_t mixingMode;           // See MIXING_MODE_* macros.
  uint8_t serialMode;           // See SERIAL_MODE_* macros.
  uint8_t serialDeviceNumber;

  uint16_t fixedBaudRateRegister; // Value to put in USART->BRR (only used if useFixedBaudRate is non-zero)
  uint16_t speedUpdatePeriod;   // This is the time between application of accel/decel updates to speed (units of 1 ms).  should never be 0!

  uint16_t commandTimeout;      // 0 means disabled.  Units: 10 ms
  uint16_t rcTimeout;           // Generates error and shuts down motor if we go this long without heeding a pulse (units of 1 ms)

  uint16_t overTempCompleteShutoffThreshold;
  uint16_t overTempNormalOperationThreshold;

  uint8_t pwmPeriodFactor;      // Determines the PWM frequency (0-19, 0=highest freq).
  uint8_t consecGoodPulses;     // Number of consecutive good pulses needed before we heed them (update channel's rawValue).  0 is same as 1.
  uint8_t RESERVED0;
  uint8_t RESERVED1;

  uint16_t minPulsePeriod;      // Minimum allowed time between consecutive RC pulse rising edges (units of 1 ms).
  uint16_t maxPulsePeriod;      // Maximum allowed time between consecutive RC pulse rising edges (units of 1 ms).

  uint16_t vinScaleCalibration;
  uint16_t lowVinShutoffTimeout; // Vin must stay below lowVinShutoffMv for this duration before a low-VIN error occurs (units of 1 ms)

  uint16_t lowVinShutoffMv;     // Dropping below this voltage threshold for a duration of lowVinShutoffTimeout triggers a low-voltage error (units of mV)
  uint16_t lowVinStartupMv;     // Once asserting a low-voltage error, the voltage required to stop asserting this error (units of mV)

  uint16_t highVinShutoffMv;    // Rising above this voltage threshold triggers a high-voltage error and causes the motor to immediately brake at 100% (units of mV)
  uint16_t currentLimit;

  uint16_t currentOffsetCalibration;
  uint16_t currentScaleCalibration;

  struct HpmcChannelSettings rc1;
  struct HpmcChannelSettings rc2;
  struct HpmcChannelSettings analog1;
  struct HpmcChannelSettings analog2;

  struct HpmcMotorLimits forwardLimits;
  struct HpmcMotorLimits reverseLimits;
};

struct __attribute__((packed, aligned(4))) HpmcChannelVariables
{
  uint16_t unlimitedRawValue; // 0xFFFF if disconnected but not affected by absolute max/min limits. Units of quarter-microseconds
  uint16_t rawValue;          // 0xFFFF if disconnected or outside of absolute maximum/minimum limits.
                                    // Units of quarter-microseconds if an RC channel.  12-bit ADC reading if analog.
  int16_t scaledValue;
  uint16_t RESERVED0;
};

// NOTE: the order and types of these struct members must not change because some of these
//  variables (errorStatus, errorOccurred, serialErrorOccurred, and baudRateRegister)
//  are treated as special cases by the get-variables command.  These four variables must
//  be loaded from shadow registers whenever they are requested.
//  Though not stored in this struct, the resetSource variable can be requested using the
//  get-variables command with variable ID 127.
struct __attribute__((packed, aligned(4))) HpmcVariables
{
  uint16_t errorStatus;         // varId 0: The errors that are currently happening.  See ERROR_* macros.
  uint16_t errorOccurred;       // varId 1: The errors that occurred since the last cleared.  See ERROR_* macros.

  uint16_t serialErrorOccurred; // varId 2: The serial errors that occurred since last cleared.  See SERIAL_ERROR_* macros.
  uint16_t limitStatus;         // varId 3: indicates things that are limiting operation but aren't errors.

  struct HpmcChannelVariables rc1;    // varId  4 (unlim),  5 (raw),  6 (scaled),  7 (n/a)
  struct HpmcChannelVariables rc2;    // varId  8 (unlim),  9 (raw), 10 (scaled), 11 (n/a)
  struct HpmcChannelVariables analog1;// varId 12 (unlim), 13 (raw), 14 (scaled), 15 (n/a)
  struct HpmcChannelVariables analog2;// varId 16 (unlim), 17 (raw), 18 (scaled), 19 (n/a)

  int16_t targetSpeed;          // varId 20: Target speed of motor from -3200 to 3200.
  int16_t speed;                // varId 21: Current speed of motor from -3200 to 3200.
  uint16_t brakeAmount;         // varId 22: Current braking amount: 0xFF for irrelevant, 0 for coasting, 32 for full braking
  uint16_t vinMv;               // varId 23: Units: millivolts

  uint16_t temperatureA;        // varId 24.
  uint16_t temperatureB;        // varId 25.

  uint16_t rcPeriod;            // varId 26: Period of rc signal (0 means no good signal).  Units: 0.1 ms
  uint16_t baudRateRegister;    // varId 27: Value from USART1->BRR (used to debug auto-baud detect)

  unsigned long timeMs;               // varId 28: system timer low half-word
                                      // varId 29: system timer high half-word

  struct HpmcMotorLimits forwardLimits;  // varId 30 - 35
  struct HpmcMotorLimits reverseLimits;  // varId 36 - 41

  uint16_t currentLimit;        // varId 42: Current limit (0 to 3200).
  uint16_t rawCurrent;          // varId 43: Raw voltage measurement on current sense line.

  uint16_t current;             // varId 44: Current in milliamps.
  uint16_t currentLimitingConsecutiveCount;  // varId 45

  uint16_t currentLimitingOccurrenceCount;   // varId 46
};

// Valid values for the inputMode setting:
#define INPUT_MODE_SERIAL_USB 0
#define INPUT_MODE_ANALOG     1
#define INPUT_MODE_RC         2

// Valid values for mixingMode setting:
#define MIXING_MODE_NONE  0
#define MIXING_MODE_LEFT  1
#define MIXING_MODE_RIGHT 2

// Valid values for serialMode setting:
#define SERIAL_MODE_BINARY  0  // compact or Pololu protocols
#define SERIAL_MODE_ASCII   1  // ASCII protocol with prompts and echoing of received bytes (for use with terminal program)

// Valid values for wIndex in the USB Set Duty Cycle request:
#define DIRECTION_FORWARD 0
#define DIRECTION_REVERSE 1
#define DIRECTION_BRAKE   2

// Valid values for channel setting alternateUse
#define ALTERNATE_USE_DISABLED        0
#define ALTERNATE_USE_LIMIT_FORWARD   1
#define ALTERNATE_USE_LIMIT_REVERSE   2
#define ALTERNATE_USE_KILL_SWITCH     3

// Valid values for channel setting pinMode
#define PIN_MODE_FLOATING    0  // recommended mode when not using a limit switch
#define PIN_MODE_PULL_UP     1  // internal pull-up enabled on analog input
#define PIN_MODE_PULL_DOWN   2  // internal pull-down enabled on analog input

// limitStatus variable bits
#define LIMITED_BY_STARTED_STATE  (1<<0)  // 1 => motors are not allowed to start running
#define LIMITED_BY_TEMPERATURE    (1<<1)  // 1 => temperature is actively reducing our target speed
#define LIMITED_BY_MAX_SPEED      (1<<2)  // 1 => max speed setting is actively reducing our target speed
#define LIMITED_BY_STARTING_SPEED (1<<3)  // 1 => starting speed setting is actively reducing our target speed
#define LIMITED_BY_ACCELERATION   (1<<4)  // 1 => current speed != target speed because of accel/decel/brake-duration limits
#define LIMITED_BY_RC1            (1<<5)  // 1 => RC1 limit switch triggered
#define LIMITED_BY_RC2            (1<<6)  // 1 => RC2 limit switch triggered
#define LIMITED_BY_ANALOG1        (1<<7)  // 1 => analog1 limit switch triggered
#define LIMITED_BY_ANALOG2        (1<<8)  // 1 => analog2 limit switch triggered
#define LIMITED_BY_USB_KILL       (1<<9)  // 1 => native USB kill switch active

#define LIMIT_ALL 0xFFFF

// Valid values for pwmMode setting:
#define PWM_MODE_DRIVE_BRAKE    0  // default (intentionally making this zero)
#define PWM_MODE_DRIVE_COAST    1  // config utility will not give user this option (it doesn't work well)

// errorStatus (and therefore errorOccurred) bits:
#define ERROR_SAFE_START        (1<<0)  // In RC/Analog mode: target speed > 0.0625*maxSpeed && started==0 && safe start enabled
                    // In serial/USB mode: set when started==0, cleared by special command; cannot set started=1 until cleared
#define ERROR_CHANNEL_INVALID   (1<<1)
#define ERROR_SERIAL            (1<<2)  // errorStatus bit set on serial error when in serial/USB mode; cleared on successful reception of serial command packet
#define ERROR_COMMAND_TIMEOUT   (1<<3)  // too much time has passed since we received the last valid command packet from USBCOM or UART or motor command over native USB
#define ERROR_LIMIT_SWITCH      (1<<4)
#define ERROR_VIN_LOW           (1<<5)
#define ERROR_VIN_HIGH          (1<<6)
#define ERROR_TEMPERATURE       (1<<7)
#define ERROR_MOTOR_DRIVER      (1<<8)
#define ERROR_ERR_LINE_HIGH     (1<<9)  // external source is driving ERR line high (this errorStatus bit does NOT turn on red LED
                    // and is only set when we are NOT driving our own ERR line high)

#define ERROR_ALL 0xFFFF

// serialErrorOccurred bits:
// *** NOTE: the bit values for the first four errors cannot change
//            (they match the locations of flag bits in USART1->SR status register)
#define SERIAL_ERROR_PARITY      (1 << 0)  // hardware parity error PE (not used)
#define SERIAL_ERROR_FRAME       (1 << 1)  // hardware frame error FE
#define SERIAL_ERROR_NOISE       (1 << 2)  // hardware noise error NE
#define SERIAL_ERROR_RX_OVERRUN  (1 << 3)  // rxBuffer or hardware overrun error ORE (rx byte while RXNE set)

#define SERIAL_ERROR_FORMAT      (1 << 5)  // command packet format error
#define SERIAL_ERROR_CRC         (1 << 6)  // received incorrect CRC byte


// Valid values for the reset source:
#define RESET_NRST_PIN    0x04    // NRST pin was pulled low by an external source
#define RESET_POWER       0x0C    // the device stopped running because power got too low
#define RESET_SOFTWARE    0x14    // caused when entering application from the Bootloader
#define RESET_IWDG        0x24    // independent watchdog reset caused by firmware crash (could indicate a firmware bug)


// Value of RC or analog channel that is dead/disconnected or out of range
#define DISCONNECTED_INPUT  0xFFFF


// Serial command bytes (with MSB set)
// Note: every command differs from every other command by at least two bits, so if noise changes
// a single bit of a valid command, the result will be an invalid command (similar to parity checking
// but without the extra parity bit).
#define COMMAND_EXIT_SAFE_START       0x83  // 0 data bytes
#define COMMAND_MOTOR_FORWARD         0x85  // 2 data bytes (Note: this command must be odd)
#define COMMAND_MOTOR_REVERSE         0x86  // 2 data bytes (Note: this command must be even)
#define COMMAND_MOTOR_FORWARD_7BIT    0x89  // 1 data byte (Note: this command must be odd)
#define COMMAND_MOTOR_REVERSE_7BIT    0x8A  // 1 data byte (Note: this command must be even)
#define COMMAND_SET_CURRENT_LIMIT     0x91  // 2 data bytes
#define COMMAND_MOTOR_BRAKE           0x92  // 1 data byte (0 = full coast, 32 = full brake)
#define COMMAND_GET_VARIABLE          0xA1  // 1 data byte
#define COMMAND_SET_MOTOR_LIMIT       0xA2  // 3 data bytes
#define COMMAND_GET_FIRMWARE_VERSION  0xC2  // 0 data bytes
#define COMMAND_STOP_MOTOR            0xE0  // 0 data bytes
#define COMMAND_MINI_SSC              0xFF  // 2 data bytes (2nd byte: 0 = full rev, 127 = spd 0, 254 = full forward)

#define SMC_TEMPERATURE_ERROR 3000

// Bits in wValue for the USB Get Variables command.
#define SMC_GET_VARIABLES_FLAG_CLEAR_ERROR_FLAGS_OCCURRED 0
#define SMC_GET_VARIABLES_FLAG_CLEAR_CURRENT_LIMITING_OCCURRENCE_COUNT 1

#endif
