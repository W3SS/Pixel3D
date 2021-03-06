﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Pixel3D.Audio;
using Pixel3D.Collections;
using Pixel3D.Strings;

namespace Pixel3D.Engine
{
    public abstract class Definitions : IAudioDefinitions
    {
        #region Sound Effects

        protected OrderedDictionary<string, SafeSoundEffect> soundBank;

        /// <summary>Return a sound effect for the given cue</summary>
        public SafeSoundEffect GetSound(Cue cue, int index)
        {
            if (cue == null || ReferenceEquals(missingCue, cue))
                return null;

            string path = cue.sounds[index].path;
            if (path == null)
                return null;

            SafeSoundEffect result;
            if (soundBank.TryGetValue(path, out result))
                return result;

            return null;
        }

        /// <summary>Return the sound for a given music cue. Not to be used in the simulation (play it immediately).</summary>
        public SafeSoundEffect LocalGetSoundForMusicCue(Cue cue)
        {
            SafeSoundEffect music = null;
            if (ReferenceEquals(cue, missingCue))
            {
#if DEVELOPER
                music = MissingAudio.GetMissingMusicSound();
#endif
            }
            else if (cue != null && cue.SoundCount > 0)
            {
                // Assumption: Music Cue is just a single sound with no variations!
                Debug.Assert(cue.SoundCount == 1);
                music = GetSound(cue, 0);
            }

            return music;
        }


        protected static ReadAudioPackage.Result LoadSoundEffects(byte[] header, string filename)
        {
            string audioPackagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            return ReadAudioPackage.ReadHeader(audioPackagePath, header);
        }

        #endregion

        #region Cues

        // TODO: This needs Symbol-like treatment so we aren't looking anything up based on a string
        /// <summary>Lookup of names to cues.</summary>
        protected OrderedDictionary<string, Cue> cues;

        public int cuesWithIds;

        /// <summary>Sentinel value for when we cannot find a requested cue</summary>
        public Cue missingCue = new Cue() { friendlyName = "[missing cue]" };

        public Cue GetCue(string name, object debugContext)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Cue result;
            if (cues.TryGetValue(name, out result))
                return result;

#if DEVELOPER
            MissingAudio.ReportMissingCue(name, debugContext); // <- NOTE: This has its own internal no-repeat handling, so it's fine with rollbacks
#endif
            return missingCue;
        }


        protected struct LoadCuesResult
        {
            public OrderedDictionary<string, Cue> cues;
            public int cuesWithIds;
        }

        protected static LoadCuesResult LoadCues(byte[] header, string filename)
        {
            LoadCuesResult result;
            result.cues = new OrderedDictionary<string, Cue>();
            result.cuesWithIds = 0;

            string cuePackagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cues.rcru");
            using (var fs = File.OpenRead(cuePackagePath))
            {
                for (int i = 0; i < header.Length; i++)
                    if (fs.ReadByte() != header[i])
                        throw new Exception("Cues package is corrupt");

                using (var br = new BinaryReader(new GZipStream(fs, CompressionMode.Decompress, false)))
                {
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string name = br.ReadString();
                        CueDeserializeContext context = new CueDeserializeContext(br);
                        Cue cue = new Cue(context);

                        // Post-processing:
                        result.cues.Add(name, cue);
                        if (cue.type == CueType.Cycle || cue.type == CueType.RandomCycle)
                            cue.id = result.cuesWithIds++;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Localization

        #region Get by TagSet

        public int GetStringCount(TagSet tagSet, byte language)
        {
            return stringBanks[(int)language].GetStringCount(tagSet);
        }

        public string GetSingleString(TagSet tagSet, byte language)
        {
            return stringBanks[(int)language].GetSingleString(tagSet);
        }

        public string GetIndexedString(TagSet tagSet, byte language, int index)
        {
            return stringBanks[(int)language].GetStringChoice(tagSet, index);
        }

        public string GetSingleStringUppercase(TagSet tagSet, byte language)
        {
            return stringBanks[(int)language].GetSingleStringUppercase(tagSet);
        }

        public StringList GetStrings(TagSet tagSet, byte language)
        {
            return stringBanks[(int)language].GetStrings(tagSet);
        }

        public StringList GetStringsUppercase(TagSet tagSet, byte language)
        {
            return stringBanks[(int)language].GetStringsUppercase(tagSet);
        }

        public string GetRandomString(TagSet tagSet, byte language, int choiceIndex)
        {
            return stringBanks[(int)language].GetStringChoice(tagSet, choiceIndex);
        }

        public string GetRandomStringUppercase(TagSet tagSet, byte language, int choiceIndex)
        {
            return stringBanks[(int)language].GetStringChoiceUppercase(tagSet, choiceIndex);
        }

        #endregion


        #region Get by string -- NOTE: Copy-pasted

        public int GetStringCount(string tagSet, byte language)
        {
            return stringBanks[(int)language].GetStringCount(tagSet);
        }

        public string GetSingleString(string tagSet, byte language)
        {
            return stringBanks[(int)language].GetSingleString(tagSet);
        }

        public string GetIndexedString(string tagSet, byte language, int index)
        {
            return stringBanks[(int)language].GetStringChoice(tagSet, index);
        }

        public string GetSingleStringUppercase(string tagSet, byte language)
        {
            return stringBanks[(int)language].GetSingleStringUppercase(tagSet);
        }

        public StringList GetStrings(string tagSet, byte language)
        {
            return stringBanks[(int)language].GetStrings(tagSet);
        }

        public StringList GetStringsUppercase(string tagSet, byte language)
        {
            return stringBanks[(int)language].GetStringsUppercase(tagSet);
        }

        public string GetRandomString(string tagSet, byte language, int choiceIndex)
        {
            return stringBanks[(int)language].GetStringChoice(tagSet, choiceIndex);
        }

        public string GetRandomStringUppercase(string tagSet, byte language, int choiceIndex)
        {
            return stringBanks[(int)language].GetStringChoiceUppercase(tagSet, choiceIndex);
        }

        #endregion


        protected StringBank[] stringBanks;

        protected static StringBank[] LoadStrings(byte[] header, string filename, int languageCount)
        {
            StringBank[] stringBanks = new StringBank[languageCount];

            string stringsPackagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            using (FileStream fs = File.OpenRead(stringsPackagePath))
            {
                for (int i = 0; i < header.Length; i++)
                    if (fs.ReadByte() != header[i])
                        throw new Exception("Strings package is corrupt");

                using (var br = new BinaryReader(new GZipStream(fs, CompressionMode.Decompress, true)))
                {
                    for (int i = 0; i < languageCount; i++)
                    {
                        stringBanks[i] = new StringBank(br);
                    }
                }
            }

            return stringBanks;
        }

        #endregion
    }
}