﻿using System;
using System.Globalization;
using Catel;
using Catel.Data;

namespace CannedBytes.Midi.Message
{
    /// <summary>
    /// A class that helps in generating a name for a note number.
    /// </summary>
    public class MidiNoteName : ObservableObject
    {
        /// <summary>
        /// Contains all the names of all the notes (in one octave).
        /// </summary>
        private static readonly string[] NoteNames = {"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"};

        /// <summary>12 notes in one octave.</summary>
        private const int NoteCount = 12;

        /// <summary>
        /// Constructs an empty instance.
        /// </summary>
        public MidiNoteName()
        {
        }

        /// <summary>
        /// Constructs a new instance for the specified <paramref name="noteNumber"/>.
        /// </summary>
        /// <param name="noteNumber">A note number as it is used in the NoteOn and NoteOff midi messages.</param>
        public MidiNoteName(int noteNumber)
        {
            NoteNumber = noteNumber;
        }

        /// <summary>
        /// Constructs a new instance for the specified <paramref name="noteName"/>.
        /// </summary>
        /// <param name="noteName">Must not be null or empty.</param>
        public MidiNoteName(string noteName)
        {
            Argument.IsNotNull(() => noteName);

            ParseFullNoteName(noteName);
        }

        /// <summary>
        /// Parses the note name into its components.
        /// </summary>
        /// <param name="newFullNoteName">Must not be null.</param>
        private void ParseFullNoteName(string newFullNoteName)
        {
            Argument.IsNotNull(() => newFullNoteName);

            var upperNoteName = newFullNoteName.ToUpperInvariant();
            string nn = null;
            int index = FindNoteName(upperNoteName, out nn);

            if (!String.IsNullOrEmpty(nn))
            {
                if (upperNoteName.Length > nn.Length)
                {
                    try
                    {
                        octave = int.Parse(upperNoteName.Substring(nn.Length), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        return;
                    }
                }

                noteName = nn;
                noteNumber = ((octave - OctaveOffset) * NoteCount) + index;

                CompileFullNoteName();
            }
        }

        /// <summary>
        /// Compiles a note name based on the specified <paramref name="newNoteNumber"/>.
        /// </summary>
        /// <param name="newNoteNumber">A note number as used in the midi NoteOn and NoteOff messages.</param>
        private void CompileNoteName(int newNoteNumber)
        {
            octave = (newNoteNumber / NoteCount) + OctaveOffset;
            noteName = NoteNames[newNoteNumber % 12];

            CompileFullNoteName();
        }

        /// <summary>
        /// Combines the note name and octave information.
        /// </summary>
        private void CompileFullNoteName()
        {
            fullNoteName = NoteName + Octave.ToString(CultureInfo.InvariantCulture);
            RaisePropertyChanged(nameof(FullNoteName));
        }

        /// <summary>
        /// Finds the note name in the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A full note name that is compared to the possible known names.</param>
        /// <param name="result">The resulting note name - without octave information.</param>
        /// <returns>Returns an index of the note name found.</returns>
        private static int FindNoteName(string value, out string result)
        {
            Argument.IsNotNull(() => value);

            result = null;

            var index = 0;
            var resultIndex = 0;

            foreach (var nn in NoteNames)
            {
                if (nn != null && value.StartsWith(nn, StringComparison.OrdinalIgnoreCase))
                {
                    result = nn;
                    resultIndex = index;
                }

                index++;
            }

            return resultIndex;
        }

        /// <summary>
        /// Backing field for the <see cref="NoteNumber"/> property.
        /// </summary>
        private int noteNumber;

        /// <summary>
        /// The note number as it is used in the midi NoteOn and NoteOff messages.
        /// </summary>
        public int NoteNumber
        {
            get { return this.noteNumber; }

            set
            {
                if (Equals(value, noteNumber))
                {
                    return;}
                Argument.IsNotOutOfRange(() => value, 0, 127);

                noteNumber = value;

                CompileNoteName(value);
                RaisePropertyChanged(nameof(NoteNumber));
            }
        }

        /// <summary>
        /// Backing field for the <see cref="FullNoteName"/> property.
        /// </summary>
        private string fullNoteName;

        /// <summary>
        /// Gets or sets the full note name including octave information.
        /// </summary>
        public string FullNoteName
        {
            get { return this.fullNoteName; }
            set { this.ParseFullNoteName(value); }
        }

        /// <summary>
        /// Backing field for the <see cref="NoteName"/> property.
        /// </summary>
        private string noteName;

        /// <summary>
        /// Gets or sets the bare note name.
        /// </summary>
        public string NoteName
        {
            get { return this.noteName; }

            set
            {
                ParseFullNoteName(value);
                CompileFullNoteName();
            }
        }

        /// <summary>
        /// Backing field for the <see cref="Octave"/> property.
        /// </summary>
        private int octave;

        /// <summary>
        /// Gets or sets the octave.
        /// </summary>
        public int Octave
        {
            get { return octave; }

            set
            {
                // TODO: validate.
                // octave (compensated with offset) should be in range 0-9
                octave = value;

                CompileFullNoteName();
            }
        }

        /// <summary>
        /// Backing field for the <see cref="OctaveOffset"/> property.
        /// </summary>
        private int octaveOffset = -2;

        /// <summary>
        /// Gets or sets the octave offset.
        /// </summary>
        /// <remarks>Usually a negative number or zero. Default value is 0 (zero).
        /// An octave offset of -2 is also common.</remarks>
        public int OctaveOffset
        {
            get { return octaveOffset; }

            set
            {
                octaveOffset = value;
                CompileNoteName(NoteNumber);
            }
        }
    }
}