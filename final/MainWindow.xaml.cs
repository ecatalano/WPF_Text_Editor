using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace final
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
 
    public partial class MainWindow : Window
    {
        public string pathName;
        public int currentLine;
        public bool isCurrentLineSet;
        public ArrayList savedLines;
        public ArrayList textAreaLines;
        public ArrayList hiddenLines;
        public ArrayList hiddenLineIndices;

        public MainWindow()
        {
            InitializeComponent();
            pathName = "Untitled";
            this.Window.Title = pathName;
            savedLines = new ArrayList();
            hiddenLines = new ArrayList();
            textAreaLines = new ArrayList();
            hiddenLineIndices = new ArrayList();
            currentLine = 0;
            isCurrentLineSet = false;
            this.updateStatusLabel();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            this.Save();
        }
        private void Save_As(object sender, RoutedEventArgs e)
        {
            this.Save_As();
        }
        private void Help(object sender, RoutedEventArgs e)
        {
            this.Help();
        }
        private void updateStatusLabel()
        {
            //format: name, current line #, number of lines in file
            int numberOfLines = this.textArea.LineCount;
            if (numberOfLines == -1) numberOfLines = 0;
            this.statusLabel.Content = string.Format("Path:{0} | Current Line:{1} | Number of Lines:{2}", pathName, currentLine, numberOfLines);
        }
        public void Save(){
            if (pathName == "")
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    this.pathName = dlg.FileName;
                }
            }
            if (this.pathName != "")
            {
                File.WriteAllText(pathName, this.textArea.Text);
                this.Window.Title = this.pathName;
                this.updateStatusLabel();
            }
        }
        public void Save_As()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                this.pathName = dlg.FileName;
                if (this.pathName != "")
                {
                    File.WriteAllText(pathName, this.textArea.Text);
                    this.Window.Title = this.pathName;
                    this.updateStatusLabel();
                }
            }

        }
        public void updateSuffixArea()
        {
            string tempString = "========\n";
            StringBuilder sb = new StringBuilder();

            if (this.textArea.LineCount > this.suffixArea.LineCount-1)
            {
                sb.Append(this.suffixArea.Text);
                int tempCount = this.suffixArea.LineCount-1;
                while (tempCount != this.textArea.LineCount)
                {
                    sb.Append(tempString);
                    tempCount++;
                }
            }
            else 
            {
                for (int i = 0; i < this.textArea.LineCount-1; i++)
                {
                    sb.Append(tempString);
                }
                sb.Append(tempString);
            }
            this.suffixArea.Text = sb.ToString();
        }
        public void Open()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                this.pathName = dlg.FileName;
            }
            if (this.pathName != "" && this.pathName!="Untitled")
            {
                this.textArea.Text = File.ReadAllText(this.pathName);
                this.Window.Title = this.pathName;
                this.updateStatusLabel();
                this.updateSuffixArea();
            }

        }
        public void Help()
        {
            this.helpLabel.Text = Properties.Resources.help;
            this.HelpPopup.IsOpen = true;
        }
        private void commandLine_TextChanged(object sender, TextChangedEventArgs e){
            //Eliminates possibility of the =====> being deleted (via backspace)
            if(!this.commandLine.Text.Contains("=====>")){
                this.commandLine.Text = "=====>";
                this.commandLine.CaretIndex = this.commandLine.Text.Length;
            }
            //Handle a command being sent via the command line 
            if (this.commandLine.Text.Contains("\n")){
                string inputString = this.commandLine.Text.Substring(6, this.commandLine.Text.Length - 8).ToLower();
                //Trace.WriteLine(String.Format("'{0}'",inputString));
                string[] s = inputString.Split(' ');
                if (inputString == "save")
                {
                    //Must be spelled out fully. No parameters. Overwrites the original file. 
                    //If no file exists, prompt for a path and name.
                    Save();
                }
                else if (inputString == "save as")
                {
                    //Must be spelled out fully. Saves the file to the name specified on the commandlineow to
                    Save_As();
                }
                else if (s[0] == "open")
                {
                    //Opens the specified file reads it into memory, closes it unchanged, but ready for editing
                    Open();
                }
                //NEED TO MAKE A COMMAND FOR #
                else if (s[0] == "up" && s.Length == 2 && s[1].All(char.IsDigit))
                {
                    //Scroll up # lines from the current display (display more data toward the top of file)

                    int targetLine = Convert.ToInt32(s[1]);

                    for (int i = 0; i < targetLine; i++)
                    {
                        textArea.LineUp();
                    }

                }
                else if (s[0] == "down" && s.Length == 2 && s[1].All(char.IsDigit))
                {
                    //Scroll down # lines from the current display (display more data toward the end of file)
                    int targetLine = Convert.ToInt32(s[1]);

                    for (int i = 0; i < targetLine; i++)
                    {
                        textArea.LineDown();
                    }

                }
                else if (s[0] == "left" && s.Length == 2 && s[1].All(char.IsDigit))
                {
                    //Scroll left # columns (the view shows more data on the left if any exists)
                    int targetLine = Convert.ToInt32(s[1]);

                    for (int i = 0; i < targetLine; i++)
                    {
                        textArea.LineLeft();
                    }

                }
                else if (s[0] == "right" && s.Length == 2 && s[1].All(char.IsDigit))
                {
                    //Scroll left # columns (the view shows more data on the left if any exists)
                    int targetLine = Convert.ToInt32(s[1]);

                    for (int i = 0; i < targetLine; i++)
                    {
                        textArea.LineRight();
                    }
                }
                else if (inputString == "forward")
                {
                    //Scrolls one “screenfull” toward the end of the file
                    double offset = textArea.VerticalOffset + (this.textArea.ActualHeight) - 40;

                    this.textArea.ScrollToVerticalOffset(offset);
                }
                else if (inputString == "back")
                {
                    //Scrolls one “screenfull” toward the top of the file
                    double offset = textArea.VerticalOffset - (this.textArea.ActualHeight) + 40;

                    this.textArea.ScrollToVerticalOffset(offset);
                }
                else if (s[0] == "setcl" && s.Length == 2 && s[1].All(char.IsDigit))
                {
                    //Must be spelled out fully. Defines which line number
                    //(of the lines on the screen, NOT the lines of the file) 
                    //is the “current line” until further notice. The “current line” is displayed in a red font. 
                    //The default “current line” is the 1st line of data on the screen.
                    int toChange = Convert.ToInt32(s[1]);
                    if (toChange < (this.textArea.LineCount - 1) && toChange > -1)
                    {
                        currentLine = toChange;
                        isCurrentLineSet = true;
                        this.updateStatusLabel();
                    }

                }
                else if (s[0] == "change" && s.Length == 4 && s[3].All(Char.IsDigit))
                {
                    //(NOT a popup window) Finds & modifies a searched-for string, 
                    //starting with the defined “current line”
                    //e.g.; c/abc/ABC/n1    n2
                    //changes abc to ABC on n1 lines    n2 times per line. Implementing n2 is optional.
                    //change s1 s2 n lines from curent
                    string string1 = s[1];
                    string string2 = s[2];
                    int numLines = Convert.ToInt32(s[3]);

                    int goalIndex = 0;
                    int count = 0;
                    int lineCount = textArea.LineCount;
                    if (currentLine + numLines < this.textArea.LineCount)
                    {
                        goalIndex = currentLine + numLines;
                    }
                    else
                    {
                        goalIndex = this.textArea.LineCount;
                    }
                    string[] lines = new string[lineCount];
                    for (int i = 0; i < textArea.LineCount; i++)
                    {
                        lines[i] = textArea.GetLineText(i);
                    }
                    for (int i = currentLine; i < goalIndex; i++)
                    {
                        //Trace.WriteLine(string.Format("Searching for: '{0}' on line: '{1}'", s[1], tempLine));
                        if (textArea.GetLineText(i).Contains(string1))
                        {
                            lines[i] = lines[i].Replace(string1, string2);
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        textArea.Clear();
                        for (int i = 0; i < lineCount; i++)
                        {
                            textArea.AppendText(lines[i]);

                        }
                        this.searchLabel.Content = string.Format("Changed {0} instances of {1} to {2}", count, string1, string2);
                        this.SearchPopup.IsOpen = true;
                    }

                }
                else if (inputString == "help")
                {
                    //Opens a window with a list of the above commands and their usage
                    this.Help();
                }
                else if (s[0] == "search" && s.Length == 3 && s[2].All(Char.IsDigit))
                {
                    //(NOT a popup window) Allows the user to search for a string. 
                    //See rules below these tables for how a user can specify the search.

                    //search <string> <number of lines from current line>

                    //if found, change
                    int numLines = Convert.ToInt32(s[2]);
                    int goalIndex = 0;
                    ArrayList lines = new ArrayList();
                    if (currentLine + numLines < this.textArea.LineCount)
                    {
                        goalIndex = currentLine + numLines;
                    }
                    else
                    {
                        goalIndex = this.textArea.LineCount;
                    }
                    for (int i = currentLine; i < goalIndex; i++)
                    {
                        string tempLine = textArea.GetLineText(i);
                        //Trace.WriteLine(string.Format("Searching for: '{0}' on line: '{1}'", s[1], tempLine));

                        if (tempLine.Contains(s[1]))
                        {
                            //Trace.WriteLine(string.Format("Found {0} on line: {1}", s[1], i));
                            //output popup window that displays text "found on line: %d", line
                            lines.Add(i);

                        }
                    }
                    if (lines.Count > 0)
                    {
                        if (lines.Count == 1)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(string.Format("Found {0} on line: ", s[1]));

                            for (int i = 0; i < lines.Count; i++)
                            {
                                string tempString = Convert.ToString(lines[i]);
                                sb.Append(tempString);

                            }
                            this.searchLabel.Content = sb.ToString();
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(string.Format("Found {0} on lines: ", s[1]));

                            for (int i = 0; i < lines.Count; i++)
                            {
                                string tempString = Convert.ToString(lines[i]);
                                sb.Append(tempString);
                                if (i != lines.Count) sb.Append(", ");
                            }
                            this.searchLabel.Content = sb.ToString();
                        }
                    }
                    else
                    {
                        this.searchLabel.Content = string.Format("Did Not Find {0} on any lines searched.", s[1]);
                    }
                    this.SearchPopup.IsOpen = true;
                }

                else if (inputString.Length > 0 && inputString.All(char.IsDigit))
                {
                    int targetLine = Convert.ToInt32(s[0]);
                    if (targetLine >= 0)
                    {
                        if (targetLine - 1 < this.textArea.LineCount)
                        {
                            this.textArea.ScrollToLine(targetLine - 1);
                        }
                        else
                        {
                            this.textArea.ScrollToLine(this.textArea.LineCount - 1);
                        }
                    }
                }
                else
                {
                    searchLabel.Content = "Invalid Command. Refer to help page for more info.";
                    this.SearchPopup.IsOpen = true;
                }
                this.commandLine.Text = "=====>";
                this.commandLine.CaretIndex = this.commandLine.Text.Length;

            }
        }

        private void Hide_Search_Popup(object sender, RoutedEventArgs e)
        {
            this.SearchPopup.IsOpen = false;
        }
        private void Hide_Help_Popup(object sender, RoutedEventArgs e)
        {
            this.HelpPopup.IsOpen = false;
        }

        private void textArea_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!isCurrentLineSet)
            {
                currentLine = textArea.GetLineIndexFromCharacterIndex(textArea.SelectionStart);
            }
            updateStatusLabel();
        }

        private void textArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suffixArea.LineCount-1 != textArea.LineCount)
            {
                updateSuffixArea();
            }
        }

        private void textArea_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            suffixArea.ScrollToVerticalOffset(textArea.VerticalOffset);
        }
        private void suffixArea_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            textArea.ScrollToVerticalOffset(suffixArea.VerticalOffset);

        }

        private void textArea_GotFocus(object sender, RoutedEventArgs e)
        {
            if (suffixArea.LineCount - 1 != textArea.LineCount)
            {
                updateSuffixArea();
            }
        }

        public void updateTextArea()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < textAreaLines.Count; i++)
            {
                if (!hiddenLineIndices.Contains(i))
                {
                    sb.Append(textAreaLines[i]);
                }
                else
                {
                    if (Convert.ToString(hiddenLines[hiddenLineIndices.IndexOf(i)]) != Convert.ToString(textAreaLines[i]))
                    {
                        sb.Append(textAreaLines[i]);
                    }
                }
            }
            textArea.Text = sb.ToString();
        }
        public void saveTextArea()
        {
            textAreaLines.Clear();
            for (int i = 0; i < textArea.LineCount; i++)
            {
                textAreaLines.Add(textArea.GetLineText(i));
            }
        }

        private void suffixArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            int line = suffixArea.GetLineIndexFromCharacterIndex(suffixArea.SelectionStart);
            string inputString = suffixArea.GetLineText(line);
            int firstIndex = suffixArea.GetCharacterIndexFromLineIndex(line);
            int lastIndex = suffixArea.SelectionStart-1;
            if (lastIndex >= 0)
            {
                if (lastIndex < firstIndex)
                {
                    line = suffixArea.GetLineIndexFromCharacterIndex(lastIndex);
                    inputString = suffixArea.GetLineText(line);
                    firstIndex = suffixArea.GetCharacterIndexFromLineIndex(line);

                    char lastChar = inputString[lastIndex - firstIndex];
                    if (lastChar == '\n')
                    {
                        string tempString = inputString.Replace("=", "");
                        tempString = tempString.Replace("\r\n", "");
                        string[] s = tempString.Split(' ');


                        if (s[0] == "i" && s.Length == 2 && s[1].All(Char.IsDigit))
                        {
                            //Insert # lines
                                string tempString2 = "\n";
                                saveTextArea();
                                int tempLine = line+1;
                                for(int i = 0; i < Convert.ToInt32(s[1]); i++){
                                    this.textAreaLines.Insert(tempLine, tempString2);
                                    tempLine++;
                                }
                                updateTextArea();
                        
                        }
                        else if (s[0] == "x")
                        {
                            //Exclude (hide) # lines (default #=1) both from view and from actions
                            //show a “hidden line” marker across the screen with its 
                            //own suffix area for suffix commands.

                            //Copy # lines (default #=1) starting at this line
                            if (hiddenLineIndices.Count == 0)
                            {
                                if (s.Length == 1)
                                {
                                    //copy this line

                                    hiddenLineIndices.Add(line);
                                    hiddenLines.Add(textArea.GetLineText(line));
                                }
                                else if (s.Length == 2 && s[1].All(Char.IsDigit))
                                {
                                    int tempNum = Convert.ToInt32(s[1]);
                                    if (line + tempNum > textArea.LineCount) tempNum = textArea.LineCount - line;
                                    for (int i = 0; i < tempNum; i++)
                                    {
                                       
                                        hiddenLineIndices.Add(line);
                                        hiddenLines.Add(textArea.GetLineText(line));
                                        line++;
                                    }
                                }
                                saveTextArea();
                                updateTextArea();
                            }
                            else
                            {
                                searchLabel.Content = "There are lines already hidden. Must show them before hiding more.";
                                SearchPopup.IsOpen = true;
                            }

                        }
                        else if (s[0] == "s")
                        {
                            //Show # lines (default n=1) of those excluded. 
                            //The lines to be shown are the LAST # lines of the # hidden.
                            //(must be entered on the “hidden lines” indicator line)
                            saveTextArea();
                            if (hiddenLineIndices.Count != 0)
                            {
                                if (s.Length == 1)
                                {
                                    //copy this line
                                    int tempIndex = Convert.ToInt32(hiddenLineIndices[hiddenLineIndices.Count - 1]);
                                    string tempLine = Convert.ToString(hiddenLines[hiddenLines.Count - 1]);
                                    textAreaLines.Insert(tempIndex, tempLine);
                                    hiddenLineIndices.RemoveAt(hiddenLineIndices.Count - 1);
                                    hiddenLines.RemoveAt(hiddenLines.Count - 1);
                                }
                                else if (s.Length == 2 && s[1].All(Char.IsDigit))
                                {
                                    for (int i = 0; i < Convert.ToInt32(s[1]); i++)
                                    {
                                        int tempIndex = Convert.ToInt32(hiddenLineIndices[hiddenLineIndices.Count - 1]);
                                        string tempLine = Convert.ToString(hiddenLines[hiddenLines.Count - 1]);
                                        int tempIndex2 = tempIndex - hiddenLineIndices.Count + 1;
                                        if (tempIndex2 < 0) tempIndex2 = 0;
                                        textAreaLines.Insert(tempIndex2, tempLine);
                                        hiddenLineIndices.RemoveAt(hiddenLineIndices.Count - 1);
                                        hiddenLines.RemoveAt(hiddenLines.Count - 1);
                                    }
                                }
                            }
                            else
                            {
                                searchLabel.Content = "You must hide lines before showing them.";
                                SearchPopup.IsOpen = true;
                            }
                            updateTextArea();

                        }
                        else if (s[0] == "a")
                        {
                            if (savedLines.Count == 0)
                            {
                                searchLabel.Content = "You must copy lines before inserting.";
                                SearchPopup.IsOpen = true;
                            }
                            //Insert AFTER this line
                            saveTextArea();
                            int tempLine = line+1;
                            for (int i = 0; i < savedLines.Count; i++)
                            {
                                string tempString2 = Convert.ToString(savedLines[i]);
                                this.textAreaLines.Insert(tempLine, tempString2);
                                tempLine++;
                            }
                            savedLines.Clear();
                            updateTextArea();

                        }
                        else if (s[0] == "b")
                        {
                            //Insert BEFORE this line
                            saveTextArea();
                            if (savedLines.Count == 0)
                            {
                                searchLabel.Content = "You must copy lines before inserting.";
                                SearchPopup.IsOpen = true;
                            }
                            int tempLine = line;
                            if (tempLine < 0) tempLine = 0;
                            for (int i = 0; i < savedLines.Count; i++)
                            {
                               
                                string tempString2 = Convert.ToString(savedLines[i]);
                                this.textAreaLines.Insert(tempLine, tempString2);
                                tempLine++;
                            }
                            savedLines.Clear();
                            updateTextArea();
                        }
                        else if (s[0] == "c")
                        {
                            //Copy # lines (default #=1) starting at this line
                            savedLines.Clear();
                            if (s.Length == 1)
                            {
                                //copy this line
                                savedLines.Add(textArea.GetLineText(line));
                            }
                            else if(s.Length == 2 && s[1].All(Char.IsDigit)){
                                for (int i = 0; i < Convert.ToInt32(s[1]); i++)
                                {
                                    savedLines.Add(textArea.GetLineText(line));
                                    line++;
                                }
                            }
                            searchLabel.Content = string.Format("Copied {0} lines.", Convert.ToInt32(s[1]));
                            SearchPopup.IsOpen = true;
                        }
                        else if (s[0] == "m")
                        {
                            //Move # lines (default #=1) starting at this line
                            saveTextArea();
                            int tempLine = line;
                            string tempString2 = textArea.GetLineText(line);
                            if (s.Length == 2 && s[1].All(Char.IsDigit))
                            {
                                int tempLine2 = line + 2;
                                if(s[1]!="") tempLine2 = line + Convert.ToInt32(s[1])+2;
                                if (tempLine2 >= textArea.LineCount-1)
                                {
                                    tempLine2 = textArea.LineCount;
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(textAreaLines[tempLine2 - 1]).Append("\r\n");
                                    textAreaLines[tempLine2 - 1] = sb.ToString();
                                    this.textAreaLines.Insert(tempLine2, tempString2);
                                }
                                else
                                {
                                    this.textAreaLines.Insert(tempLine2, tempString2);
                                }
                            }
                            else
                            {
                                int tempLine2 = line + 2;
                                if (tempLine2 >= textArea.LineCount - 1)
                                {
                                    tempLine2 = textArea.LineCount;
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(textAreaLines[tempLine2-1]).Append("\r\n");
                                    textAreaLines[tempLine2-1] = sb.ToString();
                                    this.textAreaLines.Insert(tempLine2, tempString2);
                                }
                                else
                                {
                                    this.textAreaLines.Insert(tempLine2, tempString2);
                                }
                            }
                            this.textAreaLines.RemoveAt(tempLine);
                            updateTextArea();
                        }
                        else if (s[0] == "\"")
                        {
                            //Duplicate this 1 line
                            saveTextArea();
                            int tempLine = line;
                            string tempString2 = textArea.GetLineText(line);
                            this.textAreaLines.Insert(tempLine, tempString2);
                            updateTextArea();
                        }
                        else
                        {
                            searchLabel.Content = "Invalid command. See help page for more info.";
                            SearchPopup.IsOpen = true;
                        }
                        updateSuffixArea();

                        int newLineIndex = suffixArea.GetLineIndexFromCharacterIndex(suffixArea.SelectionStart);
                        string newLine = suffixArea.GetLineText(newLineIndex);
                        suffixArea.CaretIndex = newLine.Length-1;

                    }
                }
            }
        }
    }
}
