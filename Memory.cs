using System;
using System.Collections.Generic;

namespace CybersecurityChatbotGUI
{
    /// <summary>
    /// Stores and retrieves user-provided information across the conversation session.
    ///
    /// Architecture:
    ///   - _store        : Key-value dictionary for named facts (name, interests, concerns)
    ///   - _topicHistory : Ordered list of topics discussed this session
    ///
    /// This class enables the chatbot to "remember" what the user has shared and
    /// reference it naturally in later responses, creating personalised engagement.
    /// Implements the Memory and Recall rubric requirement.
    /// </summary>
    public class Memory
    {
        // ── Private Storage ───────────────────────────────────────────────────

        /// <summary>Key-value store for named memory entries.</summary>
        private readonly Dictionary<string, string> _store =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Ordered history of cybersecurity topics raised this session.</summary>
        private readonly List<string> _topicHistory = new List<string>();

        /// <summary>Counter to track how many exchanges have occurred.</summary>
        private int _exchangeCount = 0;

        // ── Core Store / Recall Methods ───────────────────────────────────────

        /// <summary>
        /// Stores a named memory entry. Overwrites existing value if key already exists.
        /// Example: Store("favourite_topic", "phishing")
        /// </summary>
        public void Store(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                _store[key.ToLower()] = value;
        }

        /// <summary>
        /// Retrieves a stored memory value by key.
        /// Returns null if the key does not exist.
        /// </summary>
        public string Recall(string key)
        {
            return _store.TryGetValue(key.ToLower(), out string val) ? val : null;
        }

        /// <summary>Returns true if the given memory key exists in the store.</summary>
        public bool Has(string key) => _store.ContainsKey(key.ToLower());

        /// <summary>Increments the conversation exchange counter.</summary>
        public void IncrementExchange() => _exchangeCount++;

        /// <summary>Returns the total number of conversation exchanges this session.</summary>
        public int ExchangeCount => _exchangeCount;

        // ── Topic History Methods ─────────────────────────────────────────────

        /// <summary>
        /// Records a cybersecurity topic into the ordered topic history.
        /// Avoids duplicates — each topic is recorded only once.
        /// </summary>
        public void RecordTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic) && !_topicHistory.Contains(topic.ToLower()))
                _topicHistory.Add(topic.ToLower());
        }

        /// <summary>
        /// Returns the most recently discussed topic, or null if none yet.
        /// Used to provide seamless follow-up context in conversation flow.
        /// </summary>
        public string LastTopic() =>
            _topicHistory.Count > 0 ? _topicHistory[^1] : null;

        /// <summary>
        /// Returns a comma-separated string of all topics discussed this session.
        /// Used to acknowledge the breadth of the conversation.
        /// </summary>
        public string AllTopics() =>
            _topicHistory.Count > 0 ? string.Join(", ", _topicHistory) : null;

        /// <summary>Returns the number of distinct topics covered this session.</summary>
        public int TopicCount => _topicHistory.Count;

        // ── Automatic Memory Scanning ─────────────────────────────────────────

        /// <summary>
        /// Scans incoming user input for memory cues and stores them automatically.
        ///
        /// Detects and stores:
        ///   - Topic keywords → recorded in topic history
        ///   - Name mentions  → "my name is X", "I am X", "call me X"
        ///   - Concern topics → "I am worried about X"
        ///   - Tool mentions  → "I use X"
        /// </summary>
        public void ScanAndStore(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string lower = input.ToLower();

            // ── Detect and record topic keywords ──────────────────────────────
            string[] topics = {
                "password", "phishing", "malware", "privacy", "2fa",
                "vpn", "ransomware", "browsing", "scam", "banking",
                "spyware", "virus", "backup", "smishing", "social engineering"
            };

            foreach (var topic in topics)
            {
                if (lower.Contains(topic))
                {
                    RecordTopic(topic);
                    // Store the most recently mentioned topic as "current topic"
                    Store("current_topic", topic);
                    break;
                }
            }

            // ── Detect name mentions ───────────────────────────────────────────
            // Handles: "my name is X", "I am X", "call me X", "I'm X"
            string[] namePhrase = { "my name is ", "i am ", "call me ", "i'm ", "im " };
            foreach (var phrase in namePhrase)
            {
                int idx = lower.IndexOf(phrase);
                if (idx >= 0)
                {
                    string rest = input.Substring(idx + phrase.Length).Trim();
                    string possibleName = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                                             .Trim('.', ',', '!', '?');

                    // Validate it looks like a name (2–25 chars, no digits)
                    if (possibleName.Length >= 2 && possibleName.Length <= 25
                        && !possibleName.Any(char.IsDigit))
                    {
                        Store("mentioned_name",
                            char.ToUpper(possibleName[0]) + possibleName.Substring(1).ToLower());
                    }
                    break;
                }
            }

            // ── Detect concern/worry about a specific topic ────────────────────
            if (lower.Contains("worried about") || lower.Contains("concerned about")
                || lower.Contains("scared of") || lower.Contains("afraid of"))
            {
                foreach (var topic in topics)
                {
                    if (lower.Contains(topic))
                    {
                        Store("worried_about", topic);
                        break;
                    }
                }
            }

            // ── Detect "I use X" tool/service mentions ────────────────────────
            if (lower.Contains("i use ") || lower.Contains("i'm using ") || lower.Contains("i am using "))
                Store("mentioned_tool", input.Trim());

            // ── Detect interest statements ────────────────────────────────────
            if (lower.Contains("interested in") || lower.Contains("i like") || lower.Contains("i enjoy"))
            {
                foreach (var topic in topics)
                {
                    if (lower.Contains(topic))
                    {
                        Store("interested_in", topic);
                        break;
                    }
                }
            }
        }

        // ── Contextual Memory Note Generation ────────────────────────────────

        /// <summary>
        /// Builds a personalised contextual note based on stored memories.
        /// This note is prepended to bot responses to create the feeling of
        /// a continuous, remembered conversation.
        ///
        /// Returns an empty string if no relevant memories are available yet.
        /// </summary>
        public string GetContextNote(string userName)
        {
            var notes = new List<string>();

            // Reference a previously expressed concern
            string worriedAbout = Recall("worried_about");
            if (worriedAbout != null)
                notes.Add($"I remember you mentioned being concerned about {worriedAbout}, {userName}.");

            // Reference interest if different from concern
            string interestedIn = Recall("interested_in");
            if (interestedIn != null && interestedIn != worriedAbout)
                notes.Add($"Since you expressed interest in {interestedIn}, this may be especially relevant for you.");

            // Reference the last discussed topic for flow continuity
            string lastTopic = LastTopic();
            if (lastTopic != null && lastTopic != worriedAbout && lastTopic != interestedIn)
                notes.Add($"Building on our discussion about {lastTopic}:");

            return notes.Count > 0 ? string.Join(" ", notes) + "\n\n" : "";
        }

        /// <summary>
        /// Returns a milestone message when the user reaches topic milestones.
        /// Encourages continued engagement and learning.
        /// </summary>
        public string GetMilestoneMessage(string userName)
        {
            return TopicCount switch
            {
                3 => $"🎓 Great progress, {userName}! You have explored {TopicCount} cybersecurity topics so far. Keep going!",
                5 => $"⭐ Impressive, {userName}! You have now covered {TopicCount} topics. You are becoming a cybersecurity-aware user!",
                8 => $"🏆 Outstanding, {userName}! {TopicCount} topics covered — you are now more cyber-aware than most internet users!",
                _ => null
            };
        }
    }
}
