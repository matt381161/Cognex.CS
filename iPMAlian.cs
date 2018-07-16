using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using System.IO;
using Cognex.VisionPro.ToolBlock;

namespace PMA_with_Mask
{
    class iPMAlian
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱
        private CogPMAlignTool mPMA_Tool;
        private CogRectangleAffine mPMA_ROI;

        public bool Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mPMA_Tool = null;
                mPMA_Tool = new CogPMAlignTool();

                mPMA_ROI = null;
                mPMA_ROI = new CogRectangleAffine();
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("PMA_Tool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mPMA_Tool = null;

                mPMA_ROI = null;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("PMA_Tool unLoad Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool ROI_Create(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mPMA_ROI.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                mPMA_ROI.Interactive = true;
                mPMA_ROI.CenterX = mCogRecordDisplay.PanX;
                mPMA_ROI.CenterY = mCogRecordDisplay.PanY;
                mPMA_ROI.SideXLength = 100;
                mPMA_ROI.SideYLength = 100;
                mPMA_Tool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;
                mPMA_Tool.Pattern.TrainImage = (CogImage8Grey)mCogRecordDisplay.Image;
                mCogRecordDisplay.InteractiveGraphics.Add(mPMA_ROI, "PMA_ROI_Area", false);//在影像上加入教讀框
        
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("PMATool ROI_Create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Train()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            
            mPMA_Tool.Pattern.Origin.TranslationX = mPMA_ROI.CenterX;
            mPMA_Tool.Pattern.Origin.TranslationY = mPMA_ROI.CenterY;
            mPMA_Tool.Pattern.TrainRegion = mPMA_ROI;

            mPMA_Tool.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
            mPMA_Tool.RunParams.ZoneAngle.Low = CogMisc.DegToRad(-5);
            mPMA_Tool.RunParams.ZoneAngle.High = CogMisc.DegToRad(5);
            mPMA_Tool.RunParams.AcceptThreshold = 0.3;
            mPMA_Tool.RunParams.SaveMatchInfo = true;
            mPMA_Tool.RunParams.ApproximateNumberToFind = 1;
            mPMA_Tool.RunParams.ZoneScale.Configuration = CogPMAlignZoneConstants.LowHigh;
            mPMA_Tool.RunParams.ZoneScale.Low = 0.9;
            mPMA_Tool.RunParams.ZoneScale.High = 1.1;
            mPMA_Tool.RunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
            mPMA_Tool.RunParams.ScoreUsingClutter = false;
            
            try
            {
                mPMA_Tool.Pattern.Train();
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("PMATool Train Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Run(CogRecordDisplay mCogRecordDisplay) 
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mPMA_Tool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;
                mPMA_Tool.Run();
                mCogRecordDisplay.Record = mPMA_Tool.CreateLastRunRecord().SubRecords["InputImage"];
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("PMATool Run Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean SaveToVPPFile(string FileName)//檔案參數儲存
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //建立目錄資料夾
                string strFolderPath = @"D:\VPS_File\Product\LineMaxTool\" + @FileName + @"\";
                DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                //塞到CogTool裡面
                CogToolBlock ToolBlock1 = new CogToolBlock();

                mPMA_Tool.Name = FileName + "_LineMaxTool_";

                ToolBlock1.Tools.Add(mPMA_Tool);

                FileName = strFolderPath + FileName + "_LMT.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of LineMaxTool Saved : " + FileName);
                ToolBlock1 = null;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save LineMaxTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean LoadFromVPPFile(string FileName, CogRecordDisplay mCogRecordDisplay)//檔案參數載入
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            string TempFileName = (string)FileName;

            try
            {
                //從CogTool裡面讀出來
                string strFolderPath = @"D:\VPS_File\Product\PMA_Tool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_PMA.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mPMA_Tool = (CogPMAlignTool)ToolBlock1.Tools[TempFileName + "_PMA_Tool_"];
                this.ROI_Create(mCogRecordDisplay);

                SaveLog.Msg_("Data of PMA_Tool Loaded : " + @FileName);
                ToolBlock1 = null;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Load PMA_Tool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\PMA_Tool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_PMA.vpp";

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
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                return true;
            }
        }
    }
}
