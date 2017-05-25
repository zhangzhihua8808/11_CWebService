using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Net;
using System.IO;

namespace WebService
{
    /// <summary>
    /// Service1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        /// <summary>
        /// 完成服务【读】的功能测试
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public String Http_ConnectTest()
        {
            return "XD_456";
        }
        
        /// <summary>
        /// 创建一个目录 
        /// </summary>
        /// <param name="aDirName">格式：aaa\\bbb</param>
        /// <returns>存在或生成成功，返回true,否则false</returns>
        [WebMethod]
        public bool  Http_CheckAndMakeDir(string aDirName)
        {
            bool status = false;
            
            String aRoot = Server.MapPath("./");
            String aDir = aRoot + aDirName;

            try
            {
                if (Directory.Exists(aDir) == false)//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(aDir);
                }

                return Directory.Exists(aDir)==true;
            }
            catch (Exception ex)
            {
                status = false;                
            }

            return status;
        }

         /// <summary>
        /// 获得在服务器端的路径
        /// </summary>
        /// <param name="path">path:aaa\\bbb</param>
        /// <returns></returns>
        private String Http_GetServicePath(string path)
        {
            String aRoot = Server.MapPath("./");
            return aRoot + path;
        }

        /// <summary>
        /// 分块方式传输文件
        /// </summary>
        /// <param name="path">在服务器端的路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fileContent">文件内容</param>
        /// <param name="contentLength">内容长度</param>
        /// <param name="resume">true:不创建，false:创建</param>
        /// <returns></returns>
        [WebMethod]
        public bool Http_UploadFile_Chunk(string path,string fileName, byte[] fileContent,int contentLength, bool resume)
        {
            if (fileContent.Length <= 0) return false;
            
            bool flag = false;            
            try
            {
                //检查目录
                if (Http_CheckAndMakeDir(path) == false)
                {
                    return flag;
                }

                String aPath = Http_GetServicePath(path);  //Windows Path
                String aFullFileName = aPath + "\\" + fileName;
                if (resume) //补充
                {
                    using (FileStream fs = File.Open(aFullFileName, FileMode.Append))
                    {
                        fs.Write(fileContent, 0, contentLength);                        
                        fs.Close();                        
                    }
                }
                else
                {
                    using (FileStream fs = File.Open(aFullFileName,FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Write(fileContent, 0, contentLength);                        
                        fs.Close();
                    }                    
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;          
        } 


        /// <summary>
        /// 上传文件：适用于文件小于4M的时候
        /// </summary>
        /// <param name="fs">数据流</param>
        /// <param name="path">服务器端的路径,用于判断路径是否存在</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        [WebMethod]
        public  bool Http_UpLoadFile(byte[] fs, String path, String fileName)
        {
            if (fs.Length <= 0) return false;
            
            bool flag = false;
            try
            {
                if (Http_CheckAndMakeDir(path) == false)
                {
                    return false;
                }
                
                String aPath = Http_GetServicePath(path);  //Windows Path
                using (FileStream f = new FileStream(aPath + "\\" + fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    f.Write(fs, 0, fs.Length);
                    f.Flush();
                    f.Close();
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
    } 
     
        //FormFTPTool->FTP_UpLoadFile(aDestFileName,"Publish\\Word\\"+ ManualProject->InsProjectNo+"\\"+aFileName);

        /*
        //下载文件到本地
void TFormFTPTool::FTP_DownLoadFile( String aServerFileName, String aDestFileName )
{
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return;
    }

    CheckAndMakeDirectory( ExtractFilePath( aDestFileName ) );
    aServerFileName = ReplaceStrByUnix(gFTP->IsUnix, aServerFileName );
    bool succ = gFTP->FTPDownLoad(aServerFileName,aDestFileName);
    if(!succ)
      gError->Warn("文件下载失败！");
}

//上传文件到FTP
void TFormFTPTool::FTP_UpLoadFile(String aFileName, String aServerDir )
{
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return;
    }

    FTP_MakeDirectory( ExtractFilePath( aServerDir ) );

    //aServerDir = ReplaceStrByUnix(gFTP->IsUnix,aServerDir);
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return;
    }

    bool succ = gFTP->FTPUpload(aFileName,aServerDir);
    if(!succ)
      gError->Warn("文件上传失败！");
}

bool TFormFTPTool::FTP_MakeDirectory(String aFullDir )
{
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return false;
    }
    return gFTP->FTPCreateDir(aFullDir);
}


//在一个服务器目录中上传多个文件
void TFormFTPTool::FTP_UpLoadFiles(TStrings* aFileNames, String aServerDir )
{
  gError->HeadTitle = "上传文件";

  for( int i=0; i<aFileNames->Count; i++ )
  {
    String aFileName = aFileNames->Strings[i] ;
    gError->Warn1( IntToStr(i) + "/" + IntToStr(aFileNames->Count) + "!" + aFileName );

    FTP_UpLoadFile(aFileName, aServerDir );
  }
}

//目录到目录的上传
bool TFormFTPTool::FTP_UpLoadDir( String aLocalDir, String aFTPDir )
{
    if(!gFTP->FTPConnect())
    {
       gError->Warn("连接失败！");
        return false;
    }

    bool succ = gFTP->FTPUploadDir( aLocalDir,aFTPDir );
    if(!succ)
       gError->Warn("文件上传失败！");
    return succ;
}


bool TFormFTPTool::FTP_DownLoadDir( String aFTPDir, String aLocalDir, bool IsDeleteOld )
{
    aFTPDir = FormatDir(aFTPDir);
    aLocalDir = FormatDir(aLocalDir);

    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return false;
    }
    bool succ = gFTP->FTPDownLoadDir( aFTPDir,aLocalDir, IsDeleteOld );
    if(!succ)
       gError->Warn("文件下载失败！");
    return succ;
}


//服务器中目录是否存在
bool TFormFTPTool::FTP_DirExists(String aDir )
{
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return false;
    }
    return gFTP->FTPDirExists(aDir);
}

//一个目录下的子文件夹
TStrings* TFormFTPTool::FTP_GetSubDirectory(String aDir, bool IsRecursive )
{
   if(!gFTP->FTPConnect())
   {
     gError->Warn("连接失败！");
     return NULL;
   }
   return gFTP->FTPGetSubDirs(aDir,false);
}


//获得目录中所有的文件
void TFormFTPTool::FTP_GetFiles( String aDir, TStrings* aFileNames, TStrings* aFileDateTimes, TStrings* aFileSizes )
{
   if(!gFTP->FTPConnect())
   {
     gError->Warn("连接失败！");
     return;
   }
   gFTP->FTPGetSubFiles( aDir,aFileNames,aFileDateTimes,aFileSizes);
}


TStrings* TFormFTPTool::FTP_GetFiles( String aDir )
{
  TStrings* aFileNames = new TStringList;
  TStrings* aFileDateTimes = new TStringList;
  TStrings* aFileSizes = new TStringList;

  FTP_GetFiles( aDir, aFileNames, aFileDateTimes, aFileSizes );

  delete aFileDateTimes;
  delete aFileSizes;

  return aFileNames;
}

bool TFormFTPTool::IsFTPFileName(String aFileName)
{
   return IsPreFix( aFileName, "@FTP(" );
}
//---------------------------------------------------------------------------

bool TFormFTPTool::MaskFileExists(String aFileName)
{
  if( IsFTPFileName(aFileName)  == true )
    return FTP_FileExists(aFileName);
  else
    return FileExists(aFileName);
}

bool TFormFTPTool::FTP_FileExists( String aFileName )
{
    if(!gFTP->FTPConnect())
    {
        gError->Warn("连接失败！");
        return false;
    }
    return gFTP->FTPDirExists(aFileName);
}

bool TFormFTPTool::FTP_DeleteFile(String aFileName  )
{
   if(!gFTP->FTPConnect())
   {
     gError->Warn("连接失败！");
     return false;
   }
   aFileName = ReplaceStrByUnix(gFTP->IsUnix,aFileName);
   return gFTP->FTPDeleteFile(aFileName);
}

bool TFormFTPTool::FTP_DeleteDir( String aPathName  )
{
   if(!gFTP->FTPConnect())
   {
     gError->Warn("连接失败！");
     return false;
   }
   aPathName = ReplaceStrByUnix(gFTP->IsUnix,aPathName);
   return gFTP->FTPDeleteDir(aPathName);
}


void TFormFTPTool::FTP_OpenDirFiles_ListView( TListView* aListView, String aDir )
{
  if( aListView==NULL )
    return;

   TStrings* aFileNames  = new TStringList;
   TStrings* aFileDateTimes = new TStringList;
   TStrings* aFileSizes = new TStringList;

   FTP_GetFiles(aDir, aFileNames, aFileDateTimes, aFileSizes );

   ListView_InitForGrid(aListView);
   ListView_AddTitles( aListView, "文件名;日期;大小", "200;150;0", ";" );

   for( int i=0; i<aFileNames->Count; i++ )
   {
     String aFileName = aFileNames->Strings[i];
     String aFileDateTime = aFileDateTimes->Strings[i];
     String aFileSize = aFileSizes->Strings[i];

     String aStrAll = aFileName + "@@" + aFileDateTime + "@@" + aFileSize;
     ListView_AddItems( aListView, aStrAll, "@@" , NULL );
   }

   delete aFileNames;
   delete aFileDateTimes;
   delete aFileSizes;
}

void __fastcall TFormFTPTool::FormClose(TObject *Sender,
      TCloseAction &Action)
{
    if(fFtp->FTPConnect())
          fFtp->FTPDisConnect();
    delete fFtp;
}
*/
    }
}
