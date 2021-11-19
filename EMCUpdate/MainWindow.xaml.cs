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
        clsIniFileTopLevel AllFiles;

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

            //--------------------------------------------------------
            AllFiles = new clsIniFileTopLevel();
        }

        private void AddToComboBoxDividerChars(System.Windows.Controls.ComboBox tComboBox)
        {
            tComboBox.Items.Add("NULL (#000-Null char.)");
            tComboBox.Items.Add("SOH  (#001-Start of Header)");
            tComboBox.Items.Add("STX  (#002-Start of Text)");
            tComboBox.Items.Add("ETX  (#003-End of Text, hearts card suit)");
            tComboBox.Items.Add("EOT  (#004-End of Transmission, diamonds card suit)");
            tComboBox.Items.Add("ENQ  (#005-Enquiry, clubs card suit)");
            tComboBox.Items.Add("ACK  (#006-Acknowledgement, spade card suit)");
            tComboBox.Items.Add("BEL  (#007-Bell)");
            tComboBox.Items.Add("BS   (#008-BSpace)");
            tComboBox.Items.Add("HT   (#009-Hor. Tab)");
            tComboBox.Items.Add("LF   (#010-Line feed)");
            tComboBox.Items.Add("VT   (#011-Vert. Tab)");
            tComboBox.Items.Add("FF   (#012-Form feed)");
            tComboBox.Items.Add("CR   (#013-Carriage return)");
            tComboBox.Items.Add("SO   (#014-Shift Out)");
            tComboBox.Items.Add("SI   (#015-Shift In)");
            tComboBox.Items.Add("DLE  (#016-Data link escape)");
            tComboBox.Items.Add("DC1  (#017-Device control 1)");
            tComboBox.Items.Add("DC2  (#018-Device control 2)");
            tComboBox.Items.Add("DC3  (#019-Device control 3)");
            tComboBox.Items.Add("DC4  (#020-Device control 4)");
            tComboBox.Items.Add("NAK  (#021-NAK Neg.-Ack.)");
            tComboBox.Items.Add("SYN  (#022-Sync. idle))");
            tComboBox.Items.Add("ETB  (#023-End of trans. block)");
            tComboBox.Items.Add("CAN  (#024-Cancel)");
            tComboBox.Items.Add("EM   (#025-End of medium)");
            tComboBox.Items.Add("SUB  (#026-Substitute)");
            tComboBox.Items.Add("ESC  (#027-Escape)");
            tComboBox.Items.Add("FS   (#028-File separator)");
            tComboBox.Items.Add("GS   (#029-Group separator)");
            tComboBox.Items.Add("RS   (#030-Record separator)");
            tComboBox.Items.Add("US   (#031-Unit separator)");
            tComboBox.Items.Add("Spc  (#032-Space)");
            tComboBox.Items.Add("!    (#033-Exclamation mark)");
            tComboBox.Items.Add("\"    (#034-Double quotes)");
            tComboBox.Items.Add("#    (#035-Number sign)");
            tComboBox.Items.Add("$    (#036-Dollar sign)");
            tComboBox.Items.Add("%    (#037-Percent sign)");
            tComboBox.Items.Add("&    (#038-Ampersand)");
            tComboBox.Items.Add("'    (#039-Sngl quote)");
            tComboBox.Items.Add("(    (#040-Open parentheses)");
            tComboBox.Items.Add(")    (#041-Closing parentheses)");
            tComboBox.Items.Add("*    (#042-Asterisk)");
            tComboBox.Items.Add("+    (#043-Plus sign)");
            tComboBox.Items.Add(",    (#044-Comma)");
            tComboBox.Items.Add("-    (#045-Minus sign)");
            tComboBox.Items.Add(".    (#046-Dot, full stop)");
            tComboBox.Items.Add("/    (#047-Forward slash)");
            tComboBox.Items.Add("0    (#048-Number zero)");
            tComboBox.Items.Add("1    (#049-Number one)");
            tComboBox.Items.Add("2    (#050-Number two)");
            tComboBox.Items.Add("3    (#051-Number three)");
            tComboBox.Items.Add("4    (#052-Number four)");
            tComboBox.Items.Add("5    (#053-Number five)");
            tComboBox.Items.Add("6    (#054-Number six)");
            tComboBox.Items.Add("7    (#055-Number seven)");
            tComboBox.Items.Add("8    (#056-Number eight)");
            tComboBox.Items.Add("9    (#057-Number nine)");
            tComboBox.Items.Add(":    (#058-Colon)");
            tComboBox.Items.Add(";    (#059-Semicolon)");
            tComboBox.Items.Add("<    (#060-Less-than)");
            tComboBox.Items.Add("=    (#061-Equals sign)");
            tComboBox.Items.Add(">    (#062-Greater-than)");
            tComboBox.Items.Add("?    (#063-Question mark)");
            tComboBox.Items.Add("@    (#064-At sign)");
            tComboBox.Items.Add("A    (#065-UCase A)");
            tComboBox.Items.Add("B    (#066-UCase B)");
            tComboBox.Items.Add("C    (#067-UCase C)");
            tComboBox.Items.Add("D    (#068-UCase D)");
            tComboBox.Items.Add("E    (#069-UCase E)");
            tComboBox.Items.Add("F    (#070-UCase F)");
            tComboBox.Items.Add("G    (#071-UCase G)");
            tComboBox.Items.Add("H    (#072-UCase H)");
            tComboBox.Items.Add("I    (#073-UCase I)");
            tComboBox.Items.Add("J    (#074-UCase J)");
            tComboBox.Items.Add("K    (#075-UCase K)");
            tComboBox.Items.Add("L    (#076-UCase L)");
            tComboBox.Items.Add("M    (#077-UCase M)");
            tComboBox.Items.Add("N    (#078-UCase N)");
            tComboBox.Items.Add("O    (#079-UCase O)");
            tComboBox.Items.Add("P    (#080-UCase P)");
            tComboBox.Items.Add("Q    (#081-UCase Q)");
            tComboBox.Items.Add("R    (#082-UCase R)");
            tComboBox.Items.Add("S    (#083-UCase S)");
            tComboBox.Items.Add("T    (#084-UCase T)");
            tComboBox.Items.Add("U    (#085-UCase U)");
            tComboBox.Items.Add("V    (#086-UCase V)");
            tComboBox.Items.Add("W    (#087-UCase W)");
            tComboBox.Items.Add("X    (#088-UCase X)");
            tComboBox.Items.Add("Y    (#089-UCase Y)");
            tComboBox.Items.Add("Z    (#090-UCase Z)");
            tComboBox.Items.Add("[    (#091-Opening bracket)");
            tComboBox.Items.Add("\\    (#092-Reverse slash)");
            tComboBox.Items.Add("]    (#093-Closing bracket)");
            tComboBox.Items.Add("^    (#094-Accent)");
            tComboBox.Items.Add("_    (#095-Underscore)");
            tComboBox.Items.Add("`    (#096-Left accent)");
            tComboBox.Items.Add("a    (#097-LCase a)");
            tComboBox.Items.Add("b    (#098-LCase b)");
            tComboBox.Items.Add("c    (#099-LCase c)");
            tComboBox.Items.Add("d    (#100-LCase d)");
            tComboBox.Items.Add("e    (#101-LCase e)");
            tComboBox.Items.Add("f    (#102-LCase f)");
            tComboBox.Items.Add("g    (#103-LCase g)");
            tComboBox.Items.Add("h    (#104-LCase h)");
            tComboBox.Items.Add("i    (#105-LCase i)");
            tComboBox.Items.Add("j    (#106-LCase j)");
            tComboBox.Items.Add("k    (#107-LCase k)");
            tComboBox.Items.Add("l    (#108-LCase l)");
            tComboBox.Items.Add("m    (#109-LCase m)");
            tComboBox.Items.Add("n    (#110-LCase n)");
            tComboBox.Items.Add("o    (#111-LCase o)");
            tComboBox.Items.Add("p    (#112-LCase p)");
            tComboBox.Items.Add("q    (#113-LCase q)");
            tComboBox.Items.Add("r    (#114-LCase r)");
            tComboBox.Items.Add("s    (#115-LCase s)");
            tComboBox.Items.Add("t    (#116-LCase t)");
            tComboBox.Items.Add("u    (#117-LCase u)");
            tComboBox.Items.Add("v    (#118-LCase v)");
            tComboBox.Items.Add("w    (#119-LCase w)");
            tComboBox.Items.Add("x    (#120-LCase x)");
            tComboBox.Items.Add("y    (#121-LCase y)");
            tComboBox.Items.Add("z    (#122-LCase z)");
            tComboBox.Items.Add("{    (#123-Opening braces)");
            tComboBox.Items.Add("|    (#124-VBar)");
            tComboBox.Items.Add("}    (#125-Closing braces)");
            tComboBox.Items.Add("~    (#126-Tilde)");
            tComboBox.Items.Add("DEL  (#127-Delete)");
            tComboBox.Items.Add("Ç    (#128-Majuscule C-cedilla)");
            tComboBox.Items.Add("ü    (#129-Char. u /w umlaut)");
            tComboBox.Items.Add("é    (#130-Char. e /w right accent)");
            tComboBox.Items.Add("â    (#131-Char. a /w circ. accent)");
            tComboBox.Items.Add("ä    (#132-Char. a /w umlaut)");
            tComboBox.Items.Add("à    (#133-Char. a /w left accent)");
            tComboBox.Items.Add("å    (#134-Char. a /w a ring)");
            tComboBox.Items.Add("ç    (#135-Minuscule c-cedilla)");
            tComboBox.Items.Add("ê    (#136-Char. e /w circ. accent)");
            tComboBox.Items.Add("ë    (#137-Char. e /w umlaut)");
            tComboBox.Items.Add("è    (#138-Char. e /w left accent)");
            tComboBox.Items.Add("ï    (#139-Char. i /w umlaut)");
            tComboBox.Items.Add("î    (#140-Char. i /w circ. accent)");
            tComboBox.Items.Add("ì    (#141-Char. i /w left accent)");
            tComboBox.Items.Add("Ä    (#142-Char. A /w umlaut)");
            tComboBox.Items.Add("Å    (#143-UCase A w/ a ring)");
            tComboBox.Items.Add("É    (#144-UCase E w/ right accent)");
            tComboBox.Items.Add("æ    (#145-LCase ae)");
            tComboBox.Items.Add("Æ    (#146-UCase AE)");
            tComboBox.Items.Add("ô    (#147-Char. o w/ circ. accent)");
            tComboBox.Items.Add("ö    (#148-Char. o w/ umlaut)");
            tComboBox.Items.Add("ò    (#149-Char. o w/ left accent)");
            tComboBox.Items.Add("û    (#150-Char. u w/ circ. accent)");
            tComboBox.Items.Add("ù    (#151-Char. u w/ left accent)");
            tComboBox.Items.Add("ÿ    (#152-LCase y w/ diaeresis)");
            tComboBox.Items.Add("Ö    (#153-Char. O w/ umlaut)");
            tComboBox.Items.Add("Ü    (#154-Char. U w/ umlaut)");
            tComboBox.Items.Add("ø    (#155-LCase Ø)");
            tComboBox.Items.Add("£    (#156-Pound sign)");
            tComboBox.Items.Add("Ø    (#157-UCase Ø)");
            tComboBox.Items.Add("×    (#158-Star)");
            tComboBox.Items.Add("ƒ    (#159-Function sign)");
            tComboBox.Items.Add("á    (#160-LCase a w/ accent)");
            tComboBox.Items.Add("í    (#161-LCase i w/ accent)");
            tComboBox.Items.Add("ó    (#162-LCase o w/ accent)");
            tComboBox.Items.Add("ú    (#163-LCase u w/ accent)");
            tComboBox.Items.Add("ñ    (#164-LCase n w/ tilde)");
            tComboBox.Items.Add("Ñ    (#165-UCase N w/ tilde)");
            tComboBox.Items.Add("ª    (#166-Fem. ord. indicator)");
            tComboBox.Items.Add("º    (#167-Male ord. indicator)");
            tComboBox.Items.Add("¿    (#168-Inv. ?)");
            tComboBox.Items.Add("®    (#169-Reg. trademark)");
            tComboBox.Items.Add("¬    (#170-Log. neg. symbol)");
            tComboBox.Items.Add("½    (#171-One half)");
            tComboBox.Items.Add("¼    (#172-One Quart)");
            tComboBox.Items.Add("¡    (#173-Inv. !)");
            tComboBox.Items.Add("«    (#174-Right quot. mark)");
            tComboBox.Items.Add("»    (#175-Left quot. mark)");
            tComboBox.Items.Add("░    (#176-Low density dotted)");
            tComboBox.Items.Add("▒    (#177-Med. density dotted)");
            tComboBox.Items.Add("▓    (#178-High density dotted)");
            tComboBox.Items.Add("│    (#179-Box draw. char. sngl vert. line)");
            tComboBox.Items.Add("┤    (#180-Box draw. char. sngl vert. & left line)");
            tComboBox.Items.Add("Á    (#181-UCase A w/ right accent or A-Right)");
            tComboBox.Items.Add("Â    (#182-A w/ circ. accent)");
            tComboBox.Items.Add("À    (#183-A w/ left accent)");
            tComboBox.Items.Add("©    (#184-Copyright)");
            tComboBox.Items.Add("╣    (#185-Box draw. char. dbl line ver. & left)");
            tComboBox.Items.Add("║    (#186-Box draw. char. dbl vert. line)");
            tComboBox.Items.Add("╗    (#187-Box draw. char. dbl line upper right corner)");
            tComboBox.Items.Add("╝    (#188-Box draw. char. dbl line lower right corner)");
            tComboBox.Items.Add("¢    (#189-Cent symbol)");
            tComboBox.Items.Add("¥    (#190-YEN and YUAN sign)");
            tComboBox.Items.Add("┐    (#191-Box draw. char. sngl line upper right corner)");
            tComboBox.Items.Add("└    (#192-Box draw. char. sngl line lower left corner)");
            tComboBox.Items.Add("┴    (#193-Box draw. char. sngl line hor. & up)");
            tComboBox.Items.Add("┬    (#194-Box draw. char. sngl line hor. down)");
            tComboBox.Items.Add("├    (#195-Box draw. char. sngl line vert. & right)");
            tComboBox.Items.Add("─    (#196-Box draw. char. sngl hor. line)");
            tComboBox.Items.Add("┼    (#197-Box draw. char. sngl line hor. vert.)");
            tComboBox.Items.Add("ã    (#198-LCase a w/ tilde or a-tilde)");
            tComboBox.Items.Add("Ã    (#199-UCase A w/ tilde or A-tilde)");
            tComboBox.Items.Add("╚    (#200-Box draw. char. dbl line lower left corner)");
            tComboBox.Items.Add("╔    (#201-Box draw. char. dbl line upper left corner)");
            tComboBox.Items.Add("╩    (#202-Box draw. char. dbl line hor. and up)");
            tComboBox.Items.Add("╦    (#203-Box draw. char. dbl line hor. down)");
            tComboBox.Items.Add("╠    (#204-Box draw. char. dbl line vert. and right)");
            tComboBox.Items.Add("═    (#205-Box draw. char. dbl hor. line)");
            tComboBox.Items.Add("╬    (#206-Box draw. char. dbl line hor. vert.)");
            tComboBox.Items.Add("¤    (#207-Generic currency sign)");
            tComboBox.Items.Add("ð    (#208-LCase eth)");
            tComboBox.Items.Add("Ð    (#209-UCase Eth)");
            tComboBox.Items.Add("Ê    (#210-Char. E w/ circ. accent)");
            tComboBox.Items.Add("Ë    (#211-Char. E w/ umlaut)");
            tComboBox.Items.Add("È    (#212-UCase E w/ left accent)");
            tComboBox.Items.Add("ı    (#213-Lowercase dot less i)");
            tComboBox.Items.Add("Í    (#214-UCase I w/ right accent)");
            tComboBox.Items.Add("Î    (#215-Char. I w/ circ. accent)");
            tComboBox.Items.Add("Ï    (#216-Char. I w/ umlaut)");
            tComboBox.Items.Add("┘    (#217-Box draw. char. sngl line LRight corner)");
            tComboBox.Items.Add("┌    (#218-Box draw. char. sngl line ULeft corner)");
            tComboBox.Items.Add("█    (#219-Block)");
            tComboBox.Items.Add("▄    (#220-Bottom half block)");
            tComboBox.Items.Add("¦    (#221-Vert. broken bar)");
            tComboBox.Items.Add("Ì    (#222-UCase I w/ left accent)");
            tComboBox.Items.Add("▀    (#223-Top half block)");
            tComboBox.Items.Add("Ó    (#224-UCase O w/ right accent)");
            tComboBox.Items.Add("ß    (#225-Dbl S)");
            tComboBox.Items.Add("Ô    (#226-Char. O w/ circ. accent)");
            tComboBox.Items.Add("Ò    (#227-UCase O w/ left accent)");
            tComboBox.Items.Add("õ    (#228-LCase o w/ tilde)");
            tComboBox.Items.Add("Õ    (#229-UCase O w/ tilde)");
            tComboBox.Items.Add("µ    (#230-LCase Mu)");
            tComboBox.Items.Add("þ    (#231-LCase Thorn)");
            tComboBox.Items.Add("Þ    (#232-UCase Thorn)");
            tComboBox.Items.Add("Ú    (#233-UCase U w/ right accent)");
            tComboBox.Items.Add("Û    (#234-Char. U w/ circ. accent)");
            tComboBox.Items.Add("Ù    (#235-UCase U w/ left accent)");
            tComboBox.Items.Add("ý    (#236-LCase y w/ right accent)");
            tComboBox.Items.Add("Ý    (#237-UCase Y w/ right accent)");
            tComboBox.Items.Add("¯    (#238-Macron symbol)");
            tComboBox.Items.Add("´    (#239-Right accent)");
            tComboBox.Items.Add("≡    (#240-Congruence relation symbol)");
            tComboBox.Items.Add("±    (#241-+/- sign)");
            tComboBox.Items.Add("‗    (#242-Dbl underscore)");
            tComboBox.Items.Add("¾    (#243-3/4)");
            tComboBox.Items.Add("¶    (#244-Paragraph sign)");
            tComboBox.Items.Add("§    (#245-Section sign)");
            tComboBox.Items.Add("÷    (#246-Div. sign)");
            tComboBox.Items.Add("¸    (#247-Cedilla)");
            tComboBox.Items.Add("°    (#248-Degree symbol)");
            tComboBox.Items.Add("¨    (#249-Diaresis)");
            tComboBox.Items.Add("·    (#250-Space dot)");
            tComboBox.Items.Add("¹    (#251-Exp 1)");
            tComboBox.Items.Add("³    (#252-Exp 3)");
            tComboBox.Items.Add("²    (#253-Exp 2)");
            tComboBox.Items.Add("■    (#254-black square)");
            tComboBox.Items.Add("NBSpc(#255-Non-breaking space)");
        }

        private void AddToComboBoxConSeqDiv(System.Windows.Controls.ComboBox tComboBox)
        {
            tComboBox.Items.Add("Yes (Consequtive L0 chars)");
            tComboBox.Items.Add("No (Each L0 chars)");
        }

        public bool ReadIniFile(string InFileName)
        {
            bool MoreEntries = false;
            int EntryIndex = 1;

            AllFiles.ResetAll();

            InFileName = InFileName;

            MoreEntries = AllFiles.ReadIniFile(InFileName);

            return false;

            clsIniFileEntry F1 = AllFiles.iniFileEntries[0];
            clsIniFileEntry F2 = AllFiles.iniFileEntries[1];
            //InvestigateFile(ref ccc.iniFileEntries[0]);
            InvestigateFile(ref F1);

            int eee = 0;
            eee++;

            /*
            while (MoreEntries == true)
            {
                arrIniObj.Add(tIniElement);
                tIniElement = new clsIniFileTopLevel(); tIniElement.ResetAll();
                EntryIndex++;
                MoreEntries = tIniElement.ReadIniFile(InFileName, EntryIndex);
            }
            while (MoreEntries == true) ;

            if (EntryIndex > 1) return true;
            return false;

            clsIniFileTopLevel tBWInput1 = new clsIniFileTopLevel();

            tBWInput1.SetFormatValues('\t', '\"', true, 1, 2);
            tBWInput1.SetFilenameUpdate("", "*cLLD.txt", 200);

            tBWInput1.FilePath = @"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug";

            tBWInput1.AddNumberValues(8, 13, "", "Ignore");
            tBWInput1.AddNumberValues(0.18, 0.23, "", "Number");
            tBWInput1.AddNumberValues(800, 1200, "", "Number");
            tBWInput1.AddNumberValues(0, 0, "Rigtigt", "Text");
            tBWInput1.ResetCompareResults();
            BWInput1 = tBWInput1;
            */
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

        void FindFiles(ref clsIniFileTopLevel tAllFiles)
        {
            for(int j = 0; j < tAllFiles.iniFileEntries.Count; j++)
            {
                AllFiles.iniFileEntries[j].FilePath = AllFiles.iniFileEntries[j].FilePath.Trim();
                if (Directory.Exists(AllFiles.iniFileEntries[j].FilePath) == false)
                { return; }
                else
                {
                    //Find all files with the search pattern
                    string[] tFiles = Directory.GetFiles(AllFiles.iniFileEntries[j].FilePath, AllFiles.iniFileEntries[j].FilePattern);

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
                    AllFiles.iniFileEntries[j].FileName = tFiles[SelectedItem];
                }
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
            //Check for IO Exception
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

        private void PrintBWInputResults(string tStr, clsIniFileTopLevel tBWInput)
        {
            /*
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
            */
        }

        private bool InvestigateFile(ref clsIniFileEntry tFileEntry)
        {
            int NewStartLinePos = 0;
            int NewEndLinePos = 0;
            bool IsEndOfString = false;
            string LineString;

            try
            {
                //Read entire file into string
                string CompleteFileText = ReadCompleteFileToString(tFileEntry.FileName);
                //Go through the entire file
                if (CompleteFileText != "")
                {
                    for (int i = 0; i < tFileEntry.strNominal.Count; i++)
                    {
                        tFileEntry.PrevCorrects[i] = tFileEntry.Corrects[i]; tFileEntry.PrevWrongs[i] = tFileEntry.Wrongs[i];
                        tFileEntry.Corrects[i] = 0; tFileEntry.Wrongs[i] = 0;
                    }
                    //Lines - read until first line
                    int LineCnt = 0;
                    for (int i = 0; i < tFileEntry.FirstHdrLine; i++)
                    {
                        LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                        LineCnt++;
                        NewStartLinePos = NewEndLinePos;
                        if (IsEndOfString == true) return false;
                    }
                    //Go through all lines
                    LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                    NewStartLinePos = NewEndLinePos;
                    while (IsEndOfString == false)
                    {
                        bool bCorrects = false;
                        bool bWrongs = false;
                        //Interpret line
                        LineString = LineString.Trim();
                        List<int> StartPos = new List<int>();
                        //Find all words in line
                        string[] tWords = LineParser(LineString, tFileEntry.DividerL0, tFileEntry.DividerL1, tFileEntry.ConSeqDiv, ref StartPos);
                        //Go through all the words found
                        if (tWords == null) return true;
                        for (int i = 0; i < tWords.Length; i++)
                        {
                            bCorrects = false;
                            bWrongs = false;
                            string tInfoType = tFileEntry.InfoType[i].Trim(); tInfoType = tInfoType.ToLower();
                            if (tInfoType == "number")
                            {
                                tWords[i] = tWords[i].Trim();
                                double tDbl = ConvertToDouble(tWords[i]);
                                //Compare results
                                if ((tDbl >= tFileEntry.Min[i]) && (tDbl <= tFileEntry.Max[i]))
                                {
                                    bCorrects = true; bWrongs = false;
                                }
                                else
                                {
                                    bCorrects = false; bWrongs = true;
                                }
                            }
                            if (tInfoType == "text")
                            {
                                tWords[i] = tWords[i].Trim();

                                //Compare results
                                if (tWords[i] == tFileEntry.strNominal[i])
                                {
                                    bCorrects = true; bWrongs = false;
                                }
                                else
                                {
                                    bCorrects = false; bWrongs = true;
                                }
                            }
                            //if (tInfoType == "ignore") { }
                            //Log results
                            //  Log to vars
                            if (bCorrects == true) tFileEntry.Corrects[i]++;
                            if (bWrongs == true) tFileEntry.Wrongs[i]++;
                        }
#if (false)
                        PrintBWInputResults(LineString);
#endif
                        //Read next line
                        LineString = RetLine(CompleteFileText, NewStartLinePos, ref NewEndLinePos, ref IsEndOfString);
                        NewStartLinePos = NewEndLinePos;
                        if (LineString.Trim() == "") IsEndOfString = true;
                    }
                }
                return true;
            }
            catch (System.IO.IOException)
            {
                //Console.WriteLine("The things are not the way they are supposed to be: " + teststuff);
                return false;
            }
            return true;
        }

        private void UpdateGUI(object sender, ProgressChangedEventArgs e)
        {
            //Make the screen red if error
            bool MakeScreenRed = false;

            //Goto each file
            for (int j = 0; j < AllFiles.iniFileEntries.Count; j++)
            {
                clsIniFileEntry tFileEntry = AllFiles.iniFileEntries[j];
                for (int i = 0; i < tFileEntry.strNominal.Count; i++)
                {
                    if (tFileEntry.Wrongs[i] > tFileEntry.PrevWrongs[i])
                    {
                        //BWInput1.ErrorDisplayDelayCnt = BWInput1.ErrorDisplayDelay;
                        MakeScreenRed = true;
                    }
                    //Reset PrevValues
                    tFileEntry.PrevCorrects[i] = tFileEntry.Corrects[i];
                    tFileEntry.PrevWrongs[i] = tFileEntry.Wrongs[i];
                }
            }
            if (MakeScreenRed == true)
                AllFiles.ErrorDisplayDelayCnt = AllFiles.ErrorDisplayDelay;

            if (AllFiles.ErrorDisplayDelayCnt > 0)
            {
                AllFiles.ErrorDisplayDelayCnt--;
                tblTest.Background = Brushes.Red;
                //tblTest.Background = Brushes.White;
            }
            else
                tblTest.Background = Brushes.White;

            string tStr = "";
            tStr = DateTime.Now.ToString() + "\n" + "Correct/Wrong:\n";
            for (int j = 0; j < AllFiles.iniFileEntries.Count; j++)
            {
                clsIniFileEntry tFileEntry = AllFiles.iniFileEntries[j];
                if (ErrorDuringFileRead == true)
                    tStr += "IO-Exception added.";
                else
                {
                    for (int i = 0; i < tFileEntry.strNominal.Count; i++)
                    {
                        if (i < tFileEntry.strNominal.Count - 1)
                            tStr += tFileEntry.Corrects[i].ToString() + "/" + tFileEntry.Wrongs[i].ToString() + " | ";
                        else
                            tStr += tFileEntry.Corrects[i].ToString() + "/" + tFileEntry.Wrongs[i].ToString();
                    }
                    tStr += "\n";
                }
            }
            tblTest.Text = tStr;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            var Rnd = new Random();

            //clsFileInfo data = (clsFileInfo)stateInfo;
            clsIniFileTopLevel data = new clsIniFileTopLevel();
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
                for (int j = 0; j < AllFiles.iniFileEntries.Count; j++)
                {
                    clsIniFileEntry tFileEntry = AllFiles.iniFileEntries[j];
                    //if (CmpFilenameDate(ref LastFileDate, tFileEntry.FileName) > 0)
                    {
                        Console.WriteLine("Updated: {0} The last write time for this file was {1}.", tnow, LastFileDate);
                        InvestigateFile(ref tFileEntry);
                    }
                }

                //Make sure that the BWorker stops
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                worker.ReportProgress(Rnd.Next());
                Thread.Sleep(AllFiles.UpdateFreqMS);
            }
#if (true)
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
            if(chkStop.IsChecked == true)
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
                if (csvText[0] == L1Char) tokens.Add("");
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
            ReadIniFile("Values.ini");
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
            if (bw.IsBusy == false)
            {
                // Create an object and pass it to ThreadPool worker thread
                //clsFileInfo p = FillFileInfo(@"d:\Docs\Docs\ProgWork\VS\Visual Studio 2013\Projects\EMCUpdate\EMCUpdate\bin\Debug\MyTest.txt");
                //ThreadPool.QueueUserWorkItem(thrMonitorFile, p);
                chkStop.IsChecked = false;
                btnStartMonitoring.IsEnabled = false;

                //string IniFilePath = @"j:\Docs\VSPrj\EMCUpdate\EMCUpdate\bin\Debug\Values.ini";
                string IniFilePath = @"Values.ini";
                if (!File.Exists(IniFilePath)) return; //Leave if inifile does not exists
                ReadIniFile(IniFilePath);
                FindFiles(ref AllFiles);

                for (int j = 0; j < AllFiles.iniFileEntries.Count; j++)
                {
                    clsIniFileEntry Fx = AllFiles.iniFileEntries[0];
                    Fx.ResetCompareResults();
                    InvestigateFile(ref Fx);
                }

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
            /*
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
                    {
                        tt3 = "Rigtigt";
                    }
                    else
                    {
                        tt3 = "Forkert";
                    }
                    string tOut = DateTime.Now.ToString() + "\t" + tDbl.ToString("F") + "\t" + t2.ToString() + "\t" + tt3;
                    sw.WriteLine(tOut);
                    Console.WriteLine(tOut + "\t" + t3.ToString());
                }
            }
            */
        }
    }

    // File top level
    public class clsIniFileTopLevel
    {
        //Constants for ini-file
        //public const 
        public const string cstFileHdr = "File";            //[Filex]
        public const string cstFilePattern = "FilePattern"; //FilePattern
        public const string cstFilePath = "FilePath";       //FilePath
        public const string cstDividerL0 = "DividerL0";     //Divider Level 0
        public const string cstDividerL1 = "DividerL1";     //Divider Level 1
        public const string cstConSeqDiv = "ConSeqDiv";     //ConSeqDiv
        public const string cstFirstHdrLine = "FirstHdrLine";   //FirsHdrtLine
        public const string cstFirstDataLine = "FirstDataLine"; //FirsDatatLine
        public const string cstUpdateFreqMS = "UpdateFreqMS";   //UpdateFreqMS
        public const string cstErrorDisplayDelay = "ErrorDisplayDelay"; //ErrorDisplayDelay

        public const string cstNominal = "Nominal";         //Nominal00
        public const string cstMin = "Min";                 //Min00
        public const string cstMax = "Max";                 //Max00
        public const string cstInfoType = "InfoType";       //InfoType00

        public string IniFileName; //The inifile nane

        //Variables concerning the format of the line - read from the ini-file
        public int UpdateFreqMS;
        public int ErrorDisplayDelay; //Number of timer iterations so show an error
        public int ErrorDisplayDelayCnt;

        IniFile IniFileObj;

        //Variables concerning values read - read from the ini-file
        //public List<double> dblNominal; //Nominal00=10

        //clsIniFileEntries iniFileEntries
        public List<clsIniFileEntry> iniFileEntries;

        public clsIniFileTopLevel()
        {
            //Variables concerning the format of the line - read from the ini-file
            UpdateFreqMS = 200;
            ErrorDisplayDelay = (1000 / UpdateFreqMS) * 2;
            ErrorDisplayDelayCnt = 0;

            IniFileObj = null;

            //Variables concerning values read - read from the ini-file
            iniFileEntries = new List<clsIniFileEntry>();

            /*strNominal = new List<string>();
            Min = new List<double>(); //Min00=8
            Max = new List<double>(); //Max00=13
            InfoType = new List<string>(); //InfoType00=Ignore*/

            //Results from file reading
            /*Corrects = new List<int>();
            Wrongs = new List<int>();
            PrevCorrects = new List<int>();
            PrevWrongs = new List<int>();*/
        }

        //-------------- Reset --------------
        public void ResetAll()
        {
            //Variables concerning the format of the line - read from the ini-file
            UpdateFreqMS = -1;
            ErrorDisplayDelay = -1; //(1000 / UpdateFreqMS) * 2;
            ErrorDisplayDelayCnt = 0;

            //Variables concerning values read - read from the ini-file
            for (int i = 0; i < iniFileEntries.Count; i++)
            {
                iniFileEntries[i].ResetAll();
            }
            iniFileEntries.Clear();
        }

        public void ResetCompareResults()
        {
            for (int i = 0; i < iniFileEntries.Count; i++)
            {
                iniFileEntries[i].ResetCompareResults();
            }
        }

        //-------------- Setup --------------
        public void SetFormatValues_NotUsed(int tFileIndex, char tDividerL0, char tDividerL1, bool tConSeqDiv, int tFirstHdrLine, int tFirstDataLine)
        {
            if (iniFileEntries.Count > tFileIndex)
            {
                clsIniFileEntry tiniFileEntries = new clsIniFileEntry();
                iniFileEntries.Add(tiniFileEntries);
            }
            iniFileEntries[tFileIndex].SetFormatValues(tDividerL0, tDividerL1, tConSeqDiv, tFirstHdrLine, tFirstDataLine);

        }

        public void AddNumberValues_NotUsed(int tFileIndex, double tMin, double tMax, string tStrNom, string tInfoType)
        {
            if (iniFileEntries.Count > tFileIndex)
            {
                clsIniFileEntry tiniFileEntries = new clsIniFileEntry();
                iniFileEntries.Add(tiniFileEntries);
            }
            iniFileEntries[tFileIndex].AddNumberValues(tMin, tMax, tStrNom, tInfoType);
        }

        public bool SetFilenameUpdate(int tFileIndex, string tFileName, string tFilePattern, int tUpdateFreqMS)
        {
            //if(File.Exists(tFilePattern) == true)
            tFileName = tFileName.Trim();
            tFilePattern = tFilePattern.Trim();
            if (tFilePattern != "")
            {

                if (iniFileEntries.Count > tFileIndex)
                {
                    clsIniFileEntry tiniFileEntries = new clsIniFileEntry();
                    iniFileEntries.Add(tiniFileEntries);
                }
                iniFileEntries[tFileIndex].SetFilenameUpdate(tFileName, tFilePattern);
                UpdateFreqMS = tUpdateFreqMS;
                ErrorDisplayDelay = (1000 / UpdateFreqMS) * 2;
                ErrorDisplayDelayCnt = 0;
                return true;
            }
            return false;
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
        ///     If tstrConfigIniPath is left empty, the preset is used
        /// </summary>
        /// <param name="tstrConfigIniPath"></param>
        /// <param name="tFileIndex"></param>
        /// <param name="tiIndex"></param>
        /// <returns></returns>
        public bool ReadIniFile(string tstrConfigIniPath)
        {
            //Retrieve replace strings
            bool bolFormatExists = false;
            if (tstrConfigIniPath != "") IniFileName = tstrConfigIniPath;

            if (File.Exists(IniFileName) == true)
            {
                if (IniFileObj == null)
                    IniFileObj = new IniFile(IniFileName); //IniFile

                bolFormatExists = IniFileObj.KeyExists(cstUpdateFreqMS, "Main");
                if (bolFormatExists == false) return false;

                //Main
                // UpdateFreqMS
                string tStr = IniFileObj.Read(cstUpdateFreqMS, "Main");
                double tDbl = ConvertToDouble(tStr.Trim());
                if (tDbl > double.MinValue)
                    UpdateFreqMS = Convert.ToChar((int)tDbl);
                else return false;

                // ErrorDisplayDelay
                tStr = IniFileObj.Read(cstErrorDisplayDelay, "Main");
                tDbl = ConvertToDouble(tStr.Trim());
                if (tDbl > double.MinValue)
                    ErrorDisplayDelay = Convert.ToChar((int)tDbl);
                else return false;

                //File Numbered entries
                iniFileEntries.Clear(); //clear Fileentries from the start
                bool MoreFileEntries = true;
                int FileIndex = 1;
                string strFileEntry = cstFileHdr + FileIndex.ToString("D2"); //Create string "FileXX"
                //Check for Filepath in FileXX
                MoreFileEntries = IniFileObj.KeyExists(cstFilePath, strFileEntry);
                while (MoreFileEntries == true) //FileXX entries
                {
                    //Create FileEntries
                    clsIniFileEntry tiniFileEntry = new clsIniFileEntry();

                    //Main
                    //FilePattern and FilePath
                    string FilePattern = IniFileObj.Read(cstFilePattern, strFileEntry);
                    FilePattern = FilePattern.Trim();
                    string FilePath = IniFileObj.Read(cstFilePath, strFileEntry);
                    FilePath = FilePath.Trim();
                    //Enter data into structure
                    tiniFileEntry.SetFilenameUpdate(FilePath, FilePattern);

                    //Dividers
                    char DividerL0 = ' '; char DividerL1 = ' ';
                    tStr = IniFileObj.Read(cstDividerL0, strFileEntry);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue)
                        DividerL0 = Convert.ToChar((int)tDbl);
                    else return false;

                    tStr = IniFileObj.Read(cstDividerL1, strFileEntry);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue)
                        DividerL1 = Convert.ToChar((int)tDbl);
                    else return false;

                    //ConSeqDiv
                    bool ConSeqDiv;
                    tStr = IniFileObj.Read(cstConSeqDiv, strFileEntry);
                    if (tStr.ToLower() == "true") ConSeqDiv = true;
                    else if (tStr.ToLower() == "false") ConSeqDiv = false;
                    else return false;

                    //FirstHdrline
                    int FirstHdrLine;
                    tStr = IniFileObj.Read(cstFirstHdrLine, strFileEntry);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue) FirstHdrLine = Convert.ToChar((int)tDbl);
                    else return false;

                    //FirstDataline
                    int FirstDataLine;
                    tStr = IniFileObj.Read(cstFirstDataLine, strFileEntry);
                    tDbl = ConvertToDouble(tStr.Trim());
                    if (tDbl > double.MinValue) FirstDataLine = Convert.ToChar((int)tDbl);
                    else return false;

                    //Enter data into structure
                    tiniFileEntry.SetFormatValues(DividerL0, DividerL1, ConSeqDiv, FirstHdrLine, FirstDataLine);

                    bool MoreNomEntries = true;
                    int NomIndex = 1;
                    //Find the "NominalXX", "MinXX", "MaxXX", "InfoTypeXX"
                    MoreNomEntries = IniFileObj.KeyExists(cstInfoType + NomIndex.ToString("D2"), strFileEntry);
                    while (MoreNomEntries == true) //FileXX entries
                    {
                        string tstrInfoType = "", tstrNominal = "";
                        double tdblMin = 0, tdblMax = 0;
                        tStr = IniFileObj.Read(cstInfoType + NomIndex.ToString("D2"), strFileEntry);
                        tstrInfoType = tStr.Trim();
                        //InfoType.Add(tStr.Trim());
                        if (tStr.ToLower() == "number")
                        {
                            //Min
                            tStr = IniFileObj.Read(cstMin + NomIndex.ToString("D2"), strFileEntry);
                            tDbl = ConvertToDouble(tStr.Trim());
                            if (tDbl > double.MinValue)
                                tdblMin = tDbl; //Min.Add(tDbl);
                            else return false;
                            //Max
                            tStr = IniFileObj.Read(cstMax + NomIndex.ToString("D2"), strFileEntry);
                            tDbl = ConvertToDouble(tStr.Trim());
                            if (tDbl > double.MinValue)
                                tdblMax = tDbl; //Max.Add(tDbl);
                            else return false;
                            //Expand all none used
                            tstrNominal = "";
                            //strNominal.Add("");
                        }
                        if (tStr.ToLower() == "text")
                        {
                            //Nominal
                            tStr = IniFileObj.Read(cstNominal + NomIndex.ToString("D2"), strFileEntry);
                            tStr = tStr.Replace("\"", "");
                            tstrNominal = tStr.Trim(); //strNominal.Add(tStr.Trim());
                            //Expand all none used
                            tdblMin = 0; //Min.Add(0);
                            tdblMax = 0; //Max.Add(0);
                        }
                        if (tStr.ToLower() == "ignore")
                        {
                            tstrNominal = "";  //strNominal.Add("");
                            tdblMin = 0; //Min.Add(0);
                            tdblMax = 0; //Max.Add(0);
                        }
                        //Add data to structure
                        tiniFileEntry.AddNumberValues(tdblMin, tdblMax, tstrNominal, tstrInfoType);
                        NomIndex++;
                        MoreNomEntries = IniFileObj.KeyExists(cstInfoType + NomIndex.ToString("D2"), strFileEntry);
                    }
                    iniFileEntries.Add(tiniFileEntry);
                    FileIndex++;
                    //Check for Filepath in FileXX
                    strFileEntry = cstFileHdr + FileIndex.ToString("D2"); //Create string "FileXX"
                    MoreFileEntries = IniFileObj.KeyExists(cstFilePath, strFileEntry); //Look for FilePath
                }
                int eee = 4;
                eee++;
                return true;
            }
            else 
                return false;
        }
    }

    // File Sub level
    public class clsIniFileEntry
    {
        //Constants for ini-file
        //public const 
        public const string cstNominal = "Nominal";         //Nominal00
        public const string cstMin = "Min";                 //Min00
        public const string cstMax = "Max";                 //Max00
        public const string cstInfoType = "InfoType";       //InfoType00

        public string strHeader;

        //Variables concerning the format of the line - read from the ini-file
        public char DividerL0; //DividerL0=9  //Tab
        public char DividerL1; //DividerL1=34 //"
        public bool ConSeqDiv; //ConSeqDiv=true
        public int FirstHdrLine;  //FirstLine=1
        public int FirstDataLine;  //FirstLine=2
        public string IniFileName;
        public string FilePattern;
        public string FileName;
        public string FilePath;

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

        public clsIniFileEntry()
        {
            //Variables concerning the format of the line - read from the ini-file
            DividerL0 = '\0';   //DividerL0=9  //Tab
            DividerL1 = '\0';   //DividerL1=34 //"
            ConSeqDiv = false;  //ConSeqDiv=true
            FirstHdrLine = -1;  //FirstHdrLine=1
            FirstDataLine = -2; //FirstDataLine=2
            FilePattern = "";
            FileName = "";
            FilePath = "";

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
            //Variables concerning values read - read from the ini-file
            strNominal.Clear(); Min.Clear(); Max.Clear(); InfoType.Clear();

            //Results from file reading
            Corrects.Clear(); Wrongs.Clear(); PrevCorrects.Clear(); PrevWrongs.Clear();
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

        public void SetFormatValues(char tDividerL0, char tDividerL1, bool tConSeqDiv, int tFirstHdrLine, int tFirstDataLine)
        {
            DividerL0 = tDividerL0;
            DividerL1 = tDividerL1;
            ConSeqDiv = tConSeqDiv;
            FirstHdrLine = tFirstHdrLine;
            FirstDataLine = tFirstDataLine;
        }

        public bool SetFilenameUpdate(string tFilePath, string tFilePattern)
        {
            //if(File.Exists(tFilePattern) == true)
            tFilePath = tFilePath.Trim();
            tFilePattern = tFilePattern.Trim();
            if (tFilePattern != "")
            {
                FilePath = tFilePath;
                FilePattern = tFilePattern;
                return true;
            }
            return false;
        }

        public void AddNumberValues(double tMin, double tMax, string tStrNom, string tInfoType)
        {
            Min.Add(tMin);
            Max.Add(tMax);
            strNominal.Add(tStrNom);
            InfoType.Add(tInfoType);
            Corrects.Add(0);     Wrongs.Add(0);
            PrevCorrects.Add(0); PrevWrongs.Add(0);
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
        ///     If tstrConfigIniPath is left empty, the preset is used
        /// </summary>
        /// <param name="tstrConfigIniPath"></param>
        /// <param name="tFileIndex"></param>
        /// <param name="tiIndex"></param>
        /// <returns></returns>
        public bool ReadIniFileNotUsed(string tstrConfigIniPath)
        {
            /*
            //Retrieve replace strings
            bool bolFormatExists = false;

            if (tstrConfigIniPath != "") IniFileName = tstrConfigIniPath;
            if (File.Exists(IniFileName) == true)
            {
                IniFile tMyIniRead = new IniFile(IniFileName); //IniFile

                bolFormatExists = tMyIniRead.KeyExists(cstFilePattern, strFile);
                if (bolFormatExists == true)
                {
                    string tStr = ""; double tDbl = -1;
                    //Numbered entries
                    bool MoreEntries = true;
                    int k = 0;
                    MoreEntries = tMyIniRead.KeyExists(cstInfoType + k.ToString("D2"), strFile);
                    while (MoreEntries == true)
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
                else return false;
            }
            else return false;
            return bolFormatExists;
            */
            return false;
        }
    }
}
