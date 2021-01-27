var DEBUG = false;
var callAjax = function (url, method, data, success, failed, loadingId) {
    var spinner = SpinnerFactory.create(loadingId);
    spinner.start();
    $.ajax({
        url: url,
        method: method,
        data: data ? JSON.stringify(data) : '',
        success: function (data) {
            spinner.stop();
            if (success != undefined && success != null)
                success(data);
        },
        error: function (error) {
            if (failed != undefined && failed != null)
                failed(JSON.stringify(error));
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });
}
var callApi = function (url, method, param) {
    if (DEBUG) {
        return param.Success(param.Fake);
    }
    callAjax(url, method, param.Data, param.Success, param.Failed, param.LoadingId);
}
var api = {
    getCurrentUserInfo: function (param) {
        callApi('/api/User/GetCurrentUserInfo', 'GET', param);
    },
    login: function (param) {
        callApi('/api/User/Login', 'POST', param);
    },
    register: function (param) {
        callApi('/api/Users/Register', 'POST', param);
    },
    updatePassword: function (param) {
        callApi('/api/Users/UpdatePassword', 'POST', param);
    }
};
var odata = {
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
};