function loadPage(page, container, callback) {
    $("#" + container).load(page + ".html?201705231250", function () {
        if (typeof (callback) != "undefined") {
            callback();
        }
    });
}
function initCKEditor(id) {
    CKEDITOR.replace(id, {
        filebrowserBrowseUrl: '',
        filebrowserWindowWidth: '',
        filebrowserWindowHeight: ''
    });
}
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}
if (!Array.prototype.select) {
    Array.prototype.select = function (func) {
        var res = [];
        for (var i = 0; i < this.length; i++) {
            res.push(func(this[i]));
        }
        return res;
    };
}
function setCookie(c_name, value, expiredays) {
    var exdate = new Date()
    exdate.setDate(exdate.getDate() + expiredays)
    document.cookie = c_name + "=" + escape(value) + ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString())
}
function getCookie(c_name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(c_name + "=")
        if (c_start != -1) {
            c_start = c_start + c_name.length + 1
            c_end = document.cookie.indexOf(";", c_start)
            if (c_end == -1) {
                c_end = document.cookie.length
            }
            return unescape(document.cookie.substring(c_start, c_end))
        }
        else {
            return null;
        }
    }
    return null;
}

/// <summary>
/// get date string from unix timestamp
/// </summary>
/// <param name="createTime">unix timestramp in second</param>
/// <returns>time string</returns>
function getDateString(createTime) {
    if (createTime) {
        var date = createTime.match(/\d{4}-\d{2}-\d{2}/);
        if (date.length > 0) {
            return date[0];
        }
    }
    return null;
}

function getRandString(count) {
    var randStr = "";
    var arr = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"];
    for (var i = 0; i < count; i++) {
        var r = Math.floor(Math.random() * 36);
        randStr = randStr + arr[r];
    }
    return randStr;
}

function getParameterValue(param) {
    var ret = null;
    var url = window.location.href;
    var paramsStr = url.split('#')[1];
    if (typeof paramsStr === "undefined") {
        return null;
    }
    var paramsExpressArray = paramsStr.split('&');
    for (var i = 0; i < paramsExpressArray.length; i++) {
        var express = paramsExpressArray[i];
        var fn = express.split('=')[0];
        var val = express.split('=')[1];
        switch (fn) {
            case param:
                ret = val;
                break;
        }
    }
    return ret;
}

function getParameterValue2(param) {
    var ret = null;
    var url = window.location.href;
    var paramsStr = url.split('?')[1];
    if (typeof paramsStr === "undefined") {
        return null;
    }
    var paramsExpressArray = paramsStr.split('&');
    for (var i = 0; i < paramsExpressArray.length; i++) {
        var express = paramsExpressArray[i];
        var fn = express.split('=')[0];
        var val = express.split('=')[1];
        switch (fn) {
            case param:
                ret = val;
                break;
        }
    }
    return ret;
}

function encode(str) {
    str = str.replace(/'/g, '&#39;');
    str = str.replace(/"/g, '&quot;');
    str = str.replace(/</g, "&lt;");
    str = str.replace(/>/g, "&gt;");
    return str;
}
function decode(str) {
    str = str.replace(/&#39;/g, "'");
    str = str.replace(/&quot;/g, '"');
    str = str.replace(/&lt;/g, "<");
    str = str.replace(/&gt;/g, ">");
    return str;
}
function formatString(str) {
    var retStr = "";
    for (var i = 0; i < str.length; i++) {
        retStr += str[i];
        if ((str[i]).charCodeAt(0) > 256 && (i + 1) < str.length) {
            retStr += "+";
        }
    }
    return retStr;
}
function getCurrentUTCDate() {
    var date;
    date = new Date();
    date = date.getUTCFullYear() + '-' +
        ('00' + (date.getUTCMonth() + 1)).slice(-2) + '-' +
        ('00' + date.getUTCDate()).slice(-2) + 'T' +
        ('00' + date.getUTCHours()).slice(-2) + ':' +
        ('00' + date.getUTCMinutes()).slice(-2) + ':' +
        ('00' + date.getUTCSeconds()).slice(-2) + "Z";
    return date;
}

function showNote(str, type) {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "2000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    toastr[type](str);
}