using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;

namespace kaConv
{
    /// <summary>
    /// ���C���t�H�[���N���X
    /// </summary>
    public partial class FormMain : Form
    {
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyname, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyname, string lpString, string lpFileName);

        /// <summary>INI�t�@�C�� �Z�N�V�����F����</summary>
        private readonly string secHistory = "History";

        /// <summary>INI�t�@�C�� �L�[�F���O�t�H���_</summary>
        private readonly string keyLogDir = "LogDir";

        /// <summary>INI�t�@�C���p�X</summary>
        private string IniPath = string.Empty;


        /// <summary>
        /// 
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            var path = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(path) == false)
            {
                IniPath = Path.ChangeExtension(path, ".ini");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IniPath) == false)
            {
                var logDir = new StringBuilder(1024);
                GetPrivateProfileString(secHistory, keyLogDir, "", logDir, (uint)logDir.Capacity, IniPath);
                textBoxDir.Text = logDir.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(IniPath) == false)
            {
                WritePrivateProfileString(secHistory, keyLogDir, textBoxDir.Text, IniPath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGet_Click(object sender, EventArgs e)
        {
            try
            {
                using (var core = new kaConvCore.kaConvCore())
                {
                    string logDir = textBoxDir.Text;
                    if (string.IsNullOrEmpty(logDir))
                    {
                        MessageBox.Show("���O�t�H���_���w�肵�Ă��������B", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string url = textBoxUrl.Text;
                    if (string.IsNullOrEmpty(url))
                    {
                        MessageBox.Show("�X��URL���w�肵�Ă��������B", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    UseWaitCursor = true;

                    int exitcode = core.GetLog(url, logDir);

                    UseWaitCursor = false;

                    if (exitcode == kaConvCore.Status.OK)
                    {
                        MessageBox.Show("�ߋ��X���̎擾�ɐ������܂����B", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(core.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                UseWaitCursor = false;
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
