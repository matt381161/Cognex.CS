using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro;

namespace findlinetool
{
    class iLineMaxTool
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱
        private CogLineMaxTool mLineMaxTool;
        private CogRectangleAffine mLineMaxTool_ROI;
        private bool mLineMaxTool_status;
        private CogLine[] iLine;
        private CogLineSegment[] iLineSegment;

        public bool Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mLineMaxTool = null;
                mLineMaxTool = new CogLineMaxTool();

                mLineMaxTool_ROI = null;
                mLineMaxTool_ROI = new CogRectangleAffine();

                mLineMaxTool_status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("LineMaxTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mLineMaxTool = null;

                mLineMaxTool_ROI = null;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("LineMaxTool unLoad Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool ROI_Create(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mLineMaxTool_ROI.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                mLineMaxTool_ROI.Interactive = true;
                mLineMaxTool_ROI.CenterX = mCogRecordDisplay.PanX;
                mLineMaxTool_ROI.CenterY = mCogRecordDisplay.PanY;
                mLineMaxTool_ROI.SideXLength = 100;
                mLineMaxTool_ROI.SideYLength = 100;
                mLineMaxTool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;
                //mCogRecordDisplay.InteractiveGraphics.Add(mLineMaxTool_ROI, "LineMAx_ROI_Area", false);//在影像上加入教讀框
                
                mCogRecordDisplay.Record = mLineMaxTool.CreateCurrentRecord().SubRecords["InputImage"];

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("LineMaxTool ROI_Create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Run(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mLineMaxTool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;
                mLineMaxTool.Run();
                mCogRecordDisplay.Record = mLineMaxTool.CreateLastRunRecord().SubRecords["InputImage"];
                mLineMaxTool_status = true;

                if (mLineMaxTool.Results != null)
                {
                    iLine = new CogLine[mLineMaxTool.Results.Count];
                    iLineSegment = new CogLineSegment[mLineMaxTool.Results.Count];

                    for(int i=0;i<mLineMaxTool.Results.Count;i++)
                    {
                        iLine[i] = mLineMaxTool.Results[i].GetLine();
                        iLineSegment[i] = mLineMaxTool.Results[i].GetLineSegment();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("LineMaxTool Run Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mLineMaxTool_status = false;
                return false;
            }
        }

        public CogLine[] getLine()
        {
            if(mLineMaxTool_status)
            return iLine;

            return null;
        }

        public CogLineSegment[] getLineSegment()
        {
            if (mLineMaxTool_status)
            return iLineSegment;

            return null;
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

                mLineMaxTool.Name = FileName + "_LineMaxTool_";

                ToolBlock1.Tools.Add(mLineMaxTool);

                FileName = strFolderPath + FileName + "_LMT.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of LineMaxTool Saved : " + FileName);
                ToolBlock1 = null;
                mLineMaxTool_status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save LineMaxTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mLineMaxTool_status = false;
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
                string strFolderPath = @"D:\VPS_File\Product\LineMaxTool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_LMT.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mLineMaxTool = (CogLineMaxTool)ToolBlock1.Tools[TempFileName + "_LineMaxTool_"];
                this.ROI_Create(mCogRecordDisplay);

                SaveLog.Msg_("Data of LineMaxTool Loaded : " + @FileName);
                ToolBlock1 = null;

                mLineMaxTool_status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Load LineMaxTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mLineMaxTool_status = false;
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\LineMaxTool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_LMT.vpp";

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
                mLineMaxTool_status = false;
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                return true;
            }
        }
    }
}
