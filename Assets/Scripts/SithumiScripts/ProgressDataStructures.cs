using System;
using System.Collections.Generic;

namespace SithumiProgress
{
    [System.Serializable]
    public class Round
    {
        public int round;
        public string targetLetter;
        public float accuracy;
        public int attempts;
        public bool skipped;
    }

    [System.Serializable]
    public class Session
    {
        // Changed from "date" to "Date" to match GameStatsRecorder's SessionData1
        public string Date;
        public List<Round> rounds = new List<Round>();
    }

    [System.Serializable]
    public class ProgressData
    {
        // Changed to match the SithumiProgressData structure from GameStatsRecorder
        public List<Session> sessions = new List<Session>();
    }
}