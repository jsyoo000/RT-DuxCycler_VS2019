using System;
using System.Collections.Generic;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Win32;
using System.Diagnostics;

namespace Duxcycler_Database
{
    public class Result
    {
        [PrimaryKey, AutoIncrement]
        public int RID { get; set; }                     // Result ID
        [MaxLength(20)]
        public string UserName { get; set; }             // User Name
        [MaxLength(100)]
        public string Barcode { get; set; }              // Barcode        
        [MaxLength(100)]
        public string InstrumentType { get; set; }       // InstrumentType        
        [MaxLength(100)]
        public string BlockType { get; set; }            // BlockType        
        public int ExperimentType { get; set; }          // Experiment Type
        [MaxLength(100)]
        public string Chemisty { get; set; }             // Chemisty        
        public int RunMode { get; set; }                 // Run Mode
        [MaxLength(100)]
        public string Volume { get; set; }               // Volume        
        [MaxLength(100)]
        public string Cover { get; set; }                // Cover        
        public DateTime ResultDateTime { get; set; }     // 파일 저장 시간
        [MaxLength(1000)]
        public string Comment { get; set; }              // Comment
        [MaxLength(1000)]
        public string MethodPath { get; set; }           // Method Path
        [MaxLength(1000)]
        public string PlatePath { get; set; }            // Plate Path
        [MaxLength(1000)]
        public string ResultPath { get; set; }           // Result Path
        [MaxLength(1000)]
        public string MethodFile { get; set; }           // Method File
        [MaxLength(1000)]
        public string PlateFile { get; set; }            // Plate File
        [MaxLength(1000)]
        public string ResultFile { get; set; }           // Result File
    }

    public class ResultManager : SQLiteConnection
    {
        public ResultManager(string path)
            : base(new SQLitePlatformWin32(), path)
        {
            CreateTable<Result>();             // 환자 Table 생성
        }

        // 환자 Tabel에 환자 정보 입력후 성공하면 PID를 리턴한다. 실패하면 -1
        public int InsertResult(Result Result)
        {
            int pid = -1;
            try
            {
                //Result.ChartNo = Result.ChartNo.Trim();       // Chart No는 앞뒤 공백은 제거해서 저장한다.
                if (Insert(Result) > 0)
                {
                    pid = Result.RID;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return pid;
        }

        // 환자 Tabel에 환자 정보 Update
        public bool UpdateResult(Result Result)
        {
            bool bReturn = false;
            try
            {
                if(Update(Result) > 0)
                {
                    bReturn = true;
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bReturn;
        }

        // 환자 Tabel에 환자 정보 Delete
        public bool DeleteResult(Result Result)
        {
            bool bReturn = false;
            try
            {
                if (Delete(Result) > 0)
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bReturn;
        }

        // 결과 Table에서 모든 환자 정보 정보 가지고 오기
        public IEnumerable<Result> ListAllResults()
        {
            return Table<Result>().OrderByDescending(x => x.ResultDateTime);
        }

        // 결과 Table에서 환자 PID로 환자 찾기
        public Result SearchResult(int pid)
        {
            return (from p in Table<Result>()
                    where p.RID == pid
                    select p).FirstOrDefault();
        }

        // 결과 Table에서 환자 환자 Chart No로 환자 찾기
        public Result SearchBarcode(string Barcode)
        {
            string trimBarcode = Barcode.Trim();
            return (from p in Table<Result>()
                    where p.Barcode == trimBarcode
                    select p).FirstOrDefault();
        }

        // 결과 Table에서 Result File로 결과 찾기
        public Result SearchResultFile(string rFile)
        {
            return (from p in Table<Result>()
                    where p.ResultFile == rFile
                    select p).FirstOrDefault();
        }

        // 결과 Table에서 조건에 해당하는 환자 정보를 가지고 오기
        public IEnumerable<Result> SearchResults(bool bAll, string barcodeNo, string name, DateTime studyDateLow, DateTime studyDateHigh)
        {
            if(bAll)
                return Table<Result>().Where(x => x.Barcode.StartsWith(barcodeNo) && x.UserName.StartsWith(name)).OrderByDescending(x => x.ResultDateTime);
            else
                return Table<Result>().Where(x => x.Barcode.StartsWith(barcodeNo) && x.UserName.StartsWith(name) && x.ResultDateTime >= studyDateLow && x.ResultDateTime <= studyDateHigh).OrderByDescending(x => x.ResultDateTime);

            //if (yearlow != 0 && yearhigh != 0 && month != 0 && day != 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthMonth == month && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthMonth == month && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month != 0 && day != 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthMonth == month && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthMonth == month && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month == 0 && day != 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month != 0 && day == 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthMonth == month).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthYear >= yearlow && x.BirthYear <= yearhigh && x.BirthMonth == month).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month == 0 && day != 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthDay == day).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month != 0 && day == 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthMonth == month).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthMonth == month).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else if (yearlow != 0 && yearhigh != 0 && month == 0 && day == 0)
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender && x.BirthYear >= yearlow && x.BirthYear <= yearhigh).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.BirthYear >= yearlow && x.BirthYear <= yearhigh).OrderByDescending(x => x.LastStudyDateTime);
            //}
            //else
            //{
            //    if (gender > 0 && gender < 3)
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name) && x.Gender == gender).OrderByDescending(x => x.LastStudyDateTime);
            //    else
            //        return Table<Result>().Where(x => x.ChartNo.StartsWith(chartNo) && x.Name.StartsWith(name)).OrderByDescending(x => x.LastStudyDateTime);
            //}
        }

        // Study Date로 검색
        public IEnumerable<Result> SearchDate(DateTime studyDateLow, DateTime studyDateHigh)
        {
            return Table<Result>().Where(x => x.ResultDateTime >= studyDateLow && x.ResultDateTime <= studyDateHigh).OrderByDescending(x => x.ResultDateTime);
        }

        // 환자 Table에서 Name 이 같은 환자 정보를 가지고 오기
        public Result SearchName(string Name)
        {
            //return Table<Result>().Where(x => x.ChartNo.StartsWith(Name)).FirstOrDefault();
            return Table<Result>().Where(x => x.UserName == Name).FirstOrDefault();
        }

        public Result SearchResult(string barcodeNo, string Name)
        {
            //return Table<Result>().Where(x => x.ChartNo.StartsWith(Name)).FirstOrDefault();
            return Table<Result>().Where(x => x.Barcode == barcodeNo && x.UserName == Name).FirstOrDefault();
        }
    }
}
