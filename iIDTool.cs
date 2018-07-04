using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cognex.VisionPro;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.ToolBlock;

namespace findlinetool
{
    class iIDTool
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱
        private CogIDTool mIDTool;
        private CogRectangleAffine mID_ROI;
        private String mDecodedData;
        private bool mIDTool_Status;

        public bool Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                mIDTool = null;
                mIDTool = new CogIDTool();

                mID_ROI = null;
                mID_ROI = new CogRectangleAffine();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("IDTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                mIDTool = null;
                mID_ROI = null;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("IDTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool ROI_Create(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                Double CenterX = 111.618;
                Double CenterY = 109.769;
                Double XLength = 157.605;
                Double YLength = 157.605;

                mID_ROI.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                mID_ROI.Interactive = true;
                mID_ROI.CenterX = CenterX;
                mID_ROI.CenterY = CenterY;
                mID_ROI.SideXLength = XLength;
                mID_ROI.SideYLength = YLength;
                mCogRecordDisplay.InteractiveGraphics.Add(mID_ROI, "ID_ROI_Area", false);//在影像上加入教讀框

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("IDTool ROI_Create Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Run(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            try
            {
                mIDTool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;
                mIDTool.RunParams.DisableAllCodes();
                mIDTool.RunParams.QRCode.Enabled = true;
                mIDTool.Region = mID_ROI;

                mIDTool.Run();

                if (mIDTool.RunStatus.Result == CogToolResultConstants.Accept && mIDTool.Results.Count > 0)
                {
                    mCogRecordDisplay.StaticGraphics.Add(mIDTool.Results[0].CreateResultGraphics(CogIDResultGraphicConstants.All), "Result");

                    mIDTool_Status = true;
                    mDecodedData = mIDTool.Results[0].DecodedData.DecodedString;
                    SaveLog.Msg_("2D Code : " + mIDTool.Results[0].DecodedData.DecodedString);
                    return true;
                }
                else
                {
                    mDecodedData = "NG";
                    SaveLog.Msg_("讀取2D Code失敗。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("IDTool Run Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public Boolean SaveToVPPFile(string FileName)//檔案參數儲存
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //建立目錄資料夾
                string strFolderPath = @"D:\VPS_File\Product\IDTool\" + @FileName + @"\";
                DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                //塞到CogTool裡面
                CogToolBlock ToolBlock1 = new CogToolBlock();

                mIDTool.Name = FileName + "_IDTool_";

                ToolBlock1.Tools.Add(mIDTool);

                FileName = strFolderPath + FileName + "_IDT.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of IDTool Saved : " + FileName);
                ToolBlock1 = null;
                mIDTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save IDTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mIDTool_Status = false;
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
                string strFolderPath = @"D:\VPS_File\Product\IDTool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_IDT.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mIDTool = (CogIDTool)ToolBlock1.Tools[TempFileName + "_IDTool_"];
                this.ROI_Create(mCogRecordDisplay);

                SaveLog.Msg_("Data of IDTool Loaded : " + @FileName);
                ToolBlock1 = null;

                mIDTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Load IDTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mIDTool_Status = false;
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\IDTool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_IDT.vpp";

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
                mIDTool_Status = false;
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                
                return true;
            }
        }

        public bool get_Status()
        {
            return mIDTool_Status;
        }

        public string get_DecodedString()
        {
            return mDecodedData;
        }
    }
}
