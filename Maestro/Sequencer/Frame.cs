using System;

namespace Pololu.Usc.Sequencer
{
    public class Frame
    {
        public string name;
        public ushort[] targets;
    	public ushort length_ms;

        /// <summary>
        /// Outputs a string with all the servo positions, e.g. "0 0 4000 0 1000 0 0".
        /// </summary>
        /// <returns></returns>
        public string getTargetsString()
        {
            string targetsString = "";
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != 0)
                {
                    targetsString += " ";
                }

                targetsString += targets[i].ToString();
            }
            return targetsString;
        }

        /// <summary>
        /// Take a (potentially malformed) string with target numbers separated by spaces
        /// and use it to set the targets.
        /// </summary>
        /// <param name="targetsString"></param>
        /// <param name="servoCount"></param>
        public void setTargetsFromString(string targetsString, byte servoCount)
        {
            targets = new ushort[servoCount];

            string[] targetStrings = targetsString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < targetStrings.Length && i < servoCount; i++)
            {
                try
                {
                    targets[i] = ushort.Parse(targetStrings[i]);
                }
                catch { }
            }
        }
    }
}