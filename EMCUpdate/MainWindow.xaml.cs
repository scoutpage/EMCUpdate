using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Text.RegularExpressions;
using IniMiniFile;

/*
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
*/

namespace EMCUpdate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw = new BackgroundWorker();
        clsFileInfo BWInput1;
        clsFileInfo BWInput2;
        bool StopMonitoring;
        bool ErrorDuringFileRead;

        Thread thrMonitorFiles;

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine("Initiating Background Task");

            //Initiate the background process
            bw.WorkerSupportsCancellation = false;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(UpdateGUI);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.WorkerSupportsCancellation = true;
            StopMonitoring = false;
            ErrorDuringFileRead = false;

            tblTest.Text = "Idle";
            EMCMainForm.Title = "EMCUpdate - Idle";
        }

        static clsFileInfo FillFileInfo_NotUsed(string tFileStr)
        {
            clsFileInfo tFileInfo = new clsFileInfo();

            //Variables concerning the format of the line - read from the ini-file
            tFileInfo.SetFormatValues('\t', '\"', true, 1); //DividerL0=9 (Tab), DividerL1=34 ("), ConSeqDiv=true, FirstLine=1
            //tFileInfo.strFilePath = @"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug\MyTest.txt";
            tFileInfo.FileName = tFileStr;

            //#2021-05-28 00:20:01		0.20		01000		Rigtigt
            //Variables concerning values read - read from the ini-file
            tFileInfo.strNominal.Add("");
            tFileInfo.Min.Add(-1);
            tFileInfo.Max.Add(-1);
            tFileInfo.InfoType.Add("Ignore");

            tFileInfo.strNominal.Add("");
            tFileInfo.Min.Add(0.18);
            tFileInfo.Max.Add(0.23);
            tFileInfo.InfoType.Add("Number");

            tFileInfo.strNominal.Add("");
            tFileInfo.Min.Add(800);
            tFileInfo.Max.Add(1200);
            tFileInfo.InfoType.Add("Number");

            tFileInfo.strNominal.Add("Rigtigt");
            tFileInfo.Min.Add(-1);
            tFileInfo.Max.Add(-1);
            tFileInfo.InfoType.Add("Text");

            return tFileInfo;
        }

        public bool ReadIniFile(string InFileName)
        {
            clsFileInfo tBWInput1 = new clsFileInfo();

            tBWInput1.SetFormatValues('\t', '\"', true, 1);
            tBWInput1.SetFilenameUpdate("", "*cLLD.txt", 200);

            tBWInput1.FilePath = @"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug";

            tBWInput1.AddNumberValues(8, 13, "", "Ignore");
            tBWInput1.AddNumberValues(0.18, 0.23, "", "Number");
            tBWInput1.AddNumberValues(800, 1200, "", "Number");
            tBWInput1.AddNumberValues(0, 0, "Rigtigt", "Text");
            tBWInput1.ResetCompareResults();
            BWInput1 = tBWInput1;
            return true;
        }

        public bool FilenameMatchesPattern(string filename, string pattern)
        {
            // prepare the pattern to the form appropriate for Regex class
            StringBuilder sb = new StringBuilder(pattern);
            // remove superflous occurences of  "?*" and "*?"
            while (sb.ToString().IndexOf("?*") != -1)
            {
                sb.Replace("?*", "*");
            }
            while (sb.ToString().IndexOf("*?") != -1)
            {
                sb.Replace("*?", "*");
            }
            // remove superflous occurences of asterisk '*'
            while (sb.ToString().IndexOf("**") != -1)
            {
                sb.Replace("**", "*");
            }
            // if only asterisk '*' is left, the mask is ".*"
            if (sb.ToString().Equals("*"))
                pattern = ".*";
            else
            {
                // replace '.' with "\."
                sb.Replace(".", "\\.");
                // replaces all occurrences of '*' with ".*" 
                sb.Replace("*", ".*");
                // replaces all occurrences of '?' with '.*' 
                sb.Replace("?", ".");
                // add "\b" to the beginning and end of the pattern
                sb.Insert(0, "\\b");
                sb.Append("\\b");
                pattern = sb.ToString();
            }
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(filename);
        }

        void FindFile(ref clsFileInfo tBWInput)
        {
            tBWInput.FilePath = tBWInput.FilePath.Trim();
            if (Directory.Exists(tBWInput.FilePath) == false)
            { return; }
            else
            {
                //Find all files with the search pattern
                string[] tFiles = Directory.GetFiles(tBWInput.FilePath, tBWInput.FilePattern);

                for (int k = 0; k < tFiles.Length; k++)
                    Console.WriteLine(k.ToString() + ": " + tFiles[k]);
                
                int SelectedItem = 0;
                if (tFiles.Length == 0) return;
                else
                {
                    DateTime CurrFFD = DateTime.MinValue;
                    for (int i = 0; i < tFiles.Length; i++)
                    {
                        //Check if its a new date
                        // Less than zero  t1 is earlier than t2.
                        // Zero t1 is the same as t2.
                        // Greater than zero   t1 is later than t2.
                        int tInt = CmpFilenameDate(ref CurrFFD, tFiles[i]);
                        if (tInt > 0) SelectedItem = i;
                    }
                }
                Console.WriteLine("Chosen: " + tFiles[SelectedItem]);
                tBWInput.FileName = tFiles[SelectedItem];
            }
        }

        public string ReadCompleteFileToString_OldVersion(string InFileName)
        {
            try
            {
                string CompleteFileText;
                CompleteFileText = ""; //Start over every time
                if (File.Exists(InFileName))
                {
                    CompleteFileText = System.IO.File.ReadAllText(InFileName);
                }
                CompleteFileText = CompleteFileText + "\n";
                ErrorDuringFileRead = false;
                return CompleteFileText;

            }
            catch (System.IO.IOException)
            {
                Random RND = new Random();
                int t = RND.Next(0, 9999);
                Console.WriteLine("AAAAARRRRGGGGG: " + t.ToString());
                ErrorDuringFileRead = true;
                return "";

            }
        }

        public string ReadCompleteFileToString(string InFileName)
        {
            try
            {
                string CompleteFileText;
                CompleteFileText = ""; //Start over every time
                if (File.Exists(InFileName))
                {
                    var fs = new FileStream(InFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //var fs = new FileStream(InFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using (var sr = new StreamReader(fs))
                    {
                        CompleteFileText = sr.ReadToEnd();
                        sr.Close();
                    }
                }
                CompleteFileText = CompleteFileText + "\n";
                ErrorDuringFileRead = false;
                return CompleteFileText;
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("System.IO.IOException");
                ErrorDuringFileRead = true;
                return "";
            }
        }

        public double ConvertToDouble(string InValueString)
        {
            double tDbl;
            InValueString = InValueString.Replace("\"", "");
            try { tDbl = Convert.ToDouble(InValueString); }
            catch (FormatException) { tDbl = double.MinValue; }
            catch (OverflowException) { tDbl = double.MinValue; }
            return tDbl;
        }

        private void PrintBWInputResults(string tStr, clsFileInfo tBWInput)
        {
            Console.WriteLine("-" + tStr + "-");

            Console.Write("Corr:\t");
            for (int i = 0; i < tBWInput.strNominal.Count; i++)
                Console.Write(tBWInput.Corrects[i].ToString() + "\t");
            Console.Write("\n");

            Console.Write("Wrng:\t");
            for (int i = 0; i < tBWInput.strNominal.Count; i++)
                Console.Write(tBWInput.Wrongs[i].ToString() + "\t");
            Console.Write("\n");

            Console.Write("PCor:\t");
            for (int i = 0; i < tBWInput.strNominal.Count; i++)
                Console.Write(tBWInput.PrevCorrects[i].ToString() + "\t");
            Console.Write("\n");

            Console.Write("PWrn:\t");
            for (int i = 0; i < tBWInput.strNominal.Count; i++)
                Console.Write(tBWInput.PrevWrongs[i].ToString() + "\t");
            Console.Write("\n");
        }

        private bool InvestigateFile(ref clsFileInfo tBWInput)
        {
            int NewStartLinePos = 0;
            int NewEndLinePos = 0;
            bool IsEndOfString = false;
            string LineString;

            string teststuff = "AAA";

            try
            {
                teststuff = "A0";
                //Read entire file into string
                string CompleteFileText = ReadCompleteFileToString(tBWInput.FileName);
                teststuff = "A1";
                //Go through the entire file
                if (CompleteFileText != "")
                {
                    teststuff = "A2";
                    for (int i = 0; i < BWInput1.strNominal.Count; i++)
                    {   BWInput1.Corrects[i] = 0; BWInput1.Wrongs[i] = 0;
                    }
                    teststuff = "A3";
                    //Lines - read until first line
                    int LineCnt = 0;
                    for(int i = 0; i < tBWInput.FirstLine; i++)
                    {
                        LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                        LineCnt++;
                        NewStartLinePos = NewEndLinePos;
                        if (IsEndOfString == true) return false;
                        teststuff = "A4";
                    }
                    teststuff = "A5";
                    //Go through all lines
                    LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                    NewStartLinePos = NewEndLinePos;
                    teststuff = "A6";
                    while (IsEndOfString == false)
                    {
                        teststuff = "A6";
                        bool bCorrects = false;
                        bool bWrongs = false;
                        //Interpret line
                        LineString = LineString.Trim();
                        List<int> StartPos = new List<int>();
                        //Find all words in line
                        string[] tWords = LineParser(LineString, tBWInput.DividerL0, tBWInput.DividerL1, tBWInput.ConSeqDiv, ref StartPos);
                        teststuff = "A7";
                        //Go through all the words found
                        if (tWords == null) return true;
                        for (int i = 0; i < tWords.Length; i++)
                        {
                            teststuff = "A8";
                            bCorrects = false;
                            bWrongs = false;
                            string tInfoType = tBWInput.InfoType[i].Trim(); tInfoType = tInfoType.ToLower();
                            if (tInfoType == "number")
                            {
                                teststuff = "A9";
                                tWords[i] = tWords[i].Trim();
                                double tDbl = ConvertToDouble(tWords[i]);
                                teststuff = "A10";
                                //Compare results
                                if (tDbl > tBWInput.Min[i] && tDbl < tBWInput.Max[i])
                                {   bCorrects = true; bWrongs = false;
                                }
                                else
                                {   bCorrects = false; bWrongs = true;
                                }
                                teststuff = "A11";
                            }
                            if (tInfoType == "text")
                            {
                                tWords[i] = tWords[i].Trim();
                                teststuff = "A12";

                                //Compare results
                                if (tWords[i] == tBWInput.strNominal[i])
                                {   bCorrects = true; bWrongs = false;
                                }
                                else
                                {   bCorrects = false; bWrongs = true;
                                }
                                teststuff = "A13";
                            }
                            //if (tInfoType == "ignore") { }
                            //Log results
                            //  Log to vars
                            if (bCorrects == true) tBWInput.Corrects[i]++;
                            if (bWrongs == true) tBWInput.Wrongs[i]++;
                        }
#if (false)
                        PrintBWInputResults(LineString);
#endif
                        teststuff = "A14";
                        //Read next line
                        LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                        teststuff = "A15";
                        NewStartLinePos = NewEndLinePos;
                        if (LineString.Trim() == "") IsEndOfString = true;
                    }
                }
                return true;
            }
            catch(System.IO.IOException)
            {
                Console.WriteLine("The things are not the way they are supposed to be: " + teststuff);
                return false;
            }
            return true;
        }

        private void UpdateGUI(object sender, ProgressChangedEventArgs e)
        {
            //Make the screen red if error
            for (int i = 0; i < BWInput1.strNominal.Count; i++)
            {
                if (BWInput1.Wrongs[i] > BWInput1.PrevWrongs[i])
                {
                    BWInput1.ErrorDisplayDelayCnt = BWInput1.ErrorDisplayDelay;
                }
                //Reset PrevValues
                BWInput1.PrevCorrects[i] = BWInput1.Corrects[i];
                BWInput1.PrevWrongs[i] = BWInput1.Wrongs[i];
            }

            if (BWInput1.ErrorDisplayDelayCnt > 0)
            {
                BWInput1.ErrorDisplayDelayCnt--;
                tblTest.Background = Brushes.Red;
                //tblTest.Background = Brushes.White;
            }
            else 
                tblTest.Background = Brushes.White;

            string tStr = DateTime.Now.ToString() + "\n" + "Correct/Wrong:\n";
            if (ErrorDuringFileRead == true)
                tStr += "IO-Exception added.";
            else
            {
                for(int i = 0; i < BWInput1.strNominal.Count; i++)
                {
                    if(i < BWInput1.strNominal.Count - 1)
                        tStr += BWInput1.Corrects[i].ToString() + "/" + BWInput1.Wrongs[i].ToString() + " | ";
                    else
                        tStr += BWInput1.Corrects[i].ToString() + "/" + BWInput1.Wrongs[i].ToString();
                }
            }
            tblTest.Text = tStr;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            var Rnd = new Random();

            //clsFileInfo data = (clsFileInfo)stateInfo;
            clsFileInfo data = new clsFileInfo();
            DateTime LastFileDate = new DateTime();
            LastFileDate = DateTime.MinValue;

            while (true)
            {
                // Get the creation time of a well-known directory.
                DateTime tnow = DateTime.Now;
                //DateTime dt = File.GetLastWriteTime(data.FilePattern);

                //Check if its a new date
                // Less than zero  t1 is earlier than t2.
                // Zero t1 is the same as t2.
                // Greater than zero   t1 is later than t2.
                if(CmpFilenameDate(ref LastFileDate, BWInput1.FileName) > 0)
                {
                    Console.WriteLine("Updated: {0} The last write time for this file was {1}.", tnow, LastFileDate);
                    InvestigateFile(ref BWInput1);
                }

                //Make sure that the BWorker stops
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                worker.ReportProgress(Rnd.Next());

                Thread.Sleep(BWInput1.UpdateFreqMS);
            }
#if (false)
            //while (chkStop.IsChecked == false)
            while (true)
            {
                Console.WriteLine("2");
                worker.ReportProgress(Rnd.Next());
                //txtOutTest.Text = Rnd.Next().ToString();

                //Make sure that the BWorker stops
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                Thread.Sleep(500);
            }
#endif
        }

        /// <summary>
        ///   Check if its a new date
        ///   Less than zero  t1 is earlier than t2.
        ///   Zero t1 is the same as t2.
        ///   Greater than zero   t1 is later than t2.
        /// </summary>
        /// <param name="LastFileDate"></param>
        /// <param name="tFilename"></param>
        /// <returns></returns>
        int CmpFilenameDate(ref DateTime LastFileDate, string tFilename)
        {
            //DateTime tnow = DateTime.Now;
            DateTime dt = File.GetLastWriteTime(tFilename);

            //Check if its a new date
            // Less than zero  t1 is earlier than t2.
            // Zero t1 is the same as t2.
            // Greater than zero   t1 is later than t2.
            int tInt = DateTime.Compare(dt, LastFileDate);
            if (tInt > 0) LastFileDate = dt;
            return tInt;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EMCMainForm.Title = "EMCUpdate - Background Task Stopped";
        }

        private void chkStop_Click(object sender, RoutedEventArgs e)
        {
            //if(chkStop.IsChecked == true)
            {
                if (bw.IsBusy == true)
                {
                    // Cancel the asynchronous operation.
                    bw.CancelAsync();
                    btnStartMonitoring.IsEnabled = true;
                }
            }
        }

        public void ResizeArray_NotUsed(ref int[,] Arr, int rows, int cols)
        {
            int[,] _arr = new int[rows, cols];
            int minRows = Math.Min(rows, Arr.GetLength(0));
            int minCols = Math.Min(cols, Arr.GetLength(1));
            //Copy data to new array
            for (int trow = 0; trow < minRows; trow++)
                for (int tcol = 0; tcol < minCols; tcol++)
                    _arr[trow, tcol] = Arr[trow, tcol];
            //Copy new to old array
            Arr = _arr;
        }

        public string[] LineParser(string csvText, char L1Char, char L2Char, bool ConSeqChar, ref List<int> StartPos)
        {
            bool DoPrintOut = false;
            List<string> tokens = new List<string>();
            //List<int> StartPos = new List<int>();

            int last = -1;
            int current = 0;
            bool inText = false;
            //SepChar = L1Char;

            //char[] arrText = csvText.ToCharArray(); //csvText.Length
            int StrLen = csvText.Length;

            if (DoPrintOut == true)
            {
                Console.WriteLine("Pos         :          1         2         3         4         5         ");
                Console.WriteLine("Pos         :012345678901234567890123456789012345678901234567890123456789");
                Console.WriteLine("arrLevelType|" + csvText + "|" + ConSeqChar.ToString());
            }

            //If line is empty - make like a tree
            if (csvText.Trim() == "") return null;

            //If ConSeq - remove space in both ends
            if (ConSeqChar == true)
            {
                if(csvText[0] == L1Char) tokens.Add("");
                csvText = csvText.Trim(L1Char);
            }
            while (current < csvText.Length)
            {
                char tCh = csvText[current];
                if (tCh == L2Char) //If text L2 starts
                { inText = !inText; }
                else
                if (tCh == L1Char) //If text L1 starts
                {
                    if (ConSeqChar == false)
                    {
                        if (!inText)
                        {   //Remove L1Char from each end
                            tokens.Add(csvText.Substring(last + 1, (current - last)).Trim(L1Char));
                            StartPos.Add(last + 1); //Add the start of the section
                            last = current;
                        }
                    }
                    else
                    {
                        if (!inText)
                        {   //Avoid exceptions
                            if (current > 0 && current < StrLen - 1) //if (current > 0 && current < StrLen - 2)
                            {
                                string tStr = csvText.Substring(current, 2);
                                string tCmpStr = L1Char.ToString() + L1Char.ToString();
                                if (tStr != tCmpStr)
                                {   //Remove L1Char from each end
                                    tokens.Add(csvText.Substring(last + 1, (current - last)).Trim(L1Char));
                                    StartPos.Add(last + 1); //Add the start of the section
                                    last = current;
                                }
                            }
                        }
                    }
                }
                current++;
            }

            //Add another string if the last is a L1Char
            if ((csvText.Substring(csvText.Length - 1, 1)[0] == L1Char) && ConSeqChar == false)
            {
                tokens.Add("");
                StartPos.Add(csvText.Length - 1); //Add the start of the section
            }

            if (last != csvText.Length - 1)
            {
                tokens.Add(csvText.Substring(last + 1).Trim(L1Char));
                StartPos.Add(last + 1); //Add the start of the section
            }

            if (DoPrintOut == true)
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    string tt1 = i.ToString("D2") + ":|" + tokens[i] + "|";
                    Console.WriteLine(tt1);
                }
            }
            return tokens.ToArray();
        }

        void SetFormatValues()
        {
            //thrMonitorFiles
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            int i = 34; char c;
            c = Convert.ToChar(34); Console.WriteLine(c.ToString());
            c = Convert.ToChar(9); Console.WriteLine(c.ToString());
            c = Convert.ToChar(45); Console.WriteLine(c.ToString());
            c = Convert.ToChar(65); Console.WriteLine(c.ToString());
            Console.WriteLine("asdfasdfasfasfasdfas");

            double aa = 65;
            char s = Convert.ToChar((int)aa); Console.WriteLine(s.ToString());


            clsFileInfo ddd = new clsFileInfo();

            ddd.ReadIniFile(@"j:\Docs\VSPrj\EMCUpdate\EMCUpdate\bin\Debug\Values.ini", 1);

            return;
            ReadIniFile("");
            FindFile(ref BWInput1);

            return;
            string t1, t2;
            t1 = "111.txt"; t2 = "*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
            t1 = "abc48235.txt"; t2 = "abc*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
            t1 = "abc48235.txt"; t2 = "abc*.*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
            t1 = "789abc456.txt"; t2 = "*abc*.*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
            t1 = "789abc456.txt"; t2 = "*abc*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());

            t1 = "789abc456.txt"; t2 = "*abc7*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
            t1 = "789abc456.txt"; t2 = "abc*"; Console.WriteLine(t1 + "/" + t2 + ": " + FilenameMatchesPattern(t1, t2).ToString());
        }

        string RetLine(string InText, int StartIndex, ref int NewStartIndex, ref bool IsEndOfString)
        {
            int StIndex = StartIndex;
            int EndLastRNIndex = 0;
            int EndBefRNIndex = 0;

            if (InText.Length <= 0) return "";

            int EndIndexR = InText.IndexOf("\r", StIndex);
            int EndIndexN = InText.IndexOf("\n", StIndex);

            //-----------------------------------------------------------------
            if (EndIndexR == -1 && EndIndexN == -1) //Both \r & \n are not found
            {
                EndLastRNIndex = InText.Length - 1;
                EndBefRNIndex = InText.Length - 1;
                IsEndOfString = true;
            }
            //-----------------------------------------------------------------
            else if (EndIndexR != -1 && EndIndexN != -1) //Both \r & \n are found
            {
                if (EndIndexR == EndIndexN + 1) //\r is found before \n
                {
                    EndLastRNIndex = EndIndexR;
                    EndBefRNIndex = EndIndexN - 1;
                }
                else if (EndIndexR + 1 == EndIndexN) //\n is found before \r
                {
                    EndLastRNIndex = EndIndexN;
                    EndBefRNIndex = EndIndexR - 1;
                }
                IsEndOfString = false;
            }
            //-----------------------------------------------------------------
            else if (EndIndexR == -1 || EndIndexN == -1) //\r or \n is found
            {
                if (EndIndexR > 0 && EndIndexN == -1) //\r found but not \n
                {
                    EndLastRNIndex = EndIndexR;
                    EndBefRNIndex = EndIndexR - 1;
                }
                else if (EndIndexN > 0 && EndIndexR == -1) //\r found but not \n
                {
                    EndLastRNIndex = EndIndexN;
                    EndBefRNIndex = EndIndexN - 1;
                }
                IsEndOfString = false;
            }
            NewStartIndex = EndLastRNIndex + 1;
            string LineString = InText.Substring(StartIndex, (EndBefRNIndex - StartIndex + 1));
            LineString = LineString.TrimEnd();
            return LineString;
        }

        private void btnStartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            if(bw.IsBusy == false)
            {
                // Create an object and pass it to ThreadPool worker thread
                //clsFileInfo p = FillFileInfo(@"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug\MyTest.txt");
                //ThreadPool.QueueUserWorkItem(thrMonitorFile, p);
                chkStop.IsChecked = false;
                btnStartMonitoring.IsEnabled = false;

                if (BWInput1 == null) BWInput1 = new clsFileInfo();
                else BWInput1.ResetAll();
                //string IniFilePath = @"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug\Values.ini";
                string IniFilePath = @"j:\Docs\VSPrj\EMCUpdate\EMCUpdate\bin\Debug\Values.ini";
                if (!File.Exists(IniFilePath)) return; //Leave if inifile does not exists
                BWInput1.ReadIniFile(IniFilePath, 1);

                FindFile(ref BWInput1);
                BWInput1.ResetCompareResults();
                InvestigateFile(ref BWInput1);

                //if(bw.IsBusy == false)
                bw.RunWorkerAsync();
                EMCMainForm.Title = "EMCUpdate - Monitoring in Process";
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (IsMonitoring == true) thrMonitorFiles.Abort();
            //Kill the bitch - what sort of a geezer
            Environment.Exit(0);
        }

        private void btnUpdateFile_Click(object sender, RoutedEventArgs e)
        {
            string path = BWInput1.FileName;
            // This text is added only once to the file.
            if (File.Exists(path))
            {
                Random rd = new Random();

                int rand_num = rd.Next(100, 200);
                using (StreamWriter sw = File.AppendText(path))
                {
                    //2021-05-28 00:20:01		0.20		01000		Rigtigt
                    int t1 = rd.Next(18, 24); double tDbl = (double)t1; tDbl /= 100;
                    int t2 = rd.Next(798, 1205);
                    int t3 = rd.Next(1, 10);

                    string tt3;
                    if (t3 < 9)
                    {   tt3 = "Rigtigt";
                    }
                    else
                    {   tt3 = "Forkert";
                    }
                    string tOut = DateTime.Now.ToString() + "\t" + tDbl.ToString("F") + "\t" + t2.ToString() + "\t" + tt3;
                    sw.WriteLine(tOut);
                    Console.WriteLine(tOut + "\t" + t3.ToString());
                }
            }
        }
    }

    // Create a Person class  
    public class clsFileInfo
    {
        //Constants for ini-file
        //public const 
        public const string cstFileHdr = "File";            //[Filex]
        public const string cstFilePattern = "FilePattern"; //FilePattern
        public const string cstFilePath = "FilePath";       //FilePath
        public const string cstDivider = "Divider";         //Divider
        public const string cstConSeqDiv = "ConSeqDiv";     //ConSeqDiv
        public const string cstFirstLine = "FirstLine";     //FirstLine

        public const string cstNominal = "Nominal";         //Nominal00
        public const string cstMin = "Min";                 //Min00
        public const string cstMax = "Max";                 //Max00
        public const string cstInfoType = "InfoType";       //InfoType00

        //Variables concerning the format of the line - read from the ini-file
        public char DividerL0; //DividerL0=9  //Tab
        public char DividerL1; //DividerL1=34 //"
        public bool ConSeqDiv; //ConSeqDiv=true
        public int FirstLine;  //FirstLine=1
        public string FilePattern;
        public string FileName;
        public string FilePath;
        public int UpdateFreqMS;
        public int ErrorDisplayDelay;
        public int ErrorDisplayDelayCnt;

        //Variables concerning values read - read from the ini-file
        //public List<double> dblNominal; //Nominal00=10
        public List<string> strNominal;
        public List<double> Min; //Min00=8
        public List<double> Max; //Max00=13
        public List<string> InfoType; //InfoType00=Ignore

        //Results from file reading
        public List<int> Corrects;
        public List<int> Wrongs;
        public List<int> PrevCorrects;
        public List<int> PrevWrongs;

        public clsFileInfo()
        {
            //Variables concerning the format of the line - read from the ini-file
            DividerL0 = '\t'; //DividerL0=9  //Tab
            DividerL1 = '\"'; //DividerL1=34 //"
            ConSeqDiv = true; //ConSeqDiv=true
            FirstLine = 1;    //FirstLine=1
            FilePattern = "*.txt";
            FileName = "MyTest.txt";
            FilePath = "";
            UpdateFreqMS = 200;
            ErrorDisplayDelay = (1000 / UpdateFreqMS) * 2;
            ErrorDisplayDelayCnt = 0;

            //Variables concerning values read - read from the ini-file
            strNominal = new List<string>();
            Min = new List<double>(); //Min00=8
            Max = new List<double>(); //Max00=13
            InfoType = new List<string>(); //InfoType00=Ignore

            //Results from file reading
            Corrects = new List<int>();
            Wrongs = new List<int>();
            PrevCorrects = new List<int>();
            PrevWrongs = new List<int>();
        }

        public void ResetAll()
        {
            //Variables concerning the format of the line - read from the ini-file
            DividerL0 = '\t'; //DividerL0=9  //Tab
            DividerL1 = '\"'; //DividerL1=34 //"
            ConSeqDiv = true; //ConSeqDiv=true
            FirstLine = 1;    //FirstLine=1
            FilePattern = "*.txt";
            FileName = "MyTest.txt";
            FilePath = "";
            UpdateFreqMS = 200;
            ErrorDisplayDelay = (1000 / UpdateFreqMS) * 2;
            ErrorDisplayDelayCnt = 0;

            //Variables concerning values read - read from the ini-file
            strNominal.Clear(); Min.Clear(); Max.Clear(); InfoType.Clear();

            //Results from file reading
            Corrects.Clear(); Wrongs.Clear(); PrevCorrects.Clear(); PrevWrongs.Clear();
        }

        public void AddNumberValues(double tMin, double tMax, string tStrNom, string tInfoType)
        {
            Min.Add(tMin);
            Max.Add(tMax);
            strNominal.Add(tStrNom);
            InfoType.Add(tInfoType);
        }

        public void SetFormatValues(char tDividerL0, char tDividerL1, bool tConSeqDiv, int tFirstLine)
        {
            DividerL0 = tDividerL0;
            DividerL1 = tDividerL1;
            ConSeqDiv = tConSeqDiv;
            FirstLine = tFirstLine;
        }

        public bool SetFilenameUpdate(string tFileName, string tFilePattern, int tUpdateFreqMS)
        {
            //if(File.Exists(tFilePattern) == true)
            tFileName = tFileName.Trim();
            tFilePattern = tFilePattern.Trim();
            if (tFilePattern != "")
            {
                FileName = tFileName;
                FilePattern = tFilePattern;
                UpdateFreqMS = tUpdateFreqMS;
                ErrorDisplayDelay = (1000 / UpdateFreqMS) * 2;
                ErrorDisplayDelayCnt = 0;
                return true;
            }
            return false;
        }

        public void ResetCompareResults()
        {
            //Expand Corrects, Wrongs, PrevCorrects & PrevWrongs
            Corrects.Clear(); Wrongs.Clear();
            PrevCorrects.Clear(); PrevWrongs.Clear();
            if (Corrects.Count < strNominal.Count)
            {
                for (int j = Corrects.Count; j < strNominal.Count; j++)
                {
                    Corrects.Add(0); Corrects[j] = 0;
                    Wrongs.Add(0); Wrongs[j] = 0;
                    PrevCorrects.Add(0); PrevCorrects[j] = 0;
                    PrevWrongs.Add(0); PrevWrongs[j] = 0;
                }
            }
        }

        public double ConvertToDouble(string InValueString)
        {
            double tDbl;
            InValueString = InValueString.Replace("\"", "");
            try { tDbl = Convert.ToDouble(InValueString); }
            catch (FormatException) { tDbl = double.MinValue; }
            catch (OverflowException) { tDbl = double.MinValue; }
            return tDbl;
        }

        /// <summary>
        ///   Reading section about:
        ///     F00_HdrText00="Net"
        ///     F00_OutText00="Net"
        ///     F00_InfoType00=Text
        /// </summary>
        /// <param name="tstrConfigIniPath"></param>
        /// <param name="tFileIndex"></param>
        /// <param name="tiIndex"></param>
        /// <returns></returns>
        public bool ReadIniFile(string tstrConfigIniPath, int tFileIndex)
        {
            //Retrieve replace strings
            bool bolFormatExists = false;
            string strFile = cstFileHdr + tFileIndex.ToString("D2");
            if (File.Exists(tstrConfigIniPath) == true)
            {
                IniFile tMyIniRead = new IniFile(tstrConfigIniPath); //IniFile

                bolFormatExists = tMyIniRead.KeyExists(cstFilePattern, strFile);
                if (bolFormatExists == true)
                {
                    //FilePattern and FilePath
                    FilePattern = tMyIniRead.Read(cstFilePattern, strFile);
                    FilePattern = FilePattern.Trim();
                    FilePath = tMyIniRead.Read(cstFilePath, strFile);
                    FilePath = FilePath.Trim();

                    //Dividers
                    string tStr = tMyIniRead.Read(cstDivider + "L0", strFile);
                    double tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue) DividerL0 = Convert.ToChar((int)tDbl);
                    else return false;

                    tStr = tMyIniRead.Read(cstDivider + "L1", strFile);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue) DividerL1 = Convert.ToChar((int)tDbl);
                    else return false;

                    //ConSeqDiv
                    tStr = tMyIniRead.Read(cstConSeqDiv, strFile);
                    if (tStr.ToLower() == "true") ConSeqDiv = true;
                    else if (tStr.ToLower() == "false") ConSeqDiv = false;
                    else return false;

                    //Firstline
                    tStr = tMyIniRead.Read(cstFirstLine, strFile);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue) FirstLine = Convert.ToChar((int)tDbl);
                    else return false;

                    //Numbered entries
                    bool MoreEntries = true;
                    int k = 0;
                    MoreEntries = tMyIniRead.KeyExists(cstInfoType + k.ToString("D2"), strFile);
                    while(MoreEntries == true)
                    {
                        tStr = tMyIniRead.Read(cstInfoType + k.ToString("D2"), strFile);
                        InfoType.Add(tStr.Trim());
                        if (tStr.ToLower() == "number")
                        {
                            //Min
                            tStr = tMyIniRead.Read(cstMin + k.ToString("D2"), strFile);
                            tDbl = ConvertToDouble(tStr.Trim());
                            if (tDbl > double.MinValue) Min.Add(tDbl);
                            else return false;
                            //Max
                            tStr = tMyIniRead.Read(cstMax + k.ToString("D2"), strFile);
                            tDbl = ConvertToDouble(tStr.Trim());
                            if (tDbl > double.MinValue) Max.Add(tDbl);
                            else return false;
                            //Expand all none used
                            strNominal.Add("");
                        }
                        if (tStr.ToLower() == "text")
                        {
                            //Nominal
                            tStr = tMyIniRead.Read(cstNominal + k.ToString("D2"), strFile);
                            tStr = tStr.Replace("\"", "");
                            strNominal.Add(tStr.Trim());
                            //Expand all none used
                            Min.Add(0);
                            Max.Add(0);
                        }
                        if (tStr.ToLower() == "ignore")
                        {
                            strNominal.Add("");
                            Min.Add(0);
                            Max.Add(0);
                        }
                        k++;
                        MoreEntries = tMyIniRead.KeyExists(cstInfoType + k.ToString("D2"), strFile);
                    }
                }
            }
            return bolFormatExists;
        }
    }
}
