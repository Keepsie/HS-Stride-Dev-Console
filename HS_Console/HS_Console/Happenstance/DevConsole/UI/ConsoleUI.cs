// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Linq;
using Color = Stride.Core.Mathematics.Color;

namespace Happenstance.SE.DevConsole
{
    public enum MessageType
    {
        Normal,
        Error,
        Success,
        Warning,
        Information
    }

    public class ConsoleUI : HSSyncScript
    {
        private ConsoleManager _consoleManager;
        private SpriteFont _consoleFont;
        private ScrollViewer _scrollViewer;
        private StackPanel _consoleStack;
        private EditText _inputBlock;
        private Button _submitButton;
        public MouseOverState MouseOverState { get; }


        protected override void OnStart()
        {
            _consoleManager = Entity.Scene.FindAllComponents_HS<ConsoleManager>().FirstOrDefault();
            if (_consoleManager == null)
            {
                Logger.Error("Could not find ConsoleManager");
                return;
            }      

            //Subscribe to Console Manager
            _consoleManager.OnConsoleOutput += WriteToConsole;
            _consoleManager.OnClearConsole += ClearConsole;

            //Get strides default font or no text will show (This should be set in the source constructor and changed if needed by devs 
            try
            {
                _consoleFont = Content.Load<SpriteFont>("StrideDefaultFont");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load sprite font: {ex.Message}");
            }

           
                
            _consoleStack = Entity.GetUIElement_HS<StackPanel>("consoleStack");
            if (_consoleStack == null)
            {
                Logger.Error("Console stack panel not found");
                return;
            }

            _inputBlock = Entity.GetUIElement_HS<EditText>("consoleInput");
            if (_inputBlock == null)
            {
                Logger.Error("Console input text block not found");
                return;
            }

            _submitButton = Entity.GetUIElement_HS<Button>("submitButton");
            if (_submitButton == null)
            {
                Logger.Error("Console submit button not found");
                return;
            }

            _scrollViewer = Entity.GetUIElement_HS<ScrollViewer>("consoleScroll");
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollMode = ScrollingMode.Vertical;
                _scrollViewer.TouchScrollingEnabled = true;

                // Force scroll bar to be always visible (Doesn't work looks like source is force hiding in a update)
                var alwaysVisibleColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                _scrollViewer.ScrollBarColor = alwaysVisibleColor;
                    
                var field = typeof(ScrollViewer).GetField("transparent", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(null, alwaysVisibleColor);
                }
            }
                
            _submitButton.Click += OnSubmitClick;
            
            
            // Hide console initially
            SetActive(false);
        }

        protected override void OnUpdate()
        {
             HandleConsoleInput();
            

            if (_scrollViewer != null && IsEnabled)
            {
                // Force scroll bar to stay visible
                _scrollViewer.ScrollBarColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);

                // Handle mouse wheel scrolling
                var mouseWheel = Input.MouseWheelDelta;
                if (mouseWheel != 0 && _scrollViewer.MouseOverState != MouseOverState.MouseOverNone)
                {
                    var scrollAmount = new Vector3(0, -mouseWheel * 50, 0); // Adjust multiplier as needed
                    _scrollViewer.ScrollOf(scrollAmount);
                }
            }
        }

        private void WriteToConsole(string text, MessageType messageType = MessageType.Normal)
        {
            if (_consoleStack != null)
            {
                var textBlock = new TextBlock
                {
                    Text = text,
                    TextColor = GetColorForMessageType(messageType),
                    TextSize = 18f,
                    DefaultHeight = 200,
                    DefaultWidth = 200,
                    WrapText = true,
                    VerticalAlignment = Stride.UI.VerticalAlignment.Top,
                    Font = _consoleFont,
                };

                _consoleStack.Children.Add(textBlock);

                // Optional: Limit number of lines
                const int MaxLines = 1000;
                while (_consoleStack.Children.Count > MaxLines)
                {
                    _consoleStack.Children.RemoveAt(0);
                }

                // Force scroll to bottom after adding new text
                Script.AddTask(async () =>
                {
                    await Script.NextFrame();
                    ScrollToBottom();
                });
            }
        }

        
        //Submitting Console
        private string GetInputText() => _inputBlock?.Text ?? "";
        
        private void OnSubmitClick(object sender, EventArgs e)
        {
            if (_inputBlock != null)
            {
                SubmitCommand(_inputBlock.Text);
                ClearInputField();
            }
        }

        private void SubmitCommand(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            WriteToConsole($"> {input}");
            _consoleManager.ExecuteCommand(input);
            ClearInputField();
            FocusInputField();
        }

        private void HandleConsoleInput()
        {
            if (Input.IsKeyPressed(Keys.Up))
            {
                string previousCommand = _consoleManager.GetPreviousCommand();
                SetInputText(previousCommand);
            }
            else if (Input.IsKeyPressed(Keys.Down))
            {
                string nextCommand = _consoleManager.GetNextCommand();
                SetInputText(nextCommand);
            }
            else if (Input.IsKeyPressed(Keys.Enter))
            {
                string input = GetInputText();
                if (!string.IsNullOrEmpty(input))
                {
                    SubmitCommand(input);
                    ClearInputField();
                }
            }
            else if (Input.IsKeyPressed(Keys.Tab))
            {
                HandleTabCompletion();
            }
        }

        private void HandleTabCompletion()
        {
            string currentInput = _inputBlock.Text;

            // Use the manager's built-in suggestion system
            var matchingCommands = _consoleManager.GetSuggestions(currentInput);

            if (matchingCommands.Count == 1)
            {
                // If there's only one match, complete it fully
                _inputBlock.Text = matchingCommands[0];
                // Note: Stride might not have direct caret position control
            }
            else if (matchingCommands.Count > 1)
            {
                // If there are multiple matches, show them but don't change the input
                WriteToConsole("Available commands:");
                foreach (var command in matchingCommands)
                {
                    WriteToConsole($" {command}");
                }
            }
            // If there are no matches, do nothing
        }
        
        private void ClearConsole()
        {
            if (_consoleStack != null)
            {
                _consoleStack.Children.Clear();
                _consoleManager.DisplayConsoleInfo();
            }
        }
        
        private void ClearInputField()
        {
            if (_inputBlock != null)
            {
                _inputBlock.Text = string.Empty;
            }
        }

        private void SetInputText(string text)
        {
            if (_inputBlock != null)
            {
                _inputBlock.Text = text;
            }
        }

        private void FocusInputField()
        {
            if (_inputBlock != null)
            {
                // Need to wait a frame for the Enter key to finish processing
                Script.AddTask(async () =>
                {
                    await Script.NextFrame();
                    _inputBlock.IsSelectionActive = true;
                });
            }
        }


        //Other Helpers

        private void ScrollToBottom()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToEnd(Orientation.Vertical);
            }
        }

        private Color GetColorForMessageType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error:
                    return new Color(1.0f, 0.2f, 0.2f, 1.0f);
                case MessageType.Success:
                    return new Color(0.4f, 1.0f, 0.4f, 1.0f);
                case MessageType.Warning:
                    return new Color(1.0f, 0.9f, 0.2f, 1.0f);
                case MessageType.Information:
                    return new Color(0.4f, 0.9f, 1.0f, 1.0f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        protected override void OnEnable()
        {
            FocusInputField();
        }

        protected override void OnDisable()
        {
            ClearInputField();
        }

        protected override void OnDestroy()
        {
            if (_submitButton != null)
                _submitButton.Click -= OnSubmitClick;


            if (_consoleManager != null)
            {
                _consoleManager.OnConsoleOutput -= WriteToConsole;
                _consoleManager.OnClearConsole -= ClearConsole;
            }
              
        }
    }
}