using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;//存檔

using Cognex.VisionPro.FGGigE;//GIGE 取像函式
using Cognex.VisionPro;//灰階顯示列工具
using Cognex.VisionPro.PMAlign;//檢測函式
using Cognex.VisionPro.PatInspect;//檢測函式
using Cognex.VisionPro.Blob;//檢測函式
using Cognex.VisionPro.ImageProcessing;//檢測函式
using Cognex.VisionPro.ImageFile;//開影像函式
using Cognex.VisionPro.Display;
using System.Threading;//執行緒
using Cognex.VisionPro.ID;//2D Code功能
using Cognex.VisionPro.Dimensioning;//劃線功能

using System.Windows.Forms;//textbox
using System.Drawing;//Color


namespace findlinetool
{
    public static class Vision_Basic
    {
        private static string ModularID = System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.ToString();//進入點名稱

        private const Int32 TotalCameras = 2;//設定上CCD有幾隻相機 幾個教讀框

        public enum VisionCameras : uint//相機代號
        {
            Cameras0 = 0,
            Cameras1 = 1,
        }

        #region 基本參數
        private static String[] CCDNumber_VideoFormat = new string[TotalCameras] { string.Empty, string.Empty };//影像格式
        private static CogFrameGrabberGigEs CogFrameGrabberGigEs_TopCameras_Tool;//CCD實體相機個數
        private static ICogAcqFifo[] ICogAcqFifo_CCDNumber_AcqFifo_Tool = new ICogAcqFifo[TotalCameras];//CCD實體相機串流口 預設2台
        private static Boolean[] CCDNumber_Load_ = new bool[TotalCameras] { false, false };//上CCD1是否載入成功 預設2台
        private static Int32 CCDNumber_GrabOut_ = 0;//上CCD1單張取像的回傳結果
        private static bool[] CCDNumber_LiveFleg_ = new bool[TotalCameras] { false, false };//上CCD1連續取像旗標
        private static Int32[] CCDNumber_ = new Int32[TotalCameras] { 0, 1 };//CCD實體相機編號
        #endregion 基本參數

        #region 開起、儲存 參數
        private static CogImageFileTool CogImageFileTool_Tool = new CogImageFileTool();//CogImage開影像存影像
        #endregion 開起、儲存 參數

        #region CogDisplay元件
        public static CogRecordDisplay CogDisplay_Public;//公開的CogDisplay控制項
        #endregion CogDisplay元件
        
        /// <summary>
        /// 載入CCD
        /// </summary>
        /// <param name="TempcogDisplay">CCDDisplay畫面</param>
        /// <param name="TempCameras">CCD實體相機個數</param>
        /// <returns>回傳是否成功開啟CCD</returns>
        public static Boolean Load_Cameras(VisionCameras TempCameras)//載入CCD
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            #region 開啟相機
            CCDNumber_Load_[(Int32)TempCameras] = false;
            ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras] = null;
            try
            {
                CogFrameGrabberGigEs_TopCameras_Tool = new CogFrameGrabberGigEs();
                if (CogFrameGrabberGigEs_TopCameras_Tool.Count > 0)//有找到一台以上的相機
                {
                    CogFrameGrabberGigEs_TopCameras_Tool = new CogFrameGrabberGigEs();//宣告CogFrameGrabberGigEs
                    //中斷連線
                    CogFrameGrabberGigEs_TopCameras_Tool[CCDNumber_[(Int32)TempCameras]].Disconnect(true);
                    //設定影像格式
                    CCDNumber_VideoFormat[0] = CogFrameGrabberGigEs_TopCameras_Tool[CCDNumber_[(Int32)TempCameras]].AvailableVideoFormats[0];
                    //取得相機控制權
                    ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras] = CogFrameGrabberGigEs_TopCameras_Tool[CCDNumber_[(Int32)TempCameras]].CreateAcqFifo(CCDNumber_VideoFormat[0], CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);
                    //載入成功
                    CCDNumber_Load_[(Int32)TempCameras] = true;
                    CogFrameGrabberGigEs_TopCameras_Tool = null;//釋放CogFrameGrabberGigEs

                    SaveLog.Msg_("開起相機 " + TempCameras.ToString() + " 完成");
                    return true;
                }
                else
                {
                    //載入失敗
                    CCDNumber_Load_[(Int32)TempCameras] = false;
                    SaveLog.Msg_("開起相機 " + TempCameras.ToString() + " 失敗");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                if (CogFrameGrabberGigEs_TopCameras_Tool != null) CogFrameGrabberGigEs_TopCameras_Tool = null;//釋放CogFrameGrabberGigEs
                CCDNumber_Load_[(Int32)TempCameras] = false;
                return false;
            }
            #endregion 開啟相機

        }//載入CCD結束
        
        /// <summary>
        /// 卸載CCD
        /// </summary>
        /// <param name="TempCameras">CCD實體相機個數</param>
        /// <returns>回傳是否成功卸載CCD</returns>
        public static Boolean UnLoad_Cameras(VisionCameras TempCameras)//卸載CCD
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            #region 卸載CCD

            try
            {
                if (CCDNumber_Load_[(Int32)TempCameras])//如果相機有載入
                {
                    CogFrameGrabberGigEs_TopCameras_Tool = new CogFrameGrabberGigEs();//宣告CogFrameGrabberGigEs
                    if (CogFrameGrabberGigEs_TopCameras_Tool.Count > 0)
                    {
                        if (ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras] != null)//有取得控制權
                        {
                            CogFrameGrabberGigEs_TopCameras_Tool[CCDNumber_[(Int32)TempCameras]].Disconnect(false);//中斷連結
                            CCDNumber_Load_[(Int32)TempCameras] = false;//更改載入狀態
                            CCDNumber_VideoFormat = null;//釋放記憶體
                            ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras] = null;//釋放記憶體
                        }
                    }
                    CogFrameGrabberGigEs_TopCameras_Tool = null;//釋放CogFrameGrabberGigEs
                }

                SaveLog.Msg_("卸載相機 " + TempCameras.ToString());
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                CCDNumber_Load_[(Int32)TempCameras] = false;
                SaveLog.Msg_("卸載相機失敗 " + TempCameras.ToString());
                return false;
            }
            #endregion 卸載CCD
        }//卸載CCD結束
        
        /// <summary>
        /// 調整曝光值
        /// </summary>
        /// <param name="TempCameras">指定相機</param>
        /// <param name="Exposure">給定曝光值</param>
        /// <returns>設定成功與否</returns>
        public static Boolean CCD_Exposure_Function(VisionCameras TempCameras, double Exposure)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            try
            {
                // Get a reference to the ExposureParams interface of the AcqFifo.
                ICogAcqExposure ExposureParams = ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].OwnedExposureParams;
                ICogAcqBrightness BrightnessParams = ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].OwnedBrightnessParams;
                ICogAcqContrast ContrastParams = ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].OwnedContrastParams;
                // Always check to see an "Owned" property is supported
                // before using it.
                if (ExposureParams != null)  // Check for exposure support.
                {
                    BrightnessParams.Brightness = 0.5;
                    ContrastParams.Contrast = 0;
                    ExposureParams.Exposure = Exposure;  // sets ExposureTimeAbs
                    ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].Prepare();  // writes the properties to the camera.
                }

                SaveLog.Msg_("調整曝光值 " + TempCameras.ToString() + " :" + Exposure.ToString());
                return true;
            }
            catch (Exception ex)
            {
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                SaveLog.Msg_("調整曝光值 " + TempCameras.ToString() + " :" + Exposure.ToString() + " 失敗");
                return false;
            }
        }

        /// <summary>
        /// CCD單張取像
        /// </summary>
        /// <param name="TempcogDisplay">CCDDisplay畫面</param>
        /// <param name="TempCameras">CCD實體相機個數</param>
        /// <returns>回傳是否成功單張取像</returns>
        public static Boolean Grab_Image_Function(Cognex.VisionPro.CogRecordDisplay TempCCDDisplay, VisionCameras TempCameras)//CCD單張取像
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            #region 單張取像

            if (CCDNumber_Load_[(Int32)TempCameras])//如果CCD有初始化成功
            {
                try
                {
                    TempCCDDisplay.StaticGraphics.Clear();
                    TempCCDDisplay.InteractiveGraphics.Clear();
                    TempCCDDisplay.Image = (Cognex.VisionPro.CogImage8Grey)ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].Acquire(out CCDNumber_GrabOut_);//單張取像                

                    SaveLog.Msg_("單張取像 " + TempCameras.ToString());
                    return true;
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                    SaveLog.Msg_("單張取像失敗 " + TempCameras.ToString());
                    return false;
                }
            }
            else
            {
                SaveLog.Msg_("未載入相機 " + TempCameras.ToString());
                return true;
            }
            #endregion 單張取像
        }//CCD單張取像結束
        
        /// <summary>
        /// CCD連續取像
        /// </summary>
        /// <param name="TempCCDDisplay">CCDDisplay畫面</param>
        /// <param name="TempCCDAcqFifo">CCD實體相機串流口</param>
        /// <param name="CCD_Load">CCD是否成功載入旗標</param>
        /// <returns>回傳是否成功連續取像</returns>
        public static Boolean Live_Image_Function(Cognex.VisionPro.CogRecordDisplay TempCCDDisplay, VisionCameras TempCameras)//CCD連續取像
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            #region 連續取像
            if (CCDNumber_Load_[(Int32)TempCameras])//如果Camera有初始化成功
            {
                try
                {
                    if (CCDNumber_LiveFleg_[(Int32)TempCameras] == false)
                    {
                        //取像中
                        TempCCDDisplay.StaticGraphics.Clear();
                        TempCCDDisplay.InteractiveGraphics.Clear();//清空影像
                        TempCCDDisplay.StartLiveDisplay(ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras]);//建立影像fifo與Camers連結關聯
                        CCDNumber_LiveFleg_[(Int32)TempCameras] = true;

                        SaveLog.Msg_("取像" + TempCameras.ToString());
                    }
                    else
                    {
                        //靜止取像
                        CCDNumber_LiveFleg_[(Int32)TempCameras] = false;
                        TempCCDDisplay.StopLiveDisplay();
                        //單張取像
                        TempCCDDisplay.Image = (Cognex.VisionPro.CogImage8Grey)ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].Acquire(out CCDNumber_GrabOut_);
                       
                        SaveLog.Msg_("停止取像" + TempCameras.ToString());
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                    CCDNumber_LiveFleg_[(Int32)TempCameras] = false;
                    return false;
                }
            }
            else
            {
                return false;
            }

            #endregion 連續取像
        }//CCD連續取像結束

        public static Boolean Get_Cameras_Live_Status(VisionCameras TempCameras)
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            if (CCDNumber_Load_[(Int32)TempCameras])//如果Camera有初始化成功
            {
                try
                {
                    return CCDNumber_LiveFleg_[(Int32)TempCameras];
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// CCD停止取像
        /// </summary>
        /// <param name="TempCCDDisplay">CCDDisplay畫面</param>
        /// <param name="TempCCDAcqFifo">CCD實體相機串流口</param>
        /// <param name="CCD_Load">CCD是否成功載入旗標</param>
        /// <returns>回傳是否成功停止取像</returns>
        public static Boolean StopLive_Image_Function(Cognex.VisionPro.CogRecordDisplay TempCCDDisplay, VisionCameras TempCameras)//CCD停止取像
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            #region 停止取像

            Int32 TempCameras_ = (int)TempCameras;

            if (CCDNumber_Load_[(Int32)TempCameras])//如果Camera有初始化成功
            {
                try
                {
                    CCDNumber_LiveFleg_[TempCameras_] = false;
                    
                    //停止取像
                    TempCCDDisplay.StopLiveDisplay();
                    
                    //單張取像
                    TempCCDDisplay.Image = (Cognex.VisionPro.CogImage8Grey)ICogAcqFifo_CCDNumber_AcqFifo_Tool[(Int32)TempCameras].Acquire(out CCDNumber_GrabOut_);
                       
                    Thread.Sleep(50);

                    SaveLog.Msg_("停止取像" + TempCameras.ToString());
                    return true;
                }
                catch (Exception ex)
                {
                    SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());

                    CCDNumber_LiveFleg_[TempCameras_] = false;
                    return false;
                }
            }
            else
            {
                return false;
            }

            #endregion 停止取像
        }//CCD停止取像結束
        
        /// <summary>
        /// 開啟影像檔
        /// </summary>
        /// <param name="TempCCDDisplay">CCDDisplay畫面</param>
        /// <param name="FilePath">路徑</param>
        /// <returns>回傳是否開啟影像成功</returns>
        public static Boolean Image_Open_Function(Cognex.VisionPro.CogRecordDisplay TempCCDDisplay, string FilePath)//開啟影像檔
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();

            #region 開啟影像檔
            try
            {
                //Cognex開起影像檔
                CogImageFileTool_Tool.Operator.Open(FilePath, CogImageFileModeConstants.Read);
                CogImageFileTool_Tool.Run();
                TempCCDDisplay.Image = (Cognex.VisionPro.CogImage8Grey)CogImageFileTool_Tool.OutputImage;

                //清除畫面調整大小
                TempCCDDisplay.Fit();
                TempCCDDisplay.StaticGraphics.Clear();
                TempCCDDisplay.InteractiveGraphics.Clear();

                SaveLog.Msg_("開啟影像 " + FilePath);
                return true;
            }

            catch (Exception ex)
            {
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }


            #endregion 開啟影像檔
        }//開啟影像檔結束
        
        /// <summary>
        /// 儲存影像檔
        /// </summary>
        /// <param name="TempCCDDisplay">CCDDisplay畫面</param>
        /// <param name="FilePath">路徑</param>
        /// <returns>回傳是否儲存影像成功</returns>
        public static Boolean Image_Save_Function(Cognex.VisionPro.CogRecordDisplay TempCCDDisplay, string FilePath)//儲存影像檔
        {
            string ProcID = System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString();
            #region 儲存影像檔
            try
            {
                if (TempCCDDisplay.Image != null)
                {
                    string file_name = Path.GetFileNameWithoutExtension(FilePath);//FilePath
                    string file_path = Path.GetDirectoryName(FilePath);//FileName


                    CogImageFileTool_Tool.InputImage = TempCCDDisplay.Image;
                    string s = file_path + "\\" + file_name + ".bmp";
                    CogImageFileTool_Tool.Operator.Open(s, CogImageFileModeConstants.Write);


                    CogImageFileTool_Tool.Run();
                    CogImageFileTool_Tool.Operator.Close();


                    SaveLog.Msg_("儲存影像 : " + s);
                    return true;
                }
                else
                {
                    SaveLog.Msg_("影像無內容無法儲存");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SaveLog.Msg_(ModularID + ":\r\n" + ProcID + ":\r\n" + ex.ToString());
                return false;
            }
            #endregion 儲存影像檔
        }//儲存影像檔結束


    }
}
