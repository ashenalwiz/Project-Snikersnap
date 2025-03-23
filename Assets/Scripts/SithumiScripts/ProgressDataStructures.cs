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
        public string date;
        public List<Round> rounds;
    }

    [System.Serializable]
    public class ProgressData
    {
        public List<Session> sessions;
    }
}