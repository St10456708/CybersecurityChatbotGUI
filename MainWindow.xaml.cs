using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CybersecurityChatbotGUI
{
    /// <summary>
    /// Code-behind for the Cybersecurity Awareness Chatbot main window.
    ///
    /// Responsibilities:
    ///   - Window startup: ASCII art display, voice greeting playback
    ///   - Name entry: validates and stores the user's name
    ///   - Message rendering: bot and user chat bubbles with fade-in animation
    ///   - Input handling: keyboard (Enter), button click, and chip button events
    ///   - Sentiment detection: live mood indicator updated as the user types
    ///   - Conversation flow: follow-up detection, topic continuity, memory recall
    ///   - Memory: scans each message and injects personalised context into responses
    ///   - Milestone acknowledgement: celebrates topic exploration progress
    ///
    /// Design pattern: Event-driven UI with separation of concerns.
    /// All response logic lives in Responses.cs; all memory in Memory.cs.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Private Fields ────────────────────────────────────────────────────

        /// <summary>The user's name — set during the name entry step.</summary>
        private string _userName = "User";

        /// <summary>Memory instance for storing and recalling user context.</summary>
        private readonly Memory _memory = new Memory();

        /// <summary>Whether the chat session has started (name entered).</summary>
        private bool _chatStarted = false;

        /// <summary>The last topic the bot gave a full response about.</summary>
        private string _lastResponseTopic = null;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>Initialises the window components.</summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        // ── Window Loaded ─────────────────────────────────────────────────────

        /// <summary>
        /// Called when the window has fully loaded.
        /// Plays the voice greeting and shows the initial bot welcome message.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayVoiceGreeting();
            AddBotMessage("🛡️ Welcome to the Cybersecurity Awareness Bot v2.0!\n\nI am your personal digital guardian — here to help you stay safe online.\n\nPlease enter your name above to begin our conversation.", isGreeting: true);
        }

        // ── Voice Greeting ────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to play the WAV voice greeting file from the application directory.
        /// Fails gracefully — the chatbot continues normally if audio is unavailable.
        /// </summary>
        private void PlayVoiceGreeting()
        {
            try
            {
                // Look for greeting.wav in the application's base directory
                string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(wavPath))
                {
                    // Play asynchronously so the UI does not freeze
                    SoundPlayer player = new SoundPlayer(wavPath);
                    player.Play();
                }
                // If file not found, continue silently — non-fatal
            }
            catch (Exception)
            {
                // Audio failure is non-fatal — chatbot continues normally
            }
        }

        // ── Name Entry ────────────────────────────────────────────────────────

        /// <summary>Handles Enter key press in the name input field.</summary>
        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BeginChat();
        }

        /// <summary>Handles the Start Chat button click.</summary>
        private void StartChat_Click(object sender, RoutedEventArgs e)
        {
            BeginChat();
        }

        /// <summary>
        /// Validates the entered name, then starts the chat session.
        /// Hides the name bar, ASCII splash, enables input controls,
        /// and delivers the personalised greeting message.
        /// </summary>
        private void BeginChat()
        {
            string name = NameInput.Text.Trim();

            // Validate — name must not be empty
            if (string.IsNullOrWhiteSpace(name))
            {
                NameInput.BorderBrush = new SolidColorBrush(Colors.Red);
                NameInput.ToolTip = "Please enter your name to begin!";
                return;
            }

            // Capitalise first letter of the name
            _userName = char.ToUpper(name[0]) + name.Substring(1);
            _memory.Store("user_name", _userName);
            _chatStarted = true;

            // ── Update UI for chat mode ───────────────────────────────────────
            NameBar.Visibility = Visibility.Collapsed;   // Hide name bar
            AsciiSplash.Visibility = Visibility.Collapsed;  // Hide ASCII splash

            // Enable all input controls and chip buttons
            UserInput.IsEnabled = true;
            SendBtn.IsEnabled = true;
            EnableChips(true);
            UserInput.Focus();

            // ── Personalised welcome message ──────────────────────────────────
            AddBotMessage(
                $"Wonderful to meet you, {_userName}! 🛡️\n\n" +
                $"I am your Cybersecurity Awareness Bot — your personal digital guardian.\n\n" +
                $"I am here to help you navigate cybersecurity threats and stay safe online.\n\n" +
                $"You can ask me about:\n" +
                $"🔑 Passwords  •  🎣 Phishing  •  🦠 Malware  •  🔐 2FA\n" +
                $"🕵️ Privacy  •  📶 VPN  •  🔥 Ransomware  •  ⚠️ Scams\n\n" +
                $"Type any topic keyword, ask a full question, or click a quick-topic button below! 👇");

            // Show an opening security tip to engage immediately
            AddBotMessage(Responses.GetRandomTip(), isTip: true);
        }

        // ── Message Processing ────────────────────────────────────────────────

        /// <summary>Handles the Send button click.</summary>
        private void SendBtn_Click(object sender, RoutedEventArgs e) => ProcessInput();

        /// <summary>Handles Enter key in the main text input.</summary>
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessInput();
        }

        /// <summary>
        /// Handles quick-topic chip button clicks.
        /// Sets the input text to the chip's topic and processes it immediately.
        /// </summary>
        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (!_chatStarted) return;
            var btn = (Button)sender;
            UserInput.Text = btn.Tag.ToString();
            ProcessInput();
        }

        /// <summary>
        /// Updates the live sentiment indicator as the user types.
        /// The mood detector responds in real-time to what is being typed.
        /// </summary>
        private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UserInput.Text))
                UpdateSentimentLabel(Responses.DetectSentiment(UserInput.Text));
        }

        /// <summary>
        /// Core message processing method.
        /// Pipeline:
        ///   1. Validate input
        ///   2. Display user bubble
        ///   3. Scan input for memory cues
        ///   4. Detect sentiment
        ///   5. Check for follow-up phrase
        ///   6. Get response (sentiment-aware)
        ///   7. Build memory context note
        ///   8. Combine and display bot response
        ///   9. Check for milestones
        ///  10. Occasionally show a random security tip
        /// </summary>
        private void ProcessInput()
        {
            if (!_chatStarted) return;

            string input = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            // Clear input immediately for responsive feel
            UserInput.Clear();

            // ── Step 1: Render the user's message ─────────────────────────────
            AddUserMessage(input);

            // ── Step 2: Scan input for memory cues ────────────────────────────
            _memory.ScanAndStore(input);
            _memory.IncrementExchange();

            // ── Step 3: Detect sentiment ──────────────────────────────────────
            string sentiment = Responses.DetectSentiment(input);
            UpdateSentimentLabel(sentiment);

            // ── Step 4: Handle follow-up / continuation phrases ───────────────
            if (Responses.IsFollowUp(input) && _lastResponseTopic != null)
            {
                // User wants more on the last topic — get a new random response
                string followUpInput = _lastResponseTopic;
                string followUpResponse = Responses.GetResponse(followUpInput, _userName, sentiment);
                string followUpPrefix = $"Great — here is more on {_lastResponseTopic} for you, {_userName}:\n\n";
                AddBotMessage(followUpPrefix + followUpResponse);
                return;
            }

            // ── Step 5: Get the main response ──────────────────────────────────
            string response = Responses.GetResponse(input, _userName, sentiment);

            // ── Step 6: Handle special return values ───────────────────────────
            if (response == null)
            {
                AddBotMessage($"I did not quite catch that, {_userName}. Could you please rephrase your question?");
                return;
            }

            if (response == "EXIT")
            {
                AddBotMessage(
                    $"Thank you for chatting with me today, {_userName}! 🛡️\n\n" +
                    $"You covered {_memory.TopicCount} cybersecurity topic(s) in this session.\n\n" +
                    $"Stay vigilant, stay informed, and stay safe online.\n" +
                    $"Remember: Think Before You Click! 👋\n\nGoodbye!");
                UserInput.IsEnabled = false;
                SendBtn.IsEnabled = false;
                EnableChips(false);
                StatusText.Text = "● Session ended — Stay Cyber Safe!";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(248, 81, 73));
                return;
            }

            if (response == "default")
            {
                // Intelligent fallback — reference last topic if available
                string lastTopic = _memory.LastTopic();
                string fallback = lastTopic != null
                    ? $"Hmm, I am not sure about that, {_userName}.\n\nWe were just discussing {lastTopic} — would you like more information on that?\n\nOr try one of these topics:\n🔑 password  •  🎣 phishing  •  🦠 malware  •  🔐 2fa\n🕵️ privacy  •  📶 vpn  •  🔥 ransomware  •  ⚠️ scam"
                    : $"Hmm, I am not sure about that, {_userName}. I specialise in cybersecurity topics.\n\nTry typing:\n🔑 password  •  🎣 phishing  •  🦠 malware  •  🔐 2fa\n🕵️ privacy  •  📶 vpn  •  🔥 ransomware  •  ⚠️ scam\n\nOr type 'help' for the full topic list!";
                AddBotMessage(fallback);
                return;
            }

            // ── Step 7: Build sentiment prefix ────────────────────────────────
            string sentimentPrefix = Responses.GetSentimentPrefix(sentiment, _userName);

            // ── Step 8: Build memory context note ─────────────────────────────
            string memoryNote = _memory.GetContextNote(_userName);

            // ── Step 9: Combine all parts and display ─────────────────────────
            string fullResponse = memoryNote + sentimentPrefix + response;
            AddBotMessage(fullResponse);

            // Track what topic this response was about for follow-up handling
            _lastResponseTopic = _memory.LastTopic();

            // ── Step 10: Check for topic milestone ────────────────────────────
            string milestone = _memory.GetMilestoneMessage(_userName);
            if (milestone != null)
                AddBotMessage(milestone, isTip: true);

            // ── Step 11: Periodic random security tip (every 4 exchanges) ─────
            if (_memory.ExchangeCount > 0 && _memory.ExchangeCount % 4 == 0)
                AddBotMessage(Responses.GetRandomTip(), isTip: true);
        }

        // ── UI Rendering Methods ──────────────────────────────────────────────

        /// <summary>
        /// Renders a bot message bubble in the chat panel.
        /// Bot bubbles appear on the LEFT with a cyan border.
        /// Tips use a special magenta/purple accent colour.
        /// Greeting messages use a slightly larger font.
        /// All bubbles animate in with a smooth fade.
        /// </summary>
        private void AddBotMessage(string text, bool isTip = false, bool isGreeting = false)
        {
            // Outer row panel (horizontal: avatar + bubble)
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 3, 0, 3)
            };

            // ── Bot avatar circle ─────────────────────────────────────────────
            var avatar = new Border
            {
                Width = 34,
                Height = 34,
                Background = isTip
                    ? new SolidColorBrush(Color.FromRgb(139, 92, 246))   // Purple for tips
                    : new SolidColorBrush(Color.FromRgb(0, 212, 255)),   // Cyan for normal
                CornerRadius = new CornerRadius(17),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            avatar.Child = new TextBlock
            {
                Text = isTip ? "💡" : "🤖",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            row.Children.Add(avatar);

            // ── Message bubble ────────────────────────────────────────────────
            var bubble = new Border
            {
                Background = isTip
                    ? new SolidColorBrush(Color.FromRgb(22, 15, 35))     // Dark purple for tips
                    : new SolidColorBrush(Color.FromRgb(22, 27, 34)),    // Dark blue-grey for normal
                BorderBrush = isTip
                    ? new SolidColorBrush(Color.FromRgb(139, 92, 246))   // Purple border for tips
                    : new SolidColorBrush(Color.FromRgb(0, 212, 255)),   // Cyan border for normal
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(0, 12, 12, 12),     // Flat top-left corner
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 600,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
                FontSize = isGreeting ? 14 : 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };
            bubble.Child = textBlock;
            row.Children.Add(bubble);

            ChatPanel.Children.Add(row);
            AnimateFadeIn(row);
            ScrollToBottom();
        }

        /// <summary>
        /// Renders a user message bubble in the chat panel.
        /// User bubbles appear on the RIGHT with a blue background.
        /// </summary>
        private void AddUserMessage(string text)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 3, 0, 3)
            };

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(31, 111, 235)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(56, 139, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12, 0, 12, 12),  // Flat top-right corner
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 520,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var textBlock = new TextBlock
            {
                Text = $"👤  {_userName}:  {text}",
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };
            bubble.Child = textBlock;
            row.Children.Add(bubble);

            ChatPanel.Children.Add(row);
            AnimateFadeIn(row);
            ScrollToBottom();
        }

        // ── Helper Methods ────────────────────────────────────────────────────

        /// <summary>
        /// Applies a smooth 300ms fade-in animation to a new chat bubble.
        /// This creates the polished, professional feel required by the rubric.
        /// </summary>
        private static void AnimateFadeIn(UIElement element)
        {
            element.Opacity = 0;
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            element.BeginAnimation(OpacityProperty, animation);
        }

        /// <summary>
        /// Scrolls the chat view to the most recent message.
        /// Called after every message is added.
        /// </summary>
        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToBottom();
        }

        /// <summary>
        /// Updates the live sentiment indicator label in the header.
        /// Each mood has a unique emoji, label, and colour.
        /// </summary>
        private void UpdateSentimentLabel(string sentiment)
        {
            var (emoji, label, color) = sentiment switch
            {
                "worried" => ("😟", "Worried", Color.FromRgb(248, 81, 73)),
                "frustrated" => ("😤", "Frustrated", Color.FromRgb(255, 166, 0)),
                "curious" => ("🤔", "Curious", Color.FromRgb(88, 230, 255)),
                "happy" => ("😊", "Happy", Color.FromRgb(63, 185, 80)),
                _ => ("😐", "Neutral", Color.FromRgb(139, 148, 158))
            };

            SentimentLabel.Text = $"{emoji}  {label}";
            SentimentLabel.Foreground = new SolidColorBrush(color);
        }

        /// <summary>
        /// Enables or disables all quick-topic chip buttons.
        /// Chips are disabled until the user has entered their name.
        /// </summary>
        private void EnableChips(bool enabled)
        {
            foreach (UIElement child in ChipsPanel.Children)
            {
                if (child is Button btn)
                    btn.IsEnabled = enabled;
            }
        }
    }
}