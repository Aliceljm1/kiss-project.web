#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-23
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-23		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.IO;

namespace Kiss.Web.Utils
{
    // Class definition *********************************************************
    /// <summary>
    /// Performs a "minimize" operation on JavaScript files.
    /// It removes unnecessary characters and all comments from the file.
    /// However, it is not an obfuscator. It doesn't modify the variable names.
    /// <para>Comments will be removed. Tabs will be
    /// replaced with a single space. Carriage returns will be replaced with linefeeds.
    /// Most spaces and linefeeds will be removed.</para>
    /// <para>This code is strongly inspired of the C program jsmin, by Douglas Crockford.</para>
    /// <para><a href="http://www.crockford.com/javascript/jsmin.html">JsMin homepage</a></para>
    /// </summary>    
    public class JsMin
    {
        //*************************************************************************
        //* Enums *****************************************************************
        //*************************************************************************

        #region Enums

        // ------------------------------------------------------------------------
        private enum Action
        {
            /// <summary>
            /// Action 1: Output A. Copy A to B. Get the next B.
            /// </summary>
            OutputA1 = 1,
            /// <summary>
            /// Action 2: Copy B to A. Get the next B. (Delete A).
            /// </summary>
            CopyAtoB2 = 2,
            /// <summary>
            /// Action 3: Get the next B. (Delete B).
            /// </summary>
            GetNextB3 = 3,
        }

        #endregion

        //*************************************************************************
        //* Constants *************************************************************
        //*************************************************************************

        #region Constants

        private const int EOF = -1;
        private const int TILDE = 126;

        #endregion

        //*************************************************************************
        //* Static attributes *****************************************************
        //*************************************************************************

        #region StaticAttributes
        #endregion

        //*************************************************************************
        //* Attributes ************************************************************
        //*************************************************************************

        #region Attributes

        private string bakFileExtension = "bak";
        private string[] firstLines = null;
        private int keepFirstLines = 0;

        // Internal variables

        private int charA = '\n';
        private int charB = '\n';
        private int lookAhead = EOF;
        private bool firstLineOutput = false;

        #endregion

        //*************************************************************************
        //* Properties ************************************************************
        //*************************************************************************

        #region Properties

        // ------------------------------------------------------------------------
        /// <summary>
        /// Extension of the file created for backup.
        /// <param>This file will be deleted after process, unless the
        /// parameter keepBakFile is set to true in <see cref="Go(string,bool)" /></param>
        /// </summary>
        public string BakFileExtension
        {
            set
            {
                if ((value == null)
                  || (value.Length == 0))
                {
                    value = "bak";
                }
                bakFileExtension = value;
            }
            get
            {
                return bakFileExtension;
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Each line in this array will be added to the top output file as comments.
        /// The lines do not need to have the comment characters.
        /// </summary>
        public string[] FirstLines
        {
            set
            {
                firstLines = value;
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// A number specifying how many lines of the original file will be kept
        /// in the minimized file. This can be used to preserve copyright information,
        /// the author's name, etc...
        /// <para>Note: These lines will be added *after* the ones specified
        /// in <see cref="FirstLines"/>.</para>
        /// </summary>
        public int KeepFirstLines
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                keepFirstLines = value;
            }
        }

        #endregion

        //*************************************************************************
        //* Static methods ********************************************************
        //*************************************************************************

        #region Static methods
        #endregion

        //*************************************************************************
        //* Constructor & generated code ******************************************
        //*************************************************************************

        #region Constructors

        // ------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public JsMin()
        {
        }

        #endregion

        //*************************************************************************
        //* Event handlers ********************************************************
        //*************************************************************************

        #region Event handlers
        #endregion

        //*************************************************************************
        //* Methods ***************************************************************
        //*************************************************************************

        #region Methods

        // ------------------------------------------------------------------------
        /// <summary>
        /// Converts the input file into a minimized file with the same name.
        /// </summary>
        /// <param name="filePath">The absolute path to the file to be minimized.</param>
        public void Go(string filePath)
        {
            Go(filePath, false);
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Converts the input file into a minimized file with the same name.
        /// </summary>
        /// <param name="filePath">The absolute path to the file to be minimized.</param>
        /// <param name="keepBakFile">If true, the input file will be saved in a BAK file.
        /// The BAK file extension can be set with <see cref="BakFileExtension" /></param>
        public void Go(string filePath, bool keepBakFile)
        {
            bool bakFileCreated = false;
            FileInfo oldFile = null;
            FileInfo newFile = null;
            StreamReader input = null;
            StreamWriter output = null;
            FileAttributes oldAttributes;

            try
            {
                // Save backup file
                oldFile = new FileInfo(filePath);
                newFile = new FileInfo(filePath);
                if (!oldFile.Exists)
                {
                    throw new FileNotFoundException("File not found", filePath);
                }
                oldAttributes = oldFile.Attributes;
                oldFile.Attributes = FileAttributes.Normal;

                string bakFilePath = oldFile.FullName + "." + this.bakFileExtension;
                if (File.Exists(bakFilePath))
                {
                    FileInfo tempFile = new FileInfo(bakFilePath);
                    tempFile.Attributes = FileAttributes.Normal;
                    tempFile.Delete();
                }

                oldFile.MoveTo(bakFilePath);
                bakFileCreated = true;

                try
                {
                    input = oldFile.OpenText();
                    output = newFile.CreateText();

                    // Process file
                    this.Go(input, output);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (output != null)
                    {
                        output.Close();
                    }
                    if (input != null)
                    {
                        input.Close();
                    }
                }

                // Everything went OK!
                if (newFile.Exists)
                {
                    newFile.Attributes = oldAttributes;
                }
                if (!keepBakFile)
                {
                    if ((oldFile != null)
                      && oldFile.Exists)
                    {
                        oldFile.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                // Restore old file
                if (bakFileCreated)
                {
                    if ((newFile != null)
                      && newFile.Exists)
                    {
                        newFile.Attributes = FileAttributes.Normal;
                        newFile.Delete();
                    }
                    oldFile.MoveTo(filePath);
                }
                throw ex;
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Minimizes the script code provided by the reader and saves it to the writer.
        /// </summary>
        /// <remarks>This method will NOT close the provided reader and writer.</remarks>
        /// <param name="reader">A reader providing the script code to be minimized.</param>
        /// <param name="writer">The writer to which the minimized code will be saved.</param>
        public void Go(TextReader reader, TextWriter writer)
        {
            // Process file
            if (firstLines != null)
            {
                for (int index = 0; index < firstLines.Length; index++)
                {
                    writer.WriteLine("// " + firstLines[index]);
                }
            }

            string line = "";
            for (int index = 0; index < keepFirstLines; index++)
            {
                line = reader.ReadLine();
                if (line != null)
                {
                    writer.WriteLine(line);
                }
            }
            if (line == null)
            {
                return;
            }

            charA = '\n';
            PerformAction(Action.GetNextB3, reader, writer);
            while (charA != EOF)
            {
                switch (charA)
                {
                    case ' ':
                        {
                            if (IsAlphaNumeric(charB))
                            {
                                PerformAction(Action.OutputA1, reader, writer);
                            }
                            else
                            {
                                PerformAction(Action.CopyAtoB2, reader, writer);
                            }
                        }
                        break;

                    case '\n':
                        {
                            switch (charB)
                            {
                                case '{':
                                case '[':
                                case '(':
                                case '+':
                                case '-':
                                    PerformAction(Action.OutputA1, reader, writer);
                                    break;

                                case ' ':
                                    PerformAction(Action.GetNextB3, reader, writer);
                                    break;

                                default:
                                    if (IsAlphaNumeric(charB))
                                    {
                                        PerformAction(Action.OutputA1, reader, writer);
                                    }
                                    else
                                    {
                                        PerformAction(Action.CopyAtoB2, reader, writer);
                                    }
                                    break;
                            }
                        }
                        break;

                    default:
                        {
                            switch (charB)
                            {
                                case ' ':
                                    {
                                        if (IsAlphaNumeric(charA))
                                        {
                                            PerformAction(Action.OutputA1, reader, writer);
                                            break;
                                        }
                                        PerformAction(Action.GetNextB3, reader, writer);
                                    }
                                    break;

                                case '\n':
                                    {
                                        switch (charA)
                                        {
                                            case '}':
                                            case ']':
                                            case ')':
                                            case '+':
                                            case '-':
                                            case '"':
                                            case '\'':
                                                PerformAction(Action.OutputA1, reader, writer);
                                                break;

                                            default:
                                                if (IsAlphaNumeric(charA))
                                                {
                                                    PerformAction(Action.OutputA1, reader, writer);
                                                }
                                                else
                                                {
                                                    PerformAction(Action.GetNextB3, reader, writer);
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    PerformAction(Action.OutputA1, reader, writer);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public string MinifyString(string jsString)
        {
            using (StringWriter writer = new StringWriter())
            {
                using (StringReader rdr = new StringReader(jsString))
                {
                    Go(rdr, writer);
                }

                return writer.GetStringBuilder().ToString();
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Performs different action depending on the parameter.
        /// <list><item>Output A. Copy A to B. Get the next B.</item>
        /// <item>Copy B to A. Get the next B. (Delete A).</item>
        /// <item>Get the next B. (Delete B).</item></list>
        /// </summary>
        /// <param name="whatToDo">Specify which action to perform.</param>
        /// <param name="reader">A reader providing the script code to be minimized.</param>
        /// <param name="writer">The writer to which the minimized code will be saved.</param>
        private void PerformAction(Action whatToDo, TextReader reader, TextWriter writer)
        {
            if (whatToDo == Action.OutputA1)
            {
                if (charA != '\n'
                  || firstLineOutput == true)
                {
                    writer.Write((char)charA);
                    firstLineOutput = true;
                }
            }

            if ((whatToDo == Action.OutputA1)
              || (whatToDo == Action.CopyAtoB2))
            {
                charA = charB;
                if ((charA == '\'')
                  || (charA == '"'))
                {
                    while (true)
                    {
                        writer.Write((char)charA);
                        charA = GetChar(reader);
                        if (charA == charB)
                        {
                            break;
                        }
                        if (charA == '\n')
                        {
                            throw new ApplicationException("Unterminated string literal");
                        }
                        if (charA == '\\')
                        {
                            writer.Write((char)charA);
                            charA = GetChar(reader);
                        }
                    }
                }
            }

            if ((whatToDo == Action.OutputA1)
              || (whatToDo == Action.CopyAtoB2)
              || (whatToDo == Action.GetNextB3))
            {
                charB = NextChar(reader);
                if ((charB == '/')
                  && ((charA == '(')
                    || (charA == ',')
                    || (charA == '=')
                    || (charA == '[')
                    || (charA == '!')
                    || (charA == ':')
                    || (charA == '&')
                    || (charA == '|')
                    || (charA == '?')))
                {
                    writer.Write((char)charA);
                    writer.Write((char)charB);
                    while (true)
                    {
                        charA = GetChar(reader);
                        if (charA == '/')
                        {
                            break;
                        }
                        else
                        {
                            if (charA == '\\')
                            {
                                writer.Write((char)charA);
                                charA = GetChar(reader);
                            }
                            else
                            {
                                if (charA <= '\n')
                                {
                                    throw new ApplicationException("Unterminated Regular Expression literal.");
                                }
                            }
                        }
                        writer.Write((char)charA);
                    }
                    charB = NextChar(reader);
                }
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Watch out for lookahead. If the character is a control character,
        /// translate it to a space or linefeed.
        /// </summary>
        /// <param name="reader">A reader providing the script code to be minimized.</param>
        /// <returns>Return the next character from the input file.</returns>
        private int GetChar(TextReader reader)
        {
            int newChar = lookAhead;
            lookAhead = EOF;

            if (newChar == EOF)
            {
                newChar = reader.Read();
            }

            if ((newChar >= ' ')
              || (newChar == '\n')
              || (newChar == EOF))
            {
                return newChar;
            }

            if (newChar == '\r')
            {
                return '\n';
            }

            return ' ';
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Get the next character without getting it.
        /// </summary>
        /// <param name="reader">A reader providing the script code to be minimized.</param>
        /// <returns>The next character</returns>
        private int PeekChar(TextReader reader)
        {
            lookAhead = GetChar(reader);
            return lookAhead;
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Get the next character, excluding comments. <see cref="PeekChar" />
        /// is used to see if a '/' is followed by a '/' or '*'.
        /// </summary>
        /// <param name="reader">A reader providing the script code to be minimized.</param>
        /// <returns>The next character.</returns>
        private int NextChar(TextReader reader)
        {
            int newChar = GetChar(reader);

            if (newChar == '/')
            {
                int peekChar = PeekChar(reader);
                if (peekChar == '/')
                {
                    while (true)
                    {
                        newChar = GetChar(reader);
                        if (newChar <= '\n')
                        {
                            return newChar;
                        }
                    }
                }

                if ((peekChar == '/')
                  || (peekChar == '*'))
                {
                    GetChar(reader);
                    while (true)
                    {
                        switch (GetChar(reader))
                        {
                            case '*':
                                {
                                    if (PeekChar(reader) == '/')
                                    {
                                        GetChar(reader);
                                        return ' ';
                                    }
                                }
                                break;

                            case EOF:
                                {
                                    throw new ApplicationException("Unterminated comment");
                                }

                        }
                    }
                }
                return newChar;
            }
            return newChar;
        }

        // ------------------------------------------------------------------------
        /// <summary>
        /// Check if the character is alphanumeric.
        /// </summary>
        /// <param name="toTest">The character to be tested.</param>
        /// <returns>True if the character is a letter, digit, underscore,
        /// dollar sign, or non-ASCII character</returns>
        private bool IsAlphaNumeric(int toTest)
        {
            return ((toTest >= 'a' && toTest <= 'z')
                  || (toTest >= '0' && toTest <= '9')
                  || (toTest >= 'A' && toTest <= 'Z')
                  || toTest == '_'
                  || toTest == '$'
                  || toTest == '\\'
                  || toTest > TILDE);
        }

        #endregion

        //*************************************************************************
        //* Operators *************************************************************
        //*************************************************************************

        #region Operators
        #endregion
    }
}