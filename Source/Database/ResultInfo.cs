using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler_Database
{
    // Result를 상속받아서 StudayInfo 포함한다.
    public class ResultInfo : Result
    {
        public string AccessionNumber = "";                             // EMR시 사용한다.
        public string EMRStudyDescription = "";                         // EMR시 사용, Order의 Desription이다.
        public List<ResultInfo> listResultInfo = new List<ResultInfo>();   // StudyInfo 정보 List

        public ResultInfo(Result p = null)
        {
            RID = -1;
            UserName = "";
            Barcode = "";
            InstrumentType = "";
            BlockType = "";
            ExperimentType = -1;
            Chemisty = "";
            RunMode = -1;
            Volume = "";
            Cover = "";
            Comment = "";
            ResultDateTime = System.DateTime.Now;
            MethodPath = "";
            PlatePath = "";
            ResultPath = "";
            MethodFile = "";
            PlateFile = "";
            ResultFile = "";

            if (p != null)
            {
                this.RID = p.RID;
                this.UserName = p.UserName;
                this.Barcode = p.Barcode;
                this.InstrumentType = p.InstrumentType;
                this.BlockType = p.BlockType;
                this.ExperimentType = p.ExperimentType;
                this.Chemisty = p.Chemisty;
                this.RunMode = p.RunMode;
                this.Volume = p.Volume;
                this.Cover = p.Cover;
                this.Comment = p.Comment;
                this.ResultDateTime = p.ResultDateTime;
                this.MethodPath = p.MethodPath;
                this.PlatePath = p.PlatePath;
                this.ResultPath = p.ResultPath;
                this.MethodFile = p.MethodFile;
                this.PlateFile = p.PlateFile;
                this.ResultFile = p.ResultFile;
            }
        }

        // 결과 정보 가져오기
        public Result GetResult()
        {
            Result p = new Result
            {
                RID = this.RID,
                UserName = this.UserName,
                Barcode = this.Barcode,
                InstrumentType = this.InstrumentType,
                BlockType = this.BlockType,
                ExperimentType = this.ExperimentType,
                Chemisty = this.Chemisty,
                RunMode = this.RunMode,
                Volume = this.Volume,
                Cover = this.Cover,
                Comment = this.Comment,
                ResultDateTime = this.ResultDateTime,
                MethodPath = this.MethodPath,
                PlatePath = this.PlatePath,
                ResultPath = this.ResultPath,
                MethodFile = this.MethodFile,
                PlateFile = this.PlateFile,
                ResultFile = this.ResultFile
            };
            return p;
        }

        // 결과 정보 저장하기
        public void SetResult(Result rInfo)
        {
            this.RID = rInfo.RID;
            this.UserName = rInfo.UserName;
            this.Barcode = rInfo.Barcode;
            this.InstrumentType = rInfo.InstrumentType;
            this.BlockType = rInfo.BlockType;
            this.ExperimentType = rInfo.ExperimentType;
            this.Chemisty = rInfo.Chemisty;
            this.RunMode = rInfo.RunMode;
            this.Volume = rInfo.Volume;
            this.Cover = rInfo.Cover;
            this.Comment = rInfo.Comment;
            this.ResultDateTime = rInfo.ResultDateTime;
            this.MethodPath = rInfo.MethodPath;
            this.PlatePath = rInfo.PlatePath;
            this.ResultPath = rInfo.ResultPath;
            this.MethodFile = rInfo.MethodFile;
            this.PlateFile = rInfo.PlateFile;
            this.ResultFile = rInfo.ResultFile;
        }

        // 데이터베이스에서 Study 정보 읽어오기
        //public void LoadResult()
        //{
        //    this.listStudyInfo.Clear(); //기존에 있던 Study Data를 지운다.

        //    foreach (var s in Global.studyManager.SearchStudys(this.RID))
        //    {
        //        StudyInfo studyinfo = new StudyInfo(s);

        //        studyinfo.SetResult((Result)this);   // Result 입력후 StudyID가 변경된다. 
        //        studyinfo.StudyID = s.StudyID;         // 원래 자기것으로 다시 변경   
        //        listStudyInfo.Add(studyinfo);
        //    }

        //    if (listStudyInfo.Count > 0)            // LastStudyDateTime 다시 입력하기( StudyID를 고유하게 처리하기 위해 LastStudyID는 변경하지 않는다.)
        //    {
        //        this.LastStudyDateTime = listStudyInfo[0].StudyDateTime;
        //    }
        //}

        //// 데이터베이스에서 Study 정보를 조건에 따라 다시 읽어오기
        //public void SearchStudy(System.DateTime studyLow, System.DateTime studyHigh)
        //{
        //    this.listStudyInfo.Clear(); //기존에 있던 Study Data를 지운다.

        //    foreach (var s in Global.studyManager.SearchStudys(PID, studyLow, studyHigh))
        //    {
        //        StudyInfo studyinfo = new StudyInfo(s);
        //        studyinfo.SetResult((Result)this);       // Result 입력후 StudyID가 변경된다. 
        //        studyinfo.StudyID = s.StudyID;             // 원래 자기것으로 다시 변경  
        //        listStudyInfo.Add(studyinfo);
        //    }
        //}
    }
}
