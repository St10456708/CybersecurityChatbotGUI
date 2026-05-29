using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbotGUI
{
    /// <summary>
    /// Manages ALL chatbot response logic for the Cybersecurity Awareness Bot.
    ///
    /// Architecture:
    ///   - RandomResponses   : Dictionary of topic -> List of varied responses (random selection)
    ///   - KeywordMap        : Dictionary of keyword -> single fixed response
    ///   - FollowUpPhrases   : List of phrases that trigger "tell me more" continuation
    ///   - SentimentKeywords : Four mood categories detected from user input
    ///   - SecurityTips      : Pool of rotating daily tips
    ///
    /// Design pattern: Static class with readonly dictionaries for O(1) lookups.
    /// All string operations are case-insensitive for natural language handling.
    /// </summary>
    public static class Responses
    {
        // ── Shared Random Instance ────────────────────────────────────────────
        /// <summary>Single shared Random instance — avoids seed collisions.</summary>
        private static readonly Random Rng = new Random();

        // ── Follow-up / Continuation Phrases ─────────────────────────────────
        /// <summary>
        /// Phrases that signal the user wants more on the current topic.
        /// Enables seamless conversational flow without restarting.
        /// </summary>
        private static readonly List<string> FollowUpPhrases = new List<string>
        {
            "tell me more", "more info", "explain more", "give me another",
            "elaborate", "continue", "go on", "keep going", "and then",
            "what else", "anything else", "more please", "more tips",
            "give me more", "explain further", "i want to know more"
        };

        // ── Random Responses (3 per topic for variety) ────────────────────────
        /// <summary>
        /// Each cybersecurity topic has 3 rich, varied responses.
        /// The chatbot randomly selects one — creating natural, non-repetitive conversation.
        /// Implements the Random Responses rubric requirement.
        /// </summary>
        private static readonly Dictionary<string, List<string>> RandomResponses =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // ── PASSWORD ──────────────────────────────────────────────────────
            { "password", new List<string> {
                "🔑 Passwords are your first line of defence!\n" +
                "• Use at least 12–16 characters\n" +
                "• Mix UPPERCASE, lowercase, numbers and symbols (!@#)\n" +
                "• Never reuse the same password across different sites\n" +
                "• Use a password manager like Bitwarden or 1Password\n" +
                "❌ Never use your name, birthdate, or 'password123'\n\n" +
                "💬 Type 'tell me more' for advanced password tips!",

                "🔑 Advanced password security:\n" +
                "• A passphrase like 'Coffee!Mango#River9' is strong AND memorable\n" +
                "• Password managers generate and store secure passwords for you\n" +
                "• Change passwords immediately if you suspect a breach\n" +
                "• Enable 2FA alongside strong passwords for double protection\n" +
                "• The longer the password, the exponentially harder to crack\n\n" +
                "💬 Ask me about 'password manager' for more details!",

                "🔑 Password breach statistics:\n" +
                "• Over 80% of data breaches involve weak or stolen passwords!\n" +
                "• Use a UNIQUE password for EVERY account — no exceptions\n" +
                "• Aim for 16+ characters — length beats complexity\n" +
                "• Avoid dictionary words — attackers use automated cracking tools\n" +
                "• Check haveibeenpwned.com to see if your password was leaked\n\n" +
                "💬 Type 'password manager' to learn how to manage them safely!"
            }},

            // ── PHISHING ──────────────────────────────────────────────────────
            { "phishing", new List<string> {
                "🎣 Phishing attacks trick you into revealing sensitive info!\n" +
                "🚩 Warning signs:\n" +
                "• Urgent language: 'Act NOW or your account closes!'\n" +
                "• Suspicious sender addresses (e.g. support@paypa1.com)\n" +
                "• Links that don't match the official website\n" +
                "• Requests for your password, PIN, or OTP via email\n" +
                "✅ Always go directly to the website by typing the URL yourself\n\n" +
                "💬 Type 'spear phishing' to learn about targeted attacks!",

                "🎣 Phishing red flags — look out for these:\n" +
                "• Emails asking for personal or financial information\n" +
                "• 'Congratulations, you have won!' messages\n" +
                "• Poor grammar and spelling in official-looking emails\n" +
                "• Mismatched URLs — hover over links BEFORE clicking!\n" +
                "✅ When in doubt, call the organisation directly to verify\n\n" +
                "💬 Type 'smishing' to learn about SMS phishing attacks!",

                "🎣 Phishing by the numbers:\n" +
                "• 1 in 99 emails is a phishing attack!\n" +
                "• Spear phishing targets specific individuals using personal info\n" +
                "• Smishing = phishing via SMS text messages\n" +
                "• Vishing = phishing via phone calls pretending to be your bank\n" +
                "✅ No legitimate bank or organisation will EVER ask for your password\n\n" +
                "💬 Type 'social engineering' to learn about psychological attacks!"
            }},

            // ── MALWARE ───────────────────────────────────────────────────────
            { "malware", new List<string> {
                "🦠 Malware is software designed to harm your system!\n" +
                "Types include:\n" +
                "• Viruses — spread by infecting legitimate files\n" +
                "• Trojans — disguised as legitimate software\n" +
                "• Spyware — secretly monitors your activity\n" +
                "• Ransomware — encrypts files and demands payment\n" +
                "✅ Keep antivirus updated and never open suspicious attachments\n\n" +
                "💬 Type 'ransomware' to learn about the most dangerous malware type!",

                "🦠 How to protect against malware:\n" +
                "• Install reputable antivirus (Windows Defender, Malwarebytes)\n" +
                "• Never download software from unofficial or unknown websites\n" +
                "• Scan ALL USB drives before opening any files on them\n" +
                "• Keep your OS and all applications fully and regularly updated\n" +
                "✅ Back up your data weekly to prevent ransomware damage\n\n" +
                "💬 Type 'virus' to learn specifically about computer viruses!",

                "🦠 Signs your device may be infected with malware:\n" +
                "• Slow performance or unexplained frequent crashes\n" +
                "• Unexpected pop-ups or browser redirects to strange sites\n" +
                "• Unknown programs running at startup\n" +
                "• Files missing, renamed, or encrypted without your action\n" +
                "✅ Run a full antivirus scan immediately if you notice these signs\n\n" +
                "💬 Type 'spyware' to learn how spyware secretly steals your data!"
            }},

            // ── PRIVACY ───────────────────────────────────────────────────────
            { "privacy", new List<string> {
                "🕵️ Protecting your digital privacy is essential!\n" +
                "✅ Best practices:\n" +
                "• Read privacy policies before signing up for ANY service\n" +
                "• Limit personal information shared on social media platforms\n" +
                "• Use DuckDuckGo or Brave as privacy-friendly search engines\n" +
                "• Regularly review and revoke app permissions on your phone\n" +
                "• Check haveibeenpwned.com regularly for data breaches\n\n" +
                "💬 Type 'data' to learn how to protect your personal data!",

                "🕵️ Your data is valuable — protect it like your most important asset!\n" +
                "• Disable location tracking on apps that don't need it\n" +
                "• Use encrypted messaging apps like Signal for sensitive conversations\n" +
                "• Enable full-disk encryption on all your devices\n" +
                "• ❌ 'Free' apps very often collect and sell your personal data\n" +
                "• Delete accounts and apps you no longer actively use\n\n" +
                "💬 Type 'social media' to learn about privacy on social platforms!",

                "🕵️ Social media and online privacy:\n" +
                "• Set your profiles to private wherever possible\n" +
                "• Never share your home address, phone number, or daily routine\n" +
                "• Be cautious about who you accept as friends or followers\n" +
                "✅ Regularly audit which apps have access to your accounts\n" +
                "• Think before you post — the internet never truly forgets!\n\n" +
                "💬 Type 'data breach' to know exactly what to do if your data is leaked!"
            }},

            // ── 2FA ───────────────────────────────────────────────────────────
            { "2fa", new List<string> {
                "🔐 Two-Factor Authentication (2FA) is one of the best security tools!\n" +
                "• Even if hackers steal your password, they cannot log in without the second factor\n" +
                "✅ Enable 2FA on: email, banking, social media, and shopping accounts\n" +
                "• Use an authenticator app (Google Authenticator, Microsoft Authenticator)\n" +
                "• Hardware keys like YubiKey offer the absolute strongest protection\n\n" +
                "💬 Type 'authenticator app' to learn which app to use!",

                "🔐 Types of 2FA ranked by security strength:\n" +
                "1. 🥇 Hardware key (YubiKey) — strongest, unphishable\n" +
                "2. 🥈 Authenticator app (TOTP) — very strong, recommended\n" +
                "3. 🥉 Email verification code — good, better than nothing\n" +
                "4. ⚠️  SMS code — weakest, vulnerable to SIM-swap attacks\n" +
                "✅ Always choose an authenticator app over SMS when given the option\n\n" +
                "💬 Type 'sim swap' to understand why SMS 2FA can be risky!",

                "🔐 Why 2FA is so powerful:\n" +
                "• Requires something you KNOW (password) + something you HAVE (phone/key)\n" +
                "• Blocks 99.9% of automated account compromise attacks (Microsoft research)\n" +
                "• Takes only 30 seconds to set up — massive security improvement\n" +
                "• Even if your password leaks in a breach, your account stays secure\n" +
                "✅ Start with your EMAIL account — it's the master key to all others!\n\n" +
                "💬 Type 'password' to pair strong passwords with 2FA for maximum security!"
            }},

            // ── VPN ───────────────────────────────────────────────────────────
            { "vpn", new List<string> {
                "📶 A VPN (Virtual Private Network) encrypts your internet connection!\n" +
                "✅ Always use a VPN on public Wi-Fi (cafés, airports, hotels)\n" +
                "• VPN hides your browsing activity from your ISP and nearby hackers\n" +
                "✅ Reputable VPNs: ProtonVPN (free tier), Mullvad, NordVPN, ExpressVPN\n" +
                "❌ Avoid free VPNs — many secretly log and sell your browsing data\n\n" +
                "💬 Type 'public wifi' to learn about Wi-Fi security risks!",

                "📶 VPN benefits explained:\n" +
                "• Encrypts ALL traffic between your device and the internet\n" +
                "• Hides your real IP address for enhanced privacy\n" +
                "• Lets you access content securely while travelling abroad\n" +
                "• Protects sensitive work data on public and hotel networks\n" +
                "⚠️ Note: A VPN improves privacy but is NOT a complete security solution\n\n" +
                "💬 Type 'browsing' to combine VPN use with safe browsing habits!",

                "📶 When should you use a VPN?\n" +
                "• ALWAYS when connecting to any public Wi-Fi network\n" +
                "• When accessing work resources or files remotely\n" +
                "• When travelling abroad and using hotel or café networks\n" +
                "• When you want to keep your browsing private from your ISP\n" +
                "✅ Leave your VPN permanently ON for maximum ongoing protection\n\n" +
                "💬 Type '2fa' to add another security layer alongside your VPN!"
            }},

            // ── RANSOMWARE ────────────────────────────────────────────────────
            { "ransomware", new List<string> {
                "🔥 Ransomware encrypts ALL your files and demands payment!\n" +
                "⚠️ Even paying the ransom does NOT guarantee you get your files back\n" +
                "✅ Prevention steps:\n" +
                "• Back up data to BOTH an offline drive AND cloud storage\n" +
                "• Keep all software and your OS fully and regularly updated\n" +
                "• Never open email attachments from unknown or suspicious senders\n" +
                "• Use security software with dedicated ransomware protection\n\n" +
                "💬 Type '3-2-1 backup' to learn the gold standard backup strategy!",

                "🔥 How ransomware spreads — know the attack vectors:\n" +
                "• Phishing emails with malicious attachments or links\n" +
                "• Drive-by downloads from compromised or fake websites\n" +
                "• Exploiting unpatched software vulnerabilities\n" +
                "• Via Remote Desktop Protocol (RDP) with weak passwords\n" +
                "✅ The 3-2-1 backup rule: 3 copies, 2 different media, 1 stored offsite\n\n" +
                "💬 Type 'backup' to learn exactly how to set up effective backups!",

                "🔥 What to do if you're hit by ransomware — act immediately:\n" +
                "1. Disconnect from the internet and all networks IMMEDIATELY\n" +
                "2. Do NOT pay the ransom — it funds criminal organisations\n" +
                "3. Report the attack to your national cybercrime authority\n" +
                "4. Contact a professional cybersecurity incident response team\n" +
                "5. Restore your system from your most recent clean backup\n" +
                "✅ Prevention is always better than cure — create backups TODAY!\n\n" +
                "💬 Type 'malware' to understand the full range of malware threats!"
            }},

            // ── BROWSING ──────────────────────────────────────────────────────
            { "browsing", new List<string> {
                "🌐 Safe browsing habits protect you from countless online threats!\n" +
                "✅ Always check for 'https://' and a padlock icon in the address bar\n" +
                "✅ Keep your browser updated regularly to patch security vulnerabilities\n" +
                "✅ Use uBlock Origin to block malicious ads and trackers\n" +
                "❌ Never download software from unofficial or unknown websites\n" +
                "❌ Never enter sensitive information on public Wi-Fi without a VPN\n\n" +
                "💬 Type 'vpn' to learn how a VPN enhances safe browsing!",

                "🌐 Advanced browser security tips:\n" +
                "• Use a privacy-focused browser like Brave or Firefox\n" +
                "• Enable enhanced protection and safe browsing mode in settings\n" +
                "• Clear cookies, cache, and browsing history regularly\n" +
                "• Always use incognito or private mode on shared computers\n" +
                "✅ Check website reputation with tools like VirusTotal before downloading\n\n" +
                "💬 Type 'safe browsing' for the complete safe browsing checklist!",

                "🌐 How to spot dangerous or fake websites:\n" +
                "• Missing HTTPS (no padlock icon in the address bar)\n" +
                "• Excessive pop-ups or automatic redirects to other pages\n" +
                "• Misspelled domain names (e.g. g00gle.com, faceb00k.com)\n" +
                "• Offers and deals that seem genuinely too good to be true\n" +
                "✅ When in doubt — close the tab and type the URL directly\n\n" +
                "💬 Type 'phishing' to learn how fake websites are used in phishing attacks!"
            }},

            // ── SCAM ──────────────────────────────────────────────────────────
            { "scam", new List<string> {
                "⚠️ Online scams are becoming increasingly sophisticated — stay alert!\n" +
                "Common types:\n" +
                "• Tech support scams — fake 'Microsoft' or 'Apple' calls\n" +
                "• Romance scams — building fake relationships to steal money\n" +
                "• Investment scams — promises of unrealistic guaranteed returns\n" +
                "• Lottery scams — 'You have won!' but must pay a fee first\n" +
                "✅ If it sounds too good to be true — it absolutely is!\n\n" +
                "💬 Type 'social engineering' to learn the psychology behind scams!",

                "⚠️ How to protect yourself from online scams:\n" +
                "• Never send money to someone you have only met online\n" +
                "• Thoroughly research any investment opportunity before committing\n" +
                "• Never pay upfront fees to claim 'prizes' or supposed 'winnings'\n" +
                "• Verify caller identity by hanging up and calling back via official number\n" +
                "✅ Report scams to your national consumer protection authority immediately\n\n" +
                "💬 Type 'phishing' to understand email-based scam tactics!",

                "⚠️ The most dangerous scam red flags to recognise immediately:\n" +
                "• Extreme urgency — 'You must act within the next 24 hours!'\n" +
                "• Requests for unusual payment methods (gift cards, cryptocurrency)\n" +
                "• Pressure not to tell your family or friends about the situation\n" +
                "• Promises of guaranteed investment returns with zero risk\n" +
                "✅ Always talk to a trusted person before making any financial decisions\n\n" +
                "💬 Type 'online banking' to learn how to protect your finances online!"
            }}
        };

        // ── Fixed Keyword Response Map ────────────────────────────────────────
        /// <summary>
        /// Fixed responses for greetings, specific queries, and commands.
        /// Covers a broad range of natural language phrasings per topic.
        /// </summary>
        private static readonly Dictionary<string, string> KeywordMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ── Greetings ─────────────────────────────────────────────────────
            { "hello",        "Hello, {name}! 👋 Welcome to the Cybersecurity Awareness Bot. What cybersecurity topic would you like to explore today?" },
            { "hi",           "Hi there, {name}! 😊 Great to see you taking cybersecurity seriously. Ask me anything — I am here to help!" },
            { "hey",          "Hey, {name}! 🛡️ Ready to boost your cybersecurity knowledge? Click a topic chip below or type any keyword to begin!" },
            { "good morning", "Good morning, {name}! ☀️ Starting the day with cybersecurity awareness is a great habit! What would you like to learn about today?" },
            { "good afternoon","Good afternoon, {name}! 🛡️ Hope your day is going well and securely! What cybersecurity topic can I help you with?" },
            { "good evening", "Good evening, {name}! 🌙 Let's make sure your online accounts are secure before you wind down. What can I help with?" },
            { "how are you",  "I am operating at 100% security capacity — no breaches detected! 😄 More importantly, how can I help keep YOU safe online today, {name}?" },
            { "who are you",  "I am the Cybersecurity Awareness Bot 🤖 — your personal digital guardian, {name}! I help everyday users understand threats and best practices to stay protected." },
            { "what can you do", "I can help you with, {name}:\n🔑 Password safety\n🎣 Phishing awareness\n🌐 Safe browsing\n🦠 Malware and viruses\n🔐 Two-factor authentication\n🕵️ Privacy and data protection\n🔥 Ransomware\n📶 VPN and Wi-Fi safety\n⚠️ Online scams\n🏦 Online banking\n🎭 Social engineering\n\nJust type any topic or click a quick-topic button!" },
            { "what is your purpose", "My purpose is to raise cybersecurity awareness, {name}! I empower everyday users with practical knowledge to stay safe online. Ask me about passwords, phishing, malware, 2FA, privacy, VPN, ransomware, scams, and much more!" },

            // ── Natural Language Keyword Variants ─────────────────────────────
            { "hacked",       "⚠️ If you think you have been hacked, {name}, act immediately!\n\nStep 1: Change your password on the affected account RIGHT NOW\nStep 2: Enable 2FA if you have not already done so\nStep 3: Check if other accounts use the same password — change those too\nStep 4: Review recent account activity for unauthorised actions\nStep 5: Check haveibeenpwned.com to see what data may have been exposed\nStep 6: Notify your bank if financial accounts may be affected\n\n✅ The first 30 minutes after a breach are critical — act fast!" },
            { "got hacked",   "⚠️ That is a serious situation, {name}! Here is exactly what to do right now:\n\nStep 1: Change your password on the affected account IMMEDIATELY\nStep 2: Enable 2FA on that account straight away\nStep 3: Check all accounts that share that password and change them\nStep 4: Review account activity for unauthorised transactions or posts\nStep 5: Contact your bank if financial details may have been compromised\n✅ Report the incident to your national cybercrime authority" },
            { "data breach",  "💾 If your data was exposed in a breach, {name}, here is what to do:\n\n1. Change the affected account password immediately\n2. Enable 2FA on that account\n3. Check if other accounts share that password — change ALL of them\n4. Monitor bank statements for any suspicious transactions\n5. Check haveibeenpwned.com to see exactly what was exposed\n✅ Act fast — the first 24 hours after a breach are the most critical!" },
            { "password manager", "🔑 A password manager is one of the best cybersecurity tools available, {name}!\n\n• You only need to remember ONE strong master password\n• It generates strong, unique passwords for every single site\n• Recommended options:\n  - Bitwarden (free, open-source, highly trusted)\n  - 1Password (excellent for families and teams)\n  - Dashlane (great user interface)\n• Most also alert you if your passwords appear in known data breaches\n✅ Setting up a password manager takes 10 minutes and dramatically improves your security!" },
            { "spear phishing", "🎣 Spear phishing is a targeted phishing attack aimed at a SPECIFIC person, {name}!\n\n• Attackers research their victim in advance using social media and LinkedIn\n• Emails look extremely convincing, often pretending to be your boss or your bank\n• They may reference real projects, colleagues, or recent events to seem legitimate\n✅ Always verify unexpected requests through a second trusted channel (e.g. a phone call)\n✅ Be especially cautious of emails asking you to transfer money, share credentials, or click links" },
            { "smishing",     "📱 Smishing is phishing carried out via SMS text messages, {name}!\n\n• You receive a text that appears to be from your bank, delivery company, or government\n• The message creates urgency: 'Your parcel is held — click here to pay the fee'\n• The link takes you to a convincing fake website that steals your details\n✅ Never click links in unexpected text messages\n✅ Go directly to the official website by typing it yourself\n✅ Your bank will NEVER ask for your PIN or full password via SMS" },
            { "public wifi",  "📶 Public Wi-Fi is a significant security risk, {name}!\n\n⚠️ Hackers on the same network can intercept unencrypted traffic\n⚠️ Fake 'free wifi' hotspots are created by attackers to steal your data\n✅ Always use a VPN when connecting to any public Wi-Fi network\n✅ Only visit HTTPS websites on public networks\n❌ Never log into banking, email, or shopping on unsecured public Wi-Fi\n❌ Turn off automatic Wi-Fi connection on your devices when in public" },
            { "online banking", "🏦 Online banking security is critical, {name}! Follow these best practices:\n\n✅ Always access your bank via its official app or by typing the URL yourself\n✅ Enable 2FA / multi-factor authentication on your bank account\n✅ Check your statements regularly for unauthorised transactions\n❌ Never click links in emails claiming to be from your bank\n❌ Never access banking on public Wi-Fi without a VPN\n❌ Bank staff will NEVER ask for your PIN, full password, or OTP — ever!" },
            { "social engineering", "🎭 Social engineering manipulates people psychologically rather than hacking systems, {name}!\n\nCommon tactics:\n• Impersonation — pretending to be IT support, management, or your bank\n• Urgency — 'You MUST act RIGHT NOW or face serious consequences!'\n• Baiting — offering something enticing to get you to click or download\n• Pretexting — creating a fabricated scenario to extract information\n✅ Always verify unexpected requests through a trusted, separate communication channel\n✅ No legitimate organisation will EVER ask for your password" },
            { "virus",        "🦠 A computer virus is malicious code that replicates by attaching itself to files, {name}!\n\n• Viruses can corrupt files, steal personal data, or crash your entire system\n• They spread via email attachments, infected USB drives, and software downloads\n✅ Use updated antivirus software (Windows Defender is excellent and free)\n✅ Scan all external USB drives BEFORE opening any files on them\n✅ Back up your data regularly to an external drive or cloud storage\n✅ Never open email attachments from unknown or unexpected senders" },
            { "spyware",      "🔍 Spyware secretly monitors your activity without your knowledge or consent, {name}!\n\n• It can record your keystrokes to steal passwords and banking details\n• It can capture screenshots, access your webcam, and read your messages\n• Often bundled with free software downloads from unofficial sources\n✅ Only download software from official, trusted sources\n✅ Use reputable antivirus software that includes spyware detection\n✅ Regularly review installed programs and remove anything unfamiliar" },
            { "backup",       "💾 Regular backups are your ultimate safety net against data loss, {name}!\n\nThe 3-2-1 Backup Rule (industry gold standard):\n• 3 copies of your important data\n• 2 different storage media types (e.g. hard drive + cloud)\n• 1 copy stored offsite (e.g. cloud storage like Google Drive or OneDrive)\n\n✅ Back up at least once a week\n✅ Test your backups periodically to make sure they actually restore correctly\n✅ Keep one backup completely offline to protect against ransomware\n✅ Do NOT store the only backup on the same device as the original!" },
            { "help",         "🛡️ Here are ALL the topics I can help you with, {name}:\n\n🔑 password  |  🎣 phishing  |  🌐 browsing\n🦠 malware   |  🔐 2fa       |  🕵️ privacy\n🔥 ransomware | 📶 vpn       |  ⚠️ scam\n🏦 online banking  |  🎭 social engineering\n💾 data breach  |  🔍 spyware  |  📱 smishing\n\nYou can also click any of the quick-topic buttons below the chat! 👇" },
            { "tip",          "RANDOM_TIP" },

            // ── Exit commands ─────────────────────────────────────────────────
            { "bye",     "EXIT" },
            { "goodbye", "EXIT" },
            { "exit",    "EXIT" },
            { "quit",    "EXIT" }
        };

        // ── Sentiment Detection Keyword Lists ─────────────────────────────────
        /// <summary>Keywords that indicate the user is feeling worried or anxious.</summary>
        private static readonly List<string> WorriedKeywords = new List<string>
        {
            "worried", "scared", "afraid", "anxious", "nervous", "concerned",
            "frightened", "panic", "fear", "terrified", "stressed", "unsafe",
            "vulnerable", "at risk", "danger", "dangerous", "help me"
        };

        /// <summary>Keywords that indicate the user is feeling frustrated or confused.</summary>
        private static readonly List<string> FrustratedKeywords = new List<string>
        {
            "frustrated", "angry", "annoyed", "confused", "lost", "stuck",
            "don't understand", "dont understand", "complicated", "difficult",
            "hard", "not working", "useless", "doesn't make sense", "makes no sense",
            "unclear", "complex", "too much"
        };

        /// <summary>Keywords that indicate the user is feeling curious and engaged.</summary>
        private static readonly List<string> CuriousKeywords = new List<string>
        {
            "curious", "interested", "want to know", "tell me more", "explain",
            "how does", "what is", "why", "wondering", "can you explain",
            "i would like to know", "fascinating", "intriguing", "how do"
        };

        /// <summary>Keywords that indicate the user is feeling happy or positive.</summary>
        private static readonly List<string> HappyKeywords = new List<string>
        {
            "great", "awesome", "amazing", "thanks", "thank you", "helpful",
            "good", "love", "excellent", "fantastic", "perfect", "brilliant",
            "wonderful", "appreciate", "nice", "cool", "impressive"
        };

        // ── Sentiment-Specific Response Variants ──────────────────────────────
        /// <summary>
        /// Topic responses tailored specifically for a WORRIED user.
        /// More reassuring, step-by-step, and empathetic in tone.
        /// </summary>
        private static readonly Dictionary<string, string> WorriedResponses =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "password",    "I completely understand your concern — password security can feel overwhelming at first. But do not worry, {name}, I will walk you through it step by step.\n\n🔑 Let's start simple:\n• Step 1: Pick a passphrase — three random words + a number + symbol (e.g. 'Mango!River9Blue')\n• Step 2: Use a different passphrase for each important account\n• Step 3: Download Bitwarden (it's free) to store them safely\n• That's it! You are already more secure than 80% of internet users.\n\n✅ You are doing the right thing by learning about this — that is the first and most important step!" },
            { "phishing",    "It is completely understandable to feel worried about phishing, {name} — these attacks are designed by professionals to fool people.\n\n🎣 Here is a simple rule that will protect you almost every time:\n• NEVER click links in unexpected emails — always type the website address yourself\n• If an email creates panic or extreme urgency — it is almost certainly a scam\n• When in doubt, call the organisation directly using the number from their OFFICIAL website\n\n✅ You are already ahead of most people just by being aware of phishing. Well done for asking!" },
            { "scam",        "It is completely understandable to feel worried about scams, {name} — they are becoming more sophisticated every day.\n\n⚠️ Here is what protects you most:\n• Take your time — scammers always create artificial urgency\n• Talk to someone you trust before making any financial decisions\n• Remember: if something feels wrong, it almost always IS wrong\n• Trust your instincts — they are your best security tool\n\n✅ The fact that you are being cautious already puts you in a much safer position!" },
            { "malware",     "I understand your concern about malware, {name} — it is a very real threat. But the good news is that basic precautions protect you from the vast majority of attacks.\n\n🦠 Here is your simple protection checklist:\n• Step 1: Make sure Windows Defender is ON (it is free and built into Windows)\n• Step 2: Never open email attachments from people you do not know\n• Step 3: Only download software from official websites\n• Step 4: Keep Windows updated — updates patch security vulnerabilities\n\n✅ Following just these four steps protects you from over 95% of common malware threats!" }
        };

        // ── Security Tips Pool ────────────────────────────────────────────────
        /// <summary>
        /// A pool of 12 rotating security tips displayed periodically.
        /// Keeps interactions fresh and educational throughout the session.
        /// </summary>
        private static readonly List<string> SecurityTips = new List<string>
        {
            "💡 TIP: Always use unique, strong passwords for every single account you own!",
            "💡 TIP: Enable Two-Factor Authentication (2FA) on ALL your important accounts — especially email and banking.",
            "💡 TIP: Think before you click — phishing links are designed to look completely legitimate at first glance.",
            "💡 TIP: Keep your operating system and ALL apps updated to patch known security vulnerabilities.",
            "💡 TIP: Back up your data regularly to BOTH a local drive AND cloud storage using the 3-2-1 rule.",
            "💡 TIP: Never share your passwords, PINs, or OTPs with ANYONE — even people claiming to be IT support.",
            "💡 TIP: Always use a VPN on public Wi-Fi networks to encrypt your internet connection.",
            "💡 TIP: Check haveibeenpwned.com regularly to see if your email appeared in a data breach.",
            "💡 TIP: Use a password manager — you only need to remember ONE master password for all accounts.",
            "💡 TIP: Always log out of your accounts when using shared or public computers.",
            "💡 TIP: Regularly review which apps have permission to access your phone's camera, microphone, and location.",
            "💡 TIP: Hover over links BEFORE clicking to preview the actual destination URL."
        };

        // ── Public Methods ────────────────────────────────────────────────────

        /// <summary>
        /// Detects the emotional sentiment of the user's input message.
        /// Returns one of: "worried", "frustrated", "curious", "happy", "neutral".
        /// Used to adapt response tone and content for empathetic interaction.
        /// </summary>
        public static string DetectSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "neutral";
            string lower = input.ToLower();

            // Check each sentiment category — order matters (worried checked first)
            if (WorriedKeywords.Any(w => lower.Contains(w))) return "worried";
            if (FrustratedKeywords.Any(w => lower.Contains(w))) return "frustrated";
            if (CuriousKeywords.Any(w => lower.Contains(w))) return "curious";
            if (HappyKeywords.Any(w => lower.Contains(w))) return "happy";
            return "neutral";
        }

        /// <summary>
        /// Returns a personalised sentiment-aware prefix to prepend to responses.
        /// Creates empathetic, mood-appropriate openings for each response.
        /// </summary>
        public static string GetSentimentPrefix(string sentiment, string userName)
        {
            return sentiment switch
            {
                "worried" => $"I completely understand your concern, {userName}. It is natural to feel that way — cybersecurity can seem daunting at first. Here is clear, practical guidance to help you:\n\n",
                "frustrated" => $"I hear you, {userName}! Let me break this down as clearly and simply as possible:\n\n",
                "curious" => $"Great question, {userName}! 🎓 I love your curiosity — here is a thorough explanation:\n\n",
                "happy" => $"Glad to hear it, {userName}! 😊 Here is more useful information for you:\n\n",
                _ => ""
            };
        }

        /// <summary>
        /// Returns a random security tip from the tips pool.
        /// Called periodically to keep the conversation educational.
        /// </summary>
        public static string GetRandomTip() =>
            SecurityTips[Rng.Next(SecurityTips.Count)];

        /// <summary>
        /// Detects if the user input is a follow-up/continuation phrase.
        /// Enables seamless conversation flow without restarting topics.
        /// </summary>
        public static bool IsFollowUp(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string lower = input.ToLower().Trim();
            return FollowUpPhrases.Any(phrase => lower.Contains(phrase));
        }

        /// <summary>
        /// Core response retrieval method.
        /// Priority order:
        ///   1. Worried sentiment + known topic → specialised worried response
        ///   2. Fixed keyword map (greetings, specific topics, commands)
        ///   3. Random response dictionary (3 varied answers per topic)
        ///   4. "default" — unrecognised input fallback
        /// Returns null for empty input, "EXIT" for exit commands, "RANDOM_TIP" for tip requests.
        /// </summary>
        public static string GetResponse(string input, string userName = "User", string sentiment = "neutral")
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.Trim().ToLower();

            // ── Priority 1: Worried user + known topic → compassionate response ──
            if (sentiment == "worried")
            {
                foreach (var kvp in WorriedResponses)
                {
                    if (lower.Contains(kvp.Key.ToLower()))
                        return kvp.Value.Replace("{name}", userName);
                }
            }

            // ── Priority 2: Fixed keyword map ─────────────────────────────────
            foreach (var kvp in KeywordMap)
            {
                if (lower.Contains(kvp.Key.ToLower()))
                {
                    string val = kvp.Value;
                    if (val == "EXIT") return "EXIT";
                    if (val == "RANDOM_TIP") return GetRandomTip();
                    return val.Replace("{name}", userName);
                }
            }

            // ── Priority 3: Random multi-response topics ───────────────────────
            foreach (var kvp in RandomResponses)
            {
                if (lower.Contains(kvp.Key.ToLower()))
                {
                    var list = kvp.Value;
                    return list[Rng.Next(list.Count)].Replace("{name}", userName);
                }
            }

            // ── Priority 4: Unrecognised input ────────────────────────────────
            return "default";
        }

        /// <summary>
        /// Returns true if the user input is an exit or quit command.
        /// Checked before GetResponse to handle the exit flow cleanly.
        /// </summary>
        public static bool IsExitCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string t = input.Trim().ToLower();
            return t is "bye" or "goodbye" or "exit" or "quit";
        }
    }
}
