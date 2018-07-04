using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.OCRMax;
using System.IO;
using Cognex.VisionPro.ToolBlock;

namespace findlinetool
{
    class iOCRMaxTool
    {
        private string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱

        private CogOCRMaxTool mOCRMaxTool;
        private CogRectangleAffine mOCR_ROI;
        private bool mOCRMaxTool_Status;

        public bool Load()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mOCRMaxTool = null;
                mOCRMaxTool = new CogOCRMaxTool();

                mOCR_ROI = null;
                mOCR_ROI = new CogRectangleAffine();

                mOCRMaxTool_Status = false;

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("OCRMaxTool Load Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool unLoad()
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mOCRMaxTool = null;

                mOCR_ROI = null;

                mOCRMaxTool_Status = false;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("OCRMaxTool unLoad Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool ROI_Create(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            
            try
            {
                Double CenterX = 107.25;
                Double CenterY = 89.25;
                Double XLength = 107.5;
                Double YLength = 89.5;

                mOCR_ROI.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                mOCR_ROI.Interactive = true;
                mOCR_ROI.CenterX = CenterX;
                mOCR_ROI.CenterY = CenterY;
                mOCR_ROI.SideXLength = XLength;
                mOCR_ROI.SideYLength = YLength;
                mCogRecordDisplay.InteractiveGraphics.Add(mOCR_ROI, "ID_ROI_Area", false);//在影像上加入教讀框

                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("OCRMaxTool Teach Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Teach(CogRecordDisplay mCogRecordDisplay , String text)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mOCRMaxTool.Region = mOCR_ROI;
                mOCRMaxTool.InputImage = (CogImage8Grey)mCogRecordDisplay.Image;

                CogOCRMaxSegmenterParagraphResult aSegmenterParagraphResult = null;
                try
                {
                    aSegmenterParagraphResult = mOCRMaxTool.Segmenter.Execute((CogImage8Grey)mCogRecordDisplay.Image, mOCR_ROI);
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_("OCRMaxTool Teach Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                    return false;
                }
                CogOCRMaxSegmenterLineResult aSegmenterLineResult = aSegmenterParagraphResult[0];

                int iC = 0;
                char[] fontString = text.ToArray();
                
                foreach (CogOCRMaxSegmenterPositionResult aSegmenterPositionResult in aSegmenterLineResult)
                {
                    CogOCRMaxChar aC = aSegmenterPositionResult.Character;
                    aC.CharacterCode = fontString[iC];
                    mOCRMaxTool.Classifier.Font.Add(aC);
                    iC++;
                }

                try
                {
                    mOCRMaxTool.Classifier.Train();
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_("OCRMaxTool Teach Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                    return false;
                }

                if (!mOCRMaxTool.Classifier.Trained)
                {
                    SaveLog.Msg_("Could not train classifier.");
                    return false;
                }

                mOCRMaxTool.FieldingEnabled = true;
                CogOCRMaxFieldingDefinition aFD = mOCRMaxTool.Fielding.FieldingDefinitions['A'];
                mOCRMaxTool.Fielding.FieldString = text;

                SaveLog.Msg_("OCRMaxTool Teach OK! Text = " + mOCRMaxTool.Fielding.FieldString.ToString());
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("OCRMaxTool Teach Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
        }

        public bool Run(CogRecordDisplay mCogRecordDisplay)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                mOCRMaxTool.Run();

                ICogRunStatus aRunStatus = mOCRMaxTool.RunStatus;

                if (aRunStatus.Result == CogToolResultConstants.Error)
                {
                    SaveLog.Msg_("Error running CogOCRMaxTool.");
                    mOCRMaxTool_Status = false;
                    return false;
                }
                else
                {
                    mCogRecordDisplay.Record = mOCRMaxTool.CreateLastRunRecord().SubRecords["InputImage"];
                    SaveLog.Msg_(mOCRMaxTool.LineResult.ResultString);
                    mOCRMaxTool_Status = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("OCRMaxTool Run Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mOCRMaxTool_Status = false;
                return false;
            }
        }

        public bool get_Status()
        {
            return mOCRMaxTool_Status;
        }

        public Boolean SaveToVPPFile(string FileName)//檔案參數儲存
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //建立目錄資料夾
                string strFolderPath = @"D:\VPS_File\Product\OCRMaxTool\" + @FileName + @"\";
                DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                if (DIFO.Exists != true)
                {
                    DIFO.Create();
                }

                //塞到CogTool裡面
                CogToolBlock ToolBlock1 = new CogToolBlock();

                mOCRMaxTool.Name = FileName + "_OCRMaxTool_";

                ToolBlock1.Tools.Add(mOCRMaxTool);

                FileName = strFolderPath + FileName + "_OCR.vpp";

                //有使用到定位跟隨的時候不能存成最壓縮的檔案
                //CogSerializer.SaveObjectToFile(ToolBlock1, @FileName, typeof(BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                CogSerializer.SaveObjectToFile(ToolBlock1, @FileName);

                SaveLog.Msg_("Data of OCRMaxTool Saved : " + FileName);
                ToolBlock1 = null;
                mOCRMaxTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Save OCRMaxTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mOCRMaxTool_Status = false;
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
                string strFolderPath = @"D:\VPS_File\Product\OCRMaxTool\" + @FileName + @"\";
                CogToolBlock ToolBlock1 = new CogToolBlock();

                FileName = strFolderPath + FileName + "_OCR.vpp";

                ToolBlock1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(FileName);//開啟ToolBlock vpp檔案

                //依序載入
                mOCRMaxTool = (CogOCRMaxTool)ToolBlock1.Tools[TempFileName + "_OCRMaxTool_"];
                this.ROI_Create(mCogRecordDisplay);

                SaveLog.Msg_("Data of OCRMaxTool Loaded : " + @FileName);
                ToolBlock1 = null;

                mOCRMaxTool_Status = true;
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_("Load OCRMaxTool Data Failed : " + ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                mOCRMaxTool_Status = false;
                return false;
            }
        }

        public Boolean CheckVPPFile(string FileName)//檢查檔案是否存在
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                //檢查路徑檔案是否存在
                string strFolderPath = @"D:\VPS_File\Product\OCRMaxTool\" + @FileName + @"\";
                FileName = strFolderPath + FileName + "_OCR.vpp";

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
                mOCRMaxTool_Status = false;
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                return true;
            }
        }
    }
}
