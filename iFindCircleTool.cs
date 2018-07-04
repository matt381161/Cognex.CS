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
    
    class iFindCircleTool
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱
        private Boolean FindCircleTool_Status = false;
        private Double mCenterX, mCenterY, mRadius, mAngleStart, mAngleSpan, mCaliperSearchLength,
                    mCaliperProjectionLength, mCaliperSearchDirection;
        private int mNumCalipers ,mNumToIgnore;
        private CogFindCircleSearchDirectionConstants mCircleSearchDirectionOption;//inward,outward
        private CogFindCircleTool mFindCircleTool;
        private FindCircleTool_Results mFindCircleTool_Results;
        private CogCaliperPolarityConstants mCogCaliperPolarityConstants;

        public Boolean Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                mFindCircleTool = null;
                mFindCircleTool = new CogFindCircleTool();

                mFindCircleTool_Results = null;
                mFindCircleTool_Results = new FindCircleTool_Results();

                mCenterX = 730.041;
                mCenterY = 436.627;
                mRadius = 377.985;
                mAngleStart = 78.8641;
                mAngleSpan = -277.93;
                mNumCalipers = 6;
                mNumToIgnore = 2;
                mCaliperSearchLength = 177.499;
                mCaliperProjectionLength = 54.2951;
                mCircleSearchDirectionOption = CogFindCircleSearchDirectionConstants.Outward;
                mCogCaliperPolarityConstants = CogCaliperPolarityConstants.LightToDark;

                FindCircleTool_Status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindCircleTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
            
        }

        public Boolean unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mFindCircleTool = null;

                FindCircleTool_Status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindCircleTool unLoad Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean ROI_create(CogRecordDisplay CogRecordDisplay1)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //initial RunParams
                mFindCircleTool.RunParams.ExpectedCircularArc.CenterX = mCenterX;
                mFindCircleTool.RunParams.ExpectedCircularArc.CenterY = mCenterY;
                mFindCircleTool.RunParams.ExpectedCircularArc.Radius = mRadius;
                mFindCircleTool.RunParams.ExpectedCircularArc.AngleStart = mAngleStart;
                mFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan = mAngleSpan;
                mFindCircleTool.RunParams.NumCalipers = mNumCalipers;
                mFindCircleTool.RunParams.CaliperSearchLength = mCaliperSearchLength;
                mFindCircleTool.RunParams.CaliperProjectionLength = mCaliperProjectionLength;
                mFindCircleTool.RunParams.CaliperSearchDirection = mCircleSearchDirectionOption;
                mFindCircleTool.RunParams.NumToIgnore = mNumToIgnore;
                mFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = mCogCaliperPolarityConstants;

                mFindCircleTool.InputImage = (CogImage8Grey)CogRecordDisplay1.Image;
                CogRecordDisplay1.Record = mFindCircleTool.CreateCurrentRecord().SubRecords["InputImage"];
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindCircleTool ROI_create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindCircleTool_Status = false;
                return false;
            }
        }

        public Boolean Run(CogRecordDisplay CogRecordDisplay1)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            FindCircleTool_Status = false;

            try
            {
                mFindCircleTool.InputImage = (CogImage8Grey)CogRecordDisplay1.Image;                

                mFindCircleTool.Run();
                CogRecordDisplay1.Record = mFindCircleTool.CreateLastRunRecord().SubRecords["InputImage"];
                FindCircleTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindCircleTool Run Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }            
        }

        private Boolean make_results()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                if (mFindCircleTool.Results != null)
                {
                    CogCircle iCircle = mFindCircleTool.Results.GetCircle();
                    CogCircularArc iCircularArc = mFindCircleTool.Results.GetCircularArc();

                    if (iCircle != null && iCircularArc != null)
                    {
                        mFindCircleTool_Results.Circle_CenterX = iCircle.CenterX;
                        mFindCircleTool_Results.Circle_CenterY = iCircle.CenterY;
                        mFindCircleTool_Results.Circle_Radius = iCircle.Radius;
                        mFindCircleTool_Results.CircularArc_CenterX = iCircularArc.CenterX;
                        mFindCircleTool_Results.CircularArc_CenterY = iCircularArc.CenterY;
                        mFindCircleTool_Results.CircularArc_Radius = iCircularArc.Radius;
                        mFindCircleTool_Results.CircularArc_AngleStart = iCircularArc.AngleStart;
                        mFindCircleTool_Results.CircularArc_AngleSpan = iCircularArc.AngleSpan;
                        return true;
                    }
                    else
                    {
                        SaveLog.Msg_("iCircle or iCircularArc is null!");
                        return false;
                    }
                }
                else
                {
                    SaveLog.Msg_("FindCircleTool Doesn't have any result!");
                    return false;
                    //MessageBox.Show("Didn't find any line");
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("FindCircleTool make_results Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean SaveToVPPFile(string FileName)//檔案參數儲存
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //建立目錄資料夾
                string strFolderPath = @"D:\VPS_File\Product\FindCircleTool\" + @FileName + @"\";
                DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                //塞到CogTool裡面
                CogToolBlock ToolBlock1 = new CogToolBlock();

                mFindCircleTool.Name = FileName + "_FindCircleTool_";

                ToolBlock1.Tools.Add(mFindCircleTool);

                FileName = strFolderPath + FileName + "_FCT.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of FindCircleTool Saved : " + FileName);
                ToolBlock1 = null;
                FindCircleTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save FindCircleTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindCircleTool_Status = false;
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
                string strFolderPath = @"D:\VPS_File\Product\FindCircleTool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_FCT.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mFindCircleTool = (CogFindCircleTool)ToolBlock1.Tools[TempFileName + "_FindCircleTool_"];
                mFindCircleTool.Run();

                SaveLog.Msg_("Data of Find Circle Tool Loaded : " + @FileName);
                ToolBlock1 = null;

                FindCircleTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save FindCircleTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                FindCircleTool_Status = false;
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\FindCircleTool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_FCT.vpp";

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
                FindCircleTool_Status = false;
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                MessageBox.Show(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString(), "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                return true;
            }
        }

        public Boolean get_Status()
        {
            return FindCircleTool_Status;
        }

        public FindCircleTool_Results get_CircleResult()
        {
            if (make_results())
            {
                return mFindCircleTool_Results;
            }
            else
            {
                return null;
            }
        }

        public bool setCenterX(double centerX)
        {
            mCenterX = centerX;
            return true;
        }

        public bool setCenterY(double centerY)
        {
            mCenterY = centerY;
            return true;
        }

        public bool setRadius(double radius)
        {
            mRadius = radius;
            return true;
        }

        public bool setAngleStart(double anglestart)
        {
            mAngleStart = anglestart;
            return true;
        }

        public bool setAngleSpan(double anglespan)
        {
            mAngleSpan = anglespan;
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
            mFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = opt;
            //CogCaliperPolarityConstants.LightToDark
            //CogCaliperPolarityConstants.DarkToLight
            //CogCaliperPolarityConstants.DontCare 
            //  -----3 options-----
            return true;
        }
    }
    class FindCircleTool_Results
    {
        public double Circle_CenterX, Circle_CenterY, Circle_Radius;
        public double CircularArc_CenterX, CircularArc_CenterY, CircularArc_Radius,
            CircularArc_AngleStart, CircularArc_AngleSpan;
    }
}
