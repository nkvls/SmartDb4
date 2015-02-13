using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text.pdf.draw;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Helpers;
using SmartDb4.Models;

using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;

namespace SmartDb4.Controllers
{
    [Authorize(Roles = "Admin, Staff")]
    public class ReportController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly string _reportName = "Report" + DateTime.Now;
        private readonly Font _headerRowFont = FontFactory.GetFont("Arial", 10, new BaseColor(0, 0, 0));
        private readonly Font _rowFont = FontFactory.GetFont("Arial", 8, new BaseColor(0, 0, 0));
        private readonly Font _groupRowFont = FontFactory.GetFont("Arial", 8, new BaseColor(0, 0, 0));
        private readonly Font _addressMainFont = FontFactory.GetFont("Arial", 16, new BaseColor(0, 0, 0));
        private readonly Font _addressRowsFont = FontFactory.GetFont("Arial", 12, new BaseColor(0, 0, 0));

        private readonly BaseColor _headerRowBgColor = new BaseColor(100,149,237);
        private readonly BaseColor _groupRowBgColor = new BaseColor(255, 235, 205);
        private readonly BaseColor _totalRowBgColor = new BaseColor(175, 238, 238);
        private readonly BaseColor _percentageRowBgColor = new BaseColor(38, 179, 179);
        private readonly BaseColor _grandTotalRowBgColor = new BaseColor(17, 83, 83);


        public ActionResult Index()
        {
            ViewBag.GenderId = GenericSelectList.GetSelectList(SelectListEnums.Gender, "GenderId", "GenderName");
            ViewBag.EthnicityId = GenericSelectList.GetSelectList(SelectListEnums.Ethnicity, "EthnicityId", "EthnicityName");
            ViewBag.NominationId = GenericSelectList.GetSelectList(SelectListEnums.Nomination, "NominationId", "NominationName");
            ViewBag.FundingResponsibilityId = GetFundingResponsibilitiesExcludingOther(null);   // GenericSelectList.GetSelectList(SelectListEnums.FundingResponsibility, "FundingResponsibilityId", "FundingResponsibilityName");
            ViewBag.ProjectId = GenericSelectList.GetSelectList(SelectListEnums.Project, "ProjectId", "ProjectName");
            ViewBag.AgeBracketId = GenericSelectList.GetSelectList(SelectListEnums.AgeBracket, "AgeBracketId", "AgeBracketText"); //GetAgeBracketSelectList();
            ViewBag.GroupByClauseId = GenericSelectList.GetSelectList(SelectListEnums.GroupBy, "GroupByClauseId", "GroupByClauseText");  //GetGroupBySelectList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ReportDataModel model)
        {
            ViewBag.GenderId = GenericSelectList.GetSelectList(SelectListEnums.Gender, "GenderId", "GenderName", model.GenderId);
            ViewBag.EthnicityId = GenericSelectList.GetSelectList(SelectListEnums.Ethnicity, "EthnicityId", "EthnicityName", model.EthnicityId);
            ViewBag.NominationId = GenericSelectList.GetSelectList(SelectListEnums.Nomination, "NominationId", "NominationName", model.NominationId);
            ViewBag.FundingResponsibilityId = GetFundingResponsibilitiesExcludingOther(model);   // GenericSelectList.GetSelectList(SelectListEnums.FundingResponsibility, "FundingResponsibilityId", "FundingResponsibilityName", model.FundingResponsibilityId);
            ViewBag.ProjectId = GenericSelectList.GetSelectList(SelectListEnums.Project, "ProjectId", "ProjectName", model.ProjectId);
            ViewBag.AgeBracketId = GenericSelectList.GetSelectList(SelectListEnums.AgeBracket, "AgeBracketId", "AgeBracketText", model.AgeBracketId);  //GetAgeBracketSelectList(model.AgeGroupId);
            ViewBag.GroupByClauseId = GenericSelectList.GetSelectList(SelectListEnums.GroupBy, "GroupByClauseId", "GroupByClauseText", model.GroupByClauseId);    //GetGroupBySelectList(model.GroupById);

            return DownloadReport(model.DateFrom, model.DateTo, model.NominationId.ToString(), model.ProjectId.ToString(), model.AgeBracketId.ToString(),
                model.GenderId.ToString(), model.EthnicityId.ToString(), model.FundingResponsibilityId.ToString(), model.GroupByClauseId.ToString(), model.IncludeLeavingDetails, "pdf");
        }

        public ActionResult DownloadReport(DateTime startDt, DateTime endDt, string nominationId, string projectId, string ageBracketId, string genderId, string ethnicId, string fundingResponsibilityId, string groupBy, bool includeLeavingDetails, string format)
        {
            Utility.LogDebug("Initiating Download report");

            int ageBracket;
            int.TryParse(ageBracketId, out ageBracket);

            Utility.LogDebug("getting age group data");

            var ageGroupData = _unitOfWork.AgeBracketRepository.GetById(ageBracket);

            //Utility.WriteToLog("getting local report", "debug");
            //var localReport = GetLocalReport(groupBy);
            Utility.LogDebug("getting report data from db");

            var data = GetReportDataFromDb(startDt, endDt, nominationId, projectId, genderId, ethnicId, fundingResponsibilityId, groupBy, ageGroupData);

            int groupByClauseId;
            int.TryParse(groupBy, out groupByClauseId);
            var groupByClauseRecord = _unitOfWork.GroupByRepository.GetById(groupByClauseId);

            if(groupByClauseRecord == null)
                throw new Exception("Invalid group by clause provided " +  groupBy);

            var reportPath1 = Server.MapPath("~/Reports/GroupByProject.pdf");
            CreateANewPdfDocument(reportPath1, data, groupByClauseRecord.GroupByClauseText, includeLeavingDetails);

            var renderedBytes1 = GetBytesFromFile(reportPath1);
            return File(renderedBytes1, "application/pdf", _reportName + ".pdf");

            //Utility.WriteToLog("getting data collection list", "debug");

            //var dataCollectionList = GetDataCollectionList(data);

            //Utility.WriteToLog("create report data source", "debug");

            //var reportDataSource = new ReportDataSource {Name = "DataSet1", Value = dataCollectionList};

            ////localReport.DataSources.Add(reportDataSource);

            ////Utility.WriteToLog("collecting general information", "debug");

            ////string reportType = "Excel";            
            //string mimeType;
            //string encoding;
            //string fileNameExtension;

            ////The DeviceInfo settings should be changed based on the reportType            
            ////http://msdn2.microsoft.com/en-us/library/ms155397.aspx            
            //const string deviceInfo = "<DeviceInfo>" +
            //                          "  <OutputFormat>PDF</OutputFormat>" +
            //                          "  <PageWidth>11.5in</PageWidth>" +
            //                          "  <PageHeight>11in</PageHeight>" +
            //                          "  <MarginTop>0.5in</MarginTop>" +
            //                          "  <MarginLeft>0.5in</MarginLeft>" +
            //                          "  <MarginRight>0.5in</MarginRight>" +
            //                          "  <MarginBottom>0.5in</MarginBottom>" +
            //                          "</DeviceInfo>";

            //Warning[] warnings;
            //string[] streams;

            //Utility.WriteToLog("render the report", "debug");
            //Render the report            
            //byte[] renderedBytes = localReport.Render(format, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

            //Utility.WriteToLog("render report as byte complete", "debug");
            //Response.AddHeader("content-disposition", "attachment; filename=NorthWindCustomers." + fileNameExtension); 

            //Utility.WriteToLog("returning file as requested format", "debug");
            //switch (format)
            //{
            //    case "pdf":
            //        return File(renderedBytes, "application/pdf", _reportName + ".pdf");
            //    case "excel":
            //        return File(renderedBytes, "application/excel", _reportName + ".xls");
            //    case "word":
            //        return File(renderedBytes, "application/word", _reportName + ".doc");
            //    case "image":
            //        return File(renderedBytes, "image/jpeg", _reportName + ".jpeg");
            //    default:
            //        return File(renderedBytes, "application/pdf", _reportName + ".pdf");
            //}
        }

        private void CreateANewPdfDocument(string fileName, IEnumerable<ReportDataModel> dataModels, string groupByClauseHeader, bool includeLeavingDetails)
        {
            //Create new PDF document
            //- See more at: http://www.dotnetfox.com/articles/how-to-create-table-in-pdf-document-using-Asp-Net-with-C-Sharp-and-itextsharp-1027.aspx#sthash.x7pcQj64.dpuf
            var document = new Document(PageSize.A4, 20f, 20f, 20f, 20f);

            // set the page size, set the orientation
            //document.SetPageSize(PageSize.A4.Rotate());
            document.SetPageSize(PageSize.A4);

            int noOfColumns;
            float[] widths;

            if (includeLeavingDetails)
            {
                noOfColumns = 9;
                widths = new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
            }
            else
            {
                noOfColumns = 7;
                widths = new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f};
            }

            try
            {
                using (var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create)))
                {
                    var pageEventHelper = new PageEventHelper();
                    writer.PageEvent = pageEventHelper;

                    document.Open();

                    document.Add(new Paragraph(string.Empty));
                    var image = iTextSharp.text.Image.GetInstance(Request.MapPath("~/content/images/loginimage.png"));
                    image.ScalePercent(75f);
                    //image.SetAbsolutePosition(0f, 0f);
                    document.Add(image);

                    //iTextSharp.text.Image imageHeader = iTextSharp.text.Image.GetInstance(Request.MapPath("~/content/images/bgimg.png"));
                    ////add image; PdfPCell() overload sizes image to fit cell
                    //var imageParagraph = new Paragraph();
                    //imageParagraph.Add(new Chunk(imageHeader, 50f, 50f));
                    //imageHeader.SetAbsolutePosition(0, 0);
                    ////var c = new PdfPCell(imageHeader, true);
                    ////c.HorizontalAlignment = Element.ALIGN_RIGHT;
                    ////c.FixedHeight = document.TopMargin;
                    ////c.Border = PdfPCell.NO_BORDER;
                    //document.Add(imageParagraph);
                    //var tableHeader = new PdfPTable(2);
                    //tableHeader.TotalWidth = 550f;
                    //fix the absolute width of the table
                    //tableHeader.LockedWidth = true;
                    //var imageHeader = iTextSharp.text.Image.GetInstance(Request.MapPath("~/content/images/bgimg.png"));
                    //var c = new PdfPCell(imageHeader, true);
                    ////c.HorizontalAlignment = Element.ALIGN_RIGHT;
                    //c.FixedHeight = document.TopMargin;
                    ////c.Border = PdfPCell.NO_BORDER;
                    //tableHeader.AddCell(c);



                    // create a paragraph and add it to the document
                    var title = new Paragraph(); // new Paragraph("St Mary Abbotts Rehabiliation & Training\n");
                    title.Add(new Chunk("St Mary Abbotts Rehabiliation & Training\n", _addressMainFont));
                    title.Add(new Chunk("The Basement,\n", _addressRowsFont));
                    title.Add(new Chunk("15 Gertrude Street,\n", _addressRowsFont));
                    title.Add(new Chunk("London SW10 0JN", _addressRowsFont));
                    title.IndentationLeft = 10f;
                    document.Add(title);

                    // add line below title
                    var line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1);
                    document.Add(new Chunk(line));

                    var table = new PdfPTable(noOfColumns);
                    table.TotalWidth = 550f;
                    //fix the absolute width of the table
                    table.LockedWidth = true;

                    table.SetWidths(widths);
                    table.HorizontalAlignment = 0;
                    //leave a gap before and after the table
                    table.SpacingBefore = 10f;
                    table.SpacingAfter = 10f;

                    PdfPCell titleColumn;

                    var reportDataModels = dataModels as IList<ReportDataModel> ?? dataModels.ToList();

                    var grandTotal = reportDataModels.Count();

                    var groupData = GetQueryByGrouping(reportDataModels, groupByClauseHeader);
                    
                    int counter = 0;
                    foreach (var data in groupData)
                    {
                        counter++;
                        var rowSpan = data.Count();
                        
                        PdfPTable resultTable = null;
                        switch (groupByClauseHeader.ToLower())
                        {
                            case "nomination":
                                resultTable = CreateNominationData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "project":
                                resultTable = CreateProjectData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "gender":
                                resultTable = CreateGenderData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "ethnicity":
                                resultTable = CreateEthnicityData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "fundingresponsibility":
                            case "funding responsibility":
                                resultTable = CreateBoroughData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "agebracket":
                            case "age bracket":
                                resultTable = CreateAgeBracketData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "startdate":
                            case "start date":
                                resultTable = CreateStartDateData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                            case "membername":
                            case "member name":
                                resultTable = CreateMemberNameData(table, data, groupByClauseHeader, includeLeavingDetails, counter <= 1);
                                break;
                        }

                        table = resultTable;
                        
                        //GROUP TOTAL
                        titleColumn = GetTotalCell("Total", _groupRowFont);;
                        titleColumn.Colspan = 1;
                        table.AddCell(titleColumn);


                        var groupTotalCell = GetTotalCell(rowSpan.ToString(CultureInfo.InvariantCulture), _groupRowFont);
                        groupTotalCell.Colspan = noOfColumns - 2;
                        groupTotalCell.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                        table.AddCell(groupTotalCell);



                        var perc = Math.Round((double) (((double) rowSpan/grandTotal)*100), 2);
                        //GROUP PERCENTAGE
                        titleColumn = GetPercentageCell("Percentage", _groupRowFont);
                        titleColumn.Colspan = 1;
                        table.AddCell(titleColumn);

                        var groupPercentageCell = GetPercentageCell(perc.ToString(CultureInfo.InvariantCulture) + "%", _groupRowFont);
                        groupPercentageCell.HorizontalAlignment = 2;
                        groupPercentageCell.Colspan = noOfColumns - 2;
                        table.AddCell(groupPercentageCell);
                    }

                    //Grand Total
                    titleColumn = GetGrandTotalCell("Grand Total", _groupRowFont);
                    titleColumn.Colspan = 1;
                    table.AddCell(titleColumn);

                    var grandeTotalCell = GetGrandTotalCell(grandTotal.ToString(CultureInfo.InvariantCulture), _groupRowFont);
                    grandeTotalCell.HorizontalAlignment = 2;
                    grandeTotalCell.Colspan = noOfColumns - 1;
                    table.AddCell(grandeTotalCell);

                    document.Add(table);
                }
            }
            catch(Exception ex)
            {
            }
            finally
            {
                document.Close();
                //ShowPdf(fileName);
            }

        }

        private IOrderedEnumerable<IGrouping<string, ReportDataModel>> GetQueryByGrouping(IEnumerable<ReportDataModel> reportDataModels, string groupByClauseHeader)
        {
            IOrderedEnumerable<IGrouping<string, ReportDataModel>> groupData = null;

            switch (groupByClauseHeader.ToLower())
            {
                case "nomination":
                    groupData = from data in reportDataModels group data by data.NominationName into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "project":
                    groupData = from data in reportDataModels group data by data.ProjectName into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "gender":
                    groupData = from data in reportDataModels group data by data.GenderName into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "ethnicity":
                    groupData = from data in reportDataModels group data by data.EthnicityName into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "fundingresponsibility":
                case "funding responsibility":
                    groupData = from data in reportDataModels group data by data.FundingResponsibilityName into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "agebracket":
                case "age bracket":
                    groupData = from data in reportDataModels group data by data.AgeBracketText into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "startdate":
                case "start date":
                    groupData = from data in reportDataModels group data by data.StartDate.Value.ToShortDateString() into newGroup orderby newGroup.Key select newGroup;
                    break;
                case "membername":
                case "member name":
                    groupData = from data in reportDataModels group data by data.MemberName into newGroup orderby newGroup.Key select newGroup;
                    break;
                default:
                    throw new NotImplementedException("Invalid Grouping");
            }

            return groupData;
        }

        private PdfPCell GetHeaderCell(string cellName)
        {
            return new PdfPCell(new Phrase(cellName)) {BackgroundColor = _headerRowBgColor};
        }

        private PdfPCell GetTotalCell(string cellName, Font font)
        {
            return new PdfPCell(new Phrase(cellName, font)) {BackgroundColor = _totalRowBgColor};
        }

        private PdfPCell GetGrandTotalCell(string cellName, Font font)
        {
            return new PdfPCell(new Phrase(cellName, font)) {BackgroundColor = _grandTotalRowBgColor};
        }

        private PdfPCell GetPercentageCell(string cellName, Font font)
        {
            return new PdfPCell(new Phrase(cellName, font)) {BackgroundColor = _percentageRowBgColor};
        }

        private PdfPCell GetGroupRowCell(string cellName, Font font)
        {
            return new PdfPCell(new Phrase(cellName, font)) {BackgroundColor = _groupRowBgColor};
        }

        private PdfPTable CreateNominationData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Project"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }
            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);   // new PdfPCell(new Phrase(data.Key, rowFont));
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateProjectData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Nomination"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.NominationName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateGenderData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Project"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Nomination"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.NominationName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateEthnicityData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Project"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Nomination"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.NominationName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateBoroughData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Project"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateAgeBracketData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Project"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
                        _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateStartDateData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Member"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Project"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }

                table.AddCell(new Phrase(item.MemberName, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null, _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private PdfPTable CreateMemberNameData(PdfPTable table, IGrouping<string, ReportDataModel> data, string groupByClauseHeader, bool includeLeavingDetails, bool includeHeader)
        {
            if (includeHeader)
            {
                table.AddCell(GetHeaderCell(groupByClauseHeader));

                table.AddCell(GetHeaderCell("Start Date"));
                table.AddCell(GetHeaderCell("Gender"));
                table.AddCell(GetHeaderCell("Ethnicity"));
                table.AddCell(GetHeaderCell("Time At Smart"));
                table.AddCell(GetHeaderCell("Agency"));
                table.AddCell(GetHeaderCell("Project"));

                if (includeLeavingDetails)
                {
                    table.AddCell(GetHeaderCell("Date Of Leaving"));
                    table.AddCell(GetHeaderCell("Reason Of Leaving"));
                }
            }

            var cnt = 0;
            foreach (var item in data)
            {
                if (cnt <= 0)
                {
                    var groupByClauseColumn = GetGroupRowCell(data.Key, _rowFont);
                    groupByClauseColumn.Rowspan = data.Count() + 2;
                    table.AddCell(groupByClauseColumn);
                    cnt++;
                }
                var stDt = item.StartDate.HasValue ? item.StartDate.Value.ToShortDateString() : string.Empty;
                table.AddCell(new Phrase(stDt, _rowFont));
                table.AddCell(new Phrase(item.GenderName, _rowFont));
                table.AddCell(new Phrase(item.EthnicityName, _rowFont));
                table.AddCell(new Phrase(item.TimeAtSmart, _rowFont));
                table.AddCell(new Phrase(item.Agency, _rowFont));
                table.AddCell(new Phrase(item.ProjectName, _rowFont));
                if (includeLeavingDetails)
                {
                    table.AddCell(new Phrase(item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null, _rowFont));
                    table.AddCell(new Phrase(item.ReasonOfLeaving, _rowFont));
                }
            }

            return table;
        }

        private SelectList GetFundingResponsibilitiesExcludingOther(ReportDataModel model)
        {
            if (model == null)
            {
                var data = _unitOfWork.FundingResponsibilityRepository.Get(filter: x=> x.FundingResponsibilityId != 3, orderBy: x => x.OrderBy(k => k.FundingResponsibilityName));
                return new SelectList(data, "FundingResponsibilityId", "FundingResponsibilityName", null);

                //var boroughList = GenericSelectList.GetSelectList(SelectListEnums.FundingResponsibility,
                //    "FundingResponsibilityId", "FundingResponsibilityName");
                //return new SelectList(boroughList.Where(x => x.Value != "3"), "FundingResponsibilityId",
                //    "FundingResponsibilityName", null);
            }
            else
            {
                var data = _unitOfWork.FundingResponsibilityRepository.Get(filter: x => x.FundingResponsibilityId != 3, orderBy: x => x.OrderBy(k => k.FundingResponsibilityName));
                return new SelectList(data, "FundingResponsibilityId", "FundingResponsibilityName", model.FundingResponsibilityId);
                //var boroughList = GenericSelectList.GetSelectList(SelectListEnums.FundingResponsibility,
                //    "FundingResponsibilityId", "FundingResponsibilityName", model.FundingResponsibilityId);
                //return new SelectList(boroughList.Where(x => x.Value != "3"), "FundingResponsibilityId",
                //    "FundingResponsibilityName", model.FundingResponsibilityId);
            }

            
        }
        
        private static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)

            FileStream fs = null;
            try
            {
                return System.IO.File.ReadAllBytes(fullFilePath);
                //fs = File.OpenRead(fullFilePath);
                //byte[] bytes = new byte[fs.Length];
                //fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                //return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

        }

        private IEnumerable<ReportDataModel> GetReportDataFromDb(DateTime startDt, DateTime endDt, string nominationId, string projectId, string genderId, string ethnicId, string fundingResponsibilityId, string groupBy, AgeBracket ageGroupData)
        {
            //Utility.WriteToLog("Inside GetReportFromDb method", "debug");

            int nomination;
            int.TryParse(nominationId, out nomination);

            int project;
            int.TryParse(projectId, out project);

            int gender;
            int.TryParse(genderId, out gender);

            int ethnic;
            int.TryParse(ethnicId, out ethnic);

            int fundingResponsibility;
            int.TryParse(fundingResponsibilityId, out fundingResponsibility);

            //Utility.WriteToLog("create ReportDataModel", "debug");

            var model = new ReportDataModel
            {
                DateFrom = startDt,
                DateTo = endDt,
                NominationId = nomination,
                ProjectId = project,
                AgeBracketText = ageGroupData == null ? null : ageGroupData.AgeBracketId.ToString(CultureInfo.InvariantCulture),
                GenderId = gender,
                EthnicityId = ethnic,
                FundingResponsibilityId = fundingResponsibility,
                GroupByClause = groupBy
            };

            //Utility.WriteToLog("get report data from report repository", "debug");
            var data = _unitOfWork.ReportRepository.GetReportData(model);

            return data;
        }
        
        //private FileContentResult ViewReport(ReportDataModel reportDataModel, string groupBy, string format)
        //{
        //    var localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/QuarterlyReport.rdlc") };

        //    var model = new ReportDataModel
        //    {
        //        DateFrom = reportDataModel.DateFrom,
        //        DateTo = reportDataModel.DateTo,
        //        NominationId = reportDataModel.NominationId,
        //        ProjectId = reportDataModel.ProjectId,
        //        AgeBracketText = reportDataModel.AgeBracketId.ToString(),
        //        GenderId = reportDataModel.GenderId,
        //        EthnicityId = reportDataModel.EthnicityId,
        //        FundingResponsibilityId = reportDataModel.FundingResponsibilityId,
        //        GroupByClause = groupBy
        //    };

        //    var data = _unitOfWork.ReportRepository.GetReportData(model);

        //    var reportDataSource = new ReportDataSource { Name = "rptDataSet", Value = data.ToList() };


        //    localReport.DataSources.Add(reportDataSource);

        //    //string reportType = "Excel";            
        //    string mimeType;
        //    string encoding;
        //    string fileNameExtension;

        //    //The DeviceInfo settings should be changed based on the reportType            
        //    //http://msdn2.microsoft.com/en-us/library/ms155397.aspx            
        //    string deviceInfo = "<DeviceInfo>" +
        //        "  <OutputFormat>PDF</OutputFormat>" +
        //        "  <PageWidth>8.5in</PageWidth>" +
        //        "  <PageHeight>11in</PageHeight>" +
        //        "  <MarginTop>0.5in</MarginTop>" +
        //        "  <MarginLeft>1in</MarginLeft>" +
        //        "  <MarginRight>1in</MarginRight>" +
        //        "  <MarginBottom>0.5in</MarginBottom>" +
        //        "</DeviceInfo>";

        //    Warning[] warnings;
        //    string[] streams;
        //    byte[] renderedBytes;

        //    //Render the report            
        //    renderedBytes = localReport.Render(format, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

        //    //Response.AddHeader("content-disposition", "attachment; filename=NorthWindCustomers." + fileNameExtension); 

        //    switch (format)
        //    {
        //        case "pdf":
        //            return File(renderedBytes, "application/pdf", "Report123Test.pdf");
        //        case "excel":
        //            return File(renderedBytes, "application/excel", "Report123Test.xls");
        //        case "word":
        //            return File(renderedBytes, "application/word", "Report123Test.doc");
        //        case "image":
        //            return File(renderedBytes, "image/jpeg", "Report123Test.jpeg");
        //        default:
        //            return File(renderedBytes, "application/pdf", "Report123Test.pdf");
        //    }
        //}

        //public void ShowPdf(string filename)
        //{
        //    //Clears all content output from Buffer Stream
        //    Response.ClearContent();
        //    //Clears all headers from Buffer Stream
        //    Response.ClearHeaders();
        //    //Adds an HTTP header to the output stream
        //    Response.AddHeader("Content-Disposition", "inline;filename=" + filename);
        //    //Gets or Sets the HTTP MIME type of the output stream
        //    Response.ContentType = "application/pdf";
        //    //Writes the content of the specified file directory to an HTTP response output stream as a file block
        //    Response.WriteFile(filename);
        //    //sends all currently buffered output to the client
        //    Response.Flush();
        //    //Clears all content output from Buffer Stream
        //    Response.Clear();
        //}

        //private IList<ReportDataDisplayModel> GetDataCollectionList(IEnumerable<ReportDataModel> data)
        //{
        //    //Utility.WriteToLog("GetDataCollectionList().creating data collection list", "debug");

        //    IList<ReportDataDisplayModel> dataCollectionList = new List<ReportDataDisplayModel>();
        //    var reportDataModels = data as IList<ReportDataModel> ?? data.ToList();
        //    foreach (var item in reportDataModels)
        //    {
        //        dataCollectionList.Add(
        //            new ReportDataDisplayModel
        //            {
        //                AgeGroup = item.AgeBracketText,
        //                Agency = item.Agency,
        //                Ethnicity = item.EthnicityName,
        //                FundingResponsibility = item.FundingResponsibilityName,
        //                Gender = item.GenderName,
        //                MemberName = item.MemberName,
        //                Nomination = item.NominationName,
        //                ProjectName = item.ProjectName,
        //                StartDate = item.StartDate.HasValue ? item.StartDate.Value.ToShortDateString() : null,
        //                TimeAtSmart = item.TimeAtSmart,
        //                TotalCount = item.TotalMembers,
        //                DateOfLeaving = item.ExitDate.HasValue ? item.ExitDate.Value.ToShortDateString() : null,
        //                ReasonOfLeaving = item.ReasonOfLeaving
        //            });
        //        //}
        //    }
        //    return dataCollectionList;
        //}

        //private LocalReport GetLocalReport(string groupBy)
        //{
        //    //Utility.WriteToLog("Inside GetLocalReport", "debug");

        //    int groupById;
        //    int.TryParse(groupBy, out groupById);

        //    //Utility.WriteToLog("get group by repository", "debug");
        //    var groupByClauseData = _unitOfWork.GroupByRepository.GetById(groupById);

        //    LocalReport localReport;


        //    //Utility.WriteToLog("creating LocalReport object data", "debug");
        //    switch (groupByClauseData.GroupByClauseText.ToLower())
        //    {
        //        case "project":
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByProject.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByProject.rdlc") };
        //            break;
        //        case "ethnicity":
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByEthnicity.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByEthnicity.rdlc") };
        //            break;
        //        case "agebracket":
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByAgeBracket.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByAgeBracket.rdlc") };
        //            break;
        //        case "gender":
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByGender.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByGender.rdlc") };
        //            break;
        //        case "fundingresponsibility":
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByBorough.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByBorough.rdlc") };
        //            break;
        //        case "nomination":
        //            Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByNomination.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByNomination.rdlc") };
        //            break;
        //        default:
        //            //Utility.WriteToLog("The report path is " + Server.MapPath("~/Reports/GroupByProject.rdlc"), "debug");
        //            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/GroupByProject.rdlc") };
        //            break;
        //    }
        //    return localReport;
        //}
    }

    public class PageEventHelper : PdfPageEventHelper
    {
        PdfContentByte cb;
        PdfTemplate template;
        

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            cb = writer.DirectContent;
            template = cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
            //Font times = new Font(bfTimes, 12, Font.ITALIC, Color.Red);

            int pageN = writer.PageNumber;
            String text = "Page " + pageN.ToString() + " of ";
            float len = bfTimes.GetWidthPoint(text, 10f);

            iTextSharp.text.Rectangle pageSize = document.PageSize;

            cb.SetRGBColorFill(100, 100, 100);

            cb.BeginText();
            cb.SetFontAndSize(bfTimes, 10f);
            cb.SetTextMatrix(document.LeftMargin, pageSize.GetBottom(document.BottomMargin));
            cb.ShowText(text);

            cb.EndText();

            cb.AddTemplate(template, document.LeftMargin + len, pageSize.GetBottom(document.BottomMargin));
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);

            template.BeginText();
            template.SetFontAndSize(bfTimes, 10f);
            template.SetTextMatrix(0, 0);
            template.ShowText("" + (writer.PageNumber - 1));
            template.EndText();
        }
    }
}
