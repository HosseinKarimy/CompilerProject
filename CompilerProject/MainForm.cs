using CompilerProject.Tokens;

namespace CompilerProject;

public partial class MainForm : Form
{
    readonly List<string> Keywords = new List<string> { "program",
    "var",
    "begin",
    "end",
    "integer",
    "show"
    };

    readonly List<char> Operators = new List<char> {
        '+' ,
        '-' ,
        '*' ,
        '/' ,
        ';' ,
        ',' ,
        ':' ,
        '(' ,
        ')' ,
        '='
    };

    readonly List<char> Alphabetical = new List<char> {
    'a',
    'b',
    'c',
    'd',
    'e'
    };

    List<TokenModel> tokens = new List<TokenModel>();
    private static int line = 0;
    private static int column = 0;

    public MainForm()
    {
        InitializeComponent();
    }

    private void ButtonStart_Click(object sender, EventArgs e)
    {
        if (RButtonAutomatic.Checked is true)
        {
            SContainerTraceOption.Panel2Collapsed = true;
            AutoCompile();
        }
        else
        {
            SContainerTraceOption.Panel1Collapsed = true;
            ButtonNextWord.Enabled = true;
            ButtonNextLine.Enabled = true;
            ButtonSkipManual.Enabled = true;
        }
        StartSplitContainer.Visible = false;
    }


    private void AutoCompile()
    {
        int test = EditorRichTextBox.TextLength;
        while (column < test)
        {
            Parse();            
        }
    }

    private TokenModel? ParseToken()
    {

        EditorRichTextBox.SelectAll();
        EditorRichTextBox.SelectionBackColor = EditorRichTextBox.BackColor;        

        char _char = EditorRichTextBox.GetCharFromPosition(EditorRichTextBox.GetPositionFromCharIndex(column));
        if (_char == ' ' || _char == '\n')
        {
            column++;
            return null;
        }

        line = EditorRichTextBox.GetLineFromCharIndex(column);
        int offset = column - EditorRichTextBox.GetFirstCharIndexFromLine(line);
        int _startPosition = column;
        string _token = string.Empty;
        do
        {
            EditorRichTextBox.Select(column, 1);
            _token += EditorRichTextBox.SelectedText;
            EditorRichTextBox.SelectionBackColor = Color.YellowGreen;
            column++;
            _char = EditorRichTextBox.GetCharFromPosition(EditorRichTextBox.GetPositionFromCharIndex(column));
        } while (column <= EditorRichTextBox.TextLength && _char != ' ' && !Operators.Any(u => u == _char || u == EditorRichTextBox.SelectedText.First()) &&
        offset + _token.Length < EditorRichTextBox.Lines[EditorRichTextBox.GetLineFromCharIndex(column)].Length);

        while (_char == ' ' || _char == '\n')
        {
            column++;
            _char = EditorRichTextBox.GetCharFromPosition(EditorRichTextBox.GetPositionFromCharIndex(column));
        }

        (TokenStatus status, TokenType type) result = CheckToken(_token);
        return new TokenModel { Name = _token, Column = offset, Line = line, Status = result.status, Type = result.type ,StartPosition = _startPosition };
    }

    private (TokenStatus status, TokenType type) CheckToken(string token)
    {
        if (token.All(char.IsDigit))
        {
            return (TokenStatus.Correct, TokenType.Number);
        }

        if (token.Length == 1)
        {
            if (Operators.Any(u => u == token.First()))
            {
                return (TokenStatus.Correct, TokenType.Operator);
            }
            else if (Alphabetical.Any(u => u == token.First()))
            {
                return (TokenStatus.Correct, TokenType.Identifier);
            }
            else
            {
                return (TokenStatus.Error, TokenType.Error);
            }
        }

        if (Keywords.Any(u => u == token))
        {
            return (TokenStatus.Correct, TokenType.Keyword);
        }

        foreach (char _char in token)
        {
            if (!Alphabetical.Contains(_char) && !Char.IsDigit(_char))
            {
                return (TokenStatus.Error, TokenType.Error);
            }
        }

        return (TokenStatus.Correct, TokenType.Identifier);
    }

    private void ButtonNextWord_Click(object sender, EventArgs e)
    {
        Parse();
    }

    private void Parse()
    {
        if (column >= EditorRichTextBox.TextLength)
        {
            MessageBox.Show("End of code");
            HighLightErrors(); 

            return;
        }
        var _newToken = ParseToken();
        if (_newToken is null)
            return;

        tokens.Add(_newToken);
        UpdateStatusPanel(_newToken);
        AddTokenToTable(_newToken);
        UpdateProggressbar();
    }

    private void HighLightErrors()
    {
        EditorRichTextBox.SelectAll();
        EditorRichTextBox.SelectionBackColor = EditorRichTextBox.BackColor;
        foreach (var token in tokens.Where(u=>u.Status == TokenStatus.Error))
        {
            EditorRichTextBox.Select(token.StartPosition, token.Name.Length);
            EditorRichTextBox.SelectionBackColor = ColorTranslator.FromHtml("#bb7777");
        }
    }

    private void UpdateStatusPanel(TokenModel? newToken)
    {
        LabelTokenName.Text = newToken!.Name;
        LabelTokenStatus.Text = newToken.Status.ToString();
        LabelTokenType.Text = newToken.Type.ToString();
    }

    private void UpdateProggressbar()
    {
        int temp = column * 100 / EditorRichTextBox.TextLength;
        ProgressBarResult.Value = temp;
    }

    private void AddTokenToTable(TokenModel? newToken)
    {
        if (newToken is null)
            return;

        if (newToken.Status == TokenStatus.Correct)
        {
            ListViewTokenTable.Items.Add(new ListViewItem(new string[] { newToken.Name, newToken.Type.ToString(), newToken.Line.ToString(), newToken.Column.ToString() }));
        }
        else
        {
            ListViewErrorTable.Items.Add(new ListViewItem(new string[] { newToken.Name, newToken.Type.ToString(), newToken.Line.ToString(), newToken.Column.ToString() }));
        }
    }

    private void ButtonNextLine_Click(object sender, EventArgs e)
    {
        int _currentLine = line;
        do
        {
            Parse();
        } while (column < EditorRichTextBox.TextLength && _currentLine == line);
    }

    private void ButtonSkipManual_Click(object sender, EventArgs e)
    {
        do
        {
            Parse();
        } while (column < EditorRichTextBox.TextLength);
    }
}