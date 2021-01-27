var DEBUG = false;
var callAjax = function (url, method, data, success, failed, complete) {
    $.ajax({
        url: url,
        method: method,
        data: data ? JSON.stringify(data) : '',
        success: function (data) {
            if (success != undefined && success != null)
                success(data);
        },
        error: function (error) {
            if (failed != undefined && failed != null)
                failed(JSON.stringify(error));
        },
        complete:function(){
            if (complete != undefined && complete != null)
                complete();
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });
}
var callApi = function (url, method, param) {
    if (DEBUG) {
        return param.Success(param.Fake);
    }

    var spinner = null;
    if (typeof (param.LoadingId) != "undefined") {
        spinner = SpinnerFactory.create(param.LoadingId);
        spinner.start();
    }

    param.SuccessWrapper = function (data) {
        param.Success(data);
    }

    param.FailedWrapper = function (data) {
        param.Failed(data);
        if (spinner != null) {
            spinner.stop();
        }
    }

    param.Complete = function () {
        if (spinner != null) {
            spinner.stop();
        }
    }

    callAjax(url, method, param.Data, param.SuccessWrapper, param.FailedWrapper, param.Complete);
}
var api = {
    user: {
        getCurrentUserInfo: function (param) {
            callApi('/api/Users/GetCurrentUserInfo', 'GET', param);
        },
        //Data:{Phone:"",Email:"",Password:""}
        login: function (param) {
            callApi('/api/Users/Login', 'POST', param);
        },
        register: function (param) {
            callApi('/api/Users/Register', 'POST', param);
        },
        updatePassword: function (param) {
            callApi('/api/Users/UpdatePassword?UserId={0}'.format(param.UserId), 'POST', param);
        },
        logout: function (param) {
            callApi('/api/Users/Logout', 'GET', param);
        },
        resetPassword: function (param) {
            callApi('/api/Users/ResetPassword?userId={0}'.format(param.UserId), 'GET', param);
        }
    },
    common: {
        getCurrentServerTime: function (param) {
            callApi('/api/Common/GetCurrentServerTime', 'GET', param);
        }
    },
    mail: {
        // Data: {Address:"abc@com;abcd@com",Subject:"",Body:""}
        sendMail: function (param) {
            callApi('/api/Emails/SendMail', 'POST', param);
        }
    },
    review:{
        autoAsignReviewerByPaper:function(param){
            callApi('/api/Matches/AutoAsignReviewerByPaper?paperIds={0}&maxReviewCountEachPaper={1}'.format(param.paperIds, param.maxReviewCountEachPaper), 'GET', param);
        }
    },
    file: {
        deleteFile: function (param) {
            callApi('/api/Files/DeleteFile?fileToken={0}'.format(param.FileToken), 'GET', param);
        }
    },
    message: {
        // Data: {Title:"",Content:"",UserIds:""}
        sendMessage: function (param) {
            callApi('/api/Messages/SendMessage', 'POST', param);
        },
        getMessage: function (param) {
            callApi('/api/Messages/GetMessages?receivedMessageIds={0}'.format(param.ReceivedMessageIds), 'GET', param);
        },
    },
    shortMessage: {

    },
    odata: {
        getOne: function (param) {
            callApi('/tables/{0}({1}){2}'.format(param.SetName, param.Key, param.Uri), 'GET', param);
        },
        getList: function (param) {
            callApi('/tables/{0}{1}'.format(param.SetName, param.Uri), 'GET', param);
        },
        addOne: function (param) {
            callApi('/tables/{0}'.format(param.SetName), 'POST', param);
        },
        updateOne: function (param) {
            callApi('/tables/{0}({1})'.format(param.SetName, param.Key), 'PATCH', param);
        },
        DeleteOne: function (param) {
            callApi('/tables/{0}({1})'.format(param.SetName, param.Key), 'DELETE', param);
        }
    }
};

reviewTemplate = {};
reviewTemplate.Options = [{
    Id:1,
    Description: "所提交参赛作品、资料完整程度",
    Hint:"提交内容应包含：所选题目编号、高校指导教师信息、企业指导教师信息、毕业设计论文正文、设计图纸、设计要求、扩充版摘要、附件等内容。",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}, {
    Id:2,
    Description: "毕业设计论文格式完整程度",
    Hint: "论文应包括：题目、中文摘要(200字以上)、关键词、英文摘要、英文关键词、目录（至少包含至三级目录）、正文（不少于10000字）、参考文献（不少于20篇，其中英文文献不少于5篇）、致谢、版权声明。",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}, {
    Id:3,
    Description: "毕业设计论文正文格式规范程度",
    Hint: "毕业设计正文文档须采用Word文稿，页面设置为A4，纸张方向为纵向，正文部分双面打印；所有图表须按国家规范标准或工程要求绘制；所有公式利用Microsoft公式编辑器或Mathtype编辑。",
    TotalScore: 30,
    GetScore: 30,
    Comment: ""
}, {
    Id:4,
    Description: "毕业设计图纸要求规范程度",
    Hint: "图纸要求图面整洁，布局合理，线条粗细均匀，圆弧连接光滑，尺寸标准规范；二维图纸要求采用AutoCAD格式的.DWG文件。",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}, {
    Id:5,
    Description: "毕业设计论文整体查重率不超过20%",
    Hint: "在附件中应提供加盖学校或学院公章的查重结果证明，如页面截图等。",
    TotalScore: 10,
    GetScore: 10,
    Comment: ""
}]

reviewTemplate2 = {};
reviewTemplate2.Options = [{
    Id: 1,
    Description: "论文工作完整，章节布局合理，撰写符合规范",
    Hint: "",
    TotalScore: 10,
    GetScore: 10,
    Comment: ""
}, {
    Id: 2,
    Description: "论文工作与工程实际的相关性",
    Hint: "",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}, {
    Id: 3,
    Description: "论文体现了对复杂工程问题的理解和解决能力",
    Hint: "",
    TotalScore: 30,
    GetScore: 30,
    Comment: ""
}, {
    Id: 4,
    Description: "设计方案或结果的科学性",
    Hint: "",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}, {
    Id: 5,
    Description: "创新性",
    Hint: "",
    TotalScore: 20,
    GetScore: 20,
    Comment: ""
}]


function translatePaperCurrentStatus(code) {
    switch (code) {
        case 1:
            return "形审阶段";
            break;
        case 2:
            return "函评阶段";
            break;
        case 3:
            return "会评阶段";
            break;
        case 11:
            return "最终通过";
            break;
        case 12:
            return "最终不通过";
            break;
        default:
            return "未知状态";
            break;
    }
}
function translateMatchType(code) {
    switch (code) {
        case 1:
            return "论文";
            break;
        case 2:
            return "设计";
            break;
        default:
            return "未知类型";
            break;
    }
}