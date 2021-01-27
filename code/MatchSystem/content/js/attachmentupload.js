function reg() {
    $("#uploadattachmentbutton").click(function () {
        $("#fileToUpload").trigger("click");
    });
}
function fileSelected() {
    var file = document.getElementById('fileToUpload').files[0];
    if (file) {
        var fileSize = 0;
        if (file.size > 1024 * 1024)
            fileSize = (Math.round(file.size * 100 / (1024 * 1024)) / 100).toString() + 'MB';
        else
            fileSize = (Math.round(file.size * 100 / 1024) / 100).toString() + 'KB';
        startUpload(file.name);
    }
}
function startUpload(name) {
    var uploader = UPLOADFACTORY.create();
    uploader.fileName = name;
    uploader.formData.append("fileToUpload", document.getElementById('fileToUpload').files[0]);
    uploader.uploadURL = "/api/File/UploadFiles";
    uploader.onUploadProgressCallbackFunction = function (token, percent) {
        if ($("#" + token).length == 0) {
            var html = "<div id=\"{token}\" class=\"uploadfileitem\" state=\"new\">" +
                         "<div class=\"title\"><ico>&#57913;</ico> <span name=\"filename\">{name}</span> <span>进度:</span><span name=\"percent\"></span> <ico style=\"cursor:pointer;\" name=\"deleteattachment\">I</ico></div>" +
                       "</div>";
            html = html.replace("{token}", token);
            html = html.replace("{name}", name);
            $("#attachmentlistcontainer").append(html);
            $("#attachmentlistcontainer").find("ico[name='deleteattachment']").last().click(function () {
                $(this).parents(".uploadfileitem").remove();
            });
        }
        else {
            $("#" + token).find("span[name='percent']").text(percent + "% 请稍后...");
        }
    };
    uploader.onUPloadCompleteCallbackFunction = function (id, fileName, realToken, token) {
        $("#" + token).find("span[name='percent']").text("已完成上传！");
        $("#" + token).attr("fileid", id);
    };
    uploader.uploadFile();
}