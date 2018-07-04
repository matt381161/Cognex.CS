using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace findlinetool
{
    public static class SaveLog
    {
        public static RichTextBox RichTextBox_MSG;
        private static Queue<String> MSG_Buff = new Queue<String>();
        private static System.Windows.Forms.Timer Log_Timer;
        public static Boolean Log_Init_Result = false;

        /// <summary>
        /// 建構
        /// </summary>
        /// <returns></returns>
        public static Boolean Load_Log()
        {
            try
            {
                RichTextBox_MSG = new RichTextBox();

                Log_Timer = new System.Windows.Forms.Timer();
                Log_Timer.Tick += new System.EventHandler(Check_Msg);
                Log_Timer.Interval = 20;
                Log_Timer.Enabled = true;
                Log_Init_Result = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 解構
        /// </summary>
        /// <returns></returns>
        public static Boolean UnLoad_Log()
        {
            try
            {
                Log_Timer.Enabled = false;
                Log_Timer = null;
                RichTextBox_MSG = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查訊息列內是否有訊息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Check_Msg(object sender, EventArgs e)
        {
            try
            {
                if (MSG_Buff != null && Log_Init_Result)
                {
                    if (MSG_Buff.Count > 0)
                    {
                        SaveMsg_(MSG_Buff.Dequeue());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("訊息模組異常終止 : " + ex.ToString(), "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }
        }

        /// <summary>
        /// 外部呼叫,塞入駐列內記錄訊息
        /// </summary>
        /// <param name="MSG">訊息</param>
        public static void Msg_(string MSG)
        {
            if (Log_Init_Result)
            {
                MSG_Buff.Enqueue(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " : " + MSG.ToString().Trim());
            }
            else
            {
                MessageBox.Show("訊息模組尚未初始化", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }
        }

        /// <summary>
        /// 儲存訊息
        /// </summary>
        /// <param name="MSG">訊息</param>
        private static void SaveMsg_(string MSG)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            String str_FilePath;
            String k_dir_file;
            Double TextLine = 0;
            DirectoryInfo DIFO;
            String[] sLine;
            String[] sNewLine;

            try
            {
                #region 建立資料夾
                k_dir_file = @"D:\Log_File\";
                k_dir_file += @"\" + DateTime.Now.Year.ToString();

                DIFO = new DirectoryInfo(k_dir_file);

                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                k_dir_file += @"\" + DateTime.Now.Month.ToString();

                DIFO = null;
                DIFO = new DirectoryInfo(k_dir_file);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }
                DIFO = null;

                str_FilePath = Path.Combine(k_dir_file, DateTime.Now.ToString("yyyyMMddHH") + ".dat");
                #endregion 建立資料夾

                #region 儲存訊息
                if (!File.Exists(str_FilePath))
                {
                    using (FileStream fs = File.Create(str_FilePath))
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(str_FilePath, true))
                {
                    file.WriteLine(MSG);
                    file.Close();
                    file.Dispose();
                }
                #endregion 儲存訊息

                #region 保留500行
                sLine = new string[0];
                sNewLine = new string[500];

                RichTextBox_MSG.Invoke(new EventHandler(delegate
                {
                    RichTextBox_MSG.Text = MSG + "\r\n" + RichTextBox_MSG.Text;

                    TextLine = RichTextBox_MSG.Lines.Length;

                    if (TextLine > 500)
                    {
                        sLine = RichTextBox_MSG.Lines;

                        Array.Copy(sLine, 0, sNewLine, 0, sNewLine.Length);

                        RichTextBox_MSG.Lines = sNewLine;
                    }
                }));
                #endregion 保留500行
            }
            catch { }

            sLine = null;
            sNewLine = null;
        }
    }
}
