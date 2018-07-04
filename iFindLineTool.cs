using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro;
using System.Windows.Forms;
using System.Data;
using System.IO;
using Cognex.VisionPro.ToolBlock;

namespace findlinetool
{
    
    class iFindLineTool
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱
        private Boolean FindLineTool_Status = false;
        private Double mStartX, mStartY, mEndX, mEndY;
        private int mNumCalipers, mCaliperSearchLength, 
                    mCaliperProjectionLength, mCaliperSearchDirection;
        private CogFindLineTool mFindLineTool;
        private FindLineTool_Results mFindLineTool_Results;

        public Boolean Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                mFindLineTool = null;
                mFindLineTool = new CogFindLineTool();

                mFindLineTool_Results = null;
                mFindLineTool_Results = new FindLineTool_Results();

                mStartX = 201.412;
                mStartY = 263.353;
                mEndX = 401.412;
                mEndY = 263.353;
                mNumCalipers = 6;
                mCaliperSearchLength = 30;
                mCaliperProjectionLength = 10;
                mCaliperSearchDirection = 90;

                FindLineTool_Status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
            
        }

        public Boolean unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mFindLineTool = null;

                FindLineTool_Status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool unLoad Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean ROI_create(CogRecordDisplay CogRecordDisplay1)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //initial RunParams
                mFindLineTool.RunParams.ExpectedLineSegment.StartX = mStartX;
                mFindLineTool.RunParams.ExpectedLineSegment.StartY = mStartY;
                mFindLineTool.RunParams.ExpectedLineSegment.EndX = mEndX;
                mFindLineTool.RunParams.ExpectedLineSegment.EndY = mEndY;
                mFindLineTool.RunParams.NumCalipers = mNumCalipers;
                mFindLineTool.RunParams.CaliperSearchLength = mCaliperSearchLength;
                mFindLineTool.RunParams.CaliperProjectionLength = mCaliperProjectionLength;
                mFindLineTool.RunParams.CaliperSearchDirection = mCaliperSearchDirection;

                mFindLineTool.InputImage = (CogImage8Grey)CogRecordDisplay1.Image;
                CogRecordDisplay1.Record = mFindLineTool.CreateCurrentRecord().SubRecords["InputImage"];
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool ROI_create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindLineTool_Status = false;
                return false;
            }
        }

        public Boolean Run(CogImage8Grey mInputImage)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            FindLineTool_Status = false;

            try
            {
                mFindLineTool.InputImage = mInputImage;                

                mFindLineTool.Run();
                FindLineTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool ROI_create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }            
        }

        public void Show()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                if (mFindLineTool.Results != null)
                {
                    CogLine iLine = mFindLineTool.Results.GetLine();
                    MessageBox.Show("X=" + iLine.X + "\nY=" + iLine.Y);
                }
                else
                {

                    MessageBox.Show("Didn't find any line");
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool ROI_create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
            }
        }

        private Boolean make_results()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                if (mFindLineTool.Results != null)
                {
                    CogLine iLine = mFindLineTool.Results.GetLine();
                    CogLineSegment iLineSegment = mFindLineTool.Results.GetLineSegment();

                    if (iLine != null && iLineSegment != null)
                    {
                        mFindLineTool_Results.lineX = iLine.X;
                        mFindLineTool_Results.lineY = iLine.Y;
                        mFindLineTool_Results.rotation = iLine.Rotation;
                        mFindLineTool_Results.startX = iLineSegment.StartX;
                        mFindLineTool_Results.startY = iLineSegment.StartY;
                        mFindLineTool_Results.endX = iLineSegment.EndX;
                        mFindLineTool_Results.endY = iLineSegment.EndY;
                        return true;
                    }
                    else
                    {
                        SaveLog.Msg_("iLine or iLineSegment is null!");
                        return false;
                    }
                }
                else
                {
                    SaveLog.Msg_("FindLineTool Doesn't have any result!");
                    return false;
                    //MessageBox.Show("Didn't find any line");
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindLineTool make_results Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean SaveToVPPFile(string FileName)//檔案參數儲存
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //建立目錄資料夾
                string strFolderPath = @"D:\VPS_File\Product\FindLineTool\" + @FileName + @"\";
                DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                //塞到CogTool裡面
                CogToolBlock ToolBlock1 = new CogToolBlock();

                mFindLineTool.Name = FileName + "_FindLineTool_";

                ToolBlock1.Tools.Add(mFindLineTool);

                FileName = strFolderPath + FileName + "_FLT.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of FindLineTool Saved : " + FileName);
                ToolBlock1 = null;
                FindLineTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindLineTool_Status = false;
                return false;
            }
        }

        public Boolean LoadFromVPPFile(string FileName)//檔案參數載入
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            string TempFileName = (string)FileName;

            try
            {
                //從CogTool裡面讀出來
                string strFolderPath = @"D:\VPS_File\Product\FindLineTool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_FLT.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mFindLineTool = (CogFindLineTool)ToolBlock1.Tools[TempFileName + "_FindLineTool_"];
                mFindLineTool.Run();

                SaveLog.Msg_("Data of Find Line Tool Loaded : " + @FileName);
                ToolBlock1 = null;

                FindLineTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindLineTool_Status = false;
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\FindLineTool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_FLT.vpp";

                if (File.Exists(FileName))
                {
                    SaveLog.Msg_("Data File exists : " + FileName);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                FindLineTool_Status = false;
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                MessageBox.Show(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString(), "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                return true;
            }
        }

        public Boolean get_Status()
        {
            return FindLineTool_Status;
        }

        public FindLineTool_Results get_LineResult()
        {
            if (make_results())
            {
                return mFindLineTool_Results;
            }
            else
            {
                return null;
            }
        }

        public bool setStartX(double startX)
        {
            mStartX = startX;
            return true;
        }

        public bool setStartY(double startY)
        {
            mStartY = startY;
            return true;
        }

        public bool setEndX(double endX)
        {
            mEndX = endX;
            return true;
        }

        public bool setEndY(double endY)
        {
            mEndY = endY;
            return true;
        }

        public bool setNumCalipers(int num)
        {
            if(num >= 2)
            {
            mNumCalipers = num;
            return true;
            }
            else{
                SaveLog.Msg_("setNumCalipers Failed, must greater than 1!");
                return false;
            }
        }

        public bool setCaliperSearchLength(int length)
        {
            if (length > 0)
            {
                mCaliperSearchLength = length;
                return true;
            }
            else
            {
                SaveLog.Msg_("setCaliperSearchLength Failed, must greater than 0!");
                return false;
            }
        }

        public bool setCaliperProjectionLength(int length)
        {
            if (length > 0)
            {
                mCaliperProjectionLength = length;
                return true;
            }
            else
            {
                SaveLog.Msg_("setCaliperProjectionLength Failed, must greatr than 0!");
                return false;
            }
        }

        public bool setCaliperSearchDirection(int direction)
        {
            mCaliperSearchDirection = direction;
            return true;
        }

        public bool setCaliperSetting(CogCaliperPolarityConstants opt)
        {
            mFindLineTool.RunParams.CaliperRunParams.Edge0Polarity = opt;
            //CogCaliperPolarityConstants.LightToDark
            //CogCaliperPolarityConstants.DarkToLight
            //CogCaliperPolarityConstants.DontCare 
            //  -----3 options-----
            return true;
        }
    }
    class FindLineTool_Results
    {
        public double lineX, lineY, rotation, startX, startY, endX, endY;
    }
}
