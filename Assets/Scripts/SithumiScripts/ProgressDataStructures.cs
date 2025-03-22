using System;
using System.Collections.Generic;

namespace SithumiProgress
{
    // Represents a single round in a session, tracking progress details
    [System.Serializable]
    public class Round
    {
        public int round;         // Round number
        public string targetLetter;  // The letter the player is supposed to identify
        public float accuracy;    // Accuracy percentage for the round
        public int attempts;      // Number of attempts made
        public bool skipped;      // Whether the round was skipped
    }

    // Represents a session consisting of multiple rounds
    [System.Serializable]
    public class Session
    {
        public string Date;        // Date of the session
        public List<Round> rounds = new List<Round>(); // List of rounds in the session
    }

    // Stores overall progress data, containing multiple sessions
    [System.Serializable]
    public class ProgressData
    {
        public List<Session> sessions = new List<Session>(); // List of recorded sessions
    }
}
