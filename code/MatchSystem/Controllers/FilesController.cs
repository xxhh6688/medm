using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

namespace MatchSystem.Controllers
{
    using Authentication;
    using System.Threading.Tasks;
    using System.Web;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using System.Text;

    public class FilesController : ApiController
    {
        [HttpPost]
        public async Task<object> UploadFiles()
        {
            var sign = new SignIn();
            if (!sign.IsAuthenticated(null))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var user = Utilities.GetCurrentUser();

            // DEFINE THE PATH WHERE WE WANT TO SAVE THE FILES.
            string sPath = "";
            sPath = System.Web.Hosting.HostingEnvironment.MapPath(ConfigurationManager.AppSettings["fileLoaction"]);
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }

            // get current month
            var monthFolder = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString();
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }

            if (!Directory.Exists(Path.Combine(sPath, monthFolder)))
            {
                Directory.CreateDirectory(Path.Combine(sPath, monthFolder));
            }

            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;

            var ctx = Utilities.GetDBContext();
            var list = new List<Models.file>();

            // CHECK THE FILE COUNT.
            for (int iCnt = 0; iCnt < hfc.Count; iCnt++)
            {
                System.Web.HttpPostedFile hpf = hfc[iCnt];

                if (hpf.ContentLength > 0)
                {
                    var newFile = ctx.file.Create();
                    newFile.Name = hpf.FileName;
                    newFile.Token = Utilities.GetMd5Hash(DateTime.Now.Ticks.ToString());
                    newFile.Path = Path.Combine(monthFolder, user.Id + "_" + newFile.Token + Path.GetExtension(hpf.FileName)); // fileName =<mounthfolder>/<userid>_<md5>.<extension>
                    newFile.CreateTime = DateTime.Now;
                    newFile.CreateBy = user.Id;

                    newFile = ctx.file.Add(newFile);
                    await ctx.SaveChangesAsync();
                    var tempFile = Utilities.Copy<Models.file>(newFile, new string[] { });
                    list.Add(tempFile);
                    hpf.SaveAs(Path.Combine(sPath, newFile.Path));
                }
            }

            return list;
        }

        [HttpGet]
        public void DeleteFile(string fileToken)
        {
            var sign = new SignIn();
            if (!sign.IsAuthenticated(null))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            // get file path
            var ctx = Utilities.GetDBContext();
            var fileInDb = ctx.file.Where(f => f.Token == fileToken).FirstOrDefault();
            if (fileInDb == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var localFilePath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["fileLoaction"]);
            localFilePath = Path.Combine(localFilePath, fileInDb.Path);

            File.Delete(localFilePath);
        }

        [HttpGet]
        public HttpResponseMessage DownloadFile(string fileToken)
        {
            HttpResponseMessage result = null;

            // get file path
            var ctx = Utilities.GetDBContext();
            var fileInDb = ctx.file.Where(f => f.Token == fileToken).FirstOrDefault();

            if (fileInDb == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var localFilePath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["fileLoaction"]);
            localFilePath = Path.Combine(localFilePath, fileInDb.Path);

            // Serve the file to the client
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            
            string realName = fileInDb.Name.Replace(" ", "((_blank_))");
            realName = System.Web.HttpUtility.UrlEncode(realName, System.Text.Encoding.UTF8);
            realName = realName.Replace("((_blank_))", " ");

            result.Content.Headers.ContentDisposition.FileName = realName;

            return result;
        }

        [HttpGet]
        public HttpResponseMessage DownloadPaperFile(string fileToken)
        {
            HttpResponseMessage result = null;

            // get file path
            var ctx = Utilities.GetDBContext();
            var fileInDb = ctx.file.Where(f => f.Token == fileToken).FirstOrDefault();
            if (fileInDb == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var fileRefPaper = ctx.paper_file_ref.Where(x => x.FileId == fileInDb.Id).FirstOrDefault();
            int paperId = 0;
            if (fileRefPaper != null)
            {
                paperId = fileRefPaper.PaperId;
            }

            var localFilePath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["fileLoaction"]);
            localFilePath = Path.Combine(localFilePath, fileInDb.Path);

            // Serve the file to the client
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");

            string realName = fileInDb.Name.Replace(" ", "((_blank_))");
            realName = System.Web.HttpUtility.UrlEncode(realName, System.Text.Encoding.UTF8);
            realName = realName.Replace("((_blank_))", " ");

            result.Content.Headers.ContentDisposition.FileName = "(" + paperId + ")" + realName;

            return result;
        }

        [HttpPost]
        public HttpResponseMessage DownloadWord()
        {
            var text = Request.Content.ReadAsStringAsync();
            text.Wait();
            DocParam data = null;

            // use try catch to handle invalid parameter
            try
            {
                var value = HttpUtility.UrlDecode(text.Result.Split('=')[1]);
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<DocParam>(value);

                //check null
                if (string.IsNullOrEmpty(data.HtmlContent))
                {
                    throw new Exception();
                }
            }
            #pragma warning disable 0168
            catch (Exception e)
            #pragma warning disable 0168
            {
                var resp = Request.CreateResponse(HttpStatusCode.BadRequest);
                resp.Content = new StringContent("Invalid parameter");
                return resp;
            }

            // set to attachment
            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(CreateWordStream(data.HtmlContent));
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = string.IsNullOrEmpty(data.Name) ? "word.docx" : data.Name;
            return result;
        }

        private Stream CreateWordStream(string html)
        {
            MemoryStream mem = new MemoryStream();
            using (WordprocessingDocument doc =
                   WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
            {
                // Insert other code here. 
                string altChunkId = "myId";
                MainDocumentPart mainDocPart = doc.AddMainDocumentPart();

                //var run = new Run(new Text("test"));
                //var p = new Paragraph(new ParagraphProperties(
                //     new Justification() { Val = JustificationValues.Center }),
                //                   run);

                //mainDocPart.Document = new Document();
                //mainDocPart.Document.Body = new Body();
                //var body = mainDocPart.Document.Body;
                //body.Append(p);

                MemoryStream ms = new MemoryStream(
                    Encoding.Unicode.GetPreamble().Concat(
                        Encoding.Unicode.GetBytes(
                            string.Format("<html><head></head><body>{0}</body></html>", html)
                            )).ToArray());

                // Uncomment the following line to create an invalid word document.
                // MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("<h1>HELLO</h1>"));

                // Create alternative format import part.
                AlternativeFormatImportPart formatImportPart =
                   mainDocPart.AddAlternativeFormatImportPart(
                      AlternativeFormatImportPartType.Html, altChunkId);
                //ms.Seek(0, SeekOrigin.Begin);

                // Feed HTML data into format import part (chunk).
                formatImportPart.FeedData(ms);
                AltChunk altChunk = new AltChunk();
                altChunk.Id = altChunkId;

                mainDocPart.Document = new Document();
                mainDocPart.Document.Body = new Body();

                mainDocPart.Document.Body.Append(altChunk);
            }
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }

        public class DocParam
        {
            public string Name { get; set; }

            public string HtmlContent { get; set; }
        }
    }

}

