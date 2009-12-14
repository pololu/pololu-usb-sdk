using System.Collections.Generic;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System;

namespace Pololu.Usc.Sequencer
{
    public class Sequence
    {
        public string name;
        public List<Frame> frames = new List<Frame>();

        public Sequence(string name) { this.name = name; }

    	public Sequence() { }

        /// <summary>
        /// Saves sequences in the registry in the "sequences" subkey of the given key.
        /// </summary>
        /// <param name="list">A list of sequences to save in key/sequences in the registry.</param>
        /// <param name="parentKey">A key that has been opened so as to allow editing.</param>
        public static void saveSequencesInRegistry(IList<Sequence> list, RegistryKey parentKey)
        {
            try
            {
                parentKey.DeleteValue("sequences");
            }
            catch { }

            try
            {
                parentKey.DeleteSubKeyTree("sequences");
            }
            catch { }

            parentKey.CreateSubKey("sequences");
            RegistryKey sequencesKey = parentKey.OpenSubKey("sequences", true);

            for (int sequenceIndex = 0; sequenceIndex < list.Count; sequenceIndex++)
            {
                Sequence sequence = list[sequenceIndex];
                string sequenceKeyName = sequenceIndex.ToString("d2"); // e.g. 01
                RegistryKey sequenceKey = sequencesKey.CreateSubKey(sequenceKeyName);
                sequenceKey.SetValue("name", sequence.name, RegistryValueKind.String);

                for (int frameIndex = 0; frameIndex < sequence.frames.Count; frameIndex++)
                {
                    Frame frame = sequence.frames[frameIndex];
                    string frameKeyName = frameIndex.ToString("d4"); // e.g. 0001
                    RegistryKey frameKey = sequenceKey.CreateSubKey(frameKeyName);
                    frameKey.SetValue("name", frame.name, RegistryValueKind.String);

                    frameKey.SetValue("duration", frame.length_ms, RegistryValueKind.DWord);

                    frameKey.SetValue("targets", frame.getTargetsString(), RegistryValueKind.String);

                    frameKey.Close();
                }
                sequenceKey.Close();
            }
            sequencesKey.Close();
        }

        /// <summary>
        /// Reads sequences from the registry in the "sequences" subkey of the given key.
        /// </summary>
        public static List<Sequence> readSequencesFromRegistry(RegistryKey parentKey, byte servoCount)
        {
            List<Sequence> sequences = new List<Sequence>();
            RegistryKey sequencesKey = parentKey.OpenSubKey("sequences");

            if (sequencesKey == null)
                return sequences;

            FrameKeyNameComparer fknc = new FrameKeyNameComparer();

            if (sequencesKey == null) return sequences;

            foreach (string sequenceKeyName in sequencesKey.GetSubKeyNames())
            {
                RegistryKey sequenceKey = sequencesKey.OpenSubKey(sequenceKeyName);

                string sequenceName = sequenceKey.GetValue("name", null) as string;
                if (sequenceName == null) { sequenceName = "Sequence " + sequenceKeyName; }

                Sequence sequence = new Sequence(sequenceName);

                List<string> frameKeyNames = new List<String>(sequenceKey.GetSubKeyNames());

                // Make sure the frames are in the right order.
                frameKeyNames.Sort(fknc);

                List<Frame> frames = new List<Frame>(sequenceKey.SubKeyCount);
                foreach (string frameKeyName in frameKeyNames)
                {
                    RegistryKey frameKey = sequenceKey.OpenSubKey(frameKeyName);
                    if (frameKey == null)
                        continue;

                    Frame frame = new Frame();

                    frame.name = frameKey.GetValue("name", null) as string;
                    if (frame.name == null) { frame.name = "Frame " + frameKeyName; }

                    Nullable<int> length_ms = frameKey.GetValue("duration", "") as Nullable<int>;
                    if (length_ms != null) { frame.length_ms = (ushort)length_ms; }

                    frame.setTargetsFromString(frameKey.GetValue("targets", "") as string, servoCount);

                    frames.Add(frame);
                }
                sequence.frames = frames;

                sequences.Add(sequence);
            }

            return sequences;
        }

        /// <summary>
        /// Generates the script for this sequence - just the code for calling the frame functions.
        /// Adds any channel lists for required frame commands to the needed_channel_lists array.
        /// </summary>
        /// <param name="needed_channel_lists"></param>
        /// <returns></returns>
        private string generateScript(List<byte> enabled_channels, List<List<byte>> needed_channel_lists)
        {
            string script = "";
            bool first_time = true;
            ushort[] last_targets = null; // need to initialize to avoid compiler error

            foreach (Frame frame in frames)
            {
                List<byte> needed_channels = new List<byte>();
                List<ushort> changed_targets = new List<ushort>();

                // The first time, we need to set all channels.
                // Otherwise, set needed_channels to a list of just the
                // channels that change.  Note that non-enabled channels
                // should never change, but we skip them anyway.
                foreach (byte channel in enabled_channels)
                {
                    if (first_time || frame.targets[channel] != last_targets[channel])
                    {
                        needed_channels.Add(channel);
                        changed_targets.Add(frame.targets[channel]);
                    }
                }
                first_time = false;

                // set last_targets
                last_targets = (ushort[])frame.targets.Clone();

                if (changed_targets.Count != 0)
                {
                    // search for an existing list that matches
                    bool found = false;
                    foreach (List<byte> existing_list in needed_channel_lists)
                    {
                        if (existing_list.Count != needed_channels.Count)
                            continue;

                        int i;
                        for (i = 0; i < existing_list.Count; i++)
                        {
                            if (existing_list[i] != needed_channels[i])
                                goto does_not_match;
                        }

                        // every one matched, so the list matches
                        found = true;
                        break;

                        // this one does not match, so continue
                    does_not_match:
                        continue;
                    }

                    if (!found)
                    {
                        // add the set of channels we need this time to the list
                        needed_channel_lists.Add(needed_channels);
                    }
                }

                // actually add the code for this frame
                script += "  "; // indent
                script += frame.length_ms + " ";

                if (needed_channels.Count == 0)
                {
                    // no channels changed - just delay
                    script += "delay";
                }
                else
                {
                    foreach (ushort target in changed_targets)
                    {
                        script += target.ToString() + " ";
                    }
                    script += getFrameSubroutineName(needed_channels);
                }
                script += " # " + frame.name + "\n";
            }
            return script;
        }

        /// <summary>
        /// Generates the name of the frame subroutine that sets all of the specified channels.
        /// It will be of the form frame_5_4_3.  TODO: include ranges, to allow frame_23_to_2, etc.
        /// </summary>
        /// <param name="channels">The list of channels, in ascending numerical order.</param>
        /// <returns></returns>
        public static string getFrameSubroutineName(List<byte> channels)
        {
            string n = "frame_";
            foreach(byte channel in channels)
            {
                n += channel.ToString() + "_";
            }
            n = n.TrimEnd(new char[] {'_'});
            return n;
        }

        /// <summary>
        /// Generates the subroutine that sets the specified channels, then delays.  The channels are expected to be on the stack 
        /// in the same order as the channels list - e.g. the command will be something like 500 1 2 3 frame_3_2_1.
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static string generateFrameSubroutine(List<byte> channels)
        {
            string script = "";
            script += "sub " + getFrameSubroutineName(channels) + "\n";
            script += "  ";

            int i;
            for(i=channels.Count-1;i>=0;i--)
            {
                script += channels[i].ToString() + " servo ";
            }
            script += "delay\n";
            script += "  return\n";
            return script;
        }

        public string generateLoopedScript(List<byte> enabled_channels)
        {
            List<List<byte>> needed_channel_lists = new List<List<byte>>();

            string script = "# " + name + "\n" + "begin\n";
            script += generateScript(enabled_channels, needed_channel_lists);
            script += "repeat\n\n";

            foreach (List<byte> needed_channels in needed_channel_lists)
            {
                script += generateFrameSubroutine(needed_channels) + "\n";
            }
            return script;
        }

        public string generateSubroutine(List<byte> enabled_channels, List<List<byte>> needed_channel_lists)
        {
            string nice_name = name;

            // turn spaces into underscores
            var exp = new Regex(@"\s+");
            nice_name = exp.Replace(nice_name, "_");

            // get rid of unusual characters
            exp = new Regex(@"[^a-z0-9_]", RegexOptions.IgnoreCase);
            nice_name = exp.Replace(nice_name, "");

            string script = "# " + name + "\n" + "sub " + nice_name + "\n";
            script += generateScript(enabled_channels, needed_channel_lists);
            script += "  return\n";
            return script;
        }

        public static string generateSubroutineList(List<byte> enabled_channels, List<Sequence> sequences)
        {
            List<List<byte>> needed_channel_lists = new List<List<byte>>();
            string script = "";

            foreach (Sequence sequence in sequences)
            {
                script += sequence.generateSubroutine(enabled_channels, needed_channel_lists);
            }

            foreach (var channel_list in needed_channel_lists)
            {
                script += "\n"+Sequence.generateFrameSubroutine(channel_list);
            }
            return script;
        }

        /// <summary>
        /// We want the frame names to be sorted correctly when retrieved from the registry.
        /// This means converting the names (e.g. "0013") to integers.
        /// </summary>
        /// <remarks>http://msdn.microsoft.com/en-us/library/system.collections.icomparer.compare.aspx</remarks>
        private class FrameKeyNameComparer : IComparer<String>
        {
            public int Compare(string x, string y)
            {
                try
                {
                    return ushort.Parse(x) - ushort.Parse(y);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}

// Local Variables: **
// mode: java **
// c-basic-offset: 4 **
// tab-width: 4 **
// indent-tabs-mode: nil **
// end: **
